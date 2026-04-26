using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repo;

        public UsuarioService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<UsuarioResponseDTO>> GetAllAsync()
        {
            var lista = await _repo.GetAllAsync();
            return lista.Select(ToResponseDTO);
        }

        public async Task<UsuarioResponseDTO?> GetByIdAsync(int id)
        {
            var u = await _repo.GetByIdAsync(id);
            return u == null ? null : ToResponseDTO(u);
        }

        public async Task<UsuarioResponseDTO> CreateAsync(UsuarioCreateDTO dto)
        {
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Dni = dto.Dni,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RolId = dto.RolId,
                DireccionId = dto.DireccionId,
                FechaRegistro = DateTime.Now
            };

            await _repo.AddAsync(usuario);
            await _repo.SaveChangesAsync();

            return await GetByIdAsync(usuario.IdUsuario) ?? new UsuarioResponseDTO();
        }

        public async Task<bool> UpdateAsync(int id, UsuarioUpdateDTO dto)
        {
            var usuario = await _repo.GetByIdSimpleAsync(id);
            if (usuario == null) return false;

            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Telefono = dto.Telefono;
            usuario.Bloqueado = dto.Bloqueado;
            usuario.Activo = dto.Activo;
            usuario.RolId = dto.RolId;
            usuario.DireccionId = dto.DireccionId;

            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _repo.GetByIdSimpleAsync(id);
            if (usuario == null) return false;

            usuario.Activo = false;
            usuario.FechaBaja = DateTime.Now;

            await _repo.SaveChangesAsync();
            return true;
        }

        // ================================================================
        // Mapeo privado
        // ================================================================

        private static UsuarioResponseDTO ToResponseDTO(Usuario u) => new()
        {
            IdUsuario = u.IdUsuario,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Email = u.Email,
            Dni = u.Dni,
            Rol = u.Rol?.Nombre ?? string.Empty,
            RolId = u.RolId,
            Activo = u.Activo,
            Bloqueado = u.Bloqueado,
            FechaRegistro = u.FechaRegistro,
            DireccionId = u.DireccionId,
            Telefono = u.Telefono,
        };
    }
}