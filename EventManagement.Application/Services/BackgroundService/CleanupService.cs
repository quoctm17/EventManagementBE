using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EventManagement.Application.Interfaces.Services;

namespace EventManagement.Application.Services.BackgroundService
{
	// Periodic cleanup for expired SeatHolds and pending Orders
	// Uses IServiceScopeFactory to resolve scoped services per run.
	public class CleanupService : Microsoft.Extensions.Hosting.BackgroundService
	{
		private readonly ILogger<CleanupService> _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly TimeSpan _interval;

		public CleanupService(
			IServiceScopeFactory scopeFactory,
			ILogger<CleanupService> logger,
			IConfiguration config)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
			var minutes = int.TryParse(config["Reservation:CleanupIntervalMinutes"], out var m) && m > 0 ? m : 5;
			_interval = TimeSpan.FromMinutes(minutes);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("CleanupService running; interval = {Minutes} minutes", _interval.TotalMinutes);

			// Optional: run once immediately
			await RunOnceSafely(stoppingToken);

			using var timer = new PeriodicTimer(_interval);
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				await RunOnceSafely(stoppingToken);
			}
		}

		private async Task RunOnceSafely(CancellationToken ct)
		{
			try
			{
				await DoWorkAsync(ct);
			}
			catch (OperationCanceledException) when (ct.IsCancellationRequested)
			{
				// graceful shutdown
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "CleanupService iteration failed");
			}
		}

		private async Task DoWorkAsync(CancellationToken ct)
		{
			await using var scope = _scopeFactory.CreateAsyncScope();
			var seatHoldRepo = scope.ServiceProvider.GetRequiredService<ISeatHoldRepository>();
			var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
			var paymentRepo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
			var ticketRepo = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
			var seatMappingRepo = scope.ServiceProvider.GetRequiredService<IEventSeatMappingRepository>();
			var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
			var realtime = scope.ServiceProvider.GetService<ISeatRealtimeService>();

			var now = DateTime.UtcNow;

			// 1) Remove expired SeatHolds
			var expiredHolds = (await seatHoldRepo.WhereAsync(h => h.HoldExpiresAt <= now)).ToList();
			var releasedFromHolds = expiredHolds
				.GroupBy(h => h.EventId)
				.ToDictionary(g => g.Key, g => g.Select(x => x.SeatId).Distinct().ToList());
			foreach (var h in expiredHolds)
			{
				ct.ThrowIfCancellationRequested();
				await seatHoldRepo.DeleteAsync(h.HoldId);
			}

			// 2) Cancel expired Pending Orders and fail their Pending payments
			var expiredOrders = (await orderRepo.WhereAsync(o => o.Status == OrderStatus.Pending && o.OrderPendingExpires != null && o.OrderPendingExpires <= now)).ToList();
			foreach (var order in expiredOrders)
			{
				ct.ThrowIfCancellationRequested();

				var pendingPayments = await paymentRepo.WhereAsync(p => p.OrderId == order.OrderId && p.Status == PaymentStatus.Pending);
				foreach (var pay in pendingPayments)
				{
					pay.Status = PaymentStatus.Failed;
					pay.TransactionDate = now;
					await paymentRepo.UpdateAsync(pay);
				}

				order.Status = OrderStatus.Cancelled;
				order.OrderPendingExpires = null;
				await orderRepo.UpdateAsync(order);
			}

			// Save payment/order changes first to avoid losing them due to seat concurrency conflicts
			if (expiredOrders.Count > 0)
			{
				await uow.SaveChangesAsync();
			}

			// 3) Release seats: set IsAvailable = true for seats in cancelled orders
			var releasedCount = 0;
			var releasedByEvent = new Dictionary<Guid, HashSet<Guid>>();
			foreach (var order in expiredOrders)
			{
				var tickets = await ticketRepo.GetTicketsByOrderIdAsync(order.OrderId);
				foreach (var t in tickets)
				{
					ct.ThrowIfCancellationRequested();
					var mappings = await seatMappingRepo.WhereAsync(m => m.EventId == t.EventId && m.SeatId == t.SeatId);
					var mapping = mappings.FirstOrDefault();
					if (mapping == null) continue;
					if (mapping.IsAvailable == true) continue;

					mapping.IsAvailable = true;
					await seatMappingRepo.UpdateAsync(mapping);
					try
					{
						await uow.SaveChangesAsync();
						releasedCount++;
						if (!releasedByEvent.TryGetValue(mapping.EventId, out var set))
						{
							set = new HashSet<Guid>();
							releasedByEvent[mapping.EventId] = set;
						}
						set.Add(mapping.SeatId);
					}
					catch (DbUpdateConcurrencyException ex)
					{
						_logger.LogWarning(ex, "Concurrency when releasing seat EventId={EventId} SeatId={SeatId}", mapping.EventId, mapping.SeatId);
						// Ignore and continue; another process likely changed it.
					}
				}
			}

			if (expiredHolds.Count > 0 || expiredOrders.Count > 0 || releasedCount > 0)
			{
				_logger.LogInformation("CleanupService removed {HoldCount} holds; cancelled {OrderCount} orders; released {SeatCount} seats", expiredHolds.Count, expiredOrders.Count, releasedCount);
			}

			// Realtime notifications (non-blocking)
			if (realtime != null)
			{
				try
				{
					foreach (var kv in releasedFromHolds)
					{
						if (kv.Value.Count > 0)
							await realtime.SeatsReleased(kv.Key, kv.Value);
					}
					foreach (var kv in releasedByEvent)
					{
						if (kv.Value.Count > 0)
							await realtime.SeatsReleased(kv.Key, kv.Value);
					}
				}
				catch { }
			}
		}
	}
}

