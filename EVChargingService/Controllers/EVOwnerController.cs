using EVChargingService.Models;
using EVChargingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EVChargingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EVOwnerController : ControllerBase
    {
        private readonly EVOwnerService _service;

        public EVOwnerController(EVOwnerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<EVOwner>>> Get() =>
            await _service.GetAllAsync();

        [HttpGet("{nic}")]
        public async Task<ActionResult<EVOwner>> Get(string nic)
        {
            var owner = await _service.GetByNICAsync(nic);
            if (owner == null) return NotFound();
            return owner;
        }

        [HttpPost]
        public async Task<IActionResult> Create(EVOwner owner)
        {
            await _service.CreateAsync(owner);
            return CreatedAtAction(nameof(Get), new { nic = owner.NIC }, owner);
        }

        [HttpPut("{nic}")]
        public async Task<IActionResult> Update(string nic, EVOwner owner)
        {
            var existing = await _service.GetByNICAsync(nic);
            if (existing == null) return NotFound();

            owner.NIC = nic;
            await _service.UpdateAsync(nic, owner);
            return NoContent();
        }

        [HttpDelete("{nic}")]
        public async Task<IActionResult> Delete(string nic)
        {
            var existing = await _service.GetByNICAsync(nic);
            if (existing == null) return NotFound();

            await _service.DeleteAsync(nic);
            return NoContent();
        }

        [HttpPatch("{nic}/activate")]
        public async Task<IActionResult> Activate(string nic)
        {
            var existing = await _service.GetByNICAsync(nic);
            if (existing == null) return NotFound();

            var updated = await _service.ActivateAsync(nic);
            if (!updated) return BadRequest("Unable to activate account.");

            return Ok(new { message = $"EVOwner {nic} activated successfully" });
        }

        [HttpPatch("{nic}/deactivate")]
        public async Task<IActionResult> Deactivate(string nic)
        {
            var existing = await _service.GetByNICAsync(nic);
            if (existing == null) return NotFound();

            var updated = await _service.DeactivateAsync(nic);
            if (!updated) return BadRequest("Unable to deactivate account.");

            return Ok(new { message = $"EVOwner {nic} deactivated successfully" });
        }

        [HttpGet("{nic}/upcoming-bookings")]
        public async Task<ActionResult<List<Booking>>> GetUpcomingBookings(string nic)
        {
            var owner = await _service.GetByNICAsync(nic);
            if (owner == null) return NotFound();

            var bookings = await _service.GetUpcomingBookingsAsync(nic);
            return Ok(bookings);
        }

        [HttpGet("{nic}/past-bookings")]
        public async Task<ActionResult<List<Booking>>> GetPastBookings(string nic)
        {
            var owner = await _service.GetByNICAsync(nic);
            if (owner == null) return NotFound();

            var bookings = await _service.GetPastBookingsAsync(nic);
            return Ok(bookings);
        }

    }
}
