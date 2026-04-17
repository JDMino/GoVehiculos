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
    public class IncidenciasController : ControllerBase
    {
        private readonly IncidenciaService _service;

        public IncidenciasController(IncidenciaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var incidencias = await _service.GetAllAsync();
            var dtoList = incidencias.Select(i => new IncidenciaDTO
            {
                IdIncidencia = i.IdIncidencia,
                UsuarioId = i.UsuarioId,
                VehiculoId = i.VehiculoId,
                Tipo = i.Tipo,
                Descripcion = i.Descripcion,
                NivelGravedad = i.NivelGravedad,
                Estado = i.Estado,
                FechaReporte = i.FechaReporte
            });
            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var i = await _service.GetByIdAsync(id);
            if (i == null) return NotFound();

            var dto = new IncidenciaDTO
            {
                IdIncidencia = i.IdIncidencia,
                UsuarioId = i.UsuarioId,
                VehiculoId = i.VehiculoId,
                Tipo = i.Tipo,
                Descripcion = i.Descripcion,
                NivelGravedad = i.NivelGravedad,
                Estado = i.Estado,
                FechaReporte = i.FechaReporte
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IncidenciaDTO dto)
        {
            var i = new Incidencia
            {
                UsuarioId = dto.UsuarioId,
                VehiculoId = dto.VehiculoId,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                NivelGravedad = dto.NivelGravedad,
                Estado = dto.Estado,
                FechaReporte = dto.FechaReporte
            };

            var nueva = await _service.CreateAsync(i);
            return CreatedAtAction(nameof(GetById), new { id = nueva.IdIncidencia }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, IncidenciaDTO dto)
        {
            var i = new Incidencia
            {
                IdIncidencia = id,
                UsuarioId = dto.UsuarioId,
                VehiculoId = dto.VehiculoId,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                NivelGravedad = dto.NivelGravedad,
                Estado = dto.Estado,
                FechaReporte = dto.FechaReporte
            };

            var ok = await _service.UpdateAsync(id, i);
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
