using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoVehiculos.API.Models
{
    [Table("Ubicacion")]
    public class Ubicacion
    {
        [Key]
        [Column("id_ubicacion")]
        public int IdUbicacion { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("direccion_id")]
        public int DireccionId { get; set; }
        public Direccion? Direccion { get; set; }

        public ICollection<Vehiculo>? Vehiculos { get; set; }
    }
}
