using GoVehiculos.API.Data;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class VehiculoService
    {
        private readonly ApplicationDbContext _context;

        public VehiculoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehiculoResponseDTO>> GetAllAsync(string? estado = null, string? marca = null)
        {
            var query = _context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.UbicacionActual)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(v => v.Estado == estado);

            if (!string.IsNullOrEmpty(marca))
                query = query.Where(v => v.Modelo.Marca.Nombre.Contains(marca));

            return await query
                .Select(v => ToResponseDTO(v))
                .ToListAsync();
        }

        public async Task<VehiculoResponseDTO?> GetByIdAsync(int id)
        {
            var v = await _context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.UbicacionActual)
                .FirstOrDefaultAsync(x => x.IdVehiculo == id);

            return v == null ? null : ToResponseDTO(v);
        }

        public async Task<VehiculoResponseDTO> CreateAsync(VehiculoCreateDTO dto)
        {
            var v = new Vehiculo
            {
                SocioId               = dto.SocioId,
                Tipo                  = dto.Tipo,
                ModeloId              = dto.ModeloId,
                Anio                  = dto.Anio,
                Patente               = dto.Patente,
                Estado                = dto.Estado,
                EstadoMecanico        = dto.EstadoMecanico,
                Kilometraje           = dto.Kilometraje,
                PrecioPorDia          = dto.PrecioPorDia,
                UbicacionActualId     = dto.UbicacionActualId,
                SeguroVigente         = dto.SeguroVigente,
                DocumentacionVigente  = dto.DocumentacionVigente,
                MantenimientoACargoDe = dto.MantenimientoACargoDe,
                ImagenUrl             = dto.ImagenUrl,
                Activo                = true
            };

            _context.Vehiculos.Add(v);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(v.IdVehiculo) ?? new VehiculoResponseDTO();
        }

        public async Task<bool> UpdateAsync(int id, VehiculoUpdateDTO dto)
        {
            var v = await _context.Vehiculos.FindAsync(id);
            if (v == null) return false;

            v.Estado               = dto.Estado;
            v.EstadoMecanico       = dto.EstadoMecanico;
            v.Kilometraje          = dto.Kilometraje;
            v.PrecioPorDia         = dto.PrecioPorDia;
            v.SeguroVigente        = dto.SeguroVigente;
            v.DocumentacionVigente = dto.DocumentacionVigente;
            v.Activo               = dto.Activo;
            v.ModeloId             = dto.ModeloId;
            v.UbicacionActualId    = dto.UbicacionActualId;

            await _context.SaveChangesAsync();
            return true;
        }

        
        public async Task<(bool exito, string mensaje)> PasarAFueraDeServicioAsync(int vehiculoId)
        {
            var v = await _context.Vehiculos.FindAsync(vehiculoId);
            if (v == null) return (false, "Vehículo no encontrado.");
            if (v.MantenimientoACargoDe != "socio") return (false, "Solo aplica a vehículos de socio.");

            v.Estado = "fuera_de_servicio";
            await _context.SaveChangesAsync();
            return (true, "Vehículo pasado a fuera de servicio.");
        }

        // Baja lógica — el modelo solo tiene Activo, no FechaBaja
        public async Task<bool> DeleteAsync(int id)
        {
            var v = await _context.Vehiculos.FindAsync(id);
            if (v == null) return false;

            v.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Mapeo privado centralizado ──────────────────────────────────────
        private static VehiculoResponseDTO ToResponseDTO(Vehiculo v) => new()
        {
            IdVehiculo            = v.IdVehiculo,
            SocioId               = v.SocioId,
            Tipo                  = v.Tipo,
            Anio                  = v.Anio,
            Patente               = v.Patente,
            Estado                = v.Estado,
            EstadoMecanico        = v.EstadoMecanico,
            Kilometraje           = v.Kilometraje,
            PrecioPorDia          = v.PrecioPorDia,
            MantenimientoACargoDe = v.MantenimientoACargoDe,
            SeguroVigente         = v.SeguroVigente,
            DocumentacionVigente  = v.DocumentacionVigente,
            Activo                = v.Activo,
            ImagenUrl             = v.ImagenUrl,
            ModeloId              = v.ModeloId,
            ModeloNombre          = v.Modelo?.Nombre ?? string.Empty,
            MarcaNombre           = v.Modelo?.Marca?.Nombre ?? string.Empty,
            UbicacionActualId     = v.UbicacionActualId,
            UbicacionNombre       = v.UbicacionActual?.Nombre
        };
    }
}
