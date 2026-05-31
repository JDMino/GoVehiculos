namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// DTO de respuesta que el frontend consume para listar y editar multas.
    /// Aplana los datos de las tres entidades (Multa, Incidencia, Penalización)
    /// en un único objeto para que el listado funcione sin llamadas adicionales.
    ///
    /// El repositorio ya carga todas las navegaciones necesarias mediante Include,
    /// por lo que el servicio puede mapear todos los campos aquí sin consultas extra.
    ///
    /// EstaCancelada es una propiedad calculada que el frontend usa para:
    ///   - Deshabilitar el botón "Editar" en el listado.
    ///   - Bloquear todos los controles en la page de edición.
    /// </summary>
    public class MultaResponseDTO
    {
        // ── Multa ────────────────────────────────────────────────────
        public int IdMulta { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

        public bool EstaCancelada => Estado == "cancelada";

        // ── Incidencia vinculada ─────────────────────────────────────
        public int IncidenciaId { get; set; }
        public string IncidenciaTipo { get; set; } = string.Empty;
        public string IncidenciaNivelGravedad { get; set; } = string.Empty;
        public string IncidenciaDescripcion { get; set; } = string.Empty;
        public DateTime IncidenciaFechaReporte { get; set; }

        // ── Usuario involucrado (via Incidencia) ─────────────────────
        public int UsuarioId { get; set; }
        public string UsuarioNombreCompleto { get; set; } = string.Empty;

        // ── Vehículo involucrado (via Incidencia) ────────────────────
        public int VehiculoId { get; set; }
        public string VehiculoPatente { get; set; } = string.Empty;
        public string VehiculoMarca { get; set; } = string.Empty;
        public string VehiculoModelo { get; set; } = string.Empty;

        // ── Penalización vinculada ───────────────────────────────────
        public int? IdPenalizacion { get; set; }
        public string? PenalizacionTipo { get; set; }
        public string? PenalizacionMotivo { get; set; }
        public string? PenalizacionEstado { get; set; }
        public DateTime? PenalizacionFechaInicio { get; set; }
        public DateTime? PenalizacionFechaFin { get; set; }
    }
}