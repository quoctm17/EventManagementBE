namespace EventManagement.Application.DTOs.Responses
{
    public class HTTPResponseValue<T>
    {
        public T? Content { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public HTTPResponseValue() { }

        public HTTPResponseValue(T? content, string status, string message)
        {
            Content = content;
            Status  = status;
            Message = message;
        }
    }
}