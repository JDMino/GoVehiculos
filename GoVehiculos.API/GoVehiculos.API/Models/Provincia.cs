using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoVehiculos.API.Models
{
    [Table("Provincia")]
    public class Provincia
    {
        [Key]
        [Column("id_provincia")]
        public int IdProvincia { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Localidad>? Localidades { get; set; }
    }
}
