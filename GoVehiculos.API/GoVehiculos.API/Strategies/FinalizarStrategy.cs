using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;

namespace GoVehiculos.API.Strategies
{
    /// <summary>
    /// Estrategia concreta que encapsula el algoritmo de finalizar una orden
    /// de mantenimiento.
    ///
    /// PATRÓN STRATEGY — Algoritmo concreto:
    /// Es la estrategia más compleja del conjunto porque "finalizar" implica
    /// validar múltiples campos del DTO, una regla de negocio sobre fechas,
    /// y además actualizar el estado mecánico del vehículo asociado.
    /// Toda esa complejidad queda contenida aquí, sin contaminar al service
    /// ni a las demás estrategias.
    ///
    /// Nota sobre el repositorio:
    /// Esta estrategia recibe la entidad ya cargada con Include(Vehiculo)
    /// desde el service (usando GetByIdConVehiculoAsync), porque necesita
    /// acceder a la navegación Vehiculo para actualizar EstadoMecanico.
    /// La responsabilidad de qué query usar sigue siendo del service (contexto),
    /// no de la estrategia — esto respeta la separación de responsabilidades.
    /// </summary>
    public class FinalizarStrategy : IAccionMantenimientoStrategy
    {
        /// <param name="mantenimiento">Orden cargada con navegación a Vehiculo.</param>
        /// <param name="empleadoId">Empleado que solicita finalizar.</param>
        /// <param name="contexto">Se espera un MantenimientoFinalizarDTO con los datos del cierre.</param>
        public Task<(bool exito, string mensaje)> EjecutarAsync(
            Mantenimiento mantenimiento,
            int empleadoId,
            object? contexto = null)
        {
            // Verificación de permiso
            if (mantenimiento.EmpleadoId != empleadoId)
                return Task.FromResult((false, "No tenés permiso para operar este mantenimiento."));

            // Validación de estado — solo se puede finalizar desde iniciado
            if (mantenimiento.Estado != "iniciado")
                return Task.FromResult((false,
                    $"El mantenimiento no puede finalizarse porque está en estado '{mantenimiento.Estado}'."));

            // Casting del contexto al DTO específico de esta estrategia.
            // El service es responsable de pasar el tipo correcto.
            if (contexto is not MantenimientoFinalizarDTO dto)
                return Task.FromResult((false, "Datos de finalización inválidos."));

            // Regla de negocio: la fecha de realización no puede ser anterior a la programada
            if (mantenimiento.FechaProgramada.HasValue && dto.FechaRealizacion < mantenimiento.FechaProgramada.Value)
                return Task.FromResult((false,
                    $"La fecha de realización no puede ser anterior a la fecha programada " +
                    $"({mantenimiento.FechaProgramada.Value:dd/MM/yyyy})."));

            // Aplicación de cambios sobre la entidad
            mantenimiento.Descripcion      = dto.Descripcion;
            mantenimiento.FechaRealizacion = dto.FechaRealizacion;
            mantenimiento.Costo            = dto.Costo;
            mantenimiento.RealizadoPor     = dto.RealizadoPor;
            mantenimiento.Estado           = "finalizado";

            // Efecto secundario sobre el vehículo: al finalizar, su estado mecánico mejora
            if (mantenimiento.Vehiculo != null)
                mantenimiento.Vehiculo.EstadoMecanico = "bueno";

            return Task.FromResult((true, "Mantenimiento finalizado correctamente."));
        }
    }
}
