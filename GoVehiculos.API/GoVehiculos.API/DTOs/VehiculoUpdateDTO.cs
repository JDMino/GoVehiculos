namespace GoVehiculos.API.DTOs
{
    public class VehiculoUpdateDTO
    {
        public string Estado { get; set; } = "disponible";
        public string EstadoMecanico { get; set; } = "bueno";
        public decimal Kilometraje { get; set; }
        public decimal PrecioPorDia { get; set; }
        public bool SeguroVigente { get; set; } = true;
        public bool DocumentacionVigente { get; set; } = true;
        public bool Activo { get; set; } = true;

        public int ModeloId { get; set; }
        public int? UbicacionActualId { get; set; }
    }
}
