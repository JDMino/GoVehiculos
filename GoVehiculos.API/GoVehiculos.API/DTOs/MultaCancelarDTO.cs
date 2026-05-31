namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Datos opcionales que el administrador puede adjuntar al cancelar una multa
    /// por error humano en la creación (Parte 3 — botón "Cancelar Multa").
    ///
    /// Al confirmar la cancelación el servicio:
    ///   1. Establece Multa.Estado = "cancelada"
    ///   2. Establece Penalizacion.Estado = "revocada"
    ///   3. Adjunta MotivoCancelacion a Multa.Descripcion para trazabilidad
    ///
    /// Nota deliberada: la cancelación NO revierte automáticamente el EstadoMecanico
    /// del vehículo (si la incidencia era "daño_fisico"), porque restaurar el estado
    /// mecánico requiere una orden de mantenimiento, no deshacer una sanción.
    /// Tampoco desbloquea al usuario si la penalización era "bloqueo_cuenta",
    /// ya que esa decisión requiere intervención explícita del administrador.
    /// </summary>
    public class MultaCancelarDTO
    {
        /// <summary>
        /// Motivo por el que se cancela la multa.
        /// Se concatena a Multa.Descripcion con el formato:
        /// "[descripción original] | CANCELADA: {motivo}"
        /// </summary>
        public string MotivoCancelacion { get; set; } = string.Empty;
    }
}