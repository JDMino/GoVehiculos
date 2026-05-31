using GoVehiculos.API.DTOs;
using GoVehiculos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVehiculos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PenalizacionesController : ControllerBase
    {
        private readonly PenalizacionService _service;

        public PenalizacionesController(PenalizacionService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET /api/penalizaciones
        /// Devuelve todas las penalizaciones.
        /// Admite filtro opcional por estado:
        ///   ?estado=activa | cumplida | revocada
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? estado)
        {
            var resultado = await _service.GetAllAsync(estado);
            return Ok(resultado);
        }

        /// <summary>
        /// GET /api/penalizaciones/{id}
        /// Devuelve una penalización por ID.
        /// El frontend usa este endpoint al cargar la sección de penalización
        /// dentro de la page de Editar Multa.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resultado = await _service.GetByIdAsync(id);
            if (resultado == null) return NotFound();
            return Ok(resultado);
        }

        /// <summary>
        /// PUT /api/penalizaciones/{id}
        /// Actualiza los campos editables de una penalización existente.
        /// No permite pasar el estado a "revocada": ese estado solo se asigna
        /// al cancelar la multa padre mediante PATCH /api/multas/{id}/cancelar.
        /// Rechaza la operación si la penalización ya está revocada.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PenalizacionUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (exito, mensaje) = await _service.UpdateAsync(id, dto);
            if (!exito) return UnprocessableEntity(new { mensaje });
            return NoContent();
        }
    }
}