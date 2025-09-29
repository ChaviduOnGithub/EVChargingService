using EVChargingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EVChargingService.Services
{
    public class StationService
    {
        private readonly IMongoCollection<Station> _stations;

        public StationService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _stations = database.GetCollection<Station>(dbSettings.Value.StationsCollection);
        }

        public async Task<List<Station>> GetAllAsync() =>
            await _stations.Find(_ => true).ToListAsync();

        public async Task<Station> GetByIdAsync(string id) =>
            await _stations.Find(s => s.StationId == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Station station) =>
            await _stations.InsertOneAsync(station);

        public async Task UpdateAsync(string id, Station station) =>
            await _stations.ReplaceOneAsync(s => s.StationId == id, station);

        public async Task DeactivateAsync(string id)
        {
            var update = Builders<Station>.Update.Set(s => s.IsActive, false);
            await _stations.UpdateOneAsync(s => s.StationId == id, update);
        }
    }
}
