using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public int RolId { get; set; }
        public bool Verificado { get; set; } = false;
        public bool Bloqueado { get; set; } = false;
        public bool Activo { get; set; } = true;
        public DateTime? FechaBaja { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public Rol? Rol { get; set; }
    }
}
