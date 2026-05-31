using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos que el administrador completa para la penalización operativa
    /// vinculada a una Multa.
    ///
    /// Tipos válidos para Tipo y sus efectos secundarios automáticos:
    ///   suspension_temporal     — registro formal; sin efecto automático en otras entidades.
    ///   bloqueo_cuenta          — establece Usuario.Bloqueado = true al persistir.
    ///   inhabilitacion_vehiculo — establece Vehiculo.Estado = "fuera_de_servicio" al persistir.
    ///   advertencia             — registro formal sin acción inmediata.
    ///
    /// Estado siempre "activa" al crear — fijado en el Builder,
    /// no depende del frontend.
    ///
    /// MultaId no forma parte de este DTO: lo asigna el Director
    /// del Builder una vez que la Multa fue persistida y tiene ID.
    /// </summary>
    public class PenalizacionCreateDTO
    {
        [Required(ErrorMessage = "El tipo de penalización es obligatorio.")]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [MaxLength(255)]
        public string Motivo { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de fin opcional. Null indica penalización indefinida.
        /// Solo aplica para "suspension_temporal"; para los demás tipos
        /// el administrador puede dejarlo vacío.
        /// </summary>
        public DateTime? FechaFin { get; set; }
    }
}