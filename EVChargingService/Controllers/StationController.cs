using EVChargingService.Models;
using EVChargingService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVChargingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StationController : ControllerBase
    {
        private readonly StationService _service;
        private readonly StaffAuthService _authService;

        public StationController(StationService service, StaffAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Station>>> Get() =>
            await _service.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Station>> Get(string id)
        {
            var station = await _service.GetByIdAsync(id);
            if (station == null) return NotFound();
            return station;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Station station)
        {
            await _service.CreateAsync(station);
            return CreatedAtAction(nameof(Get), new { id = station.StationId }, station);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Station station)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            station.StationId = id;
            await _service.UpdateAsync(id, station);
            return NoContent();
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(string id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            bool hasActiveBookings = false; // TODO: add booking logic later
            if (hasActiveBookings)
                return BadRequest("Cannot deactivate station with active bookings.");

            await _service.DeactivateAsync(id);
            return NoContent();
        }

        // All staff can view their assigned station dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetStationDashboard()
        {
            var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var stationId = User.Claims.FirstOrDefault(c => c.Type == "StationId")?.Value;

            if (string.IsNullOrEmpty(staffId) || string.IsNullOrEmpty(stationId))
                return Unauthorized("Invalid token or station assignment.");

            var staff = await _authService.GetByIdAsync(staffId);
            if (staff == null) return Unauthorized("Staff not found or inactive.");

            var station = await _service.GetByIdAsync(stationId);
            if (station == null) return NotFound("Station not found.");

            var assignedStaff = await _service.GetByStationAsync(stationId);

            var response = new
            {
                Station = station,
                LoggedInStaff = new
                {
                    staff.Name,
                    staff.Role,
                    staff.Email
                },
                AssignedStaff = assignedStaff.Select(s => new
                {
                    s.Name,
                    s.Email,
                    s.Role
                })
            };

            return Ok(response);
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> Nearby(double lat, double lng, int radiusKm = 10)
        {
            var allStations = await _service.GetAllAsync();
            var nearbyStations = allStations
                .Where(s => s.IsActive)
                .Where(s => GetDistanceKm(s.Latitude, s.Longitude, lat, lng) <= radiusKm)
                .Select(s => new {
                    _id = s.StationId, 
                    lat = s.Latitude,
                    lng = s.Longitude,
                    name = s.Name
                }).ToList();

            return Ok(new { items = nearbyStations });
        }

        // Haversine formula
        private double GetDistanceKm(double lat1, double lng1, double lat2, double lng2)
        {
            var R = 6371.0; // km
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLng = (lng2 - lng1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

    }
}
