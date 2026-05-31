namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Contiene las listas de valores válidos para cada campo de tipo "enum de dominio"
    /// en el módulo de multas. El frontend consume GET /api/multas/valores una sola vez
    /// al montar los formularios de creación y edición para poblar los desplegables.
    ///
    /// Centralizar estos valores aquí evita hardcodearlos en el cliente y mantiene
    /// una única fuente de verdad en el backend. Si un valor cambia, solo se modifica
    /// este archivo y el cambio se refleja automáticamente en todos los formularios.
    /// </summary>
    public class MultaValoresDTO
    {
        /// <summary>
        /// Valores válidos para Incidencia.Tipo
        /// El frontend muestra un aviso cuando el usuario selecciona "daño_fisico".
        /// </summary>
        public List<string> TiposIncidencia { get; set; } =
        [
            "daño_fisico",
            "accidente",
            "infraccion_vial",
            "comportamiento_indebido",
            "retraso_en_pago"
        ];

        /// <summary>
        /// Valores válidos para Incidencia.NivelGravedad
        /// </summary>
        public List<string> NivelesGravedad { get; set; } =
        [
            "baja",
            "media",
            "alta"
        ];

        /// <summary>
        /// Valores válidos para Multa.Tipo
        /// </summary>
        public List<string> TiposMulta { get; set; } =
        [
            "economica",
            "administrativa",
            "mixta"
        ];

        /// <summary>
        /// Estados editables de Multa.
        /// Excluye "cancelada": ese estado solo se asigna mediante
        /// PATCH /api/multas/{id}/cancelar.
        /// </summary>
        public List<string> EstadosMultaEditables { get; set; } =
        [
            "pendiente",
            "pagada"
        ];

        /// <summary>
        /// Valores válidos para Penalizacion.Tipo
        /// </summary>
        public List<string> TiposPenalizacion { get; set; } =
        [
            "suspension_temporal",
            "bloqueo_cuenta",
            "inhabilitacion_vehiculo",
            "advertencia"
        ];

        /// <summary>
        /// Estados editables de Penalizacion.
        /// Excluye "revocada": ese estado solo se asigna al cancelar la multa padre.
        /// </summary>
        public List<string> EstadosPenalizacionEditables { get; set; } =
        [
            "activa",
            "cumplida"
        ];
    }
}