using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoVehiculos.API.Models
{
    [Table("Localidad")]
    public class Localidad
    {
        [Key]
        [Column("id_localidad")]
        public int IdLocalidad { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("provincia_id")]
        public int ProvinciaId { get; set; }
        public Provincia? Provincia { get; set; }

        public ICollection<Direccion>? Direcciones { get; set; }
    }
}
