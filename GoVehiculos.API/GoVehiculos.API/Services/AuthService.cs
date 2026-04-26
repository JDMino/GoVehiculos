using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GoVehiculos.API.Services
{
    public class AuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            if (!user.Activo)
            {
                return new LoginResponse
                {
                    ErrorMessage = "Tu cuenta está inactiva. Contacta al administrador.",
                    Token = string.Empty,
                    IdUsuario = user.IdUsuario,
                    RolId = user.RolId,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Email = user.Email
                };
            }

            var token = GenerarToken(user);

            return new LoginResponse
            {
                Token = token,
                IdUsuario = user.IdUsuario,
                RolId = user.RolId,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email
            };
        }

        public async Task<Usuario> RegisterAsync(RegisterRequest request)
        {
            var user = new Usuario
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Dni = request.Dni,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RolId = request.RolId,
                DireccionId = request.DireccionId
            };

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();
            return user;
        }

        // ================================================================
        // Privado — generación del JWT
        // ================================================================

        private string GenerarToken(Usuario user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("rol_id", user.RolId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}