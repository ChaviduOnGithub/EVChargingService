using EVChargingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EVChargingService.Services
{
    public class StaffService
    {
        private readonly IMongoCollection<Staff> _staff;

        public StaffService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _staff = database.GetCollection<Staff>("Staff");
        }

        public async Task<List<Staff>> GetAllAsync() =>
            await _staff.Find(_ => true).ToListAsync();

        public async Task<List<Staff>> GetByStationAsync(string stationId) =>
            await _staff.Find(s => s.StationId == stationId).ToListAsync();

        public async Task<Staff?> GetByIdAsync(string staffId) =>
            await _staff.Find(s => s.StaffId == staffId).FirstOrDefaultAsync();

        public async Task CreateAsync(Staff staff) =>
            await _staff.InsertOneAsync(staff);

        public async Task UpdateAsync(string staffId, Staff staff) =>
            await _staff.ReplaceOneAsync(s => s.StaffId == staffId, staff);

        public async Task DeleteAsync(string staffId) =>
            await _staff.DeleteOneAsync(s => s.StaffId == staffId);
       
        public async Task<Staff?> GetByEmailAsync(string email) =>
        await _staff.Find(s => s.Email == email).FirstOrDefaultAsync();

        public async Task<bool> DeactivateAsync(string staffId)
        {
            var update = Builders<Staff>.Update.Set(s => s.IsActive, false);
            var result = await _staff.UpdateOneAsync(s => s.StaffId == staffId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ActivateAsync(string staffId)
        {
            var update = Builders<Staff>.Update.Set(s => s.IsActive, true);
            var result = await _staff.UpdateOneAsync(s => s.StaffId == staffId, update);
            return result.ModifiedCount > 0;
        }
    }
}
