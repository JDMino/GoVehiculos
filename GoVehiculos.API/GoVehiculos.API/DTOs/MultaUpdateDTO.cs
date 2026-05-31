using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos editables de una Multa existente.
    ///
    /// MultaEstado admite solo "pendiente" o "pagada" desde este DTO.
    /// El estado "cancelada" se asigna exclusivamente mediante el endpoint
    /// dedicado PATCH /api/multas/{id}/cancelar, nunca por edición directa.
    /// Esto evita que se cancele una multa sin pasar por el flujo de negocio
    /// que revoca la penalización asociada.
    ///
    /// IncidenciaId no es editable (ver IncidenciaUpdateDTO para la misma razón).
    /// </summary>
    public class MultaUpdateDTO
    {
        [Required(ErrorMessage = "El tipo de multa es obligatorio.")]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
        public decimal Monto { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Valores válidos desde este DTO: pendiente | pagada
        /// </summary>
        [Required(ErrorMessage = "El estado de la multa es obligatorio.")]
        [MaxLength(50)]
        public string Estado { get; set; } = string.Empty;
    }
}