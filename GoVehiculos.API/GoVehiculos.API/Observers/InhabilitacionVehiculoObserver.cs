using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Observers
{
    /// <summary>
    /// PATRÓN OBSERVADOR — Observador concreto.
    ///
    /// Reacciona al evento "multa creada" cuando la penalización es de tipo
    /// "inhabilitacion_vehiculo". En ese caso, busca el vehículo involucrado
    /// en la BD y establece su Estado = "fuera_de_servicio".
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

        public async Task ActualizarAsync(
            string tipoIncidencia,
            string tipoPenalizacion,
            int vehiculoId,
            int usuarioId)
        {
            // Este observador solo actúa ante penalizaciones de inhabilitación
            if (tipoPenalizacion != "inhabilitacion_vehiculo") return;

            var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(vehiculoId);
            if (vehiculo == null) return;

            vehiculo.Estado = "fuera_de_servicio";
            await _vehiculoRepo.SaveChangesAsync();
        }
    }
}