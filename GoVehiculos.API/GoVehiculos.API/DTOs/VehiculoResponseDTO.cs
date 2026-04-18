namespace GoVehiculos.API.DTOs
{
    public class VehiculoResponseDTO
    {
        public int IdVehiculo { get; set; }
        public int? SocioId { get; set; }  // expone si el vehículo pertenece a un socio
        public string Tipo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoMecanico { get; set; } = string.Empty;
        public decimal Kilometraje { get; set; }
        public decimal PrecioPorDia { get; set; }
        public string MantenimientoACargoDe { get; set; } = string.Empty;
        public bool SeguroVigente { get; set; }
        public bool DocumentacionVigente { get; set; }
        public bool Activo { get; set; }
        public string? ImagenUrl { get; set; }

        // Datos normalizados de Modelo y Marca
        public int ModeloId { get; set; }
        public string ModeloNombre { get; set; } = string.Empty;
        public string MarcaNombre { get; set; } = string.Empty;

        // Datos normalizados de Ubicacion
        public int? UbicacionActualId { get; set; }
        public string? UbicacionNombre { get; set; }
    }
}