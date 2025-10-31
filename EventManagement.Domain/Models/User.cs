using System;
using System.Collections.Generic;

namespace EventManagement.Domain.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Address { get; set; }

    public DateOnly? Birthdate { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public string? Additional { get; set; }

    public virtual ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<EventStaff> EventStaffAssignedByNavigations { get; set; } = new List<EventStaff>();

    public virtual ICollection<EventStaff> EventStaffUsers { get; set; } = new List<EventStaff>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<OrganizerRequest> OrganizerRequestProcessedByNavigations { get; set; } = new List<OrganizerRequest>();

    public virtual ICollection<OrganizerRequest> OrganizerRequestUsers { get; set; } = new List<OrganizerRequest>();

    public virtual ICollection<RefundRequest> RefundRequestProcessedByNavigations { get; set; } = new List<RefundRequest>();

    public virtual ICollection<RefundRequest> RefundRequestUsers { get; set; } = new List<RefundRequest>();

    public virtual ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserBankAccount> UserBankAccounts { get; set; } = new List<UserBankAccount>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
