using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoVehiculos.API.Models
{
    [Table("Direccion")]
    public class Direccion
    {
        [Key]
        [Column("id_direccion")]
        public int IdDireccion { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("calle")]
        public string Calle { get; set; } = string.Empty;

        [Column("numero")]
        public int Numero { get; set; }

        [MaxLength(50)]
        [Column("piso_depto")]
        public string? PisoDepto { get; set; }

        [MaxLength(20)]
        [Column("codigo_postal")]
        public string? CodigoPostal { get; set; }

        [Column("localidad_id")]
        public int LocalidadId { get; set; }
        public Localidad? Localidad { get; set; }
    }
}
