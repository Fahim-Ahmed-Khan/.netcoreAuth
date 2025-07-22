using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using AuthDemo.Data;   // Replace with your actual namespace
using AuthDemo.DTOs;     // Ensure UpdateUserDto is here

namespace AuthDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // 🔍 GET: api/admin/users
     
[HttpGet("users")]
public async Task<IActionResult> GetAllNonAdminUsers()
{
    var allUsers = _userManager.Users.ToList();
    var result = new List<object>();

    foreach (var user in allUsers)
    {
        var roles = await _userManager.GetRolesAsync(user);

        // Skip if user is Admin
        if (roles.Contains("Admin"))
            continue;

        result.Add(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.IsActive,
            Role = roles.FirstOrDefault() ?? "No Role"
        });
    }

    return Ok(result);
}



        // 🚫 PUT: api/admin/inactivate/{id}
        [HttpPut("inactivate/{id}")]
        public async Task<IActionResult> InactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            if (!user.IsActive) return BadRequest("User is already inactive");

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User inactivated successfully.");
        }

        // ✅ PUT: api/admin/activate/{id}
        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            if (user.IsActive) return BadRequest("User is already active");

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User activated successfully.");
        }

        // 🔄 PUT: api/admin/toggle-status/{id}
        [HttpPut("toggle-status/{id}")]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            // Check if user is Admin
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return BadRequest("Cannot modify Admin status");

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"User {(user.IsActive ? "activated" : "deactivated")} successfully.");
        }

        // 🛠️ PUT: api/admin/update-user/{id}
        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            user.UserName = dto.UserName ?? user.UserName;
            user.Email = dto.Email ?? user.Email;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;

            // 🔒 Handle safe role change
            if (!string.IsNullOrEmpty(dto.Role))
            {
                var allowedRoles = new[] { "User", "Manager" };

                if (!allowedRoles.Contains(dto.Role))
                    return BadRequest("You are not allowed to assign this role.");

                var currentRoles = await _userManager.GetRolesAsync(user);

                // ⛔ Prevent changing Admin role
                if (currentRoles.Contains("Admin"))
                    return BadRequest("You cannot modify the role of an Admin.");

                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    return BadRequest("Failed to remove existing roles.");

                var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
                if (!addResult.Succeeded)
                    return BadRequest("Failed to assign new role.");
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User updated successfully.");
        }
    }
}
