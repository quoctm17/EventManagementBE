// CLEAN REIMPLEMENTATION OF RefundService AFTER CORRUPTION REMOVAL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.Application.DTOs.Requests.Refunds;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Repositories;
using EventManagement.Application.Interfaces.Services;
using EventManagement.Domain.Enums;
using EventManagement.Domain.Models;

namespace EventManagement.Application.Services
{
    public class RefundService : IRefundService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IRefundRequestRepository _refundRepo;
        private readonly IUserBankAccountRepository _bankRepo;
        private readonly ITransactionRepository _txnRepo;
        private readonly IRefundPolicyRepository _policyRepo; // interface must be implemented & registered
        private readonly IUnitOfWork _uow;

        public RefundService(IOrderRepository orderRepo,
            ITicketRepository ticketRepo,
            IPaymentRepository paymentRepo,
            IRefundRequestRepository refundRepo,
            IUserBankAccountRepository bankRepo,
            ITransactionRepository txnRepo,
            IRefundPolicyRepository policyRepo,
            IUnitOfWork uow)
        {
            _orderRepo = orderRepo;
            _ticketRepo = ticketRepo;
            _paymentRepo = paymentRepo;
            _refundRepo = refundRepo;
            _bankRepo = bankRepo;
            _txnRepo = txnRepo;
            _policyRepo = policyRepo;
            _uow = uow;
        }

