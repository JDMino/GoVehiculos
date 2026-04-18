using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoVehiculos.API.Models
{
    [Table("Marca")]
    public class Marca
    {
        [Key]
        [Column("id_marca")]
        public int IdMarca { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Modelo>? Modelos { get; set; }
    }
}
