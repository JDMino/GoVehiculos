using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // ESTO ES VITAL PARA LOS MAPEOS

namespace GoVehiculos.API.Models
{
    [Table("Usuario")] // Asegura que busque la tabla "Usuario" y no "Usuarios"
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("dni")]
        public string Dni { get; set; } = string.Empty;

        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("direccion_id")]
        public int? DireccionId { get; set; }

        public Direccion? Direccion { get; set; }

        [Column("rol_id")]
        public int RolId { get; set; }

        [Column("verificado")]
        public bool Verificado { get; set; } = false;

        [Column("bloqueado")]
        public bool Bloqueado { get; set; } = false;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_baja")]
        public DateTime? FechaBaja { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public Rol? Rol { get; set; }
    }
}