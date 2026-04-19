using GoVehiculos.API.DTOs;
using GoVehiculos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVehiculos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehiculosController : ControllerBase
    {
        private readonly VehiculoService _service;

        public VehiculosController(VehiculoService service)
        {
            _service = service;
        }

        // GET /api/vehiculos?estado=disponible&marca=Ford
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] string? marca)
        {
            var vehiculos = await _service.GetAllAsync(estado, marca);
            return Ok(vehiculos);
        }

        // GET /api/vehiculos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehiculo = await _service.GetByIdAsync(id);
            if (vehiculo == null) return NotFound();
            return Ok(vehiculo);
        }

        // POST /api/vehiculos
        [HttpPost]
        public async Task<IActionResult> Create(VehiculoCreateDTO dto)
        {
            var nuevo = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = nuevo.IdVehiculo }, nuevo);
        }

        // PUT /api/vehiculos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, VehiculoUpdateDTO dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/fuera-de-servicio")]
        public async Task<IActionResult> PasarAFueraDeServicio(int id)
        {
            var result = await _service.PasarAFueraDeServicioAsync(id);
            if (!result.exito) return BadRequest(result.mensaje);
            return Ok(result.mensaje);
        }


        // DELETE /api/vehiculos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
