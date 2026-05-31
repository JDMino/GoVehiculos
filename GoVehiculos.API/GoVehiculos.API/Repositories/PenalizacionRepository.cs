using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Repositories
{
    public interface IPenalizacionRepository
    {
        // Consultas
        Task<List<Penalizacion>> GetAllAsync(string? estado = null);
        Task<Penalizacion?> GetByIdAsync(int id);
        Task<Penalizacion?> GetByMultaIdAsync(int multaId);
        Task<List<Penalizacion>> GetByMultaIdsAsync(List<int> multaIds);

        // Persistencia
        Task AddAsync(Penalizacion penalizacion);
        Task SaveChangesAsync();
    }

    public class PenalizacionRepository : IPenalizacionRepository
    {
        private readonly ApplicationDbContext _context;

        public PenalizacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        /// <summary>
        /// Devuelve todas las penalizaciones con su navegación a Multa.
        /// Admite filtro opcional por estado (activa | cumplida | revocada).
        /// </summary>
        public async Task<List<Penalizacion>> GetAllAsync(string? estado = null)
        {
            var query = _context.Penalizaciones
                .Include(p => p.Multa)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(p => p.Estado == estado);

            return await query
                .OrderByDescending(p => p.IdPenalizacion)
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve una penalización por su propio ID con navegación a Multa.
        /// </summary>
        public async Task<Penalizacion?> GetByIdAsync(int id)
        {
            return await _context.Penalizaciones
                .Include(p => p.Multa)
                .FirstOrDefaultAsync(p => p.IdPenalizacion == id);
        }

        /// <summary>
        /// Busca la penalización asociada a una multa por su FK MultaId.
        /// Usado por MultaService al mapear el MultaResponseDTO en GetByIdAsync.
        /// Devuelve null si la multa no tiene penalización asociada.
        /// </summary>
        public async Task<Penalizacion?> GetByMultaIdAsync(int multaId)
        {
            return await _context.Penalizaciones
                .FirstOrDefaultAsync(p => p.MultaId == multaId);
        }

        /// <summary>
        /// Carga en una sola query todas las penalizaciones de un conjunto de multas.
        /// Usado por MultaService.GetAllAsync para evitar el problema N+1:
        /// en lugar de hacer una query por cada multa del listado, se trae
        /// todo el lote y el servicio lo indexa en un diccionario por MultaId.
        /// </summary>
        public async Task<List<Penalizacion>> GetByMultaIdsAsync(List<int> multaIds)
        {
            if (multaIds.Count == 0)
                return [];

            return await _context.Penalizaciones
                .Where(p => p.MultaId.HasValue && multaIds.Contains(p.MultaId.Value))
                .ToListAsync();
        }

        // ================================================================
        // PERSISTENCIA
        // ================================================================

        public async Task AddAsync(Penalizacion penalizacion)
        {
            await _context.Penalizaciones.AddAsync(penalizacion);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}