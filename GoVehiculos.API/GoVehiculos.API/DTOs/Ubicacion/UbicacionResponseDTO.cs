namespace GoVehiculos.API.DTOs
{
    public class UbicacionResponseDTO
    {
        public int IdUbicacion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DireccionId { get; set; }

        public string? Calle { get; set; }
        public int? Numero { get; set; }
        public string? LocalidadNombre { get; set; }
        public string? ProvinciaNombre { get; set; }

        public int CantidadVehiculos { get; set; }
    }
}
