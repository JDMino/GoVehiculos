using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;

namespace GoVehiculos.API.Observers
{
    /// <summary>
    /// PATRÓN OBSERVADOR — Observador concreto.
    ///
    /// Reacciona al evento "multa creada" cuando la penalización es de tipo
    /// "bloqueo_cuenta". En ese caso, busca el usuario involucrado en la BD
    /// y establece su campo Bloqueado = true.
    ///
    /// Respeta la estructura canónica del patrón Observer: recibe al sujeto
    /// como parámetro en ActualizarAsync, hace cast a MultaService para
    /// consultar su estado, y decide de forma autónoma si debe actuar.
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

        public async Task ActualizarAsync(MultaAbs multaAbs)
        {
            // Cast al sujeto concreto para consultar su estado
            if (multaAbs is not MultaService multaService) return;

            // Este observador solo actúa ante penalizaciones de bloqueo de cuenta
            if (multaService.TipoPenalizacion != "bloqueo_cuenta") return;

            var usuario = await _usuarioRepo
                .GetByIdSimpleAsync(multaService.UsuarioId);
            if (usuario == null) return;

            usuario.Bloqueado = true;
            await _usuarioRepo.SaveChangesAsync();
        }
    }
}
