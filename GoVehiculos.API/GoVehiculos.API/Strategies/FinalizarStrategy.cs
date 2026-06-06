using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;

namespace GoVehiculos.API.Strategies
{
    public class FinalizarStrategy : IAccionMantenimientoStrategy
    {
        public bool NecesitaVehiculo => true;

        public Task<(bool exito, string mensaje)> EjecutarAsync(
            Mantenimiento mantenimiento,
            int empleadoId,
            object? contexto = null)
        {
            if (mantenimiento.EmpleadoId != empleadoId)
                return Task.FromResult((false, "No tenés permiso para operar este mantenimiento."));

            if (mantenimiento.Estado != "iniciado")
                return Task.FromResult((false,
                    $"El mantenimiento no puede finalizarse porque está en estado '{mantenimiento.Estado}'."));

            if (contexto is not MantenimientoFinalizarDTO dto)
                return Task.FromResult((false, "Datos de finalización inválidos."));

            var errorCampos = ValidarCampos(dto);
            if (errorCampos != null)
                return Task.FromResult((false, errorCampos));

            if (mantenimiento.FechaProgramada.HasValue && dto.FechaRealizacion < mantenimiento.FechaProgramada.Value)
                return Task.FromResult((false,
                    $"La fecha de realización no puede ser anterior a la fecha programada " +
                    $"({mantenimiento.FechaProgramada.Value:dd/MM/yyyy})."));

            mantenimiento.Descripcion = dto.Descripcion;
            mantenimiento.FechaRealizacion = dto.FechaRealizacion;
            mantenimiento.Costo = dto.Costo;
            mantenimiento.RealizadoPor = dto.RealizadoPor;
            mantenimiento.Estado = "finalizado";

            if (mantenimiento.Vehiculo != null)
                mantenimiento.Vehiculo.EstadoMecanico = "bueno";

            return Task.FromResult((true, "Mantenimiento finalizado correctamente."));
        }

        private static string? ValidarCampos(MantenimientoFinalizarDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Descripcion))
                return "La descripción es obligatoria.";
            if (string.IsNullOrWhiteSpace(dto.RealizadoPor))
                return "Debe indicar quién realizó el trabajo.";
            if (dto.FechaRealizacion == default)
                return "La fecha de realización es obligatoria.";
            if (dto.Costo < 0)
                return "El costo no puede ser negativo.";
            return null;
        }
    }
}