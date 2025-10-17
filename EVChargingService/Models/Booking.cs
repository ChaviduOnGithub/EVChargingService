using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Booking
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? BookingId { get; set; }

    public string OwnerNIC { get; set; }
    public string StationId { get; set; }

    [BsonIgnoreIfNull] // optional: ignore if null
    public string? StationName { get; set; }

    public DateTime ReservationDateTime { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? QRCode { get; set; }
}
