using Haven.Infrastructure.Identity;
using Haven.Web.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Haven.Web.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string UserRole = "User";
    private const string AdminRole = "Admin";

    private readonly UserManager<HavenUser> _userManager;
    private readonly SignInManager<HavenUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<HavenUser> userManager,
        SignInManager<HavenUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("antiforgery")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAntiforgeryToken(
        [FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { requestToken = tokens.RequestToken });
    }

    [HttpPost("register")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountResponse>> Register(
        RegisterRequest request)
    {
        var rolesReady = await EnsureRolesAsync();
        if (!rolesReady)
        {
            return Problem(
                title: "Account registration is temporarily unavailable.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var user = new HavenUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            DisplayName = request.DisplayName.Trim()
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return ValidationProblem(ModelState);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, UserRole);
        if (!roleResult.Succeeded)
        {
            var deleteResult = await _userManager.DeleteAsync(user);
            _logger.LogError(
                "Could not assign the default role to a newly registered account. " +
                "Account rollback succeeded: {RollbackSucceeded}. Identity error codes: {ErrorCodes}",
                deleteResult.Succeeded,
                string.Join(", ", roleResult.Errors.Select(error => error.Code)));

            return Problem(
                title: "Account registration is temporarily unavailable.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        var response = await ToAccountResponseAsync(user);
        return CreatedAtAction(nameof(GetCurrentUser), response);
    }

    [HttpPost("login")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning(
                    "Account sign-in was rejected because the account is temporarily locked.");
            }

            return Unauthorized();
        }

        return Ok(await ToAccountResponseAsync(user));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return ValidationProblem(ModelState);
        }

        await _signInManager.RefreshSignInAsync(user);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountResponse>> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(await ToAccountResponseAsync(user));
    }

    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountResponse>> UpdateCurrentUser(
        UpdateDisplayNameRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        user.DisplayName = request.DisplayName.Trim();
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return ValidationProblem(ModelState);
        }

        return Ok(await ToAccountResponseAsync(user));
    }

    private async Task<bool> EnsureRolesAsync()
    {
        foreach (var roleName in new[] { UserRole, AdminRole })
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await _roleManager.CreateAsync(
                new IdentityRole(roleName));

            if (!result.Succeeded &&
                !await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogError(
                    "Could not ensure the required identity role {RoleName}. Identity error codes: {ErrorCodes}",
                    roleName,
                    string.Join(", ", result.Errors.Select(error => error.Code)));
                return false;
            }
        }

        return true;
    }

    private async Task<AccountResponse> ToAccountResponseAsync(HavenUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new AccountResponse(
            user.Id,
            user.DisplayName,
            user.Email ?? string.Empty,
            roles.Order(StringComparer.Ordinal).ToArray());
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
