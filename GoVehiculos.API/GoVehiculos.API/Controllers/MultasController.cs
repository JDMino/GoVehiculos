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
    public class MultasController : ControllerBase
    {
        private readonly MultaService _service;

        public MultasController(MultaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var multas = await _service.GetAllAsync();
            var dtoList = multas.Select(m => new MultaDTO
            {
                IdMulta = m.IdMulta,
                IncidenciaId = m.IncidenciaId,
                UsuarioId = m.UsuarioId,
                VehiculoId = m.VehiculoId,
                Tipo = m.Tipo,
                Monto = m.Monto,
                Descripcion = m.Descripcion,
                Estado = m.Estado,
                FechaCreacion = m.FechaCreacion
            });
            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var m = await _service.GetByIdAsync(id);
            if (m == null) return NotFound();

            var dto = new MultaDTO
            {
                IdMulta = m.IdMulta,
                IncidenciaId = m.IncidenciaId,
                UsuarioId = m.UsuarioId,
                VehiculoId = m.VehiculoId,
                Tipo = m.Tipo,
                Monto = m.Monto,
                Descripcion = m.Descripcion,
                Estado = m.Estado,
                FechaCreacion = m.FechaCreacion
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MultaDTO dto)
        {
            var m = new Multa
            {
                IncidenciaId = dto.IncidenciaId,
                UsuarioId = dto.UsuarioId,
                VehiculoId = dto.VehiculoId,
                Tipo = dto.Tipo,
                Monto = dto.Monto,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado,
                FechaCreacion = dto.FechaCreacion
            };

            var nueva = await _service.CreateAsync(m);
            return CreatedAtAction(nameof(GetById), new { id = nueva.IdMulta }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MultaDTO dto)
        {
            var m = new Multa
            {
                IdMulta = id,
                IncidenciaId = dto.IncidenciaId,
                UsuarioId = dto.UsuarioId,
                VehiculoId = dto.VehiculoId,
                Tipo = dto.Tipo,
                Monto = dto.Monto,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado,
                FechaCreacion = dto.FechaCreacion
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
