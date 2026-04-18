namespace GoVehiculos.API.DTOs
{
    public class VehiculoCreateDTO
    {
        public int? SocioId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int ModeloId { get; set; }
        public int Anio { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string Estado { get; set; } = "disponible";
        public string EstadoMecanico { get; set; } = "bueno";
        public decimal Kilometraje { get; set; } = 0;
        public decimal PrecioPorDia { get; set; }
        public int? UbicacionActualId { get; set; }
        public bool SeguroVigente { get; set; } = true;
        public bool DocumentacionVigente { get; set; } = true;
        public string MantenimientoACargoDe { get; set; } = "empresa";
        public string? ImagenUrl { get; set; }
    }
}