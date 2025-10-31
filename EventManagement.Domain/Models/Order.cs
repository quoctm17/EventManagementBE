using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class Order
{
    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? OrderPendingExpires { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();

    public virtual ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
