using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Strategies;

namespace GoVehiculos.API.Services
{
    /// <summary>
    /// Contexto del patrón Strategy.
    ///
    /// PATRÓN STRATEGY — Contexto:
    /// En el patrón Strategy el "contexto" es la clase que conoce qué estrategia
    /// usar según la situación, pero delega la ejecución del algoritmo a la
    /// estrategia concreta seleccionada. MantenimientoService cumple ese rol:
    /// recibe la solicitud de acción (iniciar, finalizar, cancelar), selecciona
    /// la estrategia correspondiente, le pasa la entidad y el DTO, y persiste
    /// el resultado.
    ///
    /// El service no contiene la lógica de cada acción — solo orquesta.
    /// Esto permite agregar nuevas acciones (ej: "pausar") creando una nueva
    /// estrategia sin modificar este archivo (Open/Closed Principle).
    /// </summary>
    public class MantenimientoService
    {
        private readonly IMantenimientoRepository _repo;
        private readonly IVehiculoRepository _vehiculoRepo;

        // ================================================================
        // Estrategias registradas en el contexto.
        //
        // PATRÓN STRATEGY — Registro de estrategias:
        // Cada estrategia es una instancia de una clase que implementa
        // IAccionMantenimientoStrategy. Al instanciarlas aquí (o inyectarlas
        // por DI si se prefiere), el contexto las tiene disponibles sin
        // acoplarse a sus implementaciones concretas.
        // ================================================================
        private readonly IAccionMantenimientoStrategy _iniciarStrategy   = new IniciarStrategy();
        private readonly IAccionMantenimientoStrategy _finalizarStrategy = new FinalizarStrategy();
        private readonly IAccionMantenimientoStrategy _cancelarStrategy  = new CancelarStrategy();

        public MantenimientoService(IMantenimientoRepository repo, IVehiculoRepository vehiculoRepo)
        {
            _repo = repo;
            _vehiculoRepo = vehiculoRepo;
        }

        // ================================================================
        // CONTADORES (protagonista: Mantenimientos)
        // ================================================================

        public Task<int> GetContadorEmpleadoAsync(int empleadoId)
            => _repo.ContarPendientesPorEmpleadoAsync(empleadoId);

        public Task<int> GetContadorNuevasTerminadasAsync()
            => _repo.ContarTerminadosAsync();

        // ================================================================
        // CONSULTAS
        // ================================================================

        public async Task<IEnumerable<MantenimientoResponseDTO>> GetAllAsync(string? estado = null)
        {
            var lista = await _repo.GetAllAsync(estado);
            return lista.Select(ToResponseDTO);
        }

        public async Task<MantenimientoResponseDTO?> GetByIdAsync(int id)
        {
            var m = await _repo.GetByIdAsync(id);
            return m == null ? null : ToResponseDTO(m);
        }

        public async Task<IEnumerable<MantenimientoResponseDTO>> GetByEmpleadoAsync(int empleadoId)
        {
            var lista = await _repo.GetByEmpleadoAsync(empleadoId);
            return lista.Select(ToResponseDTO);
        }

        // ================================================================
        // PARTE 1 — Órdenes (admin)
        // ================================================================

        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> CreateAsync(MantenimientoCreateDTO dto)
        {
            // 1. Validación de campos vacíos o inválidos
            var errorCampos = ValidarCamposCreate(dto);
            if (errorCampos != null) return (false, errorCampos, null);

            // 2. Verificaciones de reglas de negocio
            var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(dto.VehiculoId);

            if (vehiculo == null)
                return (false, "Vehículo no encontrado.", null);

            if (vehiculo.EstadoMecanico != "regular" && vehiculo.EstadoMecanico != "malo")
                return (false, "El vehículo no requiere mantenimiento según su estado mecánico.", null);

            if (dto.EmpleadoId == 0)
                return (false, "Debe asignar un empleado para generar la orden.", null);

            if (await _repo.TieneActivoAsync(dto.VehiculoId))
                return (false, "El vehículo ya tiene una orden de mantenimiento activa.", null);

            if (vehiculo.MantenimientoACargoDe == "socio")
                return (false, "Este vehículo tiene el mantenimiento a cargo del socio. Usá la opción correspondiente.", null);

            // 3. Construcción de la entidad y persistencia
            var mantenimiento = CrearOrdenDesdeDTO(dto);
            vehiculo.Estado = "mantenimiento";

            await _repo.AddAsync(mantenimiento);
            await _repo.SaveChangesAsync();

            var result = await GetByIdAsync(mantenimiento.IdMantenimiento);
            return (true, "Orden de mantenimiento creada correctamente.", result);
        }

        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> HabilitarVehiculoSocioAsync(HabilitarVehiculoSocioDTO dto)
        {
            // 1. Validación de campos vacíos o inválidos
            var errorCampos = ValidarCamposHabilitar(dto);
            if (errorCampos != null) return (false, errorCampos, null);

            // 2. Verificaciones de reglas de negocio
            var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(dto.VehiculoId);

            if (vehiculo == null)
                return (false, "Vehículo no encontrado.", null);

            if (vehiculo.MantenimientoACargoDe != "socio")
                return (false, "Este flujo solo aplica a vehículos con mantenimiento a cargo del socio.", null);

            if (vehiculo.Estado != "fuera_de_servicio")
                return (false, "El vehículo debe estar en estado 'fuera de servicio' para poder habilitarlo.", null);

            // 3. Construcción de la entidad y persistencia
            var mantenimiento = CrearRegistroSocioDesdeDTO(dto);
            vehiculo.Estado = "disponible";
            vehiculo.EstadoMecanico = "bueno";

            await _repo.AddAsync(mantenimiento);
            await _repo.SaveChangesAsync();

            var result = await GetByIdAsync(mantenimiento.IdMantenimiento);
            return (true, "Vehículo habilitado correctamente. El mantenimiento del socio fue registrado.", result);
        }

