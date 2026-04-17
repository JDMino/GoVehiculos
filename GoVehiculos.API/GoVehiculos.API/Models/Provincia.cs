using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Provincia
    {
        [Key]
        public int IdProvincia { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Localidad>? Localidades { get; set; }
    }
}
