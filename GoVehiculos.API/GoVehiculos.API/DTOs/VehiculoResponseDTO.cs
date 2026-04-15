namespace GoVehiculos.API.DTOs
{
    public class VehiculoResponseDTO
    {
        public int IdVehiculo { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoMecanico { get; set; } = string.Empty;
        public decimal Kilometraje { get; set; }
        public decimal PrecioPorDia { get; set; }
        public string? UbicacionActual { get; set; }
        public string MantenimientoACargoDe { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }

        // ✅ añadimos estos dos campos
        public bool SeguroVigente { get; set; }
        public bool DocumentacionVigente { get; set; }
    }
}
