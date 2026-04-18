using GoVehiculos.API.Data;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class ModeloService
    {
        private readonly ApplicationDbContext _context;

        public ModeloService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ModeloResponseDTO>> GetAllAsync(int? marcaId = null)
        {
            var query = _context.Modelos
                .Include(m => m.Marca)
                .AsQueryable();

            // Filtro opcional por marca — útil para el frontend al cargar modelos de una marca seleccionada
            if (marcaId.HasValue)
                query = query.Where(m => m.MarcaId == marcaId.Value);

            return await query
                .Select(m => ToResponseDTO(m))
                .ToListAsync();
        }

        public async Task<ModeloResponseDTO?> GetByIdAsync(int id)
        {
            var modelo = await _context.Modelos
                .Include(m => m.Marca)
                .FirstOrDefaultAsync(m => m.IdModelo == id);

            return modelo == null ? null : ToResponseDTO(modelo);
        }

        public async Task<ModeloResponseDTO> CreateAsync(ModeloCreateDTO dto)
        {
            var modelo = new Modelo
            {
                Nombre  = dto.Nombre.Trim(),
                MarcaId = dto.MarcaId
            };

            _context.Modelos.Add(modelo);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(modelo.IdModelo) ?? new ModeloResponseDTO();
        }

        public async Task<bool> UpdateAsync(int id, ModeloUpdateDTO dto)
        {
            var modelo = await _context.Modelos.FindAsync(id);
            if (modelo == null) return false;

            modelo.Nombre  = dto.Nombre.Trim();
            modelo.MarcaId = dto.MarcaId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var modelo = await _context.Modelos
                .Include(m => m.Vehiculos)
                .FirstOrDefaultAsync(m => m.IdModelo == id);

            if (modelo == null) return false;

            // No eliminar si tiene vehículos asociados
            if (modelo.Vehiculos != null && modelo.Vehiculos.Any())
                return false;

            _context.Modelos.Remove(modelo);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ModeloResponseDTO ToResponseDTO(Modelo m) => new()
        {
            IdModelo   = m.IdModelo,
            Nombre     = m.Nombre,
            MarcaId    = m.MarcaId,
            MarcaNombre = m.Marca?.Nombre ?? string.Empty
        };
    }
}
