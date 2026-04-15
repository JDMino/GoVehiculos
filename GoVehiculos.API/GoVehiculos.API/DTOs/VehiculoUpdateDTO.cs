namespace GoVehiculos.API.DTOs
{
    public class VehiculoUpdateDTO
    {
        public string Estado { get; set; } = "disponible";
        public string EstadoMecanico { get; set; } = "bueno";
        public decimal Kilometraje { get; set; }
        public decimal PrecioPorDia { get; set; }
        public string? UbicacionActual { get; set; }

        // ✅ aseguramos que se envíen y se guarden correctamente
        public bool SeguroVigente { get; set; } = true;
        public bool DocumentacionVigente { get; set; } = true;

        public bool Activo { get; set; } = true;
    }
}
