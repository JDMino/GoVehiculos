using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoVehiculos.API.Models
{
    [Table("Vehiculo")]
    public class Vehiculo
    {
        [Key]
        [Column("id_vehiculo")]
        public int IdVehiculo { get; set; }

        // FK opcional al socio propietario del vehículo
        [Column("socio_id")]
        public int? SocioId { get; set; }
        public Usuario? Socio { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("patente")]
        public string Patente { get; set; } = string.Empty;

        [Column("anio")]
        public int Anio { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        // CHECK: 'disponible' | 'reservado' | 'en_uso' | 'mantenimiento' | 'fuera_de_servicio'
        [Required]
        [MaxLength(50)]
        [Column("estado")]
        public string Estado { get; set; } = "disponible";

        [Required]
        [MaxLength(50)]
        [Column("estado_mecanico")]
        public string EstadoMecanico { get; set; } = "bueno";

        [Column("kilometraje")]
        public decimal Kilometraje { get; set; } = 0;

        [Column("precio_por_dia")]
        public decimal PrecioPorDia { get; set; }

        // CHECK: 'empresa' | 'socio'
        [Required]
        [MaxLength(50)]
        [Column("mantenimiento_a_cargo_de")]
        public string MantenimientoACargoDe { get; set; } = "empresa";

        [Column("seguro_vigente")]
        public bool SeguroVigente { get; set; } = true;

        [Column("documentacion_vigente")]
        public bool DocumentacionVigente { get; set; } = true;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [MaxLength(255)]
        [Column("imagen_url")]
        public string? ImagenUrl { get; set; }

        [Column("modelo_id")]
        public int ModeloId { get; set; }
        public Modelo? Modelo { get; set; }

        [Column("ubicacion_actual_id")]
        public int? UbicacionActualId { get; set; }
        public Ubicacion? UbicacionActual { get; set; }
    }
}
