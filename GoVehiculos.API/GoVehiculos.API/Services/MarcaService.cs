using GoVehiculos.API.Data;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class MarcaService
    {
        private readonly ApplicationDbContext _context;

        public MarcaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MarcaResponseDTO>> GetAllAsync()
        {
            return await _context.Marcas
                .Include(m => m.Modelos)
                .Select(m => ToResponseDTO(m))
                .ToListAsync();
        }

        public async Task<MarcaResponseDTO?> GetByIdAsync(int id)
        {
            var marca = await _context.Marcas
                .Include(m => m.Modelos)
                .FirstOrDefaultAsync(m => m.IdMarca == id);

            return marca == null ? null : ToResponseDTO(marca);
        }

        public async Task<MarcaResponseDTO> CreateAsync(MarcaCreateDTO dto)
        {
            var marca = new Marca
            {
                Nombre = dto.Nombre.Trim()
            };

            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(marca.IdMarca) ?? new MarcaResponseDTO();
        }

        public async Task<bool> UpdateAsync(int id, MarcaUpdateDTO dto)
        {
            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null) return false;

            marca.Nombre = dto.Nombre.Trim();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var marca = await _context.Marcas
                .Include(m => m.Modelos)
                .FirstOrDefaultAsync(m => m.IdMarca == id);

            if (marca == null) return false;

            // No eliminar si tiene modelos asociados
            if (marca.Modelos != null && marca.Modelos.Any())
                return false;

            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();
            return true;
        }

        private static MarcaResponseDTO ToResponseDTO(Marca m) => new()
        {
            IdMarca          = m.IdMarca,
            Nombre           = m.Nombre,
            CantidadModelos  = m.Modelos?.Count ?? 0
        };
    }
}
