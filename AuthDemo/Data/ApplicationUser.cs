using Microsoft.AspNetCore.Identity;

namespace AuthDemo.Data
{
	public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true; // ✅ set default to true
}
}
