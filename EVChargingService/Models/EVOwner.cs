using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingService.Models
{
    public class EVOwner
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string NIC { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Phone")]
        public string Phone { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; } = "Active";
    }
}

