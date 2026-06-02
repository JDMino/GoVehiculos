using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;

namespace GoVehiculos.API.Observers
{
    /// <summary>
    /// PATRÓN OBSERVADOR — Observador concreto.
    ///
    /// Reacciona al evento "multa creada" cuando la incidencia es de tipo
    /// "daño_fisico". En ese caso, busca el vehículo involucrado en la BD
    /// y actualiza su EstadoMecanico a "malo".
    ///
    /// Respeta la estructura canónica del patrón Observer: recibe al sujeto
    /// como parámetro en ActualizarAsync, hace cast a MultaService para
    /// consultar su estado, y decide de forma autónoma si debe actuar.
    ///
    /// Si el tipo de incidencia es cualquier otro, no hace nada.
    /// </summary>
    public class EstadoMecanicoObserver : IMultaObserver
    {
        private readonly IVehiculoRepository _vehiculoRepo;

        public EstadoMecanicoObserver(IVehiculoRepository vehiculoRepo)
        {
            _vehiculoRepo = vehiculoRepo;
        }

        public async Task ActualizarAsync(MultaAbs multaAbs)
        {
            // Cast al sujeto concreto para consultar su estado
            if (multaAbs is not MultaService multaService) return;

            // Este observador solo actúa ante daños físicos
            if (multaService.TipoIncidencia != "daño_fisico") return;

            var vehiculo = await _vehiculoRepo
                .GetByIdSimpleAsync(multaService.VehiculoId);
            if (vehiculo == null) return;

            vehiculo.EstadoMecanico = "malo";
            await _vehiculoRepo.SaveChangesAsync();
        }
    }
}
