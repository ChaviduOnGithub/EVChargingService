using EVChargingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QRCoder;

namespace EVChargingService.Services
{
    public class BookingService
    {
        private readonly IMongoCollection<Booking> _bookings;

        public BookingService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _bookings = database.GetCollection<Booking>(dbSettings.Value.BookingsCollection);
        }

        public async Task<List<Booking>> GetAllAsync() =>
            await _bookings.Find(_ => true).ToListAsync();

        public async Task<Booking> GetByIdAsync(string id) =>
            await _bookings.Find(b => b.BookingId == id).FirstOrDefaultAsync();

        // Get bookings by station
        public async Task<List<Booking>> GetByStationAsync(string stationId) =>
            await _bookings.Find(b => b.StationId == stationId).ToListAsync();


        // Create Booking with 7-day rule
        public async Task<Booking> CreateAsync(Booking booking)
        {
            if ((booking.ReservationDateTime - DateTime.UtcNow).TotalDays > 7)
                throw new Exception("Reservation can only be within 7 days from today.");

            await _bookings.InsertOneAsync(booking);

            // Generate QR after booking is saved (use BookingId as payload)
            booking.QRCode = GenerateQRCode(booking.BookingId);

            // Update booking with QRCode string
            var update = Builders<Booking>.Update.Set(b => b.QRCode, booking.QRCode);
            await _bookings.UpdateOneAsync(b => b.BookingId == booking.BookingId, update);

            return booking;
        }

        private string GenerateQRCode(string bookingId)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(bookingId, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }

        // Update Booking with 12-hour rule
        public async Task UpdateAsync(string id, Booking booking)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null) throw new Exception("Booking not found.");

            if ((existing.ReservationDateTime - DateTime.UtcNow).TotalHours < 12)
                throw new Exception("Cannot update booking less than 12 hours before reservation.");

            booking.BookingId = id;
            await _bookings.ReplaceOneAsync(b => b.BookingId == id, booking);
        }

        // Cancel Booking with 12-hour rule
        public async Task CancelAsync(string id)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null) throw new Exception("Booking not found.");

            if ((existing.ReservationDateTime - DateTime.UtcNow).TotalHours < 12)
                throw new Exception("Cannot cancel booking less than 12 hours before reservation.");

            var update = Builders<Booking>.Update.Set(b => b.Status, "Cancelled");
            await _bookings.UpdateOneAsync(b => b.BookingId == id, update);
        }

        // Get bookings by owner
        public async Task<List<Booking>> GetByOwnerAsync(string ownerNIC) =>
            await _bookings.Find(b => b.OwnerNIC == ownerNIC).ToListAsync();
    }
}
