using GoVehiculos.API.DTOs;
using GoVehiculos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVehiculos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MultasController : ControllerBase
    {
        private readonly MultaService _service;

        public MultasController(MultaService service)
        {
            _service = service;
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        /// <summary>
        /// GET /api/multas
        /// Devuelve el listado de multas con sus datos de Incidencia desnormalizados.
        /// Admite filtros opcionales combinables por query string:
        ///   ?estado=pendiente
        ///   ?tipoIncidencia=daño_fisico
        ///   ?nivelGravedad=alta
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? estado,
            [FromQuery] string? tipoIncidencia,
            [FromQuery] string? nivelGravedad)
        {
            var resultado = await _service.GetAllAsync(estado, tipoIncidencia, nivelGravedad);
            return Ok(resultado);
        }

        /// <summary>
        /// GET /api/multas/{id}
        /// Devuelve una multa por ID.
        /// El frontend usa este endpoint al cargar la page de Editar Multa.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resultado = await _service.GetByIdAsync(id);
            if (resultado == null) return NotFound();
            return Ok(resultado);
        }

        /// <summary>
        /// GET /api/multas/valores
        /// Devuelve los valores válidos de todos los campos de tipo enum del módulo.
        /// El frontend lo consume una sola vez al montar los formularios de
        /// creación y edición para poblar los desplegables.
        /// </summary>
        [HttpGet("valores")]
        public IActionResult GetValores()
        {
            return Ok(new MultaValoresDTO());
        }

        /// <summary>
        /// GET /api/multas/usuario/{usuarioId}
        /// Devuelve las multas cuya incidencia pertenece al usuario indicado.
        /// Usado por la page "Mis Multas" de clientes y socios.
        /// Admite filtro opcional por estado: ?estado=pendiente | pagada | cancelada
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetByUsuario(int usuarioId, [FromQuery] string? estado)
        {
            var resultado = await _service.GetByUsuarioAsync(usuarioId, estado);
            return Ok(resultado);
        }

        // ================================================================
        // CREACIÓN COMPLETA
        // ================================================================

        /// <summary>
        /// POST /api/multas
        /// Crea una multa completa: persiste Incidencia, Multa y Penalización
        /// en orden respetando las dependencias de FK, y aplica los efectos
        /// secundarios sobre Vehículo y Usuario según los tipos seleccionados.
        ///
        /// Body esperado: tres secciones independientes en un objeto JSON:
        /// {
        ///   "incidencia": { ... },
        ///   "multa":      { ... },
        ///   "penalizacion": { ... }
        /// }
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearMultaRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (exito, mensaje, resultado) = await _service.CrearMultaCompletaAsync(
                dto.Incidencia,
                dto.Multa,
                dto.Penalizacion);

            if (!exito) return UnprocessableEntity(new { mensaje });

            return CreatedAtAction(nameof(GetById), new { id = resultado!.IdMulta }, resultado);
        }

        // ================================================================
        // ACTUALIZACIÓN
        // ================================================================

        /// <summary>
        /// PUT /api/multas/{id}
        /// Actualiza los campos editables de una Multa existente.
        /// No permite pasar el estado a "cancelada": use PATCH /cancelar.
        /// Rechaza la operación si la multa ya está cancelada.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MultaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (exito, mensaje) = await _service.UpdateAsync(id, dto);
            if (!exito) return UnprocessableEntity(new { mensaje });
            return NoContent();
        }

        // ================================================================
        // CANCELACIÓN
        // ================================================================

        /// <summary>
        /// PATCH /api/multas/{id}/cancelar
        /// Cancela una multa y revoca su penalización asociada.
        /// Esta es la única vía para establecer multa.estado = "cancelada".
        /// Una multa cancelada no puede volver a modificarse.
        /// </summary>
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id, [FromBody] MultaCancelarDTO dto)
        {
            var (exito, mensaje) = await _service.CancelarAsync(id, dto);
            
            // Si falla (ya sea por validación del Service o lógica interna del SP), devolvemos 422
            if (!exito) return UnprocessableEntity(new { mensaje });
            
            // Si el SP fue exitoso, devolvemos el mensaje exacto generado por la base de datos
            return Ok(new { mensaje });
        }
    }
}