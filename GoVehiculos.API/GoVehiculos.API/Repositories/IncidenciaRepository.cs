using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Repositories
{
    public interface IIncidenciaRepository
    {
        // Consultas
        Task<List<Incidencia>> GetAllAsync();
        Task<Incidencia?> GetByIdAsync(int id);

        // Persistencia
        Task AddAsync(Incidencia incidencia);
        Task SaveChangesAsync();
    }

    public class IncidenciaRepository : IIncidenciaRepository
    {
        private readonly ApplicationDbContext _context;

        public IncidenciaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        /// <summary>
        /// Devuelve todas las incidencias con sus navegaciones a Usuario y Vehículo
        /// (incluyendo Modelo y Marca del vehículo) para que el servicio pueda
        /// construir los DTOs de respuesta sin llamadas adicionales.
        /// </summary>
        public async Task<List<Incidencia>> GetAllAsync()
        {
            return await _context.Incidencias
                .Include(i => i.Usuario)
                .Include(i => i.Vehiculo)
                    .ThenInclude(v => v.Modelo)
                        .ThenInclude(m => m.Marca)
                .OrderByDescending(i => i.IdIncidencia)
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve una incidencia por ID con sus navegaciones cargadas.
        /// </summary>
        public async Task<Incidencia?> GetByIdAsync(int id)
        {
            return await _context.Incidencias
                .Include(i => i.Usuario)
                .Include(i => i.Vehiculo)
                    .ThenInclude(v => v.Modelo)
                        .ThenInclude(m => m.Marca)
                .FirstOrDefaultAsync(i => i.IdIncidencia == id);
        }

        // ================================================================
        // PERSISTENCIA
        // ================================================================

        public async Task AddAsync(Incidencia incidencia)
        {
            await _context.Incidencias.AddAsync(incidencia);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}