        public async Task<bool> UpdateAsync(int id, MantenimientoUpdateDTO dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Estado           = dto.Estado;
            existing.Prioridad        = dto.Prioridad;
            existing.FechaProgramada  = dto.FechaProgramada;
            existing.FechaRealizacion = dto.FechaRealizacion;
            existing.Costo            = dto.Costo;
            existing.RealizadoPor     = dto.RealizadoPor;
            existing.Descripcion      = dto.Descripcion;
            existing.EmpleadoId       = dto.EmpleadoId;

            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return false;

            await _repo.DeleteAsync(id);
            await _repo.SaveChangesAsync();
            return true;
        }

        // ================================================================
        // PARTE 2 — Vista del empleado
        //
        // PATRÓN STRATEGY — Métodos de despacho:
        // Cada método público selecciona la estrategia correspondiente,
        // carga la entidad con la query correcta para esa acción, y delega
        // la ejecución. El service no contiene ninguna lógica de transición
        // de estado — eso es responsabilidad exclusiva de cada estrategia.
        //
        // Si se agrega una nueva acción (ej: "pausar"), basta con crear
        // PausarStrategy e implementar un método EjecutarAccionAsync aquí
        // que la invoque. Este archivo no necesita cambios estructurales.
        // ================================================================

        public async Task<(bool exito, string mensaje)> IniciarAsync(int id, int empleadoId)
        {
            // IniciarStrategy no necesita navegación al vehículo
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return (false, "Mantenimiento no encontrado.");

            // Delegación a la estrategia: el service no sabe cómo se inicia, solo a quién pedírselo
            var (exito, mensaje) = await _iniciarStrategy.EjecutarAsync(m, empleadoId);
            if (!exito) return (false, mensaje);

            await _repo.SaveChangesAsync();
            return (true, mensaje);
        }

        public async Task<(bool exito, string mensaje)> FinalizarAsync(int id, int empleadoId, MantenimientoFinalizarDTO dto)
        {
            // 1. Validación de campos antes de tocar la base de datos
            var errorCampos = ValidarCamposFinalizar(dto);
            if (errorCampos != null) return (false, errorCampos);

            // FinalizarStrategy necesita la navegación al Vehiculo para actualizar EstadoMecanico
            var m = await _repo.GetByIdConVehiculoAsync(id);
            if (m == null) return (false, "Mantenimiento no encontrado.");

            // El DTO se pasa como contexto — la estrategia lo casteará internamente
            var (exito, mensaje) = await _finalizarStrategy.EjecutarAsync(m, empleadoId, dto);
            if (!exito) return (false, mensaje);

            await _repo.SaveChangesAsync();
            return (true, mensaje);
        }

        public async Task<(bool exito, string mensaje)> CancelarAsync(int id, int empleadoId, MantenimientoCancelarDTO dto)
        {
            // 1. Validación de campos antes de tocar la base de datos
            var errorCampos = ValidarCamposCancelar(dto);
            if (errorCampos != null) return (false, errorCampos);

            // CancelarStrategy solo necesita la entidad básica
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return (false, "Mantenimiento no encontrado.");

            // El DTO se pasa como contexto — la estrategia lo casteará internamente
            var (exito, mensaje) = await _cancelarStrategy.EjecutarAsync(m, empleadoId, dto);
            if (!exito) return (false, mensaje);

            await _repo.SaveChangesAsync();
            return (true, mensaje);
        }

        // ================================================================
        // PARTE 3 — Disponibilizar vehículo
        // ================================================================

