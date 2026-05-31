using System.ComponentModel.DataAnnotations;

namespace GoVehiculos.API.DTOs
{
    /// <summary>
    /// Envelope que agrupa los tres DTOs de creación en el body del
    /// POST /api/multas. Es el único DTO "compuesto" del módulo y
    /// su única responsabilidad es ser el contrato HTTP del endpoint.
    ///
    /// No contiene lógica de construcción ni de validación de negocio:
    /// eso pertenece al Builder y al servicio respectivamente.
    /// Se ubica en DTOs porque es parte del contrato de la API,
    /// no de la lógica de construcción de entidades.
    /// </summary>
    public class CrearMultaRequestDTO
    {
        [Required]
        public IncidenciaCreateDTO Incidencia { get; set; } = new();

        [Required]
        public MultaCreateDTO Multa { get; set; } = new();

        [Required]
        public PenalizacionCreateDTO Penalizacion { get; set; } = new();
    }
}