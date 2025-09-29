namespace EVChargingService.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string EVOwnersCollection { get; set; }
        public string StationsCollection { get; set; }
        public string BookingsCollection { get; set; }
        public string UsersCollection { get; set; }
    }

}
