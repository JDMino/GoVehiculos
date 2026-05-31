namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos de una Incidencia que el frontend consume.
    /// Incluye datos desnormalizados del Usuario y del Vehículo
    /// para evitar llamadas adicionales desde el cliente.
    /// </summary>
    public class IncidenciaResponseDTO
    {
        public int IdIncidencia { get; set; }

        // ── Usuario involucrado ─────────────────────────────────────
        public int UsuarioId { get; set; }
        public string UsuarioNombreCompleto { get; set; } = string.Empty;

        // ── Vehículo involucrado ────────────────────────────────────
        public int VehiculoId { get; set; }
        public string VehiculoPatente { get; set; } = string.Empty;
        public string VehiculoMarca { get; set; } = string.Empty;
        public string VehiculoModelo { get; set; } = string.Empty;

        // ── Datos propios de la incidencia ──────────────────────────
        public string Tipo { get; set; } = string.Empty;
        public string NivelGravedad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaReporte { get; set; }
    }
}