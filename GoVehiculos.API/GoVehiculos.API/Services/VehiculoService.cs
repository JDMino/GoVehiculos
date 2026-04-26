using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Services
{
    public class VehiculoService
    {
        private readonly IVehiculoRepository _repo;
        private readonly IMantenimientoRepository _mantenimientoRepo;

        public VehiculoService(IVehiculoRepository repo, IMantenimientoRepository mantenimientoRepo)
        {
            _repo = repo;
            _mantenimientoRepo = mantenimientoRepo;
        }

        // ================================================================
        // CONTADOR admin (protagonista: Vehiculos)
        // ================================================================

        public Task<int> GetContadorAdminAsync()
            => _repo.ContarCandidatosAsync();

        // ================================================================
        // CANDIDATOS A MANTENIMIENTO (protagonista: Vehiculos)
        // ================================================================

        public async Task<IEnumerable<VehiculoCandidatoDTO>> GetVehiculosCandidatosAsync()
        {
            var vehiculos = await _repo.GetCandidatosAsync();
            var ids = vehiculos.Select(v => v.IdVehiculo).ToList();

            var mantenimientosActivos = await _mantenimientoRepo.GetActivosPorVehiculosAsync(ids);

            var mantPorVehiculo = mantenimientosActivos
                .GroupBy(m => m.VehiculoId)
                .ToDictionary(g => g.Key, g => g.First());

            return vehiculos.Select(v =>
            {
                mantPorVehiculo.TryGetValue(v.IdVehiculo, out var mant);
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

        // ================================================================
        // CRUD general
        // ================================================================

        public async Task<IEnumerable<VehiculoResponseDTO>> GetAllAsync(string? estado = null, string? marca = null)
        {
            var lista = await _repo.GetAllAsync(estado, marca);
            return lista.Select(ToResponseDTO);
        }

        public async Task<VehiculoResponseDTO?> GetByIdAsync(int id)
        {
            var v = await _repo.GetByIdAsync(id);
            return v == null ? null : ToResponseDTO(v);
        }

        public async Task<VehiculoResponseDTO> CreateAsync(VehiculoCreateDTO dto)
        {
            var v = new Vehiculo
            {
                SocioId = dto.SocioId,
                Tipo = dto.Tipo,
                ModeloId = dto.ModeloId,
                Anio = dto.Anio,
                Patente = dto.Patente,
                Estado = dto.Estado,
                EstadoMecanico = dto.EstadoMecanico,
                Kilometraje = dto.Kilometraje,
                PrecioPorDia = dto.PrecioPorDia,
                UbicacionActualId = dto.UbicacionActualId,
                SeguroVigente = dto.SeguroVigente,
                DocumentacionVigente = dto.DocumentacionVigente,
                MantenimientoACargoDe = dto.MantenimientoACargoDe,
                ImagenUrl = dto.ImagenUrl,
                Activo = true
            };

            await _repo.AddAsync(v);
            await _repo.SaveChangesAsync();

            return await GetByIdAsync(v.IdVehiculo) ?? new VehiculoResponseDTO();
        }

        public async Task<bool> UpdateAsync(int id, VehiculoUpdateDTO dto)
        {
            var v = await _repo.GetByIdSimpleAsync(id);
            if (v == null) return false;

            v.Estado = dto.Estado;
            v.EstadoMecanico = dto.EstadoMecanico;
            v.Kilometraje = dto.Kilometraje;
            v.PrecioPorDia = dto.PrecioPorDia;
            v.SeguroVigente = dto.SeguroVigente;
            v.DocumentacionVigente = dto.DocumentacionVigente;
            v.Activo = dto.Activo;
            v.ModeloId = dto.ModeloId;
            v.UbicacionActualId = dto.UbicacionActualId;

            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<(bool exito, string mensaje)> PasarAFueraDeServicioAsync(int vehiculoId)
        {
            var v = await _repo.GetByIdSimpleAsync(vehiculoId);
            if (v == null) return (false, "Vehículo no encontrado.");
            if (v.MantenimientoACargoDe != "socio") return (false, "Solo aplica a vehículos de socio.");

            v.Estado = "fuera_de_servicio";
            await _repo.SaveChangesAsync();
            return (true, "Vehículo pasado a fuera de servicio.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var v = await _repo.GetByIdSimpleAsync(id);
            if (v == null) return false;

            v.Activo = false;
            await _repo.SaveChangesAsync();
            return true;
        }

        // ================================================================
        // Mapeo privado
        // ================================================================

        private static VehiculoResponseDTO ToResponseDTO(Vehiculo v) => new()
        {
            IdVehiculo = v.IdVehiculo,
            SocioId = v.SocioId,
            Tipo = v.Tipo,
            Anio = v.Anio,
            Patente = v.Patente,
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
            UbicacionNombre = v.UbicacionActual?.Nombre
        };
    }
}