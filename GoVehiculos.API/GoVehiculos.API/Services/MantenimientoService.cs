using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Services
{
    public class MantenimientoService
    {
        private readonly IMantenimientoRepository _repo;
        private readonly IVehiculoRepository _vehiculoRepo;

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

            var mantenimiento = new Mantenimiento
            {
                VehiculoId = dto.VehiculoId,
                EmpleadoId = dto.EmpleadoId,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                Estado = "pendiente",
                Prioridad = dto.Prioridad,
                FechaProgramada = dto.FechaProgramada,
                Costo = 0,
                RealizadoPor = string.Empty,
                Disponibilizado = false
            };

            vehiculo.Estado = "mantenimiento";

            await _repo.AddAsync(mantenimiento);
            await _repo.SaveChangesAsync();

            var result = await GetByIdAsync(mantenimiento.IdMantenimiento);
            return (true, "Orden de mantenimiento creada correctamente.", result);
        }

        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> HabilitarVehiculoSocioAsync(HabilitarVehiculoSocioDTO dto)
        {
            var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(dto.VehiculoId);

            if (vehiculo == null)
                return (false, "Vehículo no encontrado.", null);

            if (vehiculo.MantenimientoACargoDe != "socio")
                return (false, "Este flujo solo aplica a vehículos con mantenimiento a cargo del socio.", null);

            if (vehiculo.Estado != "fuera_de_servicio")
                return (false, "El vehículo debe estar en estado 'fuera de servicio' para poder habilitarlo.", null);

            var mantenimiento = new Mantenimiento
            {
                VehiculoId = dto.VehiculoId,
                EmpleadoId = null,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                Estado = "finalizado",
                Prioridad = dto.Prioridad,
                FechaProgramada = null,
                FechaRealizacion = dto.FechaRealizacion,
                Costo = 0,
                RealizadoPor = "A cargo del Socio",
                Disponibilizado = true
            };

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

            existing.Estado = dto.Estado;
            existing.Prioridad = dto.Prioridad;
            existing.FechaProgramada = dto.FechaProgramada;
            existing.FechaRealizacion = dto.FechaRealizacion;
            existing.Costo = dto.Costo;
            existing.RealizadoPor = dto.RealizadoPor;
            existing.Descripcion = dto.Descripcion;
            existing.EmpleadoId = dto.EmpleadoId;

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
        // ================================================================

        public async Task<(bool exito, string mensaje)> IniciarAsync(int id, int empleadoId)
        {
            var m = await _repo.GetByIdAsync(id);

            if (m == null)
                return (false, "Mantenimiento no encontrado.");

            if (m.EmpleadoId != empleadoId)
                return (false, "No tenés permiso para operar este mantenimiento.");

            if (m.Estado != "pendiente")
                return (false, $"El mantenimiento no puede iniciarse porque está en estado '{m.Estado}'.");

            m.Estado = "iniciado";
            await _repo.SaveChangesAsync();
            return (true, "Mantenimiento iniciado correctamente.");
        }

        public async Task<(bool exito, string mensaje)> FinalizarAsync(int id, int empleadoId, MantenimientoFinalizarDTO dto)
        {
            var m = await _repo.GetByIdConVehiculoAsync(id);

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

            m.Descripcion = dto.Descripcion;
            m.FechaRealizacion = dto.FechaRealizacion;
            m.Costo = dto.Costo;
            m.RealizadoPor = dto.RealizadoPor;
            m.Estado = "finalizado";

            if (m.Vehiculo != null)
                m.Vehiculo.EstadoMecanico = "bueno";

            await _repo.SaveChangesAsync();
            return (true, "Mantenimiento finalizado correctamente.");
        }

        public async Task<(bool exito, string mensaje)> CancelarAsync(int id, int empleadoId, MantenimientoCancelarDTO dto)
        {
            var m = await _repo.GetByIdAsync(id);

            if (m == null)
                return (false, "Mantenimiento no encontrado.");

            if (m.EmpleadoId != empleadoId)
                return (false, "No tenés permiso para operar este mantenimiento.");

            if (m.Estado != "iniciado")
                return (false, $"El mantenimiento no puede cancelarse porque está en estado '{m.Estado}'.");

            m.Descripcion = dto.Descripcion;
            m.Estado = "cancelado";

            await _repo.SaveChangesAsync();
            return (true, "Mantenimiento cancelado.");
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
        // Mapeo privado
        // ================================================================

        private static MantenimientoResponseDTO ToResponseDTO(Mantenimiento m) => new()
        {
            IdMantenimiento = m.IdMantenimiento,
            VehiculoId = m.VehiculoId,
            VehiculoPatente = m.Vehiculo?.Patente ?? string.Empty,
            VehiculoMarca = m.Vehiculo?.Modelo?.Marca?.Nombre ?? string.Empty,
            VehiculoModelo = m.Vehiculo?.Modelo?.Nombre ?? string.Empty,
            VehiculoEstado = m.Vehiculo?.Estado ?? string.Empty,
            EmpleadoId = m.EmpleadoId,
            EmpleadoNombre = m.Empleado != null
                                   ? $"{m.Empleado.Nombre} {m.Empleado.Apellido}"
                                   : null,
            Tipo = m.Tipo,
            Descripcion = m.Descripcion,
            Estado = m.Estado,
            Prioridad = m.Prioridad,
            FechaProgramada = m.FechaProgramada,
            FechaRealizacion = m.FechaRealizacion,
            Costo = m.Costo,
            RealizadoPor = m.RealizadoPor,
            Disponibilizado = m.Disponibilizado,
        };
    }
}