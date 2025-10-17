using System.Security.Claims;
using EVChargingService.Models;
using EVChargingService.Services;
using Microsoft.AspNetCore.Authorization;
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
            staff.PasswordHash = existing.PasswordHash; // preserve password
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

        //Staff self-deletes their own account
        [Authorize]
        [HttpDelete("self")]
        public async Task<IActionResult> DeleteSelf()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid user token");

            var staff = await _service.GetByEmailAsync(email);
            if (staff == null)
                return NotFound("Staff not found");

            await _service.DeleteAsync(staff.StaffId);
            return Ok(new { message = "Your account has been permanently deleted." });
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

        [Authorize]
        [HttpPut("self")]
        public async Task<IActionResult> UpdateSelf([FromBody] Staff updated)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid token");

            var staff = await _service.GetByEmailAsync(email);
            if (staff == null)
                return NotFound("Staff not found");

            // Preserve ID and password if not changing
            updated.StaffId = staff.StaffId;
            updated.PasswordHash = string.IsNullOrEmpty(updated.PasswordHash)
                                    ? staff.PasswordHash
                                    : updated.PasswordHash;

            await _service.UpdateAsync(staff.StaffId, updated);
            return Ok(updated);
        }

    }
}
