using System;
using System.Collections.Generic;
using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence;

public partial class EventManagementDbContext : DbContext
{
    public EventManagementDbContext()
    {
    }

    public EventManagementDbContext(DbContextOptions<EventManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Checkin> Checkins { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventImage> EventImages { get; set; }

    public virtual DbSet<EventSeatMapping> EventSeatMappings { get; set; }

    public virtual DbSet<EventStaff> EventStaffs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrganizerRequest> OrganizerRequests { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<RefundRequest> RefundRequests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SeatHold> SeatHolds { get; set; }

    public virtual DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBankAccount> UserBankAccounts { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Venue> Venues { get; set; }

    public virtual DbSet<VenueImage> VenueImages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=127.0.0.1,1433;Database=EventManagementDB;User Id=sa;Password=khaicybersoft@123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BA1AD062C");

            entity.HasIndex(e => e.CategoryName, "UQ__Categori__8517B2E022329F96").IsUnique();

            entity.Property(e => e.CategoryId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
        });

        modelBuilder.Entity<Checkin>(entity =>
        {
            entity.HasKey(e => e.CheckinId).HasName("PK__Checkins__F3C85D713B39B771");

            entity.Property(e => e.CheckinId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckinTime).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Staff).WithMany(p => p.Checkins)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Checkins_Staff");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Checkins)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Checkins_Ticket");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__C90D3469D7F4DD68");

            entity.Property(e => e.ContractId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ContractFileUrl).HasMaxLength(500);
            entity.Property(e => e.ContractType).HasMaxLength(20);
            entity.Property(e => e.EffectiveDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Event).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Contracts_Event");

            entity.HasOne(d => d.Organizer).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.OrganizerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contracts_Organizer");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__7944C81089707E8D");

            entity.Property(e => e.EventId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.EventName).HasMaxLength(200);
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Scheduled");

            entity.HasOne(d => d.Organizer).WithMany(p => p.Events)
                .HasForeignKey(d => d.OrganizerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Events_Organizer");

            entity.HasOne(d => d.Venue).WithMany(p => p.Events)
                .HasForeignKey(d => d.VenueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Events_Venue");

            entity.HasMany(d => d.Categories).WithMany(p => p.Events)
                .UsingEntity<Dictionary<string, object>>(
                    "EventCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EventCategories_Category"),
                    l => l.HasOne<Event>().WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_EventCategories_Event"),
                    j =>
                    {
                        j.HasKey("EventId", "CategoryId").HasName("PK__EventCat__D8D45BB028418361");
                        j.ToTable("EventCategories");
                    });
        });

        modelBuilder.Entity<EventImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__EventIma__7516F70CA74023B7");

            entity.Property(e => e.ImageId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Caption).HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsCover).HasDefaultValue(false);

