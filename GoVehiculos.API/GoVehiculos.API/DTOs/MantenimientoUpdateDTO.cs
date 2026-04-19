namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos para actualizar una orden existente.
    /// Se usa en las etapas posteriores del flujo: ejecución y cierre del trabajo.
    /// </summary>
    public class MantenimientoUpdateDTO
    {
        public string Estado { get; set; } = string.Empty;

        public string Prioridad { get; set; } = string.Empty;

        public DateOnly? FechaProgramada { get; set; }

        // Se completa cuando el trabajo se realiza
        public DateOnly? FechaRealizacion { get; set; }

        // Se completa cuando el trabajo se realiza
        public decimal Costo { get; set; }

        // Se completa cuando el trabajo se realiza: taller, técnico, empleado externo, etc.
        public string RealizadoPor { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int? EmpleadoId { get; set; }
    }
}