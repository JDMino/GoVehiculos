using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Marca
    {
        [Key]
        public int IdMarca { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Modelo>? Modelos { get; set; }
    }
}
