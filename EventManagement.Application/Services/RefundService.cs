using System;
using System.Linq;
using System.Text.Json;
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
        private readonly IUnitOfWork _uow;

        public RefundService(
            IOrderRepository orderRepo,
            ITicketRepository ticketRepo,
            IPaymentRepository paymentRepo,
            IRefundRequestRepository refundRepo,
            IUserBankAccountRepository bankRepo,
            ITransactionRepository txnRepo,
            IUnitOfWork uow)
        {
            _orderRepo = orderRepo;
            _ticketRepo = ticketRepo;
            _paymentRepo = paymentRepo;
            _refundRepo = refundRepo;
            _bankRepo = bankRepo;
            _txnRepo = txnRepo;
            _uow = uow;
        }

        public async Task<RefundRequestResponseDTO> CreateRefundRequestAsync(string authHeader, CreateRefundRequestDTO request)
        {
            if (request == null) throw new InvalidOperationException("Invalid request");
            // Extract user id from JWT (copy from OrderService)
            if (string.IsNullOrEmpty(authHeader)) throw new InvalidOperationException("Missing auth token");
            var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length).Trim() : authHeader;
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken;
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                throw new InvalidOperationException("Invalid token");
            }
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userId)) throw new InvalidOperationException("Invalid token");

            // Validate order ownership and status
            var order = await _orderRepo.GetByIdAsync(request.OrderId);
            if (order == null) throw new InvalidOperationException("Order not found");
            if (order.UserId != userId) throw new InvalidOperationException("You are not allowed to refund this order");
            if (order.Status != OrderStatus.Paid) throw new InvalidOperationException("Only paid orders can be refunded");

            // Amount validation: must be > 0 and <= paid amount
            var paid = await _paymentRepo.WhereAsync(p => p.OrderId == order.OrderId && p.Status == PaymentStatus.Success);
            var paidTotal = paid.Sum(p => p.Amount);
            if (request.Amount <= 0 || request.Amount > paidTotal)
                throw new InvalidOperationException("Invalid refund amount");

            await _uow.BeginTransactionAsync();
            try
            {
                // Bank account resolution
                Guid? bankAccountId = null;
                string? bankName = null;
                string? accountNumber = null;
                string? holder = null;

                if (request.BankAccountId.HasValue)
                {
                    var acc = await _bankRepo.GetByIdAsync(request.BankAccountId.Value);
                    if (acc == null || acc.UserId != userId) throw new InvalidOperationException("Bank account not found");
                    bankAccountId = acc.BankAccountId;
                    bankName = acc.BankName;
                    accountNumber = acc.AccountNumber;
                    holder = acc.AccountHolderName;
                }
                else
                {
                    bankName = request.BankName;
                    accountNumber = request.AccountNumber;
                    holder = request.AccountHolderName;
                    if (request.SaveBankAccount && !string.IsNullOrWhiteSpace(bankName) && !string.IsNullOrWhiteSpace(accountNumber) && !string.IsNullOrWhiteSpace(holder))
                    {
                        // Create and optionally set as default, clearing others
                        if (request.SetAsDefault)
                        {
                            var all = await _bankRepo.GetByUserAsync(userId);
                            foreach (var b in all)
                            {
                                if (b.IsDefault == true)
                                {
                                    b.IsDefault = false;
                                    await _bankRepo.UpdateAsync(b);
                                }
                            }
                        }
                        var newAcc = new UserBankAccount
                        {
                            BankAccountId = Guid.NewGuid(),
                            UserId = userId,
                            BankName = bankName!,
                            AccountNumber = accountNumber!,
                            AccountHolderName = holder!,
                            IsActive = true,
                            IsDefault = request.SetAsDefault,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _bankRepo.AddAsync(newAcc);
                        bankAccountId = newAcc.BankAccountId;
                    }
                }

                // Create RefundRequest
                var rr = new RefundRequest
                {
                    RefundRequestId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    UserId = userId,
                    BankAccountId = bankAccountId,
                    BankName = bankName,
                    AccountNumber = accountNumber,
                    AccountHolderName = holder,
                    Amount = request.Amount,
                    Status = RefundRequestStatus.Pending,
                    Reason = request.Reason,
                    CreatedAt = DateTime.UtcNow
                };
                await _refundRepo.AddAsync(rr);

                // Update order and tickets to PendingRefund
                order.Status = OrderStatus.PendingRefund;
                await _orderRepo.UpdateAsync(order);
                var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(order.OrderId);
                foreach (var t in tickets)
                {
                    t.Status = TicketStatus.PendingRefund;
                    await _ticketRepo.UpdateAsync(t);
                }

                // Optionally create a pending Out transaction record
                var txn = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = userId,
                    RefundRequestId = rr.RefundRequestId,
                    OrderId = order.OrderId,
                    Amount = request.Amount,
                    Direction = TransactionDirection.Out,
                    Purpose = TransactionPurpose.Refund,
                    Status = TransactionStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Note = request.Reason,
                    SystemBalance = 0m
                };
                await _txnRepo.AddAsync(txn);

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();

                return new RefundRequestResponseDTO
                {
                    RefundRequestId = rr.RefundRequestId,
                    OrderId = rr.OrderId,
                    UserId = rr.UserId,
                    BankAccountId = rr.BankAccountId,
                    BankName = rr.BankName,
                    AccountNumber = rr.AccountNumber,
                    AccountHolderName = rr.AccountHolderName,
                    Amount = rr.Amount,
                    Status = rr.Status,
                    Reason = rr.Reason,
                    AdminNote = rr.AdminNote,
                    ReceiptImageUrl = rr.ReceiptImageUrl,
                    CreatedAt = rr.CreatedAt,
                    ProcessedAt = rr.ProcessedAt,
                    ProcessedBy = rr.ProcessedBy
                };
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> AdminMarkRefundPaidAsync(Guid adminUserId, AdminCompleteRefundDTO request)
        {
            if (request == null) return false;
            var rr = await _refundRepo.GetByIdAsync(request.RefundRequestId);
            if (rr == null) return false;

            await _uow.BeginTransactionAsync();
            try
            {
                rr.Status = RefundRequestStatus.Paid;
                rr.ReceiptImageUrl = request.ReceiptImageUrl;
                rr.AdminNote = request.AdminNote;
                rr.ProcessedAt = DateTime.UtcNow;
                rr.ProcessedBy = adminUserId;
                await _refundRepo.UpdateAsync(rr);

                // Update order and tickets to Refunded
                var order = await _orderRepo.GetByIdAsync(rr.OrderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Refunded;
                    await _orderRepo.UpdateAsync(order);
                    var tickets = await _ticketRepo.GetTicketsByOrderIdAsync(order.OrderId);
                    foreach (var t in tickets)
                    {
                        t.Status = TicketStatus.Refunded;
                        await _ticketRepo.UpdateAsync(t);
                    }
                }

                // Finalize the transaction as Success (Out)
                var txns = await _txnRepo.WhereAsync(t => t.RefundRequestId == rr.RefundRequestId);
                var first = txns.FirstOrDefault();
                if (first != null)
                {
                    first.Status = TransactionStatus.Success;
                    // Compute system balance including this Out transaction
                    var baseBalance = await ComputeCurrentSystemBalanceAsync();
                    var signed = -Math.Abs(first.Amount);
                    first.SystemBalance = baseBalance + signed;
                    await _txnRepo.UpdateAsync(first);
                }
                else
                {
                    var txn = new Transaction
                    {
                        TransactionId = Guid.NewGuid(),
                        UserId = rr.UserId,
                        RefundRequestId = rr.RefundRequestId,
                        OrderId = rr.OrderId,
                        Amount = rr.Amount,
                        Direction = TransactionDirection.Out,
                        Purpose = TransactionPurpose.Refund,
                        Status = TransactionStatus.Success,
                        CreatedAt = DateTime.UtcNow,
                        Note = rr.AdminNote
                    };
                    var baseBalance = await ComputeCurrentSystemBalanceAsync();
                    var signed = -Math.Abs(txn.Amount);
                    txn.SystemBalance = baseBalance + signed;
                    await _txnRepo.AddAsync(txn);
                }

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
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
