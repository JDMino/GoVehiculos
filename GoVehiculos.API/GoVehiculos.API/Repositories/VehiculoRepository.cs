using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Repositories
{
    public interface IVehiculoRepository
    {
        // Lecturas generales
        Task<List<Vehiculo>> GetAllAsync(string? estado = null, string? marca = null);
        Task<Vehiculo?> GetByIdAsync(int id);
        Task<Vehiculo?> GetByIdSimpleAsync(int id);

        // Candidatos a mantenimiento (protagonista: Vehiculos)
        Task<int> ContarCandidatosAsync();
        Task<List<Vehiculo>> GetCandidatosAsync();

        // Persistencia
        Task AddAsync(Vehiculo vehiculo);
        Task SaveChangesAsync();
    }

    public class VehiculoRepository : IVehiculoRepository
    {
        private readonly ApplicationDbContext _context;

        public VehiculoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vehiculo>> GetAllAsync(string? estado = null, string? marca = null)
        {
            var query = _context.Vehiculos
                .Include(v => v.Modelo).ThenInclude(m => m.Marca)
                .Include(v => v.UbicacionActual)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(v => v.Estado == estado);

            if (!string.IsNullOrEmpty(marca))
                query = query.Where(v => v.Modelo.Marca.Nombre.Contains(marca));

            return await query.ToListAsync();
        }

        public async Task<Vehiculo?> GetByIdAsync(int id)
        {
            return await _context.Vehiculos
                .Include(v => v.Modelo).ThenInclude(m => m.Marca)
                .Include(v => v.UbicacionActual)
                .FirstOrDefaultAsync(x => x.IdVehiculo == id);
        }

        /// <summary>
        /// Sin includes — para operaciones de escritura donde solo se necesita la entidad.
        /// </summary>
        public async Task<Vehiculo?> GetByIdSimpleAsync(int id)
        {
            return await _context.Vehiculos.FindAsync(id);
        }

        public async Task<int> ContarCandidatosAsync()
        {
            return await _context.Vehiculos
                .CountAsync(v => v.Activo &&
                                 (v.EstadoMecanico == "regular" || v.EstadoMecanico == "malo"));
        }

        public async Task<List<Vehiculo>> GetCandidatosAsync()
        {
            return await _context.Vehiculos
                .Include(v => v.Modelo).ThenInclude(m => m.Marca)
                .Include(v => v.UbicacionActual)
                .Where(v => v.Activo &&
                            (v.EstadoMecanico == "regular" || v.EstadoMecanico == "malo"))
                .ToListAsync();
        }

        public async Task AddAsync(Vehiculo vehiculo)
        {
            await _context.Vehiculos.AddAsync(vehiculo);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}