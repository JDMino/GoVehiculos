using GoVehiculos.API.DTOs;
using GoVehiculos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVehiculos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MantenimientosController : ControllerBase
    {
        private readonly MantenimientoService _service;

        public MantenimientosController(MantenimientoService service)
        {
            _service = service;
        }

        // GET /api/mantenimientos/candidatos
        // Devuelve vehículos con estadoMecanico "regular" o "malo" para mostrar en la lista
        [HttpGet("candidatos")]
        public async Task<IActionResult> GetCandidatos()
        {
            var vehiculos = await _service.GetVehiculosCandidatosAsync();
            return Ok(vehiculos);
        }

        // GET /api/mantenimientos
        // Devuelve todas las órdenes de mantenimiento generadas
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var resultado = await _service.GetAllAsync();
            return Ok(resultado);
        }

        // GET /api/mantenimientos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resultado = await _service.GetByIdAsync(id);
            if (resultado == null) return NotFound();
            return Ok(resultado);
        }

        // POST /api/mantenimientos
        // Aplica las reglas de negocio:
        //   - Si MantenimientoACargoDe == "socio" → 422 con mensaje, vehículo pasa a fuera_de_servicio
        //   - Si MantenimientoACargoDe == "empresa" → 201 con la orden creada, vehículo pasa a mantenimiento
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MantenimientoCreateDTO dto)
        {
            var (exito, mensaje, orden) = await _service.CreateAsync(dto);

            if (!exito)
            {
                // 422 Unprocessable Entity: la solicitud fue válida pero no se pudo procesar
                // por una regla de negocio (el socio se hace cargo del mantenimiento)
                return UnprocessableEntity(new { mensaje });
            }

            return CreatedAtAction(nameof(GetById), new { id = orden!.IdMantenimiento }, orden);
        }

        // PUT /api/mantenimientos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MantenimientoUpdateDTO dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE /api/mantenimientos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}