using GoVehiculos.API.Data;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class MantenimientoService
    {
        private readonly ApplicationDbContext _context;

        // Estados que bloquean generar una nueva orden sobre el mismo vehículo
        private static readonly string[] EstadosActivos = ["pendiente", "en_proceso", "iniciado"];

        // Estados terminales: el empleado ya no puede operar sobre ellos
        private static readonly string[] EstadosTerminales = ["finalizado", "cancelado"];

        public MantenimientoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // PARTE 1 — Vista del administrador
        // ================================================================

        /// <summary>
        /// Vehículos candidatos a mantenimiento (estadoMecanico "regular" o "malo"),
        /// enriquecidos con su orden activa si existe.
        /// </summary>
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

        /// <summary>Todas las órdenes (para vistas de admin o reportes).</summary>
        public async Task<IEnumerable<MantenimientoResponseDTO>> GetAllAsync()
        {
            return await _context.Mantenimientos
                .Include(m => m.Vehiculo).ThenInclude(v => v.Modelo).ThenInclude(mo => mo.Marca)
                .Include(m => m.Empleado)
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

        /// <summary>
        /// Crea una orden de mantenimiento (acción del administrador).
        /// Reglas:
        ///   - estadoMecanico debe ser "regular" o "malo"
        ///   - No puede haber otra orden activa para el mismo vehículo
        ///   - EmpleadoId y FechaProgramada son obligatorios (validados en DTO y aquí)
        ///   - Si MantenimientoACargoDe == "socio" → rechaza con mensaje explicativo
        ///   - Si MantenimientoACargoDe == "empresa" → orden creada, vehículo → "mantenimiento"
        /// </summary>
        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> CreateAsync(MantenimientoCreateDTO dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);

            if (vehiculo == null)
                return (false, "Vehículo no encontrado.", null);

            if (vehiculo.EstadoMecanico != "regular" && vehiculo.EstadoMecanico != "malo")
                return (false, "El vehículo no requiere mantenimiento según su estado mecánico.", null);

            // Doble validación de obligatoriedad (el DTO ya valida con [Required])
            if (dto.EmpleadoId == 0)
                return (false, "Debe asignar un empleado para generar la orden.", null);

            var tieneActivo = await _context.Mantenimientos
                .AnyAsync(m => m.VehiculoId == dto.VehiculoId &&
                               EstadosActivos.Contains(m.Estado));

            if (tieneActivo)
                return (false, "El vehículo ya tiene una orden de mantenimiento activa.", null);

            // // Si el mantenimiento es del socio, no se genera orden normal
            // // El admin debe usar el flujo de HabilitarVehiculoSocio en su lugar
            // if (vehiculo.MantenimientoACargoDe == "socio")
            // {
            //     vehiculo.Estado = "fuera_de_servicio";
            //     await _context.SaveChangesAsync();
            //     return (false, "El vehículo ha pasado a estado 'fuera de servicio' porque el mantenimiento está a cargo del socio.", null);
            // }

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

        /// <summary>
        /// Registra el mantenimiento realizado por el socio y habilita el vehículo.
        /// Crea una instancia de mantenimiento con Estado = "finalizado" y
        /// RealizadoPor = "A cargo del Socio" (fijo, no editable desde el frontend).
        /// El vehículo pasa a Estado = "disponible" y EstadoMecanico = "bueno".
        /// Solo aplica si el vehículo está en Estado = "fuera_de_servicio"
        /// y MantenimientoACargoDe = "socio".
        /// </summary>
        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> HabilitarVehiculoSocioAsync(HabilitarVehiculoSocioDTO dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);

            if (vehiculo == null)
                return (false, "Vehículo no encontrado.", null);

            if (vehiculo.MantenimientoACargoDe != "socio")
                return (false, "Este flujo solo aplica a vehículos con mantenimiento a cargo del socio.", null);

            if (vehiculo.Estado != "fuera_de_servicio")
                return (false, "El vehículo debe estar en estado 'fuera de servicio' para poder habilitarlo.", null);

            // Registrar el mantenimiento realizado por el socio
            var mantenimiento = new Mantenimiento
            {
                VehiculoId       = dto.VehiculoId,
                EmpleadoId       = null,                   // no hay empleado interno
                Tipo             = dto.Tipo,
                Descripcion      = dto.Descripcion,
                Estado           = "finalizado",           // ya fue realizado
                Prioridad        = dto.Prioridad,
                FechaProgramada  = null,                   // no había orden previa
                FechaRealizacion = dto.FechaRealizacion,
                Costo            = 0,                      // el costo lo asume el socio
                RealizadoPor     = "A cargo del Socio"    // fijo, no editable
            };

            // Habilitar el vehículo
            vehiculo.Estado        = "disponible";
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

        public async Task<(bool exito, string mensaje)> FinalizarAsync(int id, int empleadoId, MantenimientoFinalizarDTO dto)
        {
            var m = await _context.Mantenimientos.FindAsync(id);

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
            RealizadoPor     = m.RealizadoPor
        };
    }
}
