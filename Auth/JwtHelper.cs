using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Auth {
    public class JwtHelper {
        public static string GenerateJwtToken(string name) {
            if (string.IsNullOrEmpty(name)) {
                throw new InvalidOperationException("Name is not specified.");
                ;
            }

            var claims = new[] {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken("MyCoolServer", "MyCoolClients", claims, expires: DateTime.Now.AddMinutes(30), signingCredentials: credentials);
            return JwtTokenHandler.WriteToken(token);
        }

        public static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
        public static readonly SecurityKey SecurityKey = new SymmetricSecurityKey(Convert.FromBase64String("MyCoolSecret"));


    }
}