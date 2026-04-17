using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Ubicacion
    {
        [Key]
        public int IdUbicacion { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public int DireccionId { get; set; }
        public Direccion? Direccion { get; set; }

        public ICollection<Vehiculo>? Vehiculos { get; set; }
    }
}
