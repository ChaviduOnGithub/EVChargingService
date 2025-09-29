using EVChargingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EVChargingService.Services
{
    public class EVOwnerService
    {
        private readonly IMongoCollection<EVOwner> _owners;
        private readonly IMongoCollection<Booking> _bookings;

        public EVOwnerService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _owners = database.GetCollection<EVOwner>("EVOwners");
            _bookings = database.GetCollection<Booking>("Bookings");
        }

        public async Task<List<EVOwner>> GetAllAsync() =>
            await _owners.Find(_ => true).ToListAsync();

        public async Task<EVOwner?> GetByNICAsync(string nic) =>
            await _owners.Find(o => o.NIC == nic).FirstOrDefaultAsync();

        public async Task CreateAsync(EVOwner owner) =>
            await _owners.InsertOneAsync(owner);

        public async Task UpdateAsync(string nic, EVOwner owner) =>
            await _owners.ReplaceOneAsync(o => o.NIC == nic, owner);

        public async Task DeleteAsync(string nic) =>
            await _owners.DeleteOneAsync(o => o.NIC == nic);

        // Activate account
        public async Task<bool> ActivateAsync(string nic)
        {
            var update = Builders<EVOwner>.Update.Set(o => o.Status, "Active");
            var result = await _owners.UpdateOneAsync(o => o.NIC == nic, update);
            return result.ModifiedCount > 0;
        }

        // Deactivate account
        public async Task<bool> DeactivateAsync(string nic)
        {
            var update = Builders<EVOwner>.Update.Set(o => o.Status, "Inactive");
            var result = await _owners.UpdateOneAsync(o => o.NIC == nic, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<Booking>> GetUpcomingBookingsAsync(string nic)
        {
            return await _bookings.Find(b => b.OwnerNIC == nic && b.ReservationDateTime >= DateTime.UtcNow)
                                  .SortBy(b => b.ReservationDateTime)
                                  .ToListAsync();
        }

        public async Task<List<Booking>> GetPastBookingsAsync(string nic)
        {
            return await _bookings.Find(b => b.OwnerNIC == nic && b.ReservationDateTime < DateTime.UtcNow)
                                  .SortByDescending(b => b.ReservationDateTime)
                                  .ToListAsync();
        }

    }
}
