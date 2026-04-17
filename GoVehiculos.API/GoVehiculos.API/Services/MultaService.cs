using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class MultaService
    {
        private readonly ApplicationDbContext _context;

        public MultaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Multa>> GetAllAsync()
        {
            return await _context.Multas
                .Include(m => m.Incidencia)
                .Include(m => m.Usuario)
                .Include(m => m.Vehiculo)
                .ToListAsync();
        }

        public async Task<Multa?> GetByIdAsync(int id)
        {
            return await _context.Multas
                .Include(m => m.Incidencia)
                .Include(m => m.Usuario)
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.IdMulta == id);
        }

        public async Task<Multa> CreateAsync(Multa multa)
        {
            _context.Multas.Add(multa);
            await _context.SaveChangesAsync();
            return multa;
        }

        public async Task<bool> UpdateAsync(int id, Multa multa)
        {
            var existing = await _context.Multas.FindAsync(id);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(multa);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var m = await _context.Multas.FindAsync(id);
            if (m == null) return false;

            _context.Multas.Remove(m);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
