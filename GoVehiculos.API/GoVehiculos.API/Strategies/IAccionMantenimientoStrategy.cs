using GoVehiculos.API.Models;

namespace GoVehiculos.API.Strategies
{
    public interface IAccionMantenimientoStrategy
    {
        /// <summary>
        /// Indica si la estrategia necesita que la entidad Mantenimiento
        /// venga cargada con su navegación a Vehiculo.
        /// El service usa esto para decidir qué query ejecutar,
        /// sin necesidad de conocer el tipo concreto de estrategia.
        /// </summary>
        bool NecesitaVehiculo { get; }

        Task<(bool exito, string mensaje)> EjecutarAsync(
            Mantenimiento mantenimiento,
            int empleadoId,
            object? contexto = null);
    }
}