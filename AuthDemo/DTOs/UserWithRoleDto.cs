namespace AuthDemo.DTOs
{
    public class UserWithRoleDto
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string? Role { get; set; }
    }
}