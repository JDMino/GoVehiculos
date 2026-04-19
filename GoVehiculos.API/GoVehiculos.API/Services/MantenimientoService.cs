using GoVehiculos.API.Data;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class MantenimientoService
    {
        private readonly ApplicationDbContext _context;

        // Estados que se consideran "activos" — bloquean generar una nueva orden
        private static readonly string[] EstadosActivos = ["pendiente", "en_proceso"];

        public MantenimientoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // Vehículos candidatos para mantenimiento
        // Condición: activos con estadoMecanico "regular" o "malo"
        // Enriquecidos con su mantenimiento activo si existe
        // ============================
        public async Task<IEnumerable<VehiculoCandidatoDTO>> GetVehiculosCandidatosAsync()
        {
            // Traer vehículos candidatos con sus relaciones
            var vehiculos = await _context.Vehiculos
                .Include(v => v.Modelo)
                    .ThenInclude(m => m.Marca)
                .Include(v => v.UbicacionActual)
                .Where(v =>
                    v.Activo &&
                    (v.EstadoMecanico == "regular" || v.EstadoMecanico == "malo"))
                .ToListAsync();

            // Obtener los ids para buscar mantenimientos activos en una sola consulta
            var vehiculoIds = vehiculos.Select(v => v.IdVehiculo).ToList();

            var mantenimientosActivos = await _context.Mantenimientos
                .Include(m => m.Empleado)
                .Where(m =>
                    vehiculoIds.Contains(m.VehiculoId) &&
                    EstadosActivos.Contains(m.Estado))
                // Si un vehículo tuviera más de uno, tomamos el más reciente
                .OrderByDescending(m => m.IdMantenimiento)
                .ToListAsync();

            // Indexar por vehiculoId para lookup O(1)
            var mantenimientoPorVehiculo = mantenimientosActivos
                .GroupBy(m => m.VehiculoId)
                .ToDictionary(g => g.Key, g => g.First());

            return vehiculos.Select(v =>
            {
                mantenimientoPorVehiculo.TryGetValue(v.IdVehiculo, out var mant);

                return new VehiculoCandidatoDTO
                {
                    IdVehiculo = v.IdVehiculo,
                    SocioId = v.SocioId,
                    Patente = v.Patente,
                    Anio = v.Anio,
                    Tipo = v.Tipo,
                    Estado = v.Estado,
                    EstadoMecanico = v.EstadoMecanico,
                    Kilometraje = v.Kilometraje,
                    PrecioPorDia = v.PrecioPorDia,
                    MantenimientoACargoDe = v.MantenimientoACargoDe,
                    SeguroVigente = v.SeguroVigente,
                    DocumentacionVigente = v.DocumentacionVigente,
                    Activo = v.Activo,
                    ImagenUrl = v.ImagenUrl,
                    ModeloId = v.ModeloId,
                    ModeloNombre = v.Modelo?.Nombre ?? string.Empty,
                    MarcaNombre = v.Modelo?.Marca?.Nombre ?? string.Empty,
                    UbicacionActualId = v.UbicacionActualId,
                    UbicacionNombre = v.UbicacionActual?.Nombre,

                    MantenimientoActivo = mant == null ? null : new MantenimientoActivoDTO
                    {
                        IdMantenimiento = mant.IdMantenimiento,
                        Estado = mant.Estado,
                        Prioridad = mant.Prioridad,
                        Tipo = mant.Tipo,
                        EmpleadoId = mant.EmpleadoId,
                        EmpleadoNombre = mant.Empleado != null
                                              ? $"{mant.Empleado.Nombre} {mant.Empleado.Apellido}"
                                              : null,
                        FechaProgramada = mant.FechaProgramada,
                    }
                };
            });
        }

        public async Task<IEnumerable<MantenimientoResponseDTO>> GetAllAsync()
        {
            return await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                    .ThenInclude(v => v.Modelo)
                        .ThenInclude(mo => mo.Marca)
                .Include(m => m.Empleado)
                .Select(m => ToResponseDTO(m))
                .ToListAsync();
        }

        public async Task<MantenimientoResponseDTO?> GetByIdAsync(int id)
        {
            var m = await _context.Mantenimientos
                .Include(m => m.Vehiculo)
                    .ThenInclude(v => v.Modelo)
                        .ThenInclude(mo => mo.Marca)
                .Include(m => m.Empleado)
                .FirstOrDefaultAsync(m => m.IdMantenimiento == id);

            return m == null ? null : ToResponseDTO(m);
        }

        // ============================
        // Crear orden de mantenimiento
        // Reglas de negocio:
        //   - Solo candidatos con estadoMecanico "regular" o "malo"
        //   - Si ya tiene un mantenimiento activo → no se puede crear otra orden
        //   - Si MantenimientoACargoDe == "socio" → vehículo pasa a "fuera_de_servicio", sin orden
        //   - Si MantenimientoACargoDe == "empresa" → orden creada, vehículo pasa a "mantenimiento"
        // ============================
        public async Task<(bool exito, string mensaje, MantenimientoResponseDTO? dto)> CreateAsync(MantenimientoCreateDTO dto)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);

            if (vehiculo == null)
                return (false, "Vehículo no encontrado.", null);

            if (vehiculo.EstadoMecanico != "regular" && vehiculo.EstadoMecanico != "malo")
                return (false, "El vehículo no requiere mantenimiento según su estado mecánico.", null);

            // Verificar si ya tiene una orden activa
            var tieneActivo = await _context.Mantenimientos
                .AnyAsync(m =>
                    m.VehiculoId == dto.VehiculoId &&
                    EstadosActivos.Contains(m.Estado));

            if (tieneActivo)
                return (false, "El vehículo ya tiene una orden de mantenimiento activa.", null);

            // Regla: socio se hace cargo → fuera_de_servicio, sin orden
            if (vehiculo.MantenimientoACargoDe == "socio")
            {
                vehiculo.Estado = "fuera_de_servicio";
                await _context.SaveChangesAsync();
                return (false,
                    "El mantenimiento de este vehículo está a cargo del socio. El vehículo ha pasado a estado 'fuera de servicio'.",
                    null);
            }

            // Crear orden
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
                RealizadoPor = string.Empty
            };

            // Vehículo pasa a "mantenimiento" de manera fija
            vehiculo.Estado = "mantenimiento";

            _context.Mantenimientos.Add(mantenimiento);
            await _context.SaveChangesAsync();

            var result = await GetByIdAsync(mantenimiento.IdMantenimiento);
            return (true, "Orden de mantenimiento creada correctamente.", result);
        }

        public async Task<bool> UpdateAsync(int id, MantenimientoUpdateDTO dto)
        {
            var existing = await _context.Mantenimientos.FindAsync(id);
            if (existing == null) return false;

            existing.Estado = dto.Estado;
            existing.Prioridad = dto.Prioridad;
            existing.FechaProgramada = dto.FechaProgramada;
            existing.FechaRealizacion = dto.FechaRealizacion;
            existing.Costo = dto.Costo;
            existing.RealizadoPor = dto.RealizadoPor;
            existing.Descripcion = dto.Descripcion;
            existing.EmpleadoId = dto.EmpleadoId;

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

        private static MantenimientoResponseDTO ToResponseDTO(Mantenimiento m) => new()
        {
            IdMantenimiento = m.IdMantenimiento,
            VehiculoId = m.VehiculoId,
            VehiculoPatente = m.Vehiculo?.Patente ?? string.Empty,
            VehiculoMarca = m.Vehiculo?.Modelo?.Marca?.Nombre ?? string.Empty,
            VehiculoModelo = m.Vehiculo?.Modelo?.Nombre ?? string.Empty,
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
            RealizadoPor = m.RealizadoPor
        };
    }
}