using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Vehiculo
    {
        [Key]
        public int IdVehiculo { get; set; }
        public int? SocioId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string Estado { get; set; } = "disponible";
        public string EstadoMecanico { get; set; } = "bueno";
        public decimal Kilometraje { get; set; } = 0;
        public string LicenciaRequerida { get; set; } = string.Empty;
        public decimal PrecioPorDia { get; set; }
        public string? UbicacionActual { get; set; }
        public bool SeguroVigente { get; set; } = true;
        public bool DocumentacionVigente { get; set; } = true;
        public string MantenimientoACargoDe { get; set; } = "empresa";
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? FechaBaja { get; set; }
    }
}
