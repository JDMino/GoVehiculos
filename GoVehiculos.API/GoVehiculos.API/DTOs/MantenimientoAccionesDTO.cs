namespace GoVehiculos.API.DTOs
{
    public class MantenimientoIniciarDTO { }

    public class MantenimientoFinalizarDTO
    {
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Por defecto la fecha actual. No puede ser anterior a FechaProgramada.
        /// </summary>
        public DateOnly FechaRealizacion { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        /// <summary>
        /// Costo del trabajo. No puede ser negativo.
        /// </summary>
        public decimal Costo { get; set; }

        /// <summary>
        /// Quién realizó el trabajo: nombre del técnico, taller, etc.
        /// </summary>
        public string RealizadoPor { get; set; } = string.Empty;
    }

    /// <summary>
    /// Datos que completa el empleado al cancelar el mantenimiento.
    /// Puede editar la descripción original para agregar el motivo de cancelación.
    /// </summary>
    public class MantenimientoCancelarDTO
    {
        /// <summary>
        /// El empleado puede editar la descripción original del admin
        /// para agregar el motivo de cancelación al final.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;
    }
}