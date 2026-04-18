using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoVehiculos.API.Models
{
    [Table("Modelo")]
    public class Modelo
    {
        [Key]
        [Column("id_modelo")]
        public int IdModelo { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("marca_id")]
        public int MarcaId { get; set; }
        public Marca? Marca { get; set; }

        public ICollection<Vehiculo>? Vehiculos { get; set; }
    }
}
