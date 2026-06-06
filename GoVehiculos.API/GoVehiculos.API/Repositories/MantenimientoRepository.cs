using GoVehiculos.API.Data;
using GoVehiculos.API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Repositories
{
    public interface IMantenimientoRepository
    {
        Task<int> ContarPendientesPorEmpleadoAsync(int empleadoId);
        Task<int> ContarTerminadosAsync();
        Task<List<Mantenimiento>> GetAllAsync(string? estado = null);
        Task<Mantenimiento?> GetByIdAsync(int id);
        Task<Mantenimiento?> GetByIdSimpleAsync(int id);
        Task<Mantenimiento?> GetByIdConVehiculoAsync(int id);
        Task<List<Mantenimiento>> GetByEmpleadoAsync(int empleadoId);
        Task<List<Mantenimiento>> GetActivosPorVehiculosAsync(List<int> vehiculoIds);
        Task<bool> TieneActivoAsync(int vehiculoId);
        Task AddAsync(Mantenimiento mantenimiento);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();

        // ── NUEVO — Procedimiento almacenado ──────────────────────────
        // ANTES: la creación se hacía en MantenimientoService.CreateAsync()
        // con múltiples queries LINQ separadas (verificar vehículo,
        // verificar orden activa, etc.) más un AddAsync + SaveChangesAsync
        // que emitía el INSERT en Mantenimiento y el UPDATE en Vehiculo
        // como dos statements en la transacción implícita de EF.
        // En total: 3 queries a la BD + 1 SaveChangesAsync.
        //
        // DESPUÉS: este método ejecuta SP_CrearOrdenMantenimiento en una
        // sola llamada. El SP valida, inserta y actualiza en una única
        // transacción atómica del lado del servidor, devolviendo el ID
        // generado por SCOPE_IDENTITY() via parámetro OUTPUT.
        // En total: 1 llamada a la BD.
        // ─────────────────────────────────────────────────────────────
        Task<(bool exito, string mensaje, int idMantenimiento)> CrearConSPAsync(
            int      vehiculoId,
            int      empleadoId,
            string   tipo,
            string   descripcion,
            string   prioridad,
            DateOnly fechaProgramada);
    }

    public class MantenimientoRepository : IMantenimientoRepository
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] EstadosActivos    = ["pendiente", "en_proceso", "iniciado"];
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

        public async Task<Mantenimiento?> GetByIdConVehiculoAsync(int id)
        {
            return await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.IdMantenimiento == id);
        }
        
        public async Task<Mantenimiento?> GetByIdSimpleAsync(int id)
        {             return await _context.Mantenimientos
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
        // PERSISTENCIA GENERAL
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

        // ================================================================
        // PROCEDIMIENTO ALMACENADO — Crear orden de mantenimiento
        // ================================================================

        public async Task<(bool exito, string mensaje, int idMantenimiento)> CrearConSPAsync(
            int      vehiculoId,
            int      empleadoId,
            string   tipo,
            string   descripcion,
            string   prioridad,
            DateOnly fechaProgramada)
        {
            var pVehiculoId      = new SqlParameter("@VehiculoId",      vehiculoId);
            var pEmpleadoId      = new SqlParameter("@EmpleadoId",      empleadoId);
            var pTipo            = new SqlParameter("@Tipo",            tipo);
            var pDescripcion     = new SqlParameter("@Descripcion",     descripcion);
            var pPrioridad       = new SqlParameter("@Prioridad",       prioridad);
            var pFechaProgramada = new SqlParameter("@FechaProgramada", fechaProgramada.ToDateTime(TimeOnly.MinValue));

            var pIdMantenimiento = new SqlParameter("@IdMantenimiento", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            var pExito = new SqlParameter("@Exito", System.Data.SqlDbType.Bit)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            var pMensaje = new SqlParameter("@Mensaje", System.Data.SqlDbType.NVarChar, 300)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_CrearOrdenMantenimiento " +
                "@VehiculoId, @EmpleadoId, @Tipo, @Descripcion, @Prioridad, @FechaProgramada, " +
                "@IdMantenimiento OUTPUT, @Exito OUTPUT, @Mensaje OUTPUT",
                pVehiculoId, pEmpleadoId, pTipo, pDescripcion,
                pPrioridad, pFechaProgramada,
                pIdMantenimiento, pExito, pMensaje);

            var exito           = (bool)pExito.Value;
            var mensaje         = pMensaje.Value?.ToString() ?? string.Empty;
            var idMantenimiento = exito ? (int)pIdMantenimiento.Value : 0;

            return (exito, mensaje, idMantenimiento);
        }
    }
}