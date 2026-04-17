using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Localidad
    {
        [Key]
        public int IdLocalidad { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public int ProvinciaId { get; set; }
        public Provincia? Provincia { get; set; }

        public ICollection<Direccion>? Direcciones { get; set; }
    }
}
