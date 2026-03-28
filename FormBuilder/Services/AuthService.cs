using FormBuilder.Dtos;
using FormBuilder.Entities;
using FormBuilder.Interfaces;
using FormBuilder.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FormBuilder.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtOptions _jwtOptions;

        public AuthService(IUnitOfWork unitOfWork, IOptions<JwtOptions> jwtOptions)
        {
            _unitOfWork = unitOfWork;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = (await _unitOfWork.Users.FindAsync(u => u.Email == request.Email)).FirstOrDefault();
            if (existingUser != null)
                throw new Exception("User already exists.");

            CreatePasswordHash(request.Password, out string passwordHash, out string passwordSalt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);

            var tokenResponse = await GenerateTokenResponseAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return tokenResponse;
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            var user = (await _unitOfWork.Users.FindAsync(u => u.Email == request.Email)).FirstOrDefault();
            if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new Exception("Invalid credentials.");

            var tokenResponse = await GenerateTokenResponseAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return tokenResponse;
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshRequest request)
        {
            var claimsPrincipal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (claimsPrincipal == null)
                throw new Exception("Invalid access token.");

            var userIdString = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                throw new Exception("Invalid token claims.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var oldRefreshToken = (await _unitOfWork.RefreshTokens.FindAsync(rt => rt.TokenHash == HashToken(request.RefreshToken) && rt.UserId == userId)).FirstOrDefault();
            
            if (oldRefreshToken == null || oldRefreshToken.RevokedAt != null || oldRefreshToken.UsedAt != null || oldRefreshToken.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Invalid refresh token.");

            // Mark old token as used
            oldRefreshToken.UsedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(oldRefreshToken);

            var newResponse = await GenerateTokenResponseAsync(user, oldRefreshToken.FamilyId);
            await _unitOfWork.SaveChangesAsync();

            return newResponse;
        }

        public async Task LogoutAsync(Guid userId)
        {
            var tokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.UserId == userId && rt.RevokedAt == null && rt.UsedAt == null);
            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                _unitOfWork.RefreshTokens.Update(token);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<TokenResponse> GenerateTokenResponseAsync(User user, Guid? familyId = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // Generate refresh token
            var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = HashToken(refreshTokenString),
                FamilyId = familyId ?? Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays)
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString
            };
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.Secret)),
                ValidateLifetime = false // we want to get claims from an expired token
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            using var hmac = new HMACSHA512(Convert.FromBase64String(storedSalt));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(computedHash) == storedHash;
        }
    }
}
