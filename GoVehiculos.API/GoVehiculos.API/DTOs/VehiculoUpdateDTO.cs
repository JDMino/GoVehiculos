namespace GoVehiculos.API.DTOs
{
    public class VehiculoUpdateDTO
    {
        public string Estado { get; set; } = "disponible";
        public string EstadoMecanico { get; set; } = "bueno";
        public decimal Kilometraje { get; set; }
        public decimal PrecioPorDia { get; set; }
        public string? UbicacionActual { get; set; }
        public bool SeguroVigente { get; set; }
        public bool DocumentacionVigente { get; set; }
        public bool Activo { get; set; }
    }
}
