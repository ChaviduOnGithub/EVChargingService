using EVChargingService.Models;
using EVChargingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EVChargingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffAuthController : ControllerBase
    {
        private readonly StaffAuthService _authService;

        public StaffAuthController(StaffAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] StaffRegisterRequest request)
        {
            try
            {
                var staff = new Staff
                {
                    Name = request.Name,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Phone = request.Phone,
                    Role = request.Role,
                    StationId = request.StationId,
                    IsActive = true
                };

                await _authService.RegisterAsync(staff);
                return Ok(new { message = "Staff registered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.AuthenticateAsync(request.Email, request.Password);
            if (token == null) return Unauthorized(new { message = "Invalid credentials" });

            return Ok(new { token }); // lowercase token to match frontend
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaff(string id)
        {
            var staff = await _authService.GetByIdAsync(id);
            if (staff == null) return NotFound();
            return Ok(staff);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class StaffRegisterRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string StationId { get; set; }
    }
}