        public async Task<(bool exito, string mensaje)> DisponibilizarVehiculoAsync(int idMantenimiento)
        {
            var m = await _repo.GetByIdConVehiculoAsync(idMantenimiento);

            if (m == null)
                return (false, "Orden de mantenimiento no encontrada.");

            if (m.Estado != "finalizado")
                return (false, "Solo se puede disponibilizar el vehículo de una orden finalizada.");

            if (m.Vehiculo == null)
                return (false, "No se encontró el vehículo asociado.");

            if (m.Disponibilizado)
                return (false, "Esta orden ya fue disponibilizada anteriormente.");

            m.Vehiculo.Estado = "disponible";
            m.Disponibilizado = true;

            await _repo.SaveChangesAsync();
            return (true, "Vehículo disponibilizado correctamente.");
        }

        // ================================================================
        // Métodos privados — Validaciones de campos
        //
        // Verifican que los datos del DTO sean completos y coherentes
        // antes de tocar la base de datos. Se ejecutan siempre como primer
        // paso en los métodos públicos que reciben input del usuario.
        // Devuelven el primer error encontrado, o null si todo está bien.
        // Son estáticos porque no dependen del estado de la instancia.
        // ================================================================

        private static string? ValidarCamposCreate(MantenimientoCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tipo))        return "El tipo de mantenimiento es obligatorio.";
            if (string.IsNullOrWhiteSpace(dto.Descripcion)) return "La descripción es obligatoria.";
            if (string.IsNullOrWhiteSpace(dto.Prioridad))   return "La prioridad es obligatoria.";
            if (dto.FechaProgramada == null)                return "La fecha programada es obligatoria.";
            if (dto.FechaProgramada < DateOnly.FromDateTime(DateTime.Today))
                                                            return "La fecha programada no puede ser anterior a hoy.";
            return null;
        }

        private static string? ValidarCamposHabilitar(HabilitarVehiculoSocioDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tipo))        return "El tipo es obligatorio.";
            if (string.IsNullOrWhiteSpace(dto.Descripcion)) return "La descripción es obligatoria.";
            if (dto.FechaRealizacion == default)            return "La fecha de realización es obligatoria.";
            return null;
        }

        private static string? ValidarCamposFinalizar(MantenimientoFinalizarDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Descripcion))  return "La descripción es obligatoria.";
            if (string.IsNullOrWhiteSpace(dto.RealizadoPor)) return "Debe indicar quién realizó el trabajo.";
            if (dto.FechaRealizacion == default)             return "La fecha de realización es obligatoria.";
            if (dto.Costo < 0)                               return "El costo no puede ser negativo.";
            return null;
        }

        private static string? ValidarCamposCancelar(MantenimientoCancelarDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Descripcion)) return "Debe indicar el motivo de cancelación.";
            return null;
        }

        // ================================================================
        // Métodos privados — Construcción de entidades
        //
        // Centralizan la inicialización de Mantenimiento para que los
        // métodos públicos se enfoquen en orquestación sin ruido de detalles
        // de construcción. Son estáticos porque solo operan sobre el DTO.
        // ================================================================

        /// <summary>
        /// Construye la entidad para una orden generada por el administrador.
        /// Estado inicial: pendiente. Costo y RealizadoPor se completan al finalizar.
        /// </summary>
        private static Mantenimiento CrearOrdenDesdeDTO(MantenimientoCreateDTO dto) => new()
        {
            VehiculoId      = dto.VehiculoId,
            EmpleadoId      = dto.EmpleadoId,
            Tipo            = dto.Tipo,
            Descripcion     = dto.Descripcion,
            Estado          = "pendiente",
            Prioridad       = dto.Prioridad,
            FechaProgramada = dto.FechaProgramada,
            Costo           = 0,
            RealizadoPor    = string.Empty,
            Disponibilizado = false
        };

        /// <summary>
        /// Construye el registro histórico para un mantenimiento realizado por el socio.
        /// Estado inicial: finalizado. El vehículo queda disponible inmediatamente.
        /// </summary>
        private static Mantenimiento CrearRegistroSocioDesdeDTO(HabilitarVehiculoSocioDTO dto) => new()
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
            RealizadoPor     = "A cargo del Socio",
            Disponibilizado  = true
        };

        // ================================================================
        // Métodos privados — Mapeo
        // ================================================================

        /// <summary>
        /// Convierte una entidad Mantenimiento al DTO de respuesta que consume el frontend.
        /// Usa el operador ?. para tolerar navegaciones no cargadas sin lanzar excepciones.
        /// </summary>
        private static MantenimientoResponseDTO ToResponseDTO(Mantenimiento m) => new()
        {
            IdMantenimiento  = m.IdMantenimiento,
            VehiculoId       = m.VehiculoId,
            VehiculoPatente  = m.Vehiculo?.Patente               ?? string.Empty,
            VehiculoMarca    = m.Vehiculo?.Modelo?.Marca?.Nombre  ?? string.Empty,
            VehiculoModelo   = m.Vehiculo?.Modelo?.Nombre         ?? string.Empty,
            VehiculoEstado   = m.Vehiculo?.Estado                ?? string.Empty,
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
            Disponibilizado  = m.Disponibilizado,
        };
    }
}