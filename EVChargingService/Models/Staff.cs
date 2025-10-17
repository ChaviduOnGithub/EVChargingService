using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingService.Models
{
    public class Staff
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? StaffId { get; set; }

        [BsonElement("StationId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? StationId { get; set; } // Nullable now

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; } // Backoffice, Operator, or Super Admin

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Phone")]
        public string Phone { get; set; }

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;
    }
}
