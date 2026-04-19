using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos del mantenimiento realizado por el socio.
    /// Se usa para registrar la instancia y devolver el vehículo a disponible.
    /// RealizadoPor es siempre "A cargo del Socio" — fijado en el service, no en el DTO.
    /// </summary>
    public class HabilitarVehiculoSocioDTO
    {
        [Required]
        public int VehiculoId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Prioridad { get; set; } = string.Empty;

        [Required]
        public DateOnly FechaRealizacion { get; set; }
    }
}
