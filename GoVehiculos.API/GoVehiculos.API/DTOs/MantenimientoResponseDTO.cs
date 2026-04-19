namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos de respuesta de una orden de mantenimiento.
    /// Incluye datos desnormalizados del vehículo y empleado.
    /// VehiculoEstado permite al frontend saber si el vehículo
    /// ya fue disponibilizado (para deshabilitar el botón).
    /// </summary>
    public class MantenimientoResponseDTO
    {
        public int IdMantenimiento { get; set; }

        public int     VehiculoId      { get; set; }
        public string  VehiculoPatente { get; set; } = string.Empty;
        public string  VehiculoMarca   { get; set; } = string.Empty;
        public string  VehiculoModelo  { get; set; } = string.Empty;

        /// <summary>
        /// Estado actual del vehículo. El frontend lo usa para saber
        /// si el botón "Marcar como disponible" debe estar habilitado o no.
        /// </summary>
        public string VehiculoEstado { get; set; } = string.Empty;

        public int?   EmpleadoId     { get; set; }
        public string? EmpleadoNombre { get; set; }

        public string Tipo        { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado      { get; set; } = string.Empty;
        public string Prioridad   { get; set; } = string.Empty;

        public DateOnly? FechaProgramada  { get; set; }
        public DateOnly? FechaRealizacion { get; set; }

        public decimal Costo        { get; set; }
        public string  RealizadoPor { get; set; } = string.Empty;
    }
}
