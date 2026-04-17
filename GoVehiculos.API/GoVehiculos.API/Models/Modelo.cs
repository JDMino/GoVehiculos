using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Modelo
    {
        [Key]
        public int IdModelo { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public int MarcaId { get; set; }
        public Marca? Marca { get; set; }

        public ICollection<Vehiculo>? Vehiculos { get; set; }
    }
}
