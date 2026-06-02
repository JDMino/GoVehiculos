namespace GoVehiculos.API.Observers
{
    /// <summary>
    /// PATRÓN OBSERVADOR — Interfaz Observador.
    ///
    /// Define el contrato que deben cumplir todos los observadores
    /// del evento "multa creada". Cada observador concreto recibe
    /// al sujeto como parámetro y consulta su estado directamente,
    /// respetando la estructura canónica del patrón Observer.
    ///
    /// El Sujeto (MultaService) no conoce los tipos concretos de sus
    /// observadores, solo esta interfaz. Eso permite agregar nuevos
    /// efectos secundarios creando un nuevo observador y registrándolo
    /// en Program.cs, sin modificar el servicio (Open/Closed Principle).
    /// </summary>
    public interface IMultaObserver
    {
        /// <summary>
        /// Notifica al observador que se creó una multa.
        /// El observador recibe al sujeto y consulta su estado
        /// para decidir de forma autónoma si debe actuar.
        /// </summary>
        /// <param name="multaAbs">
        ///   El sujeto que disparó el evento. Los observadores
        ///   concretos hacen cast a MultaService para acceder
        ///   a TipoIncidencia, TipoPenalizacion, VehiculoId y UsuarioId.
        /// </param>
        Task ActualizarAsync(MultaAbs multaAbs);
    }
}
