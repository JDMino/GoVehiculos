using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos que el administrador completa al registrar el suceso que origina la multa.
    ///
    /// Tipos válidos para Tipo:
    ///   daño_fisico          — golpe, choque, rayón u otro daño físico al vehículo.
    ///                          Dispara el cambio de EstadoMecanico del vehículo a "malo".
    ///   accidente            — siniestro con terceros u otros involucrados.
    ///   infraccion_vial      — multa de tránsito, exceso de velocidad, etc.
    ///   comportamiento_indebido — maltrato al vehículo, uso indebido, etc.
    ///   retraso_en_pago      — incumplimiento o demora en el pago.
    ///
    /// Niveles válidos para NivelGravedad: baja | media | alta
    ///
    /// FechaReporte no se recibe desde el frontend: se fija en el backend
    /// mediante DEFAULT GETDATE() al persistir la entidad.
    /// </summary>
    public class IncidenciaCreateDTO
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El vehículo es obligatorio.")]
        public int VehiculoId { get; set; }

        [Required(ErrorMessage = "El tipo de incidencia es obligatorio.")]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nivel de gravedad es obligatorio.")]
        [MaxLength(20)]
        public string NivelGravedad { get; set; } = "media";

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;
    }
}