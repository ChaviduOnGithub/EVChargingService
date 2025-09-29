using EVChargingService.Models;
using EVChargingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EVChargingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _service;

        public BookingController(BookingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<Booking>>> Get() =>
            await _service.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> Get(string id)
        {
            var booking = await _service.GetByIdAsync(id);
            if (booking == null) return NotFound();
            return booking;
        }

        [HttpGet("owner/{nic}")]
        public async Task<ActionResult<List<Booking>>> GetByOwner(string nic) =>
            await _service.GetByOwnerAsync(nic);

        [HttpPost]
        public async Task<IActionResult> Create(Booking booking)
        {
            try
            {
                var created = await _service.CreateAsync(booking);
                return CreatedAtAction(nameof(Get), new { id = created.BookingId }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Booking booking)
        {
            try
            {
                await _service.UpdateAsync(id, booking);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                await _service.CancelAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("station/{stationId}")]
        public async Task<ActionResult<List<Booking>>> GetByStation(string stationId)
        {
            var bookings = await _service.GetByStationAsync(stationId);
            if (bookings == null || bookings.Count == 0)
                return NotFound();
            return bookings;
        }

        [HttpGet("qr/{id}")]
        public async Task<ActionResult<Booking>> GetByQRCode(string id)
        {
            var booking = await _service.GetByIdAsync(id);
            if (booking == null) return NotFound();
            return booking;
        }

    }
}
