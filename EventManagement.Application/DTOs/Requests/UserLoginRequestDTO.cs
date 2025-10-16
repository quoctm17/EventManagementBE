namespace EventManagement.Application.DTOs.Requests
{
    public class UserLoginRequestDTO
    {
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; }
    }
}