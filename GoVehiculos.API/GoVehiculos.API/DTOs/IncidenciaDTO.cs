namespace GoVehiculos.API.DTOs
{
    public class IncidenciaDTO
    {
        public int IdIncidencia { get; set; }
        public int UsuarioId { get; set; }
        public int VehiculoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NivelGravedad { get; set; } = "media";
        public string Estado { get; set; } = "registrada";
        public DateTime FechaReporte { get; set; } = DateTime.Now;
    }
}
