using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Notificacion
    {
        [Key]
        public int IdNotificacion { get; set; }
        public int UsuarioId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public bool Leido { get; set; } = false;
        public string EstadoEnvio { get; set; } = "pendiente";
        public string Prioridad { get; set; } = "media";
    }
}
