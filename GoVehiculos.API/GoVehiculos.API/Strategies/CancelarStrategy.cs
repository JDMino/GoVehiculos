using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;

namespace GoVehiculos.API.Strategies
{
    public class CancelarStrategy : IAccionMantenimientoStrategy
    {
        public bool NecesitaVehiculo => false;

        public Task<(bool exito, string mensaje)> EjecutarAsync(
            Mantenimiento mantenimiento,
            int empleadoId,
            object? contexto = null)
        {
            if (mantenimiento.EmpleadoId != empleadoId)
                return Task.FromResult((false, "No tenés permiso para operar este mantenimiento."));

            if (mantenimiento.Estado != "iniciado")
                return Task.FromResult((false,
                    $"El mantenimiento no puede cancelarse porque está en estado '{mantenimiento.Estado}'."));

            if (contexto is not MantenimientoCancelarDTO dto)
                return Task.FromResult((false, "Datos de cancelación inválidos."));

            var errorCampos = ValidarCampos(dto);
            if (errorCampos != null)
                return Task.FromResult((false, errorCampos));

            mantenimiento.Descripcion = dto.Descripcion;
            mantenimiento.Estado = "cancelado";

            return Task.FromResult((true, "Mantenimiento cancelado."));
        }

        private static string? ValidarCampos(MantenimientoCancelarDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Descripcion))
                return "Debe indicar el motivo de cancelación.";
            return null;
        }
    }
}