namespace GoVehiculos.API.DTOs
{
    public class VehiculoResponseDTO
    {
        public int IdVehiculo { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoMecanico { get; set; } = string.Empty;
        public decimal Kilometraje { get; set; }
        public decimal PrecioPorDia { get; set; }
        public string MantenimientoACargoDe { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }
        public bool SeguroVigente { get; set; }
        public bool DocumentacionVigente { get; set; }

        // ✅ datos normalizados
        public int ModeloId { get; set; }
        public string ModeloNombre { get; set; } = string.Empty;
        public string MarcaNombre { get; set; } = string.Empty;

        public int? UbicacionActualId { get; set; }
        public string? UbicacionNombre { get; set; }
    }
}
