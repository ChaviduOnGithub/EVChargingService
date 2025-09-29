using EVChargingService.Models;
using EVChargingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EVChargingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationController : ControllerBase
    {
        private readonly StationService _service;

        public StationController(StationService service)
        {
            _service = service;
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

            // TODO: check if there are active bookings
            bool hasActiveBookings = false; // replace with actual booking check
            if (hasActiveBookings)
                return BadRequest("Cannot deactivate station with active bookings.");

            await _service.DeactivateAsync(id);
            return NoContent();
        }

    }
}
