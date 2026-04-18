using GoVehiculos.API.Data;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class UbicacionService
    {
        private readonly ApplicationDbContext _context;

        public UbicacionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UbicacionResponseDTO>> GetAllAsync()
        {
            return await _context.Ubicaciones
                .Include(u => u.Direccion)
                    .ThenInclude(d => d.Localidad)
                        .ThenInclude(l => l.Provincia)
                .Include(u => u.Vehiculos)
                .Select(u => ToResponseDTO(u))
                .ToListAsync();
        }

        public async Task<UbicacionResponseDTO?> GetByIdAsync(int id)
        {
            var ubicacion = await _context.Ubicaciones
                .Include(u => u.Direccion)
                    .ThenInclude(d => d.Localidad)
                        .ThenInclude(l => l.Provincia)
                .Include(u => u.Vehiculos)
                .FirstOrDefaultAsync(u => u.IdUbicacion == id);

            return ubicacion == null ? null : ToResponseDTO(ubicacion);
        }

        public async Task<UbicacionResponseDTO> CreateAsync(UbicacionCreateDTO dto)
        {
            var ubicacion = new Ubicacion
            {
                Nombre      = dto.Nombre.Trim(),
                DireccionId = dto.DireccionId
            };

            _context.Ubicaciones.Add(ubicacion);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(ubicacion.IdUbicacion) ?? new UbicacionResponseDTO();
        }

        public async Task<bool> UpdateAsync(int id, UbicacionUpdateDTO dto)
        {
            var ubicacion = await _context.Ubicaciones.FindAsync(id);
            if (ubicacion == null) return false;

            ubicacion.Nombre      = dto.Nombre.Trim();
            ubicacion.DireccionId = dto.DireccionId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ubicacion = await _context.Ubicaciones
                .Include(u => u.Vehiculos)
                .FirstOrDefaultAsync(u => u.IdUbicacion == id);

            if (ubicacion == null) return false;

            // No eliminar si tiene vehículos asignados actualmente
            if (ubicacion.Vehiculos != null && ubicacion.Vehiculos.Any())
                return false;

            _context.Ubicaciones.Remove(ubicacion);
            await _context.SaveChangesAsync();
            return true;
        }

        private static UbicacionResponseDTO ToResponseDTO(Ubicacion u) => new()
        {
            IdUbicacion      = u.IdUbicacion,
            Nombre           = u.Nombre,
            DireccionId      = u.DireccionId,
            Calle            = u.Direccion?.Calle,
            Numero           = u.Direccion?.Numero,
            LocalidadNombre  = u.Direccion?.Localidad?.Nombre,
            ProvinciaNombre  = u.Direccion?.Localidad?.Provincia?.Nombre,
            CantidadVehiculos = u.Vehiculos?.Count ?? 0
        };
    }
}
