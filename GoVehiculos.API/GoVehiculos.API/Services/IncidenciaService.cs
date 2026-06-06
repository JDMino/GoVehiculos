﻿using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Services
{
    /// <summary>
    /// Gestiona todas las operaciones sobre la entidad Incidencia.
    /// Incluye la creación de instancias propias del flujo de multas,
    /// delegada desde MultaService quien orquesta el proceso completo.
    /// </summary>
    public class IncidenciaService
    {
        private readonly IIncidenciaRepository _repo;
        private readonly IVehiculoRepository _vehiculoRepo;
        private readonly IUsuarioRepository _usuarioRepo;

        public IncidenciaService(
            IIncidenciaRepository repo,
            IVehiculoRepository vehiculoRepo,
            IUsuarioRepository usuarioRepo)
        {
            _repo = repo;
            _vehiculoRepo = vehiculoRepo;
            _usuarioRepo = usuarioRepo;
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        public async Task<IEnumerable<IncidenciaResponseDTO>> GetAllAsync()
        {
            var lista = await _repo.GetAllAsync();
            return lista.Select(ToResponseDTO);
        }

        public async Task<IncidenciaResponseDTO?> GetByIdAsync(int id)
        {
            var incidencia = await _repo.GetByIdAsync(id);
            return incidencia == null ? null : ToResponseDTO(incidencia);
        }

        // ================================================================
        // CREACIÓN
        // ================================================================

        /// <summary>
        /// Construye y persiste una nueva Incidencia a partir del DTO.
        /// Llamado por MultaService durante el flujo de creación completa
        /// de una multa, después de que las validaciones y verificaciones
        /// de existencia ya fueron ejecutadas.
        ///
        /// Devuelve la entidad persistida con su IdIncidencia generado
        /// por la BD, que MultaService usa para asignar la FK en Multa.
        ///
        /// FechaReporte se fija aquí al momento de creación, no depende
        /// del frontend.
        /// </summary>
        public async Task<Incidencia> CrearAsync(IncidenciaCreateDTO dto)
        {
            var incidencia = new Incidencia
            {
                UsuarioId = dto.UsuarioId,
                VehiculoId = dto.VehiculoId,
                Tipo = dto.Tipo.Trim().ToLower(),
                NivelGravedad = dto.NivelGravedad.Trim().ToLower(),
                Descripcion = dto.Descripcion.Trim(),
                FechaReporte = DateTime.Now
            };

            await _repo.AddAsync(incidencia);
            await _repo.SaveChangesAsync();

            return incidencia;
        }

        // ================================================================
        // ACTUALIZACIÓN
        // ================================================================

        /// <summary>
        /// Actualiza los campos editables de una Incidencia existente.
        /// UsuarioId y VehiculoId no son modificables (ver IncidenciaUpdateDTO).
        /// </summary>
        public async Task<(bool exito, string mensaje)> UpdateAsync(int id, IncidenciaUpdateDTO dto)
        {
            var errorCampos = ValidarCamposUpdate(dto);
            if (errorCampos != null) return (false, errorCampos);

            var incidencia = await _repo.GetByIdAsync(id);
            if (incidencia == null) return (false, "Incidencia no encontrada.");

            var tipoAnterior = incidencia.Tipo;
            var tipoNuevo = dto.Tipo.Trim().ToLower();

            incidencia.Tipo = tipoNuevo;
            incidencia.NivelGravedad = dto.NivelGravedad.Trim().ToLower();
            incidencia.Descripcion = dto.Descripcion.Trim();

            // Efecto secundario si el tipo cambió a daño_fisico
            if (tipoNuevo != tipoAnterior && tipoNuevo == "daño_fisico")
            {
                var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(incidencia.VehiculoId);
                if (vehiculo != null)
                    vehiculo.EstadoMecanico = "malo";
            }

            await _repo.SaveChangesAsync();
            return (true, "Incidencia actualizada correctamente.");
        }

        // ================================================================
        // Validaciones privadas
        // ================================================================

        private static readonly string[] TiposValidos =
        [
            "daño_fisico", "accidente", "infraccion_vial",
            "comportamiento_indebido", "retraso_en_pago"
        ];

        private static readonly string[] NivelesValidos =
        [
            "baja", "media", "alta"
        ];

        private static string? ValidarCamposUpdate(IncidenciaUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tipo))
                return "El tipo de incidencia es obligatorio.";
            if (!TiposValidos.Contains(dto.Tipo.Trim().ToLower()))
                return $"Tipo de incidencia inválido. Valores permitidos: {string.Join(", ", TiposValidos)}.";

            if (string.IsNullOrWhiteSpace(dto.NivelGravedad))
                return "El nivel de gravedad es obligatorio.";
            if (!NivelesValidos.Contains(dto.NivelGravedad.Trim().ToLower()))
                return $"Nivel de gravedad inválido. Valores permitidos: {string.Join(", ", NivelesValidos)}.";

            if (string.IsNullOrWhiteSpace(dto.Descripcion))
                return "La descripción es obligatoria.";

            return null;
        }

        // ================================================================
        // Mapeo privado
        // ================================================================

        private static IncidenciaResponseDTO ToResponseDTO(Incidencia i) => new()
        {
            IdIncidencia = i.IdIncidencia,
            UsuarioId = i.UsuarioId,
            UsuarioNombreCompleto = i.Usuario != null
                                        ? $"{i.Usuario.Nombre} {i.Usuario.Apellido}"
                                        : string.Empty,
            VehiculoId = i.VehiculoId,
            VehiculoPatente = i.Vehiculo?.Patente ?? string.Empty,
            VehiculoMarca = i.Vehiculo?.Modelo?.Marca?.Nombre ?? string.Empty,
            VehiculoModelo = i.Vehiculo?.Modelo?.Nombre ?? string.Empty,
            Tipo = i.Tipo,
            NivelGravedad = i.NivelGravedad,
            Descripcion = i.Descripcion,
            FechaReporte = i.FechaReporte
        };
    }
}