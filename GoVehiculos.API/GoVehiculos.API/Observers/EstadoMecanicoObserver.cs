using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Observers
{
    /// <summary>
    /// PATRÓN OBSERVADOR — Observador concreto.
    ///
    /// Reacciona al evento "multa creada" cuando la incidencia es de tipo
    /// "daño_fisico". En ese caso, busca el vehículo involucrado en la BD
    /// y actualiza su EstadoMecanico a "malo".
    ///
    /// Si el tipo de incidencia es cualquier otro, no hace nada.
    /// Esta decisión la toma el propio observador, sin que el Sujeto
    /// (MultaService) deba conocerla ni condicionarla.
    /// </summary>
    public class EstadoMecanicoObserver : IMultaObserver
    {
        private readonly IVehiculoRepository _vehiculoRepo;

        public EstadoMecanicoObserver(IVehiculoRepository vehiculoRepo)
        {
            _vehiculoRepo = vehiculoRepo;
        }

        public async Task ActualizarAsync(
            string tipoIncidencia,
            string tipoPenalizacion,
            int vehiculoId,
            int usuarioId)
        {
            // Este observador solo actúa ante daños físicos
            if (tipoIncidencia != "daño_fisico") return;

            var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(vehiculoId);
            if (vehiculo == null) return;

            vehiculo.EstadoMecanico = "malo";
            await _vehiculoRepo.SaveChangesAsync();
        }
    }
}