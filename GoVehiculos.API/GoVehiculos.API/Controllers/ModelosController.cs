using GoVehiculos.API.DTOs;
using GoVehiculos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVehiculos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ModelosController : ControllerBase
    {
        private readonly ModeloService _service;

        public ModelosController(ModeloService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? marcaId)
        {
            var result = await _service.GetAllAsync(marcaId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ModeloCreateDTO dto)
        {
            var nuevo = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = nuevo.IdModelo }, nuevo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ModeloUpdateDTO dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok)
            {
                var existe = await _service.GetByIdAsync(id);
                if (existe == null) return NotFound();
                return Conflict(new { mensaje = "No se puede eliminar el modelo porque tiene vehículos asociados." });
            }
            return NoContent();
        }
    }
}
