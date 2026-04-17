using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class MantenimientoService
    {
        private readonly ApplicationDbContext _context;

        public MantenimientoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Mantenimiento>> GetAllAsync()
        {
            return await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .Include(m => m.Empleado)
                .ToListAsync();
        }

        public async Task<Mantenimiento?> GetByIdAsync(int id)
        {
            return await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .Include(m => m.Empleado)
                .FirstOrDefaultAsync(m => m.IdMantenimiento == id);
        }

        public async Task<Mantenimiento> CreateAsync(Mantenimiento mantenimiento)
        {
            _context.Mantenimientos.Add(mantenimiento);
            await _context.SaveChangesAsync();
            return mantenimiento;
        }

        public async Task<bool> UpdateAsync(int id, Mantenimiento mantenimiento)
        {
            var existing = await _context.Mantenimientos.FindAsync(id);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(mantenimiento);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var m = await _context.Mantenimientos.FindAsync(id);
            if (m == null) return false;

            _context.Mantenimientos.Remove(m);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
