namespace GoVehiculos.API.DTOs
{
    public class ModeloResponseDTO
    {
        public int IdModelo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int MarcaId { get; set; }
        public string MarcaNombre { get; set; } = string.Empty;
    }
}
