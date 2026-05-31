namespace GoVehiculos.API.Observers
{
    /// <summary>
    /// PATRÓN OBSERVADOR — Interfaz Observador.
    ///
    /// Define el contrato que deben cumplir todos los observadores
    /// del evento "multa creada". Cada observador concreto encapsula
    /// un único efecto secundario y decide de forma autónoma si debe
    /// actuar según los datos del evento recibido.
    ///
    /// El Sujeto (MultaService) no conoce los tipos concretos de sus
    /// observadores, solo esta interfaz. Eso permite agregar nuevos
    /// efectos secundarios creando un nuevo observador y registrándolo
    /// en Program.cs, sin modificar el servicio (Open/Closed Principle).
    /// </summary>
    public interface IMultaObserver
    {
        /// <summary>
        /// Notifica al observador que se creó una multa completa.
        /// Cada implementación evalúa si debe actuar según los tipos recibidos
        /// y aplica su efecto secundario de forma independiente.
        /// </summary>
        /// <param name="tipoIncidencia">
        ///   Tipo de la incidencia creada. Algunos observadores reaccionan
        ///   solo ante ciertos tipos (ej: "daño_fisico").
        /// </param>
        /// <param name="tipoPenalizacion">
        ///   Tipo de la penalización creada. Algunos observadores reaccionan
        ///   solo ante ciertos tipos (ej: "bloqueo_cuenta").
        /// </param>
        /// <param name="vehiculoId">ID del vehículo involucrado en la incidencia.</param>
        /// <param name="usuarioId">ID del usuario involucrado en la incidencia.</param>
        Task ActualizarAsync(
            string tipoIncidencia,
            string tipoPenalizacion,
            int vehiculoId,
            int usuarioId);
    }
}