using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class PenalizacionService
    {
        private readonly ApplicationDbContext _context;

        public PenalizacionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Penalizacion>> GetAllAsync()
        {
            return await _context.Penalizaciones
                .Include(p => p.Usuario)
                .Include(p => p.Multa)
                .Include(p => p.Incidencia)
                .ToListAsync();
        }

        public async Task<Penalizacion?> GetByIdAsync(int id)
        {
            return await _context.Penalizaciones
                .Include(p => p.Usuario)
                .Include(p => p.Multa)
                .Include(p => p.Incidencia)
                .FirstOrDefaultAsync(p => p.IdPenalizacion == id);
        }

        public async Task<Penalizacion> CreateAsync(Penalizacion penalizacion)
        {
            _context.Penalizaciones.Add(penalizacion);
            await _context.SaveChangesAsync();
            return penalizacion;
        }

        public async Task<bool> UpdateAsync(int id, Penalizacion penalizacion)
        {
            var existing = await _context.Penalizaciones.FindAsync(id);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(penalizacion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var p = await _context.Penalizaciones.FindAsync(id);
            if (p == null) return false;

            _context.Penalizaciones.Remove(p);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
