using Microsoft.AspNetCore.Mvc;
using EVChargingService.Models;
using EVChargingService.Services;

namespace EVChargingService.Controllers
{
    [ApiController]
    [Route("api/evowner/auth")]
    public class EVOwnerAuthController : ControllerBase
    {
        private readonly EVOwnerAuthService _authService;

        public EVOwnerAuthController(EVOwnerAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] EVOwnerRegisterRequest request)
        {
            var owner = new EVOwner
            {
                NIC = request.NIC,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone
            };

            var success = await _authService.RegisterAsync(owner, request.Password);
            if (!success)
                return BadRequest(new { message = "User already exists with this NIC or Email" });

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] EVOwnerLoginRequest request)
        {
            var token = await _authService.LoginAsync(request.NIC, request.Password);
            if (token == null)
                return Unauthorized(new { message = "Invalid NIC or password" });

            return Ok(new { token, role = "EVOwner" });
        }
    }

    public class EVOwnerRegisterRequest
    {
        public string NIC { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
    }

    public class EVOwnerLoginRequest
    {
        public string NIC { get; set; }
        public string Password { get; set; }
    }
}
