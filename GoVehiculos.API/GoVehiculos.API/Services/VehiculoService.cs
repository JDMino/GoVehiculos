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

            return await query.Select(v => new VehiculoResponseDTO
            {
                IdVehiculo = v.IdVehiculo,
                Tipo = v.Tipo,
                Anio = v.Anio,
                Patente = v.Patente,
                Estado = v.Estado,
                EstadoMecanico = v.EstadoMecanico,
                Kilometraje = v.Kilometraje,
                PrecioPorDia = v.PrecioPorDia,
                MantenimientoACargoDe = v.MantenimientoACargoDe,
                ImagenUrl = v.ImagenUrl,
                Activo = v.Activo,
                SeguroVigente = v.SeguroVigente,
                DocumentacionVigente = v.DocumentacionVigente,

                ModeloId = v.ModeloId,
                ModeloNombre = v.Modelo.Nombre,
                MarcaNombre = v.Modelo.Marca.Nombre,

                UbicacionActualId = v.UbicacionActualId,
                UbicacionNombre = v.UbicacionActual != null ? v.UbicacionActual.Nombre : null
            }).ToListAsync();
        }

        public async Task<VehiculoResponseDTO?> GetByIdAsync(int id)
        {
            var v = await _context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.UbicacionActual)
                .FirstOrDefaultAsync(x => x.IdVehiculo == id);

            if (v == null) return null;

            return new VehiculoResponseDTO
            {
                IdVehiculo = v.IdVehiculo,
                Tipo = v.Tipo,
                Anio = v.Anio,
                Patente = v.Patente,
                Estado = v.Estado,
                EstadoMecanico = v.EstadoMecanico,
                Kilometraje = v.Kilometraje,
                PrecioPorDia = v.PrecioPorDia,
                MantenimientoACargoDe = v.MantenimientoACargoDe,
                ImagenUrl = v.ImagenUrl,
                Activo = v.Activo,
                SeguroVigente = v.SeguroVigente,
                DocumentacionVigente = v.DocumentacionVigente,

                ModeloId = v.ModeloId,
                ModeloNombre = v.Modelo.Nombre,
                MarcaNombre = v.Modelo.Marca.Nombre,

                UbicacionActualId = v.UbicacionActualId,
                UbicacionNombre = v.UbicacionActual?.Nombre
            };
        }

        public async Task<VehiculoResponseDTO> CreateAsync(VehiculoCreateDTO dto)
        {
            var v = new Vehiculo
            {
                SocioId = dto.SocioId,
                Tipo = dto.Tipo,
                ModeloId = dto.ModeloId,
                Anio = dto.Anio,
                Patente = dto.Patente,
                Estado = dto.Estado,
                EstadoMecanico = dto.EstadoMecanico,
                Kilometraje = dto.Kilometraje,
                //LicenciaRequerida = dto.LicenciaRequerida,
                PrecioPorDia = dto.PrecioPorDia,
                UbicacionActualId = dto.UbicacionActualId,
                SeguroVigente = dto.SeguroVigente,
                DocumentacionVigente = dto.DocumentacionVigente,
                MantenimientoACargoDe = dto.MantenimientoACargoDe,
                ImagenUrl = dto.ImagenUrl,
                Activo = true
            };

            _context.Vehiculos.Add(v);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(v.IdVehiculo) ?? new VehiculoResponseDTO();
        }

        public async Task<bool> UpdateAsync(int id, VehiculoUpdateDTO dto)
        {
            var v = await _context.Vehiculos.FindAsync(id);
            if (v == null) return false;

            v.Estado = dto.Estado;
            v.EstadoMecanico = dto.EstadoMecanico;
            v.Kilometraje = dto.Kilometraje;
            v.PrecioPorDia = dto.PrecioPorDia;
            v.SeguroVigente = dto.SeguroVigente;
            v.DocumentacionVigente = dto.DocumentacionVigente;
            v.Activo = dto.Activo;

            v.ModeloId = dto.ModeloId;
            v.UbicacionActualId = dto.UbicacionActualId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var v = await _context.Vehiculos.FindAsync(id);
            if (v == null) return false;

            v.Activo = false;
            //v.FechaBaja = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
