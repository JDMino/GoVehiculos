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
    public class PenalizacionesController : ControllerBase
    {
        private readonly PenalizacionService _service;

        public PenalizacionesController(PenalizacionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var penalizaciones = await _service.GetAllAsync();
            var dtoList = penalizaciones.Select(p => new PenalizacionDTO
            {
                IdPenalizacion = p.IdPenalizacion,
                UsuarioId = p.UsuarioId,
                MultaId = p.MultaId,
                IncidenciaId = p.IncidenciaId,
                Tipo = p.Tipo,
                Motivo = p.Motivo,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                Estado = p.Estado
            });
            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _service.GetByIdAsync(id);
            if (p == null) return NotFound();

            var dto = new PenalizacionDTO
            {
                IdPenalizacion = p.IdPenalizacion,
                UsuarioId = p.UsuarioId,
                MultaId = p.MultaId,
                IncidenciaId = p.IncidenciaId,
                Tipo = p.Tipo,
                Motivo = p.Motivo,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                Estado = p.Estado
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PenalizacionDTO dto)
        {
            var p = new Penalizacion
            {
                UsuarioId = dto.UsuarioId,
                MultaId = dto.MultaId,
                IncidenciaId = dto.IncidenciaId,
                Tipo = dto.Tipo,
                Motivo = dto.Motivo,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Estado = dto.Estado
            };

            var nueva = await _service.CreateAsync(p);
            return CreatedAtAction(nameof(GetById), new { id = nueva.IdPenalizacion }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PenalizacionDTO dto)
        {
            var p = new Penalizacion
            {
                IdPenalizacion = id,
                UsuarioId = dto.UsuarioId,
                MultaId = dto.MultaId,
                IncidenciaId = dto.IncidenciaId,
                Tipo = dto.Tipo,
                Motivo = dto.Motivo,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Estado = dto.Estado
            };

            var ok = await _service.UpdateAsync(id, p);
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