            entity.HasOne(d => d.Event).WithMany(p => p.EventImages)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventImages_Event");
        });

        modelBuilder.Entity<EventSeatMapping>(entity =>
        {
            entity.HasKey(e => new { e.EventId, e.SeatId }).HasName("PK__EventSea__5A55B92FA300A911");

            entity.ToTable("EventSeatMapping");

            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TicketCategory).HasMaxLength(50);

            entity.HasOne(d => d.Event).WithMany(p => p.EventSeatMappings)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventSeat_Event");

            entity.HasOne(d => d.Seat).WithMany(p => p.EventSeatMappings)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventSeat_Seat");
        });

        modelBuilder.Entity<EventStaff>(entity =>
        {
            entity.HasKey(e => new { e.EventId, e.UserId }).HasName("PK__EventSta__A83C44D4A528580B");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.EventStaffAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventStaffs_AssignedBy");

            entity.HasOne(d => d.Event).WithMany(p => p.EventStaffs)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventStaffs_Event");

            entity.HasOne(d => d.User).WithMany(p => p.EventStaffUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventStaffs_User");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12F6174932");

            entity.Property(e => e.NotificationId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.NotificationType).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Subject).HasMaxLength(200);

            entity.HasOne(d => d.Event).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Notifications_Event");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_User");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF8B0BF95E");

            entity.Property(e => e.OrderId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_User");
        });

        modelBuilder.Entity<OrganizerRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Organize__33A8517A37274A4E");

            entity.Property(e => e.RequestId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.ProcessedByNavigation).WithMany(p => p.OrganizerRequestProcessedByNavigations)
                .HasForeignKey(d => d.ProcessedBy)
                .HasConstraintName("FK_OrganizerRequests_ProcessedBy");

            entity.HasOne(d => d.User).WithMany(p => p.OrganizerRequestUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrganizerRequests_User");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A3808BA7BE9");

            entity.Property(e => e.PaymentId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TransactionDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TransactionRef).HasMaxLength(200);

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Order");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Method");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1D31D13EB85");

            entity.Property(e => e.PaymentMethodId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.GatewayKey).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MethodName).HasMaxLength(50);
            entity.Property(e => e.Provider).HasMaxLength(100);
        });

        modelBuilder.Entity<RefundRequest>(entity =>
        {
            entity.HasKey(e => e.RefundRequestId).HasName("PK__RefundRe__A67BF2293CB7E92D");

            entity.Property(e => e.RefundRequestId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccountHolderName).HasMaxLength(150);
            entity.Property(e => e.AccountNumber).HasMaxLength(50);
            entity.Property(e => e.AdminNote).HasMaxLength(500);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.ReceiptImageUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.RefundRequests)
                .HasForeignKey(d => d.BankAccountId)
                .HasConstraintName("FK_RefundRequests_Bank");

            entity.HasOne(d => d.Order).WithMany(p => p.RefundRequests)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefundRequests_Order");

            entity.HasOne(d => d.ProcessedByNavigation).WithMany(p => p.RefundRequestProcessedByNavigations)
                .HasForeignKey(d => d.ProcessedBy)
                .HasConstraintName("FK_RefundRequests_Admin");

            entity.HasOne(d => d.User).WithMany(p => p.RefundRequestUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefundRequests_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A59A5CD8B");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61605C158319").IsUnique();

            entity.Property(e => e.RoleId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId).HasName("PK__Seats__311713F38B33AE0D");

            entity.HasIndex(e => new { e.VenueId, e.RowLabel, e.SeatNumber }, "UQ_Seats").IsUnique();

            entity.Property(e => e.SeatId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RowLabel).HasMaxLength(10);

            entity.HasOne(d => d.Venue).WithMany(p => p.Seats)
                .HasForeignKey(d => d.VenueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seats_Venues");
        });

        modelBuilder.Entity<SeatHold>(entity =>
        {
            entity.HasKey(e => e.HoldId).HasName("PK__SeatHold__6E24D9C45A451C97");

            entity.HasIndex(e => new { e.EventId, e.SeatId }, "UQ_Hold").IsUnique();

            entity.Property(e => e.HoldId).ValueGeneratedNever();

            entity.HasOne(d => d.Event).WithMany(p => p.SeatHolds)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeatHolds_Event");

            entity.HasOne(d => d.Order).WithMany(p => p.SeatHolds)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_SeatHolds_Order");

            entity.HasOne(d => d.Seat).WithMany(p => p.SeatHolds)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeatHolds_Seat");

            entity.HasOne(d => d.User).WithMany(p => p.SeatHolds)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeatHolds_User");
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasKey(e => e.SettlementId).HasName("PK__Settleme__7712545A66A606EE");

            entity.Property(e => e.SettlementId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CommissionFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NetAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Event).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Settlements_Event");

            entity.HasOne(d => d.Organizer).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.OrganizerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Settlements_Organizer");
        });

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PK__SystemCo__C3BC335CEB46F647");

            entity.HasIndex(e => e.ConfigKey, "UQ__SystemCo__4A306784D4120DD9").IsUnique();

            entity.Property(e => e.ConfigId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ConfigKey).HasMaxLength(100);
            entity.Property(e => e.ConfigValue).HasMaxLength(500);
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__SystemLo__5E548648D24A5ACF");

            entity.Property(e => e.LogId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Action).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.SystemLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_SystemLogs_User");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Tickets__712CC6070E4EB692");

            entity.Property(e => e.TicketId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PurchaseDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.QrImageUrl).HasMaxLength(500);
            entity.Property(e => e.Qrcode)
                .HasMaxLength(500)
                .HasColumnName("QRCode");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Reserved");

            entity.HasOne(d => d.Attendee).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.AttendeeId)
                .HasConstraintName("FK_Tickets_Attendee");

            entity.HasOne(d => d.Order).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Order");

            entity.HasOne(d => d.EventSeatMapping).WithMany(p => p.Tickets)
                .HasForeignKey(d => new { d.EventId, d.SeatId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_EventSeat");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A6B625FBA5E");

            entity.Property(e => e.TransactionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Direction).HasMaxLength(10);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Purpose).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Success");
            entity.Property(e => e.SystemBalance).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Transactions_Order");

            entity.HasOne(d => d.Payment).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK_Transactions_Payment");

            entity.HasOne(d => d.RefundRequest).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.RefundRequestId)
                .HasConstraintName("FK_Transactions_Refund");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Transactions_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C1FFAAAD4");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105344EE2CA6E").IsUnique();

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<UserBankAccount>(entity =>
        {
            entity.HasKey(e => e.BankAccountId).HasName("PK__UserBank__4FC8E4A1A921CC6B");

            entity.HasIndex(e => new { e.UserId, e.BankName, e.AccountNumber }, "UQ_UserBankAccounts").IsUnique();

            entity.Property(e => e.BankAccountId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccountHolderName).HasMaxLength(150);
            entity.Property(e => e.AccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.UserBankAccounts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserBankAccounts_User");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__AF2760AD5CC92FCC");

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Users");
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.VenueId).HasName("PK__Venues__3C57E5F2D8A81A6C");

            entity.Property(e => e.VenueId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Province)
                .HasMaxLength(100)
                .HasDefaultValue("Hà Nội");
            entity.Property(e => e.VenueName).HasMaxLength(200);
        });

        modelBuilder.Entity<VenueImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__VenueIma__7516F70C875B1262");

            entity.Property(e => e.ImageId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Caption).HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsMain).HasDefaultValue(false);

            entity.HasOne(d => d.Venue).WithMany(p => p.VenueImages)
                .HasForeignKey(d => d.VenueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VenueImages_Venue");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
