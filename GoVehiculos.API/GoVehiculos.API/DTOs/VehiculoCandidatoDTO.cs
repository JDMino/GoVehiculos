namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Vehículo candidato a mantenimiento enriquecido con el estado
    /// de su orden activa (pendiente / en_proceso) si existe.
    /// </summary>
    public class VehiculoCandidatoDTO
    {
        // ── Datos del vehículo ──────────────────────────────────────
        public int IdVehiculo { get; set; }
        public int? SocioId { get; set; }
        public string Patente { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoMecanico { get; set; } = string.Empty;
        public decimal Kilometraje { get; set; }
        public decimal PrecioPorDia { get; set; }
        public string MantenimientoACargoDe { get; set; } = string.Empty;
        public bool SeguroVigente { get; set; }
        public bool DocumentacionVigente { get; set; }
        public bool Activo { get; set; }
        public string? ImagenUrl { get; set; }
        public int ModeloId { get; set; }
        public string ModeloNombre { get; set; } = string.Empty;
        public string MarcaNombre { get; set; } = string.Empty;
        public int? UbicacionActualId { get; set; }
        public string? UbicacionNombre { get; set; }

        // ── Mantenimiento activo (null si no tiene ninguno pendiente/en_proceso) ──
        public MantenimientoActivoDTO? MantenimientoActivo { get; set; }

        /// <summary>
        /// True si el vehículo ya tiene una orden activa (pendiente o en_proceso).
        /// El frontend usa esto para deshabilitar el botón "Generar Orden".
        /// </summary>
        public bool TieneMantenimientoActivo => MantenimientoActivo != null;
    }

    /// <summary>
    /// Resumen del mantenimiento activo de un vehículo
    /// </summary>
    public class MantenimientoActivoDTO
    {
        public int IdMantenimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int? EmpleadoId { get; set; }
        public string? EmpleadoNombre { get; set; }
        public DateOnly? FechaProgramada { get; set; }
    }
}