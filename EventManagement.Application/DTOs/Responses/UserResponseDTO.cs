namespace EventManagement.Application.DTOs.Responses
{
    public class UserResponseDTO
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Address { get; set; }
        public DateOnly? Birthdate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? Additional { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}