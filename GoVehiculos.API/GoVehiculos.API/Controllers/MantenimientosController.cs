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

        // ================================================================
        // CONTADORES — Navbar y badge de historial
        // ================================================================

        /// <summary>
        /// GET /api/mantenimientos/contador-admin
        /// Cantidad de vehículos activos con estadoMecanico "regular" o "malo".
        /// Usado por el Navbar del administrador.
        /// </summary>
        [HttpGet("contador-admin")]
        public async Task<IActionResult> GetContadorAdmin()
        {
            var count = await _service.GetContadorAdminAsync();
            return Ok(new { count });
        }

        /// <summary>
        /// GET /api/mantenimientos/contador-empleado/{empleadoId}
        /// Cantidad de órdenes activas (pendiente o iniciado) del empleado.
        /// Usado por el Navbar del empleado.
        /// </summary>
        [HttpGet("contador-empleado/{empleadoId}")]
        public async Task<IActionResult> GetContadorEmpleado(int empleadoId)
        {
            var count = await _service.GetContadorEmpleadoAsync(empleadoId);
            return Ok(new { count });
        }

        /// <summary>
        /// GET /api/mantenimientos/contador-nuevas
        /// Cantidad total de órdenes en estado finalizado o cancelado.
        /// El frontend compara este número contra el último valor almacenado en sessionStorage
        /// para mostrar u ocultar el badge del botón "Ver Historial".
        /// </summary>
        [HttpGet("contador-nuevas")]
        public async Task<IActionResult> GetContadorNuevasTerminadas()
        {
            var count = await _service.GetContadorNuevasTerminadasAsync();
            return Ok(new { count });
        }

        // ================================================================
        // PARTE 1 — Endpoints del administrador
        // ================================================================

        [HttpGet("candidatos")]
        public async Task<IActionResult> GetCandidatos()
        {
            var vehiculos = await _service.GetVehiculosCandidatosAsync();
            return Ok(vehiculos);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? estado)
        {
            var resultado = await _service.GetAllAsync(estado);
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resultado = await _service.GetByIdAsync(id);
            if (resultado == null) return NotFound();
            return Ok(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MantenimientoCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (exito, mensaje, orden) = await _service.CreateAsync(dto);

            if (!exito)
                return UnprocessableEntity(new { mensaje });

            return CreatedAtAction(nameof(GetById), new { id = orden!.IdMantenimiento }, orden);
        }

        [HttpPost("habilitar-socio")]
        public async Task<IActionResult> HabilitarSocio([FromBody] HabilitarVehiculoSocioDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (exito, mensaje, registro) = await _service.HabilitarVehiculoSocioAsync(dto);

            if (!exito)
                return UnprocessableEntity(new { mensaje });

            return Ok(new { mensaje, registro });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MantenimientoUpdateDTO dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // ================================================================
        // PARTE 2 — Endpoints del empleado
        // ================================================================

        [HttpGet("empleado/{empleadoId}")]
        public async Task<IActionResult> GetByEmpleado(int empleadoId)
        {
            var resultado = await _service.GetByEmpleadoAsync(empleadoId);
            return Ok(resultado);
        }

        [HttpPatch("{id}/iniciar")]
        public async Task<IActionResult> Iniciar(int id, [FromBody] MantenimientoIniciarDTO dto,
            [FromQuery] int empleadoId)
        {
            var (exito, mensaje) = await _service.IniciarAsync(id, empleadoId);
            if (!exito) return UnprocessableEntity(new { mensaje });
            return Ok(new { mensaje });
        }

        [HttpPatch("{id}/finalizar")]
        public async Task<IActionResult> Finalizar(int id, [FromBody] MantenimientoFinalizarDTO dto,
            [FromQuery] int empleadoId)
        {
            var (exito, mensaje) = await _service.FinalizarAsync(id, empleadoId, dto);
            if (!exito) return UnprocessableEntity(new { mensaje });
            return Ok(new { mensaje });
        }

        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id, [FromBody] MantenimientoCancelarDTO dto,
            [FromQuery] int empleadoId)
        {
            var (exito, mensaje) = await _service.CancelarAsync(id, empleadoId, dto);
            if (!exito) return UnprocessableEntity(new { mensaje });
            return Ok(new { mensaje });
        }

        // ================================================================
        // PARTE 3 — Disponibilizar vehículo
        // ================================================================

        [HttpPatch("{id}/disponibilizar")]
        public async Task<IActionResult> Disponibilizar(int id)
        {
            var (exito, mensaje) = await _service.DisponibilizarVehiculoAsync(id);
            if (!exito) return UnprocessableEntity(new { mensaje });
            return Ok(new { mensaje });
        }
    }
}