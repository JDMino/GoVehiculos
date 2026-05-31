using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos editables de una Penalización existente.
    ///
    /// PenalizacionEstado admite solo "activa" o "cumplida" desde este DTO.
    /// El estado "revocada" se asigna exclusivamente al cancelar la multa
    /// asociada mediante PATCH /api/multas/{id}/cancelar, nunca por edición directa.
    /// Esto garantiza que la revocación de una penalización siempre esté
    /// acompañada de la cancelación de su multa padre.
    ///
    /// MultaId no es editable: reasignar una penalización a otra multa
    /// sería una operación sin sentido de negocio.
    /// </summary>
    public class PenalizacionUpdateDTO
    {
        [Required(ErrorMessage = "El tipo de penalización es obligatorio.")]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [MaxLength(255)]
        public string Motivo { get; set; } = string.Empty;

        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Valores válidos desde este DTO: activa | cumplida
        /// </summary>
        [Required(ErrorMessage = "El estado de la penalización es obligatorio.")]
        [MaxLength(50)]
        public string Estado { get; set; } = string.Empty;
    }
}