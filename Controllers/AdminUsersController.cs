using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeExperimentLab.DTO.User;
using RecipeExperimentLab.Models;

namespace RecipeExperimentLab.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminUserResponseDto>>> GetAllUsers()
        {
            var users = await _userManager.Users
                .OrderBy(user => user.Email)
                .ToListAsync();

            var response = new List<AdminUserResponseDto>();

            foreach (var user in users) 
            {
                var roles = await _userManager.GetRolesAsync(user);

                response.Add(new AdminUserResponseDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return Ok(response);
        }

        [HttpPut("{userId}/role")]
        public async Task<IActionResult> UpdateRole(
            string userId,
            UpdateUserRoleDto request)
        {
            if (request.Role is not ("Admin" or "User"))
            {
                return BadRequest("Rollen måste vara Admin eller User.");
            }

            var currentUserId = _userManager.GetUserId(User);

            if (userId == currentUserId)
            {
                return BadRequest("Du kan inte ändra din egen roll.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return NotFound("Användaren hittades inte.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains("Admin") && request.Role == "User")
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");

                if (admins.Count <= 1)
                {
                    return BadRequest("Det måste alltid finnas minst en admin.");
                }
            }

            if (!currentRoles.Contains(request.Role))
            {
                var addResult = await _userManager.AddToRoleAsync(user, request.Role);

                if (!addResult.Succeeded)
                {
                    return BadRequest(addResult.Errors);
                }
            }

            var rolesToRemove = currentRoles
                .Where(role => role != request.Role)
                .ToList();

            if (rolesToRemove.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(
                    user,
                    rolesToRemove);

                if (!removeResult.Succeeded)
                {
                    return BadRequest(removeResult.Errors);
                }
            }

            return NoContent();
        }
    }
}
