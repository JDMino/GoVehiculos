namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos que el administrador completa al generar una orden de mantenimiento.
    /// El vehículo pasa automáticamente a estado "mantenimiento" al crear la orden.
    /// Costo, FechaRealizacion y RealizadoPor se completan en etapas posteriores.
    /// </summary>
    public class MantenimientoCreateDTO
    {
        public int VehiculoId { get; set; }

        // Nullable: el admin puede dejar sin asignar y asignar después
        public int? EmpleadoId { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        // Siempre "pendiente" al crear — se deja para que el frontend lo envíe explícito
        public string Estado { get; set; } = "pendiente";

        public string Prioridad { get; set; } = "media";

        public DateOnly? FechaProgramada { get; set; }
    }
}