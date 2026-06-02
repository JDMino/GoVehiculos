namespace GoVehiculos.API.Observers
{
    /// <summary>
    /// PATRÓN OBSERVADOR — Clase MultaAbs abstracta.
    ///
    /// Define la estructura base que toda multa concreto debe heredar.
    /// Encapsula la lista de observadores registrados y los métodos
    /// para registrarlos, eliminarlos y notificarlos.
    ///
    /// Al ser abstracta, obliga a que sea MultaService quien la herede
    /// y concrete el comportamiento específico del dominio, mientras
    /// esta clase se ocupa exclusivamente de la mecánica del patrón.
    ///
    /// NotificarAsync es protected para que solo el sujeto concreto
    /// pueda disparar la notificación, nunca desde afuera.
    /// </summary>
    public abstract class MultaAbs
    {
        private readonly List<IMultaObserver> _observadores = new();

        /// <summary>
        /// Registra un nuevo observador en la lista de suscriptores.
        /// </summary>
        public void Registrar(IMultaObserver observador)
            => _observadores.Add(observador);

        /// <summary>
        /// Elimina un observador de la lista de suscriptores.
        /// </summary>
        public void Eliminar(IMultaObserver observador)
            => _observadores.Remove(observador);

        /// <summary>
        /// Notifica a todos los observadores registrados pasándose
        /// a sí mismo como parámetro, para que cada observador
        /// consulte el estado que necesite directamente del sujeto.
        /// </summary>
        protected async Task NotificarAsync()
        {
            foreach (var observador in _observadores)
                await observador.ActualizarAsync(this);
        }
    }
}
