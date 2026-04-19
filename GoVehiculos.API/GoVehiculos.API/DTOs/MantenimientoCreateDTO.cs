using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos que el administrador completa al generar una orden de mantenimiento.
    /// El vehículo pasa automáticamente a estado "mantenimiento" al crear la orden.
    /// Costo, FechaRealizacion y RealizadoPor se completan en etapas posteriores.
    /// EmpleadoId y FechaProgramada son obligatorios para poder generar la orden.
    /// </summary>
    public class MantenimientoCreateDTO
    {
        [Required]
        public int VehiculoId { get; set; }

        // Obligatorio: el admin debe asignar un empleado al generar la orden
        [Required]
        public int EmpleadoId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        // Siempre "pendiente" al crear — el service lo fija, no depende del frontend
        public string Estado { get; set; } = "pendiente";

        [Required]
        [MaxLength(20)]
        public string Prioridad { get; set; } = "media";

        // Obligatorio: el admin debe fijar una fecha programada
        [Required]
        public DateOnly FechaProgramada { get; set; }
    }
}
