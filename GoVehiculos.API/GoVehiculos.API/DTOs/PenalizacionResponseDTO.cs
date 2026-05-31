namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos de una Penalización que el frontend consume.
    /// Incluye MultaId para que el cliente pueda navegar
    /// a la multa relacionada si lo necesita.
    /// </summary>
    public class PenalizacionResponseDTO
    {
        public int IdPenalizacion { get; set; }

        // ── Multa vinculada (FK) ─────────────────────────────────────
        public int? MultaId { get; set; }

        // ── Datos propios de la penalización ─────────────────────────
        public string Tipo { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}