using Microsoft.AspNetCore.Identity;
using RecipeExperimentLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RecipeExperimentLab.DTO.User;

namespace RecipeExperimentLab.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AccountResponsDto>> Register(RegisterRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                return Conflict("Det finns redan en användare med den här e-postadressen.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return Problem("Kunde inte tilldela användarrollen.");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(await CreateResponseAsync(user));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AccountResponsDto>> Login(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Unauthorized("Felaktig e-postadress eller lösenord.");
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                request.Password,
                isPersistent: false,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return Unauthorized("Felaktig e-postadress eller lösenord.");
            }

            return Ok(await CreateResponseAsync(user));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AccountResponsDto>> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(await CreateResponseAsync(user));
        }

        private async Task<AccountResponsDto> CreateResponseAsync(
            ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return new AccountResponsDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            };
        }
    }
}
