using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Penalizacion
    {
        [Key]
        public int IdPenalizacion { get; set; }
        public int UsuarioId { get; set; }
        public int? MultaId { get; set; }
        public int? IncidenciaId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; } = "activa";

        public Usuario? Usuario { get; set; }
        public Multa? Multa { get; set; }
        public Incidencia? Incidencia { get; set; }
    }
}
