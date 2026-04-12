using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
