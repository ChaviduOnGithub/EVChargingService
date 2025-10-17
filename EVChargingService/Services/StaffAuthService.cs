using EVChargingService.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EVChargingService.Services
{
    public class StaffAuthService
    {
        private readonly IMongoCollection<Staff> _staff;
        private readonly string _jwtKey;

        public StaffAuthService(IOptions<DatabaseSettings> dbSettings, IConfiguration config)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _staff = database.GetCollection<Staff>("Staff");

            _jwtKey = config["Jwt:Key"];
        }

        public async Task<Staff> GetByEmailAsync(string email)
        {
            return await _staff.Find(s => s.Email == email && s.IsActive).FirstOrDefaultAsync();
        }

        public async Task<Staff?> GetByIdAsync(string id)
        {
            return await _staff.Find(s => s.StaffId == id && s.IsActive).FirstOrDefaultAsync();
        }

        public async Task RegisterAsync(Staff staff)
        {
            var existing = await _staff.Find(s => s.Email == staff.Email).FirstOrDefaultAsync();
            if (existing != null)
                throw new Exception("Email already exists");

            await _staff.InsertOneAsync(staff);
        }

        public async Task<string?> AuthenticateAsync(string email, string password)
        {
            var staff = await _staff.Find(s => s.Email == email && s.IsActive).FirstOrDefaultAsync();
            if (staff == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(password, staff.PasswordHash)) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, staff.StaffId!),
                    new Claim(ClaimTypes.Role, staff.Role),
                    new Claim("StationId", staff.StationId)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
