using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Direccion
    {
        [Key]
        public int IdDireccion { get; set; }
        public string Calle { get; set; } = string.Empty;
        public int Numero { get; set; }
        public string? PisoDepto { get; set; }
        public string? CodigoPostal { get; set; }

        public int LocalidadId { get; set; }
        public Localidad? Localidad { get; set; }
    }
}
