using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class IncidenciaService
    {
        private readonly ApplicationDbContext _context;

        public IncidenciaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Incidencia>> GetAllAsync()
        {
            return await _context.Incidencias
                .Include(i => i.Usuario)
                .Include(i => i.Vehiculo)
                .ToListAsync();
        }

        public async Task<Incidencia?> GetByIdAsync(int id)
        {
            return await _context.Incidencias
                .Include(i => i.Usuario)
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(i => i.IdIncidencia == id);
        }

        public async Task<Incidencia> CreateAsync(Incidencia incidencia)
        {
            _context.Incidencias.Add(incidencia);
            await _context.SaveChangesAsync();
            return incidencia;
        }

        public async Task<bool> UpdateAsync(int id, Incidencia incidencia)
        {
            var existing = await _context.Incidencias.FindAsync(id);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(incidencia);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var i = await _context.Incidencias.FindAsync(id);
            if (i == null) return false;

            _context.Incidencias.Remove(i);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
