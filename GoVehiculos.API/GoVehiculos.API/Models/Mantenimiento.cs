using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoVehiculos.API.Models
{
    [Table("Mantenimiento")]
    public class Mantenimiento
    {
        [Key]
        [Column("id_mantenimiento")]
        public int IdMantenimiento { get; set; }

        [Column("vehiculo_id")]
        public int VehiculoId { get; set; }

        [Column("empleado_id")]
        public int? EmpleadoId { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "pendiente";

        [Required]
        [MaxLength(20)]
        [Column("prioridad")]
        public string Prioridad { get; set; } = "media";

        [Column("fecha_programada")]
        public DateOnly? FechaProgramada { get; set; }

        [Column("fecha_realizacion")]
        public DateOnly? FechaRealizacion { get; set; }

        [Column("costo")]
        public decimal Costo { get; set; } = 0;

        [Required]
        [MaxLength(20)]
        [Column("realizado_por")]
        public string RealizadoPor { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el admin ya ejecutó "Marcar como disponible" para esta orden.
        /// Una vez en true nunca vuelve a false, sin importar el estado posterior
        /// del vehículo. Evita que órdenes históricas reactiven el botón cuando
        /// el vehículo pasa a "mantenimiento" nuevamente por una orden nueva.
        /// </summary>
        [Column("disponibilizado")]
        public bool Disponibilizado { get; set; } = false;

        // Navegación
        public Vehiculo? Vehiculo { get; set; }
        public Usuario? Empleado { get; set; }
    }
}
