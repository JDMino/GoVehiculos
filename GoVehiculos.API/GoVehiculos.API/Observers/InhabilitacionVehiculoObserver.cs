using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;

namespace GoVehiculos.API.Observers
{
    /// <summary>
    /// PATRÓN OBSERVADOR — Observador concreto.
    ///
    /// Reacciona al evento "multa creada" cuando la penalización es de tipo
    /// "inhabilitacion_vehiculo". En ese caso, busca el vehículo involucrado
    /// en la BD y establece su Estado = "fuera_de_servicio".
    ///
    /// Respeta la estructura canónica del patrón Observer: recibe al sujeto
    /// como parámetro en ActualizarAsync, hace cast a MultaService para
    /// consultar su estado, y decide de forma autónoma si debe actuar.
    ///
    /// Si el tipo de penalización es cualquier otro, no hace nada.
    ///
    /// Nota: este observador y EstadoMecanicoObserver pueden actuar sobre
    /// el mismo vehículo en la misma operación (si la incidencia es "daño_fisico"
    /// y la penalización es "inhabilitacion_vehiculo"). Cada uno opera sobre
    /// un campo distinto (EstadoMecanico y Estado respectivamente), por lo que
    /// no hay conflicto entre ellos.
    /// </summary>
    public class InhabilitacionVehiculoObserver : IMultaObserver
    {
        private readonly IVehiculoRepository _vehiculoRepo;

        public InhabilitacionVehiculoObserver(IVehiculoRepository vehiculoRepo)
        {
            _vehiculoRepo = vehiculoRepo;
        }

        public async Task ActualizarAsync(MultaAbs multaAbs)
        {
            // Cast al sujeto concreto para consultar su estado
            if (multaAbs is not MultaService multaService) return;

            // Este observador solo actúa ante penalizaciones de inhabilitación
            if (multaService.TipoPenalizacion != "inhabilitacion_vehiculo") return;

            var vehiculo = await _vehiculoRepo
                .GetByIdSimpleAsync(multaService.VehiculoId);
            if (vehiculo == null) return;

            vehiculo.Estado = "fuera_de_servicio";
            await _vehiculoRepo.SaveChangesAsync();
        }
    }
}
