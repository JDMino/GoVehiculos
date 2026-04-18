using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace GoVehiculos.API.Models
{
    [Table("Rol")] 
    public class Rol
    {
        [Key]
        [Column("id_rol")] 
        public int IdRol { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}
