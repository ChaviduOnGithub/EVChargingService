using EVChargingService.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BCrypt.Net;

namespace EVChargingService.Services
{
    public class EVOwnerAuthService
    {
        private readonly IMongoCollection<EVOwner> _evOwners;
        private readonly IConfiguration _config;

        public EVOwnerAuthService(IConfiguration config)
        {
            var client = new MongoClient(config["DatabaseSettings:ConnectionString"]);
            var database = client.GetDatabase(config["DatabaseSettings:DatabaseName"]);
            _evOwners = database.GetCollection<EVOwner>(config["DatabaseSettings:EVOwnersCollection"]);
            _config = config;
        }

        public async Task<bool> RegisterAsync(EVOwner newOwner, string password)
        {
            var existing = await _evOwners.Find(o => o.NIC == newOwner.NIC || o.Email == newOwner.Email).FirstOrDefaultAsync();
            if (existing != null) return false;

            newOwner.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            await _evOwners.InsertOneAsync(newOwner);
            return true;
        }

        public async Task<string?> LoginAsync(string nic, string password)
        {
            var owner = await _evOwners.Find(o => o.NIC == nic).FirstOrDefaultAsync();
            if (owner == null || !BCrypt.Net.BCrypt.Verify(password, owner.PasswordHash))
                return null;

            return GenerateJwtToken(owner);
        }

        private string GenerateJwtToken(EVOwner owner)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, owner.NIC),
                new Claim("name", owner.Name),
                new Claim("role", "EVOwner")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
