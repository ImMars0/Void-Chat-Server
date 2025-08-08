using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthenticationService = Void.Services.AuthenticationService;


namespace Void.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;

        public AuthenticationController(
            AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Dictionary<string, string> requestData)
        {
            try
            {
                if (!requestData.TryGetValue("username", out var username) ||
                    !requestData.TryGetValue("password", out var password) ||
                    !requestData.TryGetValue("confirmPassword", out var confirmPassword) ||
                    !requestData.TryGetValue("email", out var email))
                {
                    return BadRequest("Missing required fields");
                }

                _authenticationService.Register(username, password, confirmPassword, email);
                return Ok($"User {username} registered successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (_authenticationService.Login(request.Username, request.Password))
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.Username)
        };

                var identity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties
                    {
                        IsPersistent = request.RememberMe
                    });

                return Ok(new { message = "Logged in" });
            }
            return Unauthorized(new { message = "Invalid credentials" });
        }

        public record LoginRequest(string Username, string Password, bool RememberMe = false);

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("Logged out");
        }

        [HttpGet("check")]
        public ActionResult CheckAuth()
        {
            if (User.Identity.IsAuthenticated)

                return Ok($"Hello {User.Identity.Name}");
            return Unauthorized();

        }
    }
}