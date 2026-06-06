using GoVehiculos.API.Models;

namespace GoVehiculos.API.Strategies
{
    public class IniciarStrategy : IAccionMantenimientoStrategy
    {
        public bool NecesitaVehiculo => false;

        public Task<(bool exito, string mensaje)> EjecutarAsync(
            Mantenimiento mantenimiento,
            int empleadoId,
            object? contexto = null)
        {
            if (mantenimiento.EmpleadoId != empleadoId)
                return Task.FromResult((false, "No tenés permiso para operar este mantenimiento."));

            if (mantenimiento.Estado != "pendiente")
                return Task.FromResult((false,
                    $"El mantenimiento no puede iniciarse porque está en estado '{mantenimiento.Estado}'."));

            mantenimiento.Estado = "iniciado";

            return Task.FromResult((true, "Mantenimiento iniciado correctamente."));
        }
    }
}