        public async Task<RefundRequestResponseDTO> CreateRefundRequestAsync(string authHeader, CreateRefundRequestDTO request)
        {
            if (request == null) throw new InvalidOperationException("Invalid request");
            if (string.IsNullOrEmpty(authHeader)) throw new InvalidOperationException("Missing auth token");
            var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length).Trim() : authHeader;
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken;
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                jwtToken = handler.ReadJwtToken(token);
            }
            catch { throw new InvalidOperationException("Invalid token"); }
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userId)) throw new InvalidOperationException("Invalid token");

            var order = await _orderRepo.GetByIdAsync(request.OrderId);
            if (order == null) throw new InvalidOperationException("Order not found");
            if (order.UserId != userId) throw new InvalidOperationException("You are not allowed to refund this order");
            if (order.Status != OrderStatus.Paid) throw new InvalidOperationException("Only paid orders can be refunded");

            var existingPending = await _refundRepo.WhereAsync(r => r.OrderId == order.OrderId && r.Status == RefundRequestStatus.Pending);
            if (existingPending.Any()) throw new InvalidOperationException("There is already a pending refund request for this order");

            var paid = await _paymentRepo.WhereAsync(p => p.OrderId == order.OrderId && p.Status == PaymentStatus.Success);
            var paidTotal = paid.Sum(p => p.Amount);
            var refundedPaid = await _refundRepo.WhereAsync(r => r.OrderId == order.OrderId && r.Status == RefundRequestStatus.Paid);
            var refundedTotal = refundedPaid.Sum(r => r.Amount);
            var remainingMax = paidTotal - refundedTotal;
            if (remainingMax <= 0) throw new InvalidOperationException("Order has been fully refunded");

            var allTickets = await _ticketRepo.GetTicketsByOrderIdAsync(order.OrderId);
            var targetTickets = allTickets;
            if (request.TicketIds != null && request.TicketIds.Any())
            {
                var idSet = request.TicketIds.ToHashSet();
                targetTickets = allTickets.Where(t => idSet.Contains(t.TicketId)).ToList();
                if (!targetTickets.Any()) throw new InvalidOperationException("No valid tickets selected for refund");
            }

            var items = new List<RefundRequestItem>();
            decimal total = 0m;
            foreach (var t in targetTickets)
            {
                var eventObj = t.EventSeatMapping?.Event; // assumption: navigation loaded by repository method
                var eventStart = eventObj?.EventStartTime ?? DateTime.UtcNow;
                var category = t.EventSeatMapping?.TicketCategory;
                var policy = await _policyRepo.GetApplicablePolicyAsync(eventObj?.EventId, category);
                var rule = policy?.RefundPolicyRules
                    ?.Where(r => (eventStart - DateTime.UtcNow).TotalMinutes >= r.CutoffMinutesBeforeStart)
                    .OrderByDescending(r => r.CutoffMinutesBeforeStart)
                    .FirstOrDefault();
                if (rule == null || rule.RefundPercent <= 0) continue; // not eligible, skip ticket
                var amount = Math.Max(0m, (t.Price * rule.RefundPercent / 100m) - (rule.FlatFee ?? 0m));
                if (amount <= 0) continue;
                if (total + amount > remainingMax) amount = Math.Max(0, remainingMax - total);
                if (amount <= 0) break;
                items.Add(new RefundRequestItem
                {
                    RefundRequestItemId = Guid.NewGuid(),
                    TicketId = t.TicketId,
                    RefundRuleId = rule.RefundPolicyRuleId,
                    RefundPercentApplied = rule.RefundPercent,
                    RefundAmount = amount
                });
                total += amount;
                if (total >= remainingMax) break;
            }
            if (!items.Any()) throw new InvalidOperationException("Nothing refundable for selected tickets");
            if (request.Amount.HasValue && request.Amount.Value > total) throw new InvalidOperationException($"Requested amount exceeds computed refundable total {total}");

            await _uow.BeginTransactionAsync();
            try
            {
                Guid? bankAccountId = null; string? bankName = null; string? accountNumber = null; string? holder = null;
                if (request.BankAccountId.HasValue)
                {
                    var acc = await _bankRepo.GetByIdAsync(request.BankAccountId.Value);
                    if (acc == null || acc.UserId != userId) throw new InvalidOperationException("Bank account not found");
                    bankAccountId = acc.BankAccountId; bankName = acc.BankName; accountNumber = acc.AccountNumber; holder = acc.AccountHolderName;
                }
                else
                {
                    bankName = request.BankName; accountNumber = request.AccountNumber; holder = request.AccountHolderName;
                    if (request.SaveBankAccount && !string.IsNullOrWhiteSpace(bankName) && !string.IsNullOrWhiteSpace(accountNumber) && !string.IsNullOrWhiteSpace(holder))
                    {
                        if (request.SetAsDefault)
                        {
                            var all = await _bankRepo.GetByUserAsync(userId);
                            foreach (var b in all.Where(x => x.IsDefault == true)) { b.IsDefault = false; await _bankRepo.UpdateAsync(b); }
                        }
                        var newAcc = new UserBankAccount
                        {
                            BankAccountId = Guid.NewGuid(), UserId = userId, BankName = bankName!, AccountNumber = accountNumber!, AccountHolderName = holder!, IsActive = true, IsDefault = request.SetAsDefault, CreatedAt = DateTime.UtcNow
                        };
                        await _bankRepo.AddAsync(newAcc); bankAccountId = newAcc.BankAccountId;
                    }
                }

                var rr = new RefundRequest
                {
                    RefundRequestId = Guid.NewGuid(), OrderId = order.OrderId, UserId = userId, BankAccountId = bankAccountId,
                    BankName = bankName, AccountNumber = accountNumber, AccountHolderName = holder, Amount = request.Amount.HasValue && request.Amount.Value > 0 ? Math.Min(request.Amount.Value, total) : total,
                    Status = RefundRequestStatus.Pending, Reason = request.Reason, CreatedAt = DateTime.UtcNow
                };
                foreach (var it in items) rr.RefundRequestItems.Add(it);
                await _refundRepo.AddAsync(rr);

                var txn = new Transaction
                {
                    TransactionId = Guid.NewGuid(), UserId = userId, RefundRequestId = rr.RefundRequestId, OrderId = order.OrderId,
                    Amount = rr.Amount, Direction = TransactionDirection.Out, Purpose = TransactionPurpose.Refund, Status = TransactionStatus.Pending, CreatedAt = DateTime.UtcNow, Note = request.Reason, SystemBalance = 0m
                };
                await _txnRepo.AddAsync(txn);

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                return new RefundRequestResponseDTO
                {
                    RefundRequestId = rr.RefundRequestId, OrderId = rr.OrderId, UserId = rr.UserId, BankAccountId = rr.BankAccountId,
                    BankName = rr.BankName, AccountNumber = rr.AccountNumber, AccountHolderName = rr.AccountHolderName, Amount = rr.Amount,
                    Status = rr.Status, Reason = rr.Reason, AdminNote = rr.AdminNote, ReceiptImageUrl = rr.ReceiptImageUrl, CreatedAt = rr.CreatedAt, ProcessedAt = rr.ProcessedAt, ProcessedBy = rr.ProcessedBy,
                    Items = rr.RefundRequestItems.Select(i => new RefundRequestItemDTO { RefundRequestItemId = i.RefundRequestItemId, TicketId = i.TicketId, RefundRuleId = i.RefundRuleId, RefundPercentApplied = i.RefundPercentApplied, RefundAmount = i.RefundAmount }).ToList()
                };
            }
            catch { await _uow.RollbackTransactionAsync(); throw; }
        }

        public async Task<bool> AdminAcceptRefundAsync(Guid adminUserId, AdminAcceptRefundDTO request)
        {
            if (request == null) return false;
            var rr = await _refundRepo.GetWithItemsAsync(request.RefundRequestId);
            if (rr == null) return false;
            if (rr.Status != RefundRequestStatus.Pending) throw new InvalidOperationException("Refund request is not in Pending state");
            if (rr.RefundRequestItems == null || rr.RefundRequestItems.Count == 0) throw new InvalidOperationException("Refund request has no items to accept");

            await _uow.BeginTransactionAsync();
            try
            {
                rr.Status = RefundRequestStatus.Approved; rr.AdminNote = request.AdminNote; rr.ProcessedAt = DateTime.UtcNow; rr.ProcessedBy = adminUserId; await _refundRepo.UpdateAsync(rr);
                var order = await _orderRepo.GetByIdAsync(rr.OrderId);
                if (order != null)
                {
                    var allTickets = await _ticketRepo.GetTicketsByOrderIdAsync(order.OrderId);
                    var affected = rr.RefundRequestItems.Select(i => i.TicketId).ToHashSet();
                    foreach (var t in allTickets.Where(t => affected.Contains(t.TicketId))) { t.Status = TicketStatus.PendingRefund; await _ticketRepo.UpdateAsync(t); }
                    var remainingActive = allTickets.Count(t => t.Status != TicketStatus.PendingRefund && t.Status != TicketStatus.Refunded);
                    order.Status = remainingActive == 0 ? OrderStatus.PendingRefund : OrderStatus.PartiallyRefunded; await _orderRepo.UpdateAsync(order);
                }
                await _uow.SaveChangesAsync(); await _uow.CommitTransactionAsync(); return true;
            }
            catch { await _uow.RollbackTransactionAsync(); throw; }
        }

        public async Task<bool> AdminMarkRefundPaidAsync(Guid adminUserId, AdminCompleteRefundDTO request)
        {
            if (request == null) return false;
            var rr = await _refundRepo.GetWithItemsAsync(request.RefundRequestId) ?? await _refundRepo.GetByIdAsync(request.RefundRequestId);
            if (rr == null) return false;
            await _uow.BeginTransactionAsync();
            try
            {
                rr.Status = RefundRequestStatus.Paid; rr.ReceiptImageUrl = request.ReceiptImageUrl; rr.AdminNote = request.AdminNote; rr.ProcessedAt = DateTime.UtcNow; rr.ProcessedBy = adminUserId; await _refundRepo.UpdateAsync(rr);
                var order = await _orderRepo.GetByIdAsync(rr.OrderId);
                if (order != null)
                {
                    var allTickets = await _ticketRepo.GetTicketsByOrderIdAsync(order.OrderId);
                    var refundedIds = rr.RefundRequestItems.Select(i => i.TicketId).ToHashSet();
                    foreach (var t in allTickets.Where(t => refundedIds.Contains(t.TicketId))) { t.Status = TicketStatus.Refunded; await _ticketRepo.UpdateAsync(t); }
                    var remainingActive = allTickets.Count(t => t.Status != TicketStatus.Refunded);
                    order.Status = remainingActive == 0 ? OrderStatus.Refunded : OrderStatus.PartiallyRefunded; await _orderRepo.UpdateAsync(order);
                }
                var txns = await _txnRepo.WhereAsync(t => t.RefundRequestId == rr.RefundRequestId);
                var first = txns.FirstOrDefault();
                if (first != null)
                {
                    first.Status = TransactionStatus.Success; var baseBalance = await ComputeCurrentSystemBalanceAsync(); var signed = -Math.Abs(first.Amount); first.SystemBalance = baseBalance + signed; await _txnRepo.UpdateAsync(first);
                }
                else
                {
                    var txn = new Transaction { TransactionId = Guid.NewGuid(), UserId = rr.UserId, RefundRequestId = rr.RefundRequestId, OrderId = rr.OrderId, Amount = rr.Amount, Direction = TransactionDirection.Out, Purpose = TransactionPurpose.Refund, Status = TransactionStatus.Success, CreatedAt = DateTime.UtcNow, Note = rr.AdminNote };
                    var baseBalance = await ComputeCurrentSystemBalanceAsync(); var signed = -Math.Abs(txn.Amount); txn.SystemBalance = baseBalance + signed; await _txnRepo.AddAsync(txn);
                }
                await _uow.SaveChangesAsync(); await _uow.CommitTransactionAsync(); return true;
            }
            catch { await _uow.RollbackTransactionAsync(); throw; }
        }

        public async Task<bool> AdminRejectRefundAsync(Guid adminUserId, AdminRejectRefundDTO request)
        {
            if (request == null) return false;
            var rr = await _refundRepo.GetWithItemsAsync(request.RefundRequestId) ?? await _refundRepo.GetByIdAsync(request.RefundRequestId);
            if (rr == null) return false;
            if (rr.Status != RefundRequestStatus.Pending && rr.Status != RefundRequestStatus.Approved) throw new InvalidOperationException("Only Pending or Approved requests can be rejected");
            await _uow.BeginTransactionAsync();
            try
            {
                if (rr.Status == RefundRequestStatus.Approved)
                {
                    var order = await _orderRepo.GetByIdAsync(rr.OrderId);
                    if (order != null)
                    {
                        var allTickets = await _ticketRepo.GetTicketsByOrderIdAsync(order.OrderId);
                        var affected = rr.RefundRequestItems.Select(i => i.TicketId).ToHashSet();
                        foreach (var t in allTickets.Where(t => affected.Contains(t.TicketId) && t.Status == TicketStatus.PendingRefund)) { t.Status = TicketStatus.Issued; await _ticketRepo.UpdateAsync(t); }
                        var allAfter = await _ticketRepo.GetTicketsByOrderIdAsync(order.OrderId);
                        if (allAfter.All(t => t.Status == TicketStatus.Refunded)) order.Status = OrderStatus.Refunded;
                        else if (allAfter.Any(t => t.Status == TicketStatus.Refunded)) order.Status = OrderStatus.PartiallyRefunded;
                        else order.Status = OrderStatus.Paid;
                        await _orderRepo.UpdateAsync(order);
                    }
                }
                rr.Status = RefundRequestStatus.Rejected; rr.AdminNote = request.AdminNote; rr.ProcessedAt = DateTime.UtcNow; rr.ProcessedBy = adminUserId; await _refundRepo.UpdateAsync(rr);
                var pendingTxns = await _txnRepo.WhereAsync(t => t.RefundRequestId == rr.RefundRequestId && t.Status == TransactionStatus.Pending);
                foreach (var t in pendingTxns) { t.Status = TransactionStatus.Failed; await _txnRepo.UpdateAsync(t); }
                await _uow.SaveChangesAsync(); await _uow.CommitTransactionAsync(); return true;
            }
            catch { await _uow.RollbackTransactionAsync(); throw; }
        }

        private async Task<decimal> ComputeCurrentSystemBalanceAsync()
        {
            var successTxns = await _txnRepo.WhereAsync(t => t.Status == TransactionStatus.Success);
            decimal balance = 0m;
            foreach (var t in successTxns)
            {
                var amt = Math.Abs(t.Amount);
                if (t.Direction == TransactionDirection.In) balance += amt; else balance -= amt;
            }
            return balance;
        }
    }
}
