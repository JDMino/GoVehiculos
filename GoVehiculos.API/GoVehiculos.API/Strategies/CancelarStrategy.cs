using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;

namespace GoVehiculos.API.Strategies
{
    /// <summary>
    /// Estrategia concreta que encapsula el algoritmo de cancelar una orden
    /// de mantenimiento.
    ///
    /// PATRÓN STRATEGY — Algoritmo concreto:
    /// "Cancelar" comparte con Iniciar y Finalizar la misma estructura general
    /// (verificar permiso → validar estado → aplicar cambios), pero su lógica
    /// específica es distinta: solo acepta órdenes iniciadas y actualiza la
    /// descripción con el motivo de cancelación.
    /// Al estar encapsulada, esta variante no interfiere con las demás.
    /// </summary>
    public class CancelarStrategy : IAccionMantenimientoStrategy
    {
        /// <param name="mantenimiento">Orden sobre la que se aplica la cancelación.</param>
        /// <param name="empleadoId">Empleado que solicita cancelar.</param>
        /// <param name="contexto">Se espera un MantenimientoCancelarDTO con el motivo.</param>
        public Task<(bool exito, string mensaje)> EjecutarAsync(
            Mantenimiento mantenimiento,
            int empleadoId,
            object? contexto = null)
        {
            // Verificación de permiso
            if (mantenimiento.EmpleadoId != empleadoId)
                return Task.FromResult((false, "No tenés permiso para operar este mantenimiento."));

            // Validación de estado — solo se puede cancelar desde iniciado
            if (mantenimiento.Estado != "iniciado")
                return Task.FromResult((false,
                    $"El mantenimiento no puede cancelarse porque está en estado '{mantenimiento.Estado}'."));

            // Casting del contexto al DTO específico de esta estrategia
            if (contexto is not MantenimientoCancelarDTO dto)
                return Task.FromResult((false, "Datos de cancelación inválidos."));

            // Aplicación de cambios: el empleado actualiza la descripción con el motivo
            mantenimiento.Descripcion = dto.Descripcion;
            mantenimiento.Estado      = "cancelado";

            return Task.FromResult((true, "Mantenimiento cancelado."));
        }
    }
}
