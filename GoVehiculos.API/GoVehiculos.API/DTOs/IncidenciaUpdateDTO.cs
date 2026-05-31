using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos editables de una Incidencia existente.
    ///
    /// UsuarioId y VehiculoId no son editables: modificar el sujeto
    /// o el objeto de una sanción ya registrada implicaría una operación
    /// de negocio distinta (cancelar y recrear), no una edición simple.
    /// </summary>
    public class IncidenciaUpdateDTO
    {
        [Required(ErrorMessage = "El tipo de incidencia es obligatorio.")]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nivel de gravedad es obligatorio.")]
        [MaxLength(20)]
        public string NivelGravedad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;
    }
}