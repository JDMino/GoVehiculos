using GoVehiculos.API.Models;

namespace GoVehiculos.API.Strategies
{
    /// <summary>
    /// Estrategia concreta que encapsula el algoritmo de iniciar una orden
    /// de mantenimiento.
    ///
    /// PATRÓN STRATEGY — Algoritmo concreto:
    /// Implementa IAccionMantenimientoStrategy definiendo exactamente qué significa
    /// "iniciar" una orden: verificar permiso, validar que esté en estado pendiente,
    /// y cambiar el estado a iniciado.
    ///
    /// Al estar aislada en su propia clase, esta lógica puede modificarse, testearse
    /// o reemplazarse sin afectar al service ni a las demás estrategias.
    /// </summary>
    public class IniciarStrategy : IAccionMantenimientoStrategy
    {
        /// <param name="mantenimiento">Orden sobre la que se aplica la acción.</param>
        /// <param name="empleadoId">Empleado que solicita iniciar.</param>
        /// <param name="contexto">No se usa en esta estrategia (IniciarDTO está vacío).</param>
        public Task<(bool exito, string mensaje)> EjecutarAsync(
            Mantenimiento mantenimiento,
            int empleadoId,
            object? contexto = null)
        {
            // Verificación de permiso — el empleado debe ser el asignado a la orden
            if (mantenimiento.EmpleadoId != empleadoId)
                return Task.FromResult((false, "No tenés permiso para operar este mantenimiento."));

            // Validación de estado — solo se puede iniciar desde pendiente
            if (mantenimiento.Estado != "pendiente")
                return Task.FromResult((false,
                    $"El mantenimiento no puede iniciarse porque está en estado '{mantenimiento.Estado}'."));

            // Aplicación del cambio de estado
            mantenimiento.Estado = "iniciado";

            return Task.FromResult((true, "Mantenimiento iniciado correctamente."));
        }
    }
}
