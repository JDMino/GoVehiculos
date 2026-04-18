﻿namespace GoVehiculos.API.DTOs
{
    public class UsuarioResponseDTO
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int RolId { get; set; }
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string? Telefono { get; set; }

        public int? DireccionId { get; set; }
    }
}