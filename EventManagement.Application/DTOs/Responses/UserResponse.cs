namespace EventManagement.Application.DTOs.Responses
{
    public class UserResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}