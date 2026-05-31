using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Observers
{
    /// <summary>
    /// PATRÓN OBSERVADOR — Observador concreto.
    ///
    /// Reacciona al evento "multa creada" cuando la penalización es de tipo
    /// "bloqueo_cuenta". En ese caso, busca el usuario involucrado en la BD
    /// y establece su campo Bloqueado = true.
    ///
    /// Si el tipo de penalización es cualquier otro, no hace nada.
    /// </summary>
    public class BloqueoUsuarioObserver : IMultaObserver
    {
        private readonly IUsuarioRepository _usuarioRepo;

        public BloqueoUsuarioObserver(IUsuarioRepository usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        public async Task ActualizarAsync(
            string tipoIncidencia,
            string tipoPenalizacion,
            int vehiculoId,
            int usuarioId)
        {
            // Este observador solo actúa ante penalizaciones de bloqueo de cuenta
            if (tipoPenalizacion != "bloqueo_cuenta") return;

            var usuario = await _usuarioRepo.GetByIdSimpleAsync(usuarioId);
            if (usuario == null) return;

            usuario.Bloqueado = true;
            await _usuarioRepo.SaveChangesAsync();
        }
    }
}