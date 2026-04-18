namespace GoVehiculos.API.DTOs
{
    public class ModeloCreateDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public int MarcaId { get; set; }
    }
}
