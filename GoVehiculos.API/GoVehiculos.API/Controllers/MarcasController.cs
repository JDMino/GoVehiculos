using GoVehiculos.API.DTOs;
using GoVehiculos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVehiculos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MarcasController : ControllerBase
    {
        private readonly MarcaService _service;

        public MarcasController(MarcaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
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
        public async Task<IActionResult> Create([FromBody] MarcaCreateDTO dto)
        {
            var nueva = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = nueva.IdMarca }, nueva);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MarcaUpdateDTO dto)
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
                return Conflict(new { mensaje = "No se puede eliminar la marca porque tiene modelos asociados." });
            }
            return NoContent();
        }
    }
}
