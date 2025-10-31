namespace EventManagement.Domain.Enums
{
    public static class TransactionPurpose
    {
        public const string TicketPayment    = "TicketPayment";
        public const string Refund           = "Refund";
        public const string SystemAdjustment = "SystemAdjustment";
        public const string Settlement       = "Settlement";
    }
}
