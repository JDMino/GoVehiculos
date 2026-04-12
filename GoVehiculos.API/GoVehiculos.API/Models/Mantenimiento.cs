using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Mantenimiento
    {
        [Key]
        public int IdMantenimiento { get; set; }
        public int VehiculoId { get; set; }
        public int? EmpleadoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = "pendiente";
        public string Prioridad { get; set; } = "media";
        public DateTime? FechaProgramada { get; set; }
        public DateTime? FechaRealizacion { get; set; }
        public decimal Costo { get; set; } = 0;
        public string RealizadoPor { get; set; } = "empresa";
    }
}
