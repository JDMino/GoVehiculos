using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Multa
    {
        [Key]
        public int IdMulta { get; set; }
        public int IncidenciaId { get; set; }
        public int UsuarioId { get; set; }
        public int VehiculoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string Estado { get; set; } = "pendiente";
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public Incidencia? Incidencia { get; set; }
        public Usuario? Usuario { get; set; }
        public Vehiculo? Vehiculo { get; set; }
    }
}
