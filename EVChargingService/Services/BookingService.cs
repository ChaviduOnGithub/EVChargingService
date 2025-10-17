using EVChargingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QRCoder;

namespace EVChargingService.Services
{
    public class BookingService
    {
        private readonly IMongoCollection<Booking> _bookings;
        private readonly IMongoCollection<Station> _stations;

        public BookingService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _bookings = database.GetCollection<Booking>(dbSettings.Value.BookingsCollection);
            _stations = database.GetCollection<Station>(dbSettings.Value.StationsCollection);
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
            // 1️⃣ Ensure reservation is within 7 days
            if ((booking.ReservationDateTime - DateTime.UtcNow).TotalDays > 7)
                throw new Exception("Reservation can only be within 7 days from today.");

            // 2️⃣ Get station info
            var station = await _stations.Find(s => s.StationId == booking.StationId).FirstOrDefaultAsync();
            if (station == null)
                throw new Exception("Station not found.");

            booking.StationName = station.Name;

            // 3️⃣ Normalize booking hour (round down to the hour)
            var bookingHour = new DateTime(
                booking.ReservationDateTime.Year,
                booking.ReservationDateTime.Month,
                booking.ReservationDateTime.Day,
                booking.ReservationDateTime.Hour,
                0, 0, DateTimeKind.Utc);

            // 4️⃣ Count existing bookings for this station in the same hour
            var existingCount = await _bookings.CountDocumentsAsync(b =>
                b.StationId == booking.StationId &&
                b.Status != "Cancelled" &&
                b.ReservationDateTime.Year == bookingHour.Year &&
                b.ReservationDateTime.Month == bookingHour.Month &&
                b.ReservationDateTime.Day == bookingHour.Day &&
                b.ReservationDateTime.Hour == bookingHour.Hour
            );

            // 5️⃣ Compare with available slots
            if (existingCount >= station.AvailableSlots)
                throw new Exception($"No available slots at {bookingHour:yyyy-MM-dd HH:mm}. Please select another time.");

            // 6️⃣ Save booking
            await _bookings.InsertOneAsync(booking);

            // 7️⃣ Generate QR Code (booking reference)
            booking.QRCode = GenerateQRCode(booking.BookingId);

            var update = Builders<Booking>.Update.Set(b => b.QRCode, booking.QRCode);
            await _bookings.UpdateOneAsync(b => b.BookingId == booking.BookingId, update);

            return booking;
        }
        //public async Task<Booking> CreateAsync(Booking booking)
        //{
        //    if ((booking.ReservationDateTime - DateTime.UtcNow).TotalDays > 7)
        //        throw new Exception("Reservation can only be within 7 days from today.");

        //    // Lookup station
        //    var station = await _stations.Find(s => s.StationId == booking.StationId).FirstOrDefaultAsync();
        //    if (station == null)
        //        throw new Exception("Station not found.");

        //    booking.StationName = station.Name;

        //    await _bookings.InsertOneAsync(booking);

        //    // Generate QR
        //    booking.QRCode = GenerateQRCode(booking.BookingId);

        //    var update = Builders<Booking>.Update.Set(b => b.QRCode, booking.QRCode);
        //    await _bookings.UpdateOneAsync(b => b.BookingId == booking.BookingId, update);

        //    return booking;
        //}

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
        public async Task<List<Booking>> GetByOwnerAsync(string ownerNIC)
        {
            var bookings = await _bookings.Find(b => b.OwnerNIC == ownerNIC).ToListAsync();

            foreach (var b in bookings)
            {
                if (string.IsNullOrEmpty(b.StationName))
                {
                    var station = await _stations.Find(s => s.StationId == b.StationId).FirstOrDefaultAsync();
                    b.StationName = station?.Name;
                }
            }

            return bookings;
        }


    }
}
