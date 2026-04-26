using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Repositories
{
    public interface IMantenimientoRepository
    {
        // Contadores (protagonista: Mantenimientos)
        Task<int> ContarPendientesPorEmpleadoAsync(int empleadoId);
        Task<int> ContarTerminadosAsync();

        // Consultas
        Task<List<Mantenimiento>> GetAllAsync(string? estado = null);
        Task<Mantenimiento?> GetByIdAsync(int id);
        Task<Mantenimiento?> GetByIdConVehiculoAsync(int id);
        Task<List<Mantenimiento>> GetByEmpleadoAsync(int empleadoId);
        Task<List<Mantenimiento>> GetActivosPorVehiculosAsync(List<int> vehiculoIds);
        Task<bool> TieneActivoAsync(int vehiculoId);

        // Persistencia
        Task AddAsync(Mantenimiento mantenimiento);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }

    public class MantenimientoRepository : IMantenimientoRepository
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] EstadosActivos = ["pendiente", "en_proceso", "iniciado"];
        private static readonly string[] EstadosTerminales = ["finalizado", "cancelado"];

        public MantenimientoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // CONTADORES
        // ================================================================

        public async Task<int> ContarPendientesPorEmpleadoAsync(int empleadoId)
        {
            return await _context.Mantenimientos
                .CountAsync(m => m.EmpleadoId == empleadoId &&
                                 (m.Estado == "pendiente" || m.Estado == "iniciado"));
        }

        public async Task<int> ContarTerminadosAsync()
        {
            return await _context.Mantenimientos
                .CountAsync(m => EstadosTerminales.Contains(m.Estado));
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        public async Task<List<Mantenimiento>> GetAllAsync(string? estado = null)
        {
            var query = _context.Mantenimientos
                .Include(m => m.Vehiculo).ThenInclude(v => v.Modelo).ThenInclude(mo => mo.Marca)
                .Include(m => m.Empleado)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(m => m.Estado == estado);

            return await query
                .OrderByDescending(m => m.IdMantenimiento)
                .ToListAsync();
        }

        public async Task<Mantenimiento?> GetByIdAsync(int id)
        {
            return await _context.Mantenimientos
                .Include(m => m.Vehiculo).ThenInclude(v => v.Modelo).ThenInclude(mo => mo.Marca)
                .Include(m => m.Empleado)
                .FirstOrDefaultAsync(m => m.IdMantenimiento == id);
        }

        /// <summary>
        /// Solo incluye Vehiculo — para Finalizar y Disponibilizar donde no se necesita Marca/Modelo.
        /// </summary>
        public async Task<Mantenimiento?> GetByIdConVehiculoAsync(int id)
        {
            return await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.IdMantenimiento == id);
        }

        public async Task<List<Mantenimiento>> GetByEmpleadoAsync(int empleadoId)
        {
            return await _context.Mantenimientos
                .Include(m => m.Vehiculo).ThenInclude(v => v.Modelo).ThenInclude(mo => mo.Marca)
                .Include(m => m.Empleado)
                .Where(m => m.EmpleadoId == empleadoId)
                .OrderByDescending(m => m.IdMantenimiento)
                .ToListAsync();
        }

        /// <summary>
        /// Usado por VehiculoService para armar la vista de candidatos.
        /// </summary>
        public async Task<List<Mantenimiento>> GetActivosPorVehiculosAsync(List<int> vehiculoIds)
        {
            return await _context.Mantenimientos
                .Include(m => m.Empleado)
                .Where(m => vehiculoIds.Contains(m.VehiculoId) &&
                            EstadosActivos.Contains(m.Estado))
                .OrderByDescending(m => m.IdMantenimiento)
                .ToListAsync();
        }

        public async Task<bool> TieneActivoAsync(int vehiculoId)
        {
            return await _context.Mantenimientos
                .AnyAsync(m => m.VehiculoId == vehiculoId &&
                               EstadosActivos.Contains(m.Estado));
        }

        // ================================================================
        // PERSISTENCIA
        // ================================================================

        public async Task AddAsync(Mantenimiento mantenimiento)
        {
            await _context.Mantenimientos.AddAsync(mantenimiento);
        }

        public async Task DeleteAsync(int id)
        {
            var m = await _context.Mantenimientos.FindAsync(id);
            if (m != null)
                _context.Mantenimientos.Remove(m);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}