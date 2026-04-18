namespace GoVehiculos.API.DTOs
{
    public class MarcaResponseDTO
    {
        public int IdMarca { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CantidadModelos { get; set; }
    }
}
