using GoVehiculos.API.DTOs;
using GoVehiculos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVehiculos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IncidenciasController : ControllerBase
    {
        private readonly IncidenciaService _service;

        public IncidenciasController(IncidenciaService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET /api/incidencias
        /// Devuelve todas las incidencias con sus datos de Usuario y Vehículo desnormalizados.
        /// Usado por el frontend para mostrar el listado completo o para navegar
        /// desde una multa a su incidencia de origen.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var resultado = await _service.GetAllAsync();
            return Ok(resultado);
        }

        /// <summary>
        /// GET /api/incidencias/{id}
        /// Devuelve una incidencia por ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resultado = await _service.GetByIdAsync(id);
            if (resultado == null) return NotFound();
            return Ok(resultado);
        }

        /// <summary>
        /// PUT /api/incidencias/{id}
        /// Actualiza los campos editables de una incidencia existente.
        /// UsuarioId y VehiculoId no son modificables desde este endpoint.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] IncidenciaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (exito, mensaje) = await _service.UpdateAsync(id, dto);
            if (!exito) return UnprocessableEntity(new { mensaje });
            return NoContent();
        }
    }
}