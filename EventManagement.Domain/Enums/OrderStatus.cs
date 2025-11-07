namespace EventManagement.Domain.Enums
{
    public static class OrderStatus
    {
        public const string Pending   = "Pending";
        public const string Paid      = "Paid";
        public const string Cancelled = "Cancelled";
        public const string PendingRefund = "PendingRefund";
        public const string Refunded  = "Refunded";
        public const string PartiallyRefunded = "PartiallyRefunded";
    }
}