using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Void.Models;
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
        public IActionResult Register(string username, string password, string confirmpassword, string email)
        {
            try
            {
                _authenticationService.Register(username, password, confirmpassword, email);

                var user = new User { UserName = username, Password = password, Email = email };

                return Ok($"User {username} registered successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (_authenticationService.Login(username, password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                return Ok("Logged in");
            }
            return Unauthorized("Invalid credentials");
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