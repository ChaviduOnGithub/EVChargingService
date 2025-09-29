using EVChargingService.Models;
using EVChargingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EVChargingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly StaffService _service;

        public StaffController(StaffService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<Staff>>> GetAll() =>
            await _service.GetAllAsync();

        [HttpGet("station/{stationId}")]
        public async Task<ActionResult<List<Staff>>> GetByStation(string stationId) =>
            await _service.GetByStationAsync(stationId);

        [HttpGet("{id}")]
        public async Task<ActionResult<Staff>> GetById(string id)
        {
            var staff = await _service.GetByIdAsync(id);
            if (staff == null) return NotFound();
            return staff;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Staff staff)
        {
            await _service.CreateAsync(staff);
            return CreatedAtAction(nameof(GetById), new { id = staff.StaffId }, staff);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Staff staff)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            staff.StaffId = id;

            // ✅ Preserve password hash
            staff.PasswordHash = existing.PasswordHash;

            await _service.UpdateAsync(id, staff);
            return Ok(staff);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(string id)
        {
            var result = await _service.DeactivateAsync(id);
            if (!result) return BadRequest("Unable to deactivate staff");
            return Ok(new { message = $"Staff {id} deactivated" });
        }

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> Activate(string id)
        {
            var result = await _service.ActivateAsync(id);
            if (!result) return BadRequest("Unable to activate staff");
            return Ok(new { message = $"Staff {id} activated" });
        }
    }
}
