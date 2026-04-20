namespace GoVehiculos.API.DTOs
{
    public class MantenimientoResponseDTO
    {
        public int IdMantenimiento { get; set; }

        public int    VehiculoId      { get; set; }
        public string VehiculoPatente { get; set; } = string.Empty;
        public string VehiculoMarca   { get; set; } = string.Empty;
        public string VehiculoModelo  { get; set; } = string.Empty;
        public string VehiculoEstado  { get; set; } = string.Empty;

        public int?    EmpleadoId     { get; set; }
        public string? EmpleadoNombre { get; set; }

        public string Tipo        { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado      { get; set; } = string.Empty;
        public string Prioridad   { get; set; } = string.Empty;

        public DateOnly? FechaProgramada  { get; set; }
        public DateOnly? FechaRealizacion { get; set; }

        public decimal Costo        { get; set; }
        public string  RealizadoPor { get; set; } = string.Empty;

        /// <summary>
        /// True si el admin ya ejecutó "Marcar como disponible" para esta orden.
        /// El frontend usa este campo (no VehiculoEstado) para deshabilitar el botón,
        /// evitando que órdenes históricas se reactiven cuando el vehículo vuelve
        /// a estado "mantenimiento" por una orden nueva.
        /// </summary>
        public bool Disponibilizado { get; set; }
    }
}
