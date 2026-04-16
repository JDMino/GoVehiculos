namespace GoVehiculos.API.DTOs
{
    public class UsuarioUpdateDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public bool Bloqueado { get; set; }
        public bool Activo { get; set; }
        public int RolId { get; set; }
    }

}
