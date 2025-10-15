namespace EventManagement.Domain.Enums
{
    public static class TicketStatus
    {
        public const string Reserved  = "Reserved";
        public const string Issued    = "Issued";
        public const string Cancelled = "Cancelled";
        public const string Refunded  = "Refunded";
        public const string CheckedIn = "CheckedIn";
        public const string NoShow    = "NoShow";
    }
}