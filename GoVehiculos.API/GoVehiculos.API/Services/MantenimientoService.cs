using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Strategies;

namespace GoVehiculos.API.Services
{
    /// <summary>
    /// Contexto del patrón Strategy para las acciones del empleado.
    /// Las tres estrategias (Iniciar, Finalizar, Cancelar) permanecen
    /// intactas. El SP reemplaza únicamente la creación de órdenes,
    /// que no tenía Strategy asociada.
    /// </summary>
    public class MantenimientoService
    {
        private readonly IMantenimientoRepository _repo;
        private readonly IVehiculoRepository      _vehiculoRepo;

        private readonly IAccionMantenimientoStrategy _iniciarStrategy   = new IniciarStrategy();
        private readonly IAccionMantenimientoStrategy _finalizarStrategy = new FinalizarStrategy();
        private readonly IAccionMantenimientoStrategy _cancelarStrategy  = new CancelarStrategy();

        public MantenimientoService(IMantenimientoRepository repo, IVehiculoRepository vehiculoRepo)
        {
            _repo         = repo;
            _vehiculoRepo = vehiculoRepo;
        }

        // ================================================================
        // CONTADORES
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

        // ── CreateAsync — con SP ──────────────────────────────────────
        // ANTES: este método ejecutaba 3 queries LINQ separadas para
        // validar (GetByIdSimpleAsync + TieneActivoAsync + verificaciones
        // en memoria), luego modificaba vehiculo.Estado en memoria,
        // llamaba AddAsync(mantenimiento) y finalmente SaveChangesAsync(),
        // que emitía el INSERT y el UPDATE como statements separados
        // en la transacción implícita de EF.
        //
        // DESPUÉS: solo valida los campos del DTO (responsabilidad del
        // service) y delega la operación completa a _repo.CrearConSPAsync().
        // El SP encapsula las validaciones de negocio, el INSERT en
        // Mantenimiento y el UPDATE en Vehiculo en una única transacción
        // atómica en SQL Server. El service recibe el ID generado y lo
        // usa para devolver el DTO de la orden creada.
        // ─────────────────────────────────────────────────────────────
        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> CreateAsync(
            MantenimientoCreateDTO dto)
        {
            var errorCampos = ValidarCamposCreate(dto);
            if (errorCampos != null) return (false, errorCampos, null);

            var (exito, mensaje, idMantenimiento) = await _repo.CrearConSPAsync(
                vehiculoId:      dto.VehiculoId,
                empleadoId:      dto.EmpleadoId,
                tipo:            dto.Tipo,
                descripcion:     dto.Descripcion,
                prioridad:       dto.Prioridad,
                fechaProgramada: dto.FechaProgramada);

            if (!exito) return (false, mensaje, null);

            var result = await GetByIdAsync(idMantenimiento);
            return (true, mensaje, result);
        }

        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> HabilitarVehiculoSocioAsync(
            HabilitarVehiculoSocioDTO dto)
        {
            var errorCampos = ValidarCamposHabilitar(dto);
            if (errorCampos != null) return (false, errorCampos, null);

            var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(dto.VehiculoId);
            if (vehiculo == null)
                return (false, "Vehículo no encontrado.", null);
            if (vehiculo.MantenimientoACargoDe != "socio")
                return (false, "Este flujo solo aplica a vehículos con mantenimiento a cargo del socio.", null);
            if (vehiculo.Estado != "fuera_de_servicio")
                return (false, "El vehículo debe estar en estado 'fuera de servicio' para poder habilitarlo.", null);

            var mantenimiento = CrearRegistroSocioDesdeDTO(dto);
            vehiculo.Estado         = "disponible";
            vehiculo.EstadoMecanico = "bueno";

            await _repo.AddAsync(mantenimiento);
            await _repo.SaveChangesAsync();

            var result = await GetByIdAsync(mantenimiento.IdMantenimiento);
            return (true, "Vehículo habilitado correctamente.", result);
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
        // PARTE 2 — Vista del empleado (patrón Strategy intacto)
        // ================================================================

        public async Task<(bool exito, string mensaje)> IniciarAsync(int id, int empleadoId)
        {
            var m = await _repo.GetByIdAsync(id);
            if (m == null) return (false, "Mantenimiento no encontrado.");

            var (exito, mensaje) = await _iniciarStrategy.EjecutarAsync(m, empleadoId);
            if (!exito) return (false, mensaje);

            await _repo.SaveChangesAsync();
            return (true, mensaje);
        }

        public async Task<(bool exito, string mensaje)> FinalizarAsync(
            int id, int empleadoId, MantenimientoFinalizarDTO dto)
        {
            var errorCampos = ValidarCamposFinalizar(dto);
            if (errorCampos != null) return (false, errorCampos);

            var m = await _repo.GetByIdConVehiculoAsync(id);
            if (m == null) return (false, "Mantenimiento no encontrado.");

            var (exito, mensaje) = await _finalizarStrategy.EjecutarAsync(m, empleadoId, dto);
            if (!exito) return (false, mensaje);

            await _repo.SaveChangesAsync();
            return (true, mensaje);
        }

        public async Task<(bool exito, string mensaje)> CancelarAsync(
            int id, int empleadoId, MantenimientoCancelarDTO dto)
        {
            var errorCampos = ValidarCamposCancelar(dto);
            if (errorCampos != null) return (false, errorCampos);

            var m = await _repo.GetByIdAsync(id);
            if (m == null) return (false, "Mantenimiento no encontrado.");

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
        // Validaciones privadas
        // ================================================================

        private static string? ValidarCamposCreate(MantenimientoCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tipo))        return "El tipo de mantenimiento es obligatorio.";
            if (string.IsNullOrWhiteSpace(dto.Descripcion)) return "La descripción es obligatoria.";
            if (string.IsNullOrWhiteSpace(dto.Prioridad))   return "La prioridad es obligatoria.";
            if (dto.FechaProgramada == null)                 return "La fecha programada es obligatoria.";
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
        // Construcción de entidades
        // ================================================================

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
        // Mapeo privado
        // ================================================================

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