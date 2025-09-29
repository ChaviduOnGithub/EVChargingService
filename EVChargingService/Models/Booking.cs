using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingService.Models
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? BookingId { get; set; }

        public string OwnerNIC { get; set; }
        public string StationId { get; set; }

        public DateTime ReservationDateTime { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Approved / Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? QRCode { get; set; } // optional: QR code string
    }
}
