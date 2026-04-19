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
        // PARTE 1 — Endpoints del administrador
        // ================================================================

        /// <summary>
        /// GET /api/mantenimientos/candidatos
        /// Vehículos con estadoMecanico "regular" o "malo",
        /// enriquecidos con su orden activa si existe.
        /// </summary>
        [HttpGet("candidatos")]
        public async Task<IActionResult> GetCandidatos()
        {
            var vehiculos = await _service.GetVehiculosCandidatosAsync();
            return Ok(vehiculos);
        }

        /// <summary>
        /// GET /api/mantenimientos
        /// Todas las órdenes de mantenimiento (admin / reportes).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var resultado = await _service.GetAllAsync();
            return Ok(resultado);
        }

        /// <summary>
        /// GET /api/mantenimientos/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resultado = await _service.GetByIdAsync(id);
            if (resultado == null) return NotFound();
            return Ok(resultado);
        }

        /// <summary>
        /// POST /api/mantenimientos
        /// Crea una orden de mantenimiento (acción del administrador).
        /// EmpleadoId y FechaProgramada son obligatorios.
        /// 422 si el socio se hace cargo o ya hay una orden activa.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MantenimientoCreateDTO dto)
        {
            // ModelState valida los [Required] del DTO antes de llegar al service
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (exito, mensaje, orden) = await _service.CreateAsync(dto);

            if (!exito)
                return UnprocessableEntity(new { mensaje });

            return CreatedAtAction(nameof(GetById), new { id = orden!.IdMantenimiento }, orden);
        }

        /// <summary>
        /// POST /api/mantenimientos/habilitar-socio
        /// Registra el mantenimiento realizado por el socio y devuelve el vehículo a disponible.
        /// Solo aplica cuando el vehículo está en "fuera_de_servicio" por cargo del socio.
        /// RealizadoPor es fijo = "A cargo del Socio" — el service lo asigna, no el frontend.
        /// </summary>
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

        /// <summary>
        /// PUT /api/mantenimientos/{id}
        /// Actualización general (admin).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MantenimientoUpdateDTO dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// DELETE /api/mantenimientos/{id}
        /// </summary>
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

        /// <summary>
        /// GET /api/mantenimientos/empleado/{empleadoId}
        /// Mantenimientos asignados a un empleado específico (activos + terminales).
        /// </summary>
        [HttpGet("empleado/{empleadoId}")]
        public async Task<IActionResult> GetByEmpleado(int empleadoId)
        {
            var resultado = await _service.GetByEmpleadoAsync(empleadoId);
            return Ok(resultado);
        }

        /// <summary>
        /// PATCH /api/mantenimientos/{id}/iniciar
        /// Inicia el mantenimiento: "pendiente" → "iniciado".
        /// Query param: empleadoId
        /// </summary>
        [HttpPatch("{id}/iniciar")]
        public async Task<IActionResult> Iniciar(int id, [FromBody] MantenimientoIniciarDTO dto,
            [FromQuery] int empleadoId)
        {
            var (exito, mensaje) = await _service.IniciarAsync(id, empleadoId);

            if (!exito)
                return UnprocessableEntity(new { mensaje });

            return Ok(new { mensaje });
        }

        /// <summary>
        /// PATCH /api/mantenimientos/{id}/finalizar
        /// Finaliza el mantenimiento: "iniciado" → "finalizado".
        /// Query param: empleadoId
        /// </summary>
        [HttpPatch("{id}/finalizar")]
        public async Task<IActionResult> Finalizar(int id, [FromBody] MantenimientoFinalizarDTO dto,
            [FromQuery] int empleadoId)
        {
            var (exito, mensaje) = await _service.FinalizarAsync(id, empleadoId, dto);

            if (!exito)
                return UnprocessableEntity(new { mensaje });

            return Ok(new { mensaje });
        }

        /// <summary>
        /// PATCH /api/mantenimientos/{id}/cancelar
        /// Cancela el mantenimiento: "iniciado" → "cancelado".
        /// Query param: empleadoId
        /// </summary>
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id, [FromBody] MantenimientoCancelarDTO dto,
            [FromQuery] int empleadoId)
        {
            var (exito, mensaje) = await _service.CancelarAsync(id, empleadoId, dto);

            if (!exito)
                return UnprocessableEntity(new { mensaje });

            return Ok(new { mensaje });
        }
    }
}
