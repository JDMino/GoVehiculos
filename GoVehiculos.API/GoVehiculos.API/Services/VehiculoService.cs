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
            var query = _context.Vehiculos.AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(v => v.Estado == estado);

            if (!string.IsNullOrEmpty(marca))
                query = query.Where(v => v.Marca.Contains(marca));

            return await query.Select(v => new VehiculoResponseDTO
            {
                IdVehiculo = v.IdVehiculo,
                Tipo = v.Tipo,
                Marca = v.Marca,
                Modelo = v.Modelo,
                Anio = v.Anio,
                Patente = v.Patente,
                Estado = v.Estado,
                EstadoMecanico = v.EstadoMecanico,
                Kilometraje = v.Kilometraje,
                PrecioPorDia = v.PrecioPorDia,
                UbicacionActual = v.UbicacionActual,
                MantenimientoACargoDe = v.MantenimientoACargoDe,
                ImagenUrl = v.ImagenUrl,
                Activo = v.Activo,
                // ✅ añadimos
                SeguroVigente = v.SeguroVigente,
                DocumentacionVigente = v.DocumentacionVigente
            }).ToListAsync();
        }

        public async Task<VehiculoResponseDTO?> GetByIdAsync(int id)
        {
            var v = await _context.Vehiculos.FindAsync(id);
            if (v == null) return null;

        return new VehiculoResponseDTO
        {
            IdVehiculo = v.IdVehiculo,
            Tipo = v.Tipo,
            Marca = v.Marca,
            Modelo = v.Modelo,
            Anio = v.Anio,
            Patente = v.Patente,
            Estado = v.Estado,
            EstadoMecanico = v.EstadoMecanico,
            Kilometraje = v.Kilometraje,
            PrecioPorDia = v.PrecioPorDia,
            UbicacionActual = v.UbicacionActual,
            MantenimientoACargoDe = v.MantenimientoACargoDe,
            ImagenUrl = v.ImagenUrl,
            Activo = v.Activo,
            // ✅ añadimos
            SeguroVigente = v.SeguroVigente,
            DocumentacionVigente = v.DocumentacionVigente
        };

        }

        public async Task<VehiculoResponseDTO> CreateAsync(VehiculoCreateDTO dto)
        {
            var v = new Vehiculo
            {
                SocioId = dto.SocioId,
                Tipo = dto.Tipo,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Anio = dto.Anio,
                Patente = dto.Patente,
                Estado = dto.Estado,
                EstadoMecanico = dto.EstadoMecanico,
                Kilometraje = dto.Kilometraje,
                LicenciaRequerida = dto.LicenciaRequerida,
                PrecioPorDia = dto.PrecioPorDia,
                UbicacionActual = dto.UbicacionActual,
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
            v.UbicacionActual = dto.UbicacionActual;

            // ✅ ahora se guardan correctamente
            v.SeguroVigente = dto.SeguroVigente;
            v.DocumentacionVigente = dto.DocumentacionVigente;

            v.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var v = await _context.Vehiculos.FindAsync(id);
            if (v == null) return false;

            v.Activo = false;
            v.FechaBaja = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
