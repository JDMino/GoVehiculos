using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Notificacion
    {
        [Key]
        public int IdNotificacion { get; set; }
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }   // FK a Usuario
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leido { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
