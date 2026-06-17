using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using visitor_admin.Entities;

namespace visitor_admin.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration; // used to access info from the appsettings.json file

        public JwtService(IConfiguration configuration) // class constructor
        {
            _configuration = configuration; // injecting the property into the class 
        }

        public string GenerateToken(User user)
        {
            // Accessing info from the appsettings.json
            var secretKey = _configuration["Authentication:SecretForKey"] ?? throw new InvalidOperationException("JWT secret key is not configured");
            var issuer = _configuration["Authentication:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured");
            var audience = _configuration["Authentication:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured");
            
            /* Converts your JWT secret string into a cryptographic key
             object that can be used to sign or validate tokens.*/
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            //Packages the key with the HMAC-SHA256 algorithm, telling the JWT library how to sign the token.
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            /* Builds an array of claims — the user's identity data (ID, username, email, role, name)
            that will be embedded inside the JWT token. */
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("given_name", user.FirstName ?? ""),
                new Claim("family_name", user.LastName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
