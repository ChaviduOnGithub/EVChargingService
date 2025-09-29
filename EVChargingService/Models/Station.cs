using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingService.Models
{
    public class Station
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? StationId { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Latitude")]
        public double Latitude { get; set; }

        [BsonElement("Longitude")]
        public double Longitude { get; set; }

        [BsonElement("Type")]
        public string Type { get; set; } // AC / DC

        [BsonElement("AvailableSlots")]
        public int AvailableSlots { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;
    }
}
