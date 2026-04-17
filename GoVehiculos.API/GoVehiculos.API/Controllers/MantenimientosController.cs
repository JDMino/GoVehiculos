using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var mantenimientos = await _service.GetAllAsync();
            var dtoList = mantenimientos.Select(m => new MantenimientoDTO
            {
                IdMantenimiento = m.IdMantenimiento,
                VehiculoId = m.VehiculoId,
                EmpleadoId = m.EmpleadoId,
                Tipo = m.Tipo,
                Descripcion = m.Descripcion,
                Estado = m.Estado,
                Prioridad = m.Prioridad,
                FechaProgramada = m.FechaProgramada,
                FechaRealizacion = m.FechaRealizacion,
                Costo = m.Costo,
                RealizadoPor = m.RealizadoPor
            });
            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var m = await _service.GetByIdAsync(id);
            if (m == null) return NotFound();

            var dto = new MantenimientoDTO
            {
                IdMantenimiento = m.IdMantenimiento,
                VehiculoId = m.VehiculoId,
                EmpleadoId = m.EmpleadoId,
                Tipo = m.Tipo,
                Descripcion = m.Descripcion,
                Estado = m.Estado,
                Prioridad = m.Prioridad,
                FechaProgramada = m.FechaProgramada,
                FechaRealizacion = m.FechaRealizacion,
                Costo = m.Costo,
                RealizadoPor = m.RealizadoPor
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MantenimientoDTO dto)
        {
            var m = new Mantenimiento
            {
                VehiculoId = dto.VehiculoId,
                EmpleadoId = dto.EmpleadoId,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado,
                Prioridad = dto.Prioridad,
                FechaProgramada = dto.FechaProgramada,
                FechaRealizacion = dto.FechaRealizacion,
                Costo = dto.Costo,
                RealizadoPor = dto.RealizadoPor
            };

            var nuevo = await _service.CreateAsync(m);
            return CreatedAtAction(nameof(GetById), new { id = nuevo.IdMantenimiento }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MantenimientoDTO dto)
        {
            var m = new Mantenimiento
            {
                IdMantenimiento = id,
                VehiculoId = dto.VehiculoId,
                EmpleadoId = dto.EmpleadoId,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado,
                Prioridad = dto.Prioridad,
                FechaProgramada = dto.FechaProgramada,
                FechaRealizacion = dto.FechaRealizacion,
                Costo = dto.Costo,
                RealizadoPor = dto.RealizadoPor
            };

            var ok = await _service.UpdateAsync(id, m);
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
    }
}
