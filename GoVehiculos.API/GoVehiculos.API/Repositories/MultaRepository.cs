using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Repositories
{
    public interface IMultaRepository
    {
        // Consultas
        Task<List<Multa>> GetAllAsync(string? estado = null, string? tipoIncidencia = null, string? nivelGravedad = null);
        Task<Multa?> GetByIdAsync(int id);
        Task<Multa?> GetByIdSimpleAsync(int id);

        // Persistencia
        Task AddAsync(Multa multa);
        Task SaveChangesAsync();
    }

    public class MultaRepository : IMultaRepository
    {
        private readonly ApplicationDbContext _context;

        public MultaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        /// <summary>
        /// Devuelve todas las multas con sus navegaciones completas (Incidencia → Usuario
        /// y Vehículo) para construir los DTOs de respuesta del listado sin llamadas
        /// adicionales.
        ///
        /// La Penalización NO se incluye aquí porque la FK está en la dirección inversa
        /// (Penalizacion.MultaId → Multa). MultaService la resuelve en lote con
        /// IPenalizacionRepository.GetByMultaIdsAsync para evitar el problema N+1.
        ///
        /// Admite filtros opcionales combinables:
        ///   estado         — filtra por Multa.Estado (pendiente | pagada | cancelada)
        ///   tipoIncidencia — filtra por Incidencia.Tipo
        ///   nivelGravedad  — filtra por Incidencia.NivelGravedad
        /// </summary>
        public async Task<List<Multa>> GetAllAsync(
            string? estado = null,
            string? tipoIncidencia = null,
            string? nivelGravedad = null)
        {
            var query = _context.Multas
                .Include(m => m.Incidencia)
                    .ThenInclude(i => i.Usuario)
                .Include(m => m.Incidencia)
                    .ThenInclude(i => i.Vehiculo)
                        .ThenInclude(v => v.Modelo)
                            .ThenInclude(mo => mo.Marca)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(m => m.Estado == estado);

            if (!string.IsNullOrWhiteSpace(tipoIncidencia))
                query = query.Where(m => m.Incidencia!.Tipo == tipoIncidencia);

            if (!string.IsNullOrWhiteSpace(nivelGravedad))
                query = query.Where(m => m.Incidencia!.NivelGravedad == nivelGravedad);

            return await query
                .OrderByDescending(m => m.IdMulta)
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve una multa por ID con todas sus navegaciones de Incidencia cargadas.
        /// Usado para construir el MultaResponseDTO completo en GetById y Update.
        ///
        /// La Penalización NO se incluye aquí porque la FK está en la dirección inversa.
        /// MultaService la resuelve con IPenalizacionRepository.GetByMultaIdAsync.
        /// </summary>
        public async Task<Multa?> GetByIdAsync(int id)
        {
            return await _context.Multas
                .Include(m => m.Incidencia)
                    .ThenInclude(i => i.Usuario)
                .Include(m => m.Incidencia)
                    .ThenInclude(i => i.Vehiculo)
                        .ThenInclude(v => v.Modelo)
                            .ThenInclude(mo => mo.Marca)
                .FirstOrDefaultAsync(m => m.IdMulta == id);
        }

        /// <summary>
        /// Sin includes — para operaciones de escritura donde solo se necesita
        /// la entidad Multa (cancelar, verificar estado).
        /// Evita el costo de cargar el grafo completo cuando no se necesita.
        /// </summary>
        public async Task<Multa?> GetByIdSimpleAsync(int id)
        {
            return await _context.Multas.FindAsync(id);
        }

        // ================================================================
        // PERSISTENCIA
        // ================================================================

        public async Task AddAsync(Multa multa)
        {
            await _context.Multas.AddAsync(multa);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}