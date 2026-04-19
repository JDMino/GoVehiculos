using GoVehiculos.API.Data;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class MantenimientoService
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] EstadosActivos    = ["pendiente", "en_proceso", "iniciado"];
        private static readonly string[] EstadosTerminales = ["finalizado", "cancelado"];

        public MantenimientoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // PARTE 1 — Vista del administrador
        // ================================================================

        public async Task<IEnumerable<VehiculoCandidatoDTO>> GetVehiculosCandidatosAsync()
        {
            var vehiculos = await _context.Vehiculos
                .Include(v => v.Modelo).ThenInclude(m => m.Marca)
                .Include(v => v.UbicacionActual)
                .Where(v => v.Activo &&
                            (v.EstadoMecanico == "regular" || v.EstadoMecanico == "malo"))
                .ToListAsync();

            var ids = vehiculos.Select(v => v.IdVehiculo).ToList();

            var mantenimientosActivos = await _context.Mantenimientos
                .Include(m => m.Empleado)
                .Where(m => ids.Contains(m.VehiculoId) &&
                            EstadosActivos.Contains(m.Estado))
                .OrderByDescending(m => m.IdMantenimiento)
                .ToListAsync();

            var mantPorVehiculo = mantenimientosActivos
                .GroupBy(m => m.VehiculoId)
                .ToDictionary(g => g.Key, g => g.First());

            return vehiculos.Select(v =>
            {
                mantPorVehiculo.TryGetValue(v.IdVehiculo, out var mant);
                return new VehiculoCandidatoDTO
                {
                    IdVehiculo            = v.IdVehiculo,
                    SocioId               = v.SocioId,
                    Patente               = v.Patente,
                    Anio                  = v.Anio,
                    Tipo                  = v.Tipo,
                    Estado                = v.Estado,
                    EstadoMecanico        = v.EstadoMecanico,
                    Kilometraje           = v.Kilometraje,
                    PrecioPorDia          = v.PrecioPorDia,
                    MantenimientoACargoDe = v.MantenimientoACargoDe,
                    SeguroVigente         = v.SeguroVigente,
                    DocumentacionVigente  = v.DocumentacionVigente,
                    Activo                = v.Activo,
                    ImagenUrl             = v.ImagenUrl,
                    ModeloId              = v.ModeloId,
                    ModeloNombre          = v.Modelo?.Nombre ?? string.Empty,
                    MarcaNombre           = v.Modelo?.Marca?.Nombre ?? string.Empty,
                    UbicacionActualId     = v.UbicacionActualId,
                    UbicacionNombre       = v.UbicacionActual?.Nombre,
                    MantenimientoActivo   = mant == null ? null : new MantenimientoActivoDTO
                    {
                        IdMantenimiento = mant.IdMantenimiento,
                        Estado          = mant.Estado,
                        Prioridad       = mant.Prioridad,
                        Tipo            = mant.Tipo,
                        EmpleadoId      = mant.EmpleadoId,
                        EmpleadoNombre  = mant.Empleado != null
                                              ? $"{mant.Empleado.Nombre} {mant.Empleado.Apellido}"
                                              : null,
                        FechaProgramada = mant.FechaProgramada,
                    }
                };
            });
        }

        /// <summary>
        /// Todas las órdenes con filtros opcionales por estado.
        /// Usado por la vista de historial (finalizadas y canceladas).
        /// </summary>
        public async Task<IEnumerable<MantenimientoResponseDTO>> GetAllAsync(string? estado = null)
        {
            var query = _context.Mantenimientos
                .Include(m => m.Vehiculo).ThenInclude(v => v.Modelo).ThenInclude(mo => mo.Marca)
                .Include(m => m.Empleado)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(m => m.Estado == estado);

            return await query
                .OrderByDescending(m => m.IdMantenimiento)
                .Select(m => ToResponseDTO(m))
                .ToListAsync();
        }

        public async Task<MantenimientoResponseDTO?> GetByIdAsync(int id)
        {
            var m = await _context.Mantenimientos
                .Include(m => m.Vehiculo).ThenInclude(v => v.Modelo).ThenInclude(mo => mo.Marca)
                .Include(m => m.Empleado)
                .FirstOrDefaultAsync(m => m.IdMantenimiento == id);

            return m == null ? null : ToResponseDTO(m);
        }

        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> CreateAsync(MantenimientoCreateDTO dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);

            if (vehiculo == null)
                return (false, "Vehículo no encontrado.", null);

            if (vehiculo.EstadoMecanico != "regular" && vehiculo.EstadoMecanico != "malo")
                return (false, "El vehículo no requiere mantenimiento según su estado mecánico.", null);

            if (dto.EmpleadoId == 0)
                return (false, "Debe asignar un empleado para generar la orden.", null);

            var tieneActivo = await _context.Mantenimientos
                .AnyAsync(m => m.VehiculoId == dto.VehiculoId &&
                               EstadosActivos.Contains(m.Estado));

            if (tieneActivo)
                return (false, "El vehículo ya tiene una orden de mantenimiento activa.", null);

            if (vehiculo.MantenimientoACargoDe == "socio")
                return (false, "Este vehículo tiene el mantenimiento a cargo del socio. Usá la opción correspondiente.", null);

            var mantenimiento = new Mantenimiento
            {
                VehiculoId      = dto.VehiculoId,
                EmpleadoId      = dto.EmpleadoId,
                Tipo            = dto.Tipo,
                Descripcion     = dto.Descripcion,
                Estado          = "pendiente",
                Prioridad       = dto.Prioridad,
                FechaProgramada = dto.FechaProgramada,
                Costo           = 0,
                RealizadoPor    = string.Empty
            };

            vehiculo.Estado = "mantenimiento";

            _context.Mantenimientos.Add(mantenimiento);
            await _context.SaveChangesAsync();

            var result = await GetByIdAsync(mantenimiento.IdMantenimiento);
            return (true, "Orden de mantenimiento creada correctamente.", result);
        }

        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> HabilitarVehiculoSocioAsync(HabilitarVehiculoSocioDTO dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);

            if (vehiculo == null)
                return (false, "Vehículo no encontrado.", null);

            if (vehiculo.MantenimientoACargoDe != "socio")
                return (false, "Este flujo solo aplica a vehículos con mantenimiento a cargo del socio.", null);

            if (vehiculo.Estado != "fuera_de_servicio")
                return (false, "El vehículo debe estar en estado 'fuera de servicio' para poder habilitarlo.", null);

            var mantenimiento = new Mantenimiento
            {
                VehiculoId       = dto.VehiculoId,
                EmpleadoId       = null,
                Tipo             = dto.Tipo,
                Descripcion      = dto.Descripcion,
                Estado           = "finalizado",
                Prioridad        = dto.Prioridad,
                FechaProgramada  = null,
                FechaRealizacion = dto.FechaRealizacion,
                Costo            = 0,
                RealizadoPor     = "A cargo del Socio"
            };

            vehiculo.Estado         = "disponible";
            vehiculo.EstadoMecanico = "bueno";

            _context.Mantenimientos.Add(mantenimiento);
            await _context.SaveChangesAsync();

            var result = await GetByIdAsync(mantenimiento.IdMantenimiento);
            return (true, "Vehículo habilitado correctamente. El mantenimiento del socio fue registrado.", result);
        }

        public async Task<bool> UpdateAsync(int id, MantenimientoUpdateDTO dto)
        {
            var existing = await _context.Mantenimientos.FindAsync(id);
            if (existing == null) return false;

            existing.Estado           = dto.Estado;
            existing.Prioridad        = dto.Prioridad;
            existing.FechaProgramada  = dto.FechaProgramada;
            existing.FechaRealizacion = dto.FechaRealizacion;
            existing.Costo            = dto.Costo;
            existing.RealizadoPor     = dto.RealizadoPor;
            existing.Descripcion      = dto.Descripcion;
            existing.EmpleadoId       = dto.EmpleadoId;

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

        // ================================================================
        // PARTE 2 — Vista del empleado
        // ================================================================

        public async Task<IEnumerable<MantenimientoResponseDTO>> GetByEmpleadoAsync(int empleadoId)
        {
            return await _context.Mantenimientos
                .Include(m => m.Vehiculo).ThenInclude(v => v.Modelo).ThenInclude(mo => mo.Marca)
                .Include(m => m.Empleado)
                .Where(m => m.EmpleadoId == empleadoId)
                .OrderByDescending(m => m.IdMantenimiento)
                .Select(m => ToResponseDTO(m))
                .ToListAsync();
        }

        public async Task<(bool exito, string mensaje)> IniciarAsync(int id, int empleadoId)
        {
            var m = await _context.Mantenimientos.FindAsync(id);

            if (m == null)
                return (false, "Mantenimiento no encontrado.");

            if (m.EmpleadoId != empleadoId)
                return (false, "No tenés permiso para operar este mantenimiento.");

            if (m.Estado != "pendiente")
                return (false, $"El mantenimiento no puede iniciarse porque está en estado '{m.Estado}'.");

            m.Estado = "iniciado";
            await _context.SaveChangesAsync();
            return (true, "Mantenimiento iniciado correctamente.");
        }

        /// <summary>
        /// Finaliza el mantenimiento: "iniciado" → "finalizado".
        /// Además actualiza EstadoMecanico del vehículo a "bueno".
        /// El Estado del vehículo NO cambia aquí (sigue en "mantenimiento"
        /// hasta que el admin lo disponibilice explícitamente).
        /// </summary>
        public async Task<(bool exito, string mensaje)> FinalizarAsync(int id, int empleadoId, MantenimientoFinalizarDTO dto)
        {
            var m = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.IdMantenimiento == id);

            if (m == null)
                return (false, "Mantenimiento no encontrado.");

            if (m.EmpleadoId != empleadoId)
                return (false, "No tenés permiso para operar este mantenimiento.");

            if (m.Estado != "iniciado")
                return (false, $"El mantenimiento no puede finalizarse porque está en estado '{m.Estado}'.");

            if (dto.Costo < 0)
                return (false, "El costo no puede ser negativo.");

            if (m.FechaProgramada.HasValue && dto.FechaRealizacion < m.FechaProgramada.Value)
                return (false, $"La fecha de realización no puede ser anterior a la fecha programada ({m.FechaProgramada.Value:dd/MM/yyyy}).");

            m.Descripcion      = dto.Descripcion;
            m.FechaRealizacion = dto.FechaRealizacion;
            m.Costo            = dto.Costo;
            m.RealizadoPor     = dto.RealizadoPor;
            m.Estado           = "finalizado";

            // Al finalizar el trabajo, el estado mecánico del vehículo pasa a "bueno"
            if (m.Vehiculo != null)
                m.Vehiculo.EstadoMecanico = "bueno";

            await _context.SaveChangesAsync();
            return (true, "Mantenimiento finalizado correctamente.");
        }

        public async Task<(bool exito, string mensaje)> CancelarAsync(int id, int empleadoId, MantenimientoCancelarDTO dto)
        {
            var m = await _context.Mantenimientos.FindAsync(id);

            if (m == null)
                return (false, "Mantenimiento no encontrado.");

            if (m.EmpleadoId != empleadoId)
                return (false, "No tenés permiso para operar este mantenimiento.");

            if (m.Estado != "iniciado")
                return (false, $"El mantenimiento no puede cancelarse porque está en estado '{m.Estado}'.");

            m.Descripcion = dto.Descripcion;
            m.Estado      = "cancelado";

            await _context.SaveChangesAsync();
            return (true, "Mantenimiento cancelado.");
        }

        // ================================================================
        // PARTE 3 — Disponibilizar vehículo (acción del administrador)
        // ================================================================

        /// <summary>
        /// Cambia el Estado del vehículo a "disponible" luego de que
        /// el mantenimiento fue finalizado por el empleado.
        /// Solo aplica si el mantenimiento está en estado "finalizado".
        /// El EstadoMecanico ya fue seteado a "bueno" por FinalizarAsync.
        /// </summary>
        public async Task<(bool exito, string mensaje)> DisponibilizarVehiculoAsync(int idMantenimiento)
        {
            var m = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                .FirstOrDefaultAsync(m => m.IdMantenimiento == idMantenimiento);

            if (m == null)
                return (false, "Orden de mantenimiento no encontrada.");

            if (m.Estado != "finalizado")
                return (false, "Solo se puede disponibilizar el vehículo de una orden finalizada.");

            if (m.Vehiculo == null)
                return (false, "No se encontró el vehículo asociado.");

            if (m.Vehiculo.Estado == "disponible")
                return (false, "El vehículo ya está en estado disponible.");

            m.Vehiculo.Estado = "disponible";

            await _context.SaveChangesAsync();
            return (true, "Vehículo disponibilizado correctamente.");
        }

        // ================================================================
        // Mapeo privado
        // ================================================================
        private static MantenimientoResponseDTO ToResponseDTO(Mantenimiento m) => new()
        {
            IdMantenimiento  = m.IdMantenimiento,
            VehiculoId       = m.VehiculoId,
            VehiculoPatente  = m.Vehiculo?.Patente ?? string.Empty,
            VehiculoMarca    = m.Vehiculo?.Modelo?.Marca?.Nombre ?? string.Empty,
            VehiculoModelo   = m.Vehiculo?.Modelo?.Nombre ?? string.Empty,
            EmpleadoId       = m.EmpleadoId,
            EmpleadoNombre   = m.Empleado != null
                                   ? $"{m.Empleado.Nombre} {m.Empleado.Apellido}"
                                   : null,
            Tipo             = m.Tipo,
            Descripcion      = m.Descripcion,
            Estado           = m.Estado,
            Prioridad        = m.Prioridad,
            FechaProgramada  = m.FechaProgramada,
            FechaRealizacion = m.FechaRealizacion,
            Costo            = m.Costo,
            RealizadoPor     = m.RealizadoPor,
            // Expone el estado actual del vehículo para que el frontend
            // sepa si el botón "Disponibilizar" ya fue usado
            VehiculoEstado   = m.Vehiculo?.Estado ?? string.Empty,
        };
    }
}
