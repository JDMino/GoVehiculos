using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace GoVehiculos.API.Repositories
{
    public interface IMultaRepository
    {
        // Consultas
        Task<List<Multa>> GetAllAsync(string? estado = null, string? tipoIncidencia = null, string? nivelGravedad = null);
        Task<Multa?> GetByIdAsync(int id);
        Task<Multa?> GetByIdSimpleAsync(int id);
        Task<List<Multa>> GetByUsuarioIdAsync(int usuarioId, string? estado = null);


        // Persistencia general
        Task AddAsync(Multa multa);
        Task SaveChangesAsync();


        // ── NUEVO — Procedimiento almacenado ──────────────────────────
        // ANTES: MultaService.CancelarAsync() usaba GetByIdSimpleAsync
        // para cargar la multa, modificaba sus propiedades en memoria
        // (estado, descripcion), luego usaba PenalizacionRepository
        // .GetByMultaIdAsync() para cargar la penalización y modificaba
        // su estado en memoria, y finalmente llamaba SaveChangesAsync()
        // que emitía los dos UPDATE como statements separados en la
        // transacción implícita de EF. En total: 2 queries + 1 SaveChanges.
        //
        // DESPUÉS: este método ejecuta SP_CancelarMulta en una sola
        // llamada. El SP verifica existencia, estado previo, construye
        // la descripción con el motivo, actualiza Multa y Penalizacion
        // en una única transacción atómica en SQL Server.
        // En total: 1 llamada a la BD.
        // ─────────────────────────────────────────────────────────────
        Task<(bool exito, string mensaje)> CancelarConSPAsync(
            int idMulta,
            string motivoCancelacion);
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


        public async Task<Multa?> GetByIdSimpleAsync(int id)
        {
            return await _context.Multas.FindAsync(id);
        }


        public async Task<List<Multa>> GetByUsuarioIdAsync(int usuarioId, string? estado = null)
        {
            var query = _context.Multas
                .Include(m => m.Incidencia)
                    .ThenInclude(i => i.Usuario)
                .Include(m => m.Incidencia)
                    .ThenInclude(i => i.Vehiculo)
                        .ThenInclude(v => v.Modelo)
                            .ThenInclude(mo => mo.Marca)
                .Where(m => m.Incidencia != null && m.Incidencia.UsuarioId == usuarioId)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(m => m.Estado == estado);


            return await query
                .OrderByDescending(m => m.IdMulta)
                .ToListAsync();
        }


        // ================================================================
        // PERSISTENCIA GENERAL
        // ================================================================


        public async Task AddAsync(Multa multa)
        {
            await _context.Multas.AddAsync(multa);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


        // ================================================================
        // PROCEDIMIENTO ALMACENADO — Cancelar multa
        // ================================================================


        public async Task<(bool exito, string mensaje)> CancelarConSPAsync(
            int idMulta,
            string motivoCancelacion)
        {
            var pIdMulta = new SqlParameter("@IdMulta", idMulta);
            var pMotivo = new SqlParameter("@MotivoCancelacion",
                string.IsNullOrWhiteSpace(motivoCancelacion)
                    ? (object)DBNull.Value
                    : motivoCancelacion.Trim());


            var pExito = new SqlParameter("@Exito", System.Data.SqlDbType.Bit)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            var pMensaje = new SqlParameter("@Mensaje", System.Data.SqlDbType.NVarChar, 300)
            {
                Direction = System.Data.ParameterDirection.Output
            };


            await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_CancelarMulta @IdMulta, @MotivoCancelacion, @Exito OUTPUT, @Mensaje OUTPUT",
                pIdMulta, pMotivo, pExito, pMensaje);


            var exito = (bool)pExito.Value;
            var mensaje = pMensaje.Value?.ToString() ?? string.Empty;


            return (exito, mensaje);
        }
    }
}
