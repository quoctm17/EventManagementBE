namespace EventManagement.Application.Constants
{
    public static class MessageResponse
    {
        public const string Success      = "Operation completed successfully.";
        public const string Error        = "An error occurred during the operation.";
        public const string NotFound     = "The requested resource was not found.";
        public const string BadRequest   = "The request was invalid or cannot be served.";
        public const string Unauthorized = "You are not authorized to perform this action.";
    }
}