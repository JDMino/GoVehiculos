namespace GoVehiculos.API.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int RolId { get; set; }
    }
}
