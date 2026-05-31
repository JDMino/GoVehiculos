using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos que el administrador completa para la sanción económica
    /// vinculada a una Incidencia.
    ///
    /// Tipos válidos para Tipo:
    ///   economica       — cobro de monto dinerario.
    ///   administrativa  — sanción formal sin monto (Monto puede ser 0).
    ///   mixta           — combinación de sanción formal y cobro dinerario.
    ///
    /// Estado siempre "pendiente" al crear — fijado en el Builder,
    /// no depende del frontend.
    ///
    /// IncidenciaId no forma parte de este DTO: lo asigna el Director
    /// del Builder una vez que la Incidencia fue persistida y tiene ID.
    /// </summary>
    public class MultaCreateDTO
    {
        [Required(ErrorMessage = "El tipo de multa es obligatorio.")]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Puede ser 0 para multas de tipo "administrativa".
        /// No puede ser negativo.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
        public decimal Monto { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }
    }
}