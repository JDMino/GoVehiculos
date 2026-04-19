namespace GoVehiculos.API.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public int IdUsuario { get; set; }
    }
}
