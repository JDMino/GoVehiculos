﻿using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Observers;
using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Services
{
    /// <summary>
    /// Orquesta todas las operaciones del módulo de multas.
    ///
    /// PATRÓN OBSERVADOR — Sujeto concreto:
    /// MultaService hereda de Multa, que encapsula la lista de observadores
    /// y los métodos Registrar, Eliminar y NotificarAsync.
    /// MultaService expone su estado (TipoIncidencia, TipoPenalizacion,
    /// VehiculoId, UsuarioId) para que los observadores concretos lo consulten
    /// directamente mediante cast, respetando la estructura canónica del patrón.
    /// Al crear una multa, setea su estado y delega la notificación a la
    /// clase base mediante NotificarCreacionAsync.
    /// </summary>
    public class MultaService : MultaAbs
    {
        private readonly IMultaRepository _multaRepo;
        private readonly IPenalizacionRepository _penalizacionRepo;
        private readonly IVehiculoRepository _vehiculoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IncidenciaService _incidenciaService;
        private readonly PenalizacionService _penalizacionService;

        // ================================================================
        // Estado que los observadores concretos consultarán via cast
        // ================================================================

        public string TipoIncidencia { get; private set; } = string.Empty;
        public string TipoPenalizacion { get; private set; } = string.Empty;
        public int VehiculoId { get; private set; }
        public int UsuarioId { get; private set; }

        public MultaService(
            IMultaRepository multaRepo,
            IPenalizacionRepository penalizacionRepo,
            IVehiculoRepository vehiculoRepo,
            IUsuarioRepository usuarioRepo,
            IncidenciaService incidenciaService,
            PenalizacionService penalizacionService,
            IEnumerable<IMultaObserver> observadores)
        {
            _multaRepo           = multaRepo;
            _penalizacionRepo    = penalizacionRepo;
            _vehiculoRepo        = vehiculoRepo;
            _usuarioRepo         = usuarioRepo;
            _incidenciaService   = incidenciaService;
            _penalizacionService = penalizacionService;

            // Registra los observadores en la clase base Sujeto
            foreach (var obs in observadores)
                Registrar(obs);
        }

        // ================================================================
        // PATRÓN OBSERVADOR — Notificación
        // ================================================================

        /// <summary>
        /// Setea el estado del sujeto con los datos de la multa creada
        /// y delega la notificación a la clase base, que itera sobre
        /// todos los observadores registrados pasándose a sí misma.
        /// </summary>
        private async Task NotificarCreacionAsync(
            string tipoIncidencia,
            string tipoPenalizacion,
            int vehiculoId,
            int usuarioId)
        {
            TipoIncidencia   = tipoIncidencia;
            TipoPenalizacion = tipoPenalizacion;
            VehiculoId       = vehiculoId;
            UsuarioId        = usuarioId;
            await NotificarAsync();
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        public async Task<IEnumerable<MultaResponseDTO>> GetAllAsync(
            string? estado = null,
            string? tipoIncidencia = null,
            string? nivelGravedad = null)
        {
            var lista = await _multaRepo.GetAllAsync(estado, tipoIncidencia, nivelGravedad);

            var multaIds = lista.Select(m => m.IdMulta).ToList();
            var pens = await _penalizacionRepo.GetByMultaIdsAsync(multaIds);
            var penPorMultaId = pens
                .Where(p => p.MultaId.HasValue)
                .ToDictionary(p => p.MultaId!.Value);

            return lista.Select(m =>
            {
                penPorMultaId.TryGetValue(m.IdMulta, out var pen);
                return ToResponseDTO(m, pen);
            });
        }

        public async Task<MultaResponseDTO?> GetByIdAsync(int id)
        {
            var multa = await _multaRepo.GetByIdAsync(id);
            if (multa == null) return null;

            var pen = await _penalizacionRepo.GetByMultaIdAsync(id);
            return ToResponseDTO(multa, pen);
        }

        public async Task<IEnumerable<MultaResponseDTO>> GetByUsuarioAsync(int usuarioId, string? estado = null)
        {
            var lista = await _multaRepo.GetByUsuarioIdAsync(usuarioId, estado);

            var multaIds = lista.Select(m => m.IdMulta).ToList();
            var pens = await _penalizacionRepo.GetByMultaIdsAsync(multaIds);
            var penPorMultaId = pens
                .Where(p => p.MultaId.HasValue)
                .ToDictionary(p => p.MultaId!.Value);

            return lista.Select(m =>
            {
                penPorMultaId.TryGetValue(m.IdMulta, out var pen);
                return ToResponseDTO(m, pen);
            });
        }

        // ================================================================
        // CREACIÓN COMPLETA
        // ================================================================

        public async Task<(bool exito, string mensaje, MultaResponseDTO? dto)> CrearMultaCompletaAsync(
            IncidenciaCreateDTO incidenciaDto,
            MultaCreateDTO multaDto,
            PenalizacionCreateDTO penalizacionDto)
        {
            var errorIncidencia = ValidarCamposIncidencia(incidenciaDto);
            if (errorIncidencia != null) return (false, errorIncidencia, null);

            var errorMulta = ValidarCamposMulta(multaDto);
            if (errorMulta != null) return (false, errorMulta, null);

            var errorPenalizacion = ValidarCamposPenalizacion(penalizacionDto);
            if (errorPenalizacion != null) return (false, errorPenalizacion, null);

            var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(incidenciaDto.VehiculoId);
            if (vehiculo == null)
                return (false, "El vehículo indicado no existe.", null);

            var usuario = await _usuarioRepo.GetByIdSimpleAsync(incidenciaDto.UsuarioId);
            if (usuario == null)
                return (false, "El usuario indicado no existe.", null);

            var incidencia = await _incidenciaService.CrearAsync(incidenciaDto);

            var multa = new Multa
            {
                IncidenciaId  = incidencia.IdIncidencia,
                Tipo          = multaDto.Tipo.Trim().ToLower(),
                Monto         = multaDto.Monto,
                Descripcion   = multaDto.Descripcion?.Trim(),
                Estado        = "pendiente",
                FechaCreacion = DateTime.Now
            };
            await _multaRepo.AddAsync(multa);
            await _multaRepo.SaveChangesAsync();

            var (exitoPen, mensajePen, _) = await _penalizacionService.CrearAsync(penalizacionDto, multa.IdMulta);
            if (!exitoPen) return (false, mensajePen, null);

            // Setea el estado del sujeto y notifica a todos los observadores
            await NotificarCreacionAsync(
                incidenciaDto.Tipo.Trim().ToLower(),
                penalizacionDto.Tipo.Trim().ToLower(),
                incidenciaDto.VehiculoId,
                incidenciaDto.UsuarioId);

            var resultado = await GetByIdAsync(multa.IdMulta);
            return (true, "Multa creada correctamente.", resultado);
        }

        // ================================================================
        // ACTUALIZACIÓN
        // ================================================================

        public async Task<(bool exito, string mensaje)> UpdateAsync(int id, MultaUpdateDTO dto)
        {
            var errorCampos = ValidarCamposUpdate(dto);
            if (errorCampos != null) return (false, errorCampos);

            var multa = await _multaRepo.GetByIdSimpleAsync(id);
            if (multa == null) return (false, "Multa no encontrada.");

            if (multa.Estado == "cancelada")
                return (false, "Una multa cancelada no puede modificarse.");

            if (dto.Estado == "cancelada")
                return (false, "Para cancelar una multa use el endpoint dedicado PATCH /cancelar.");

            multa.Tipo        = dto.Tipo.Trim().ToLower();
            multa.Monto       = dto.Monto;
            multa.Descripcion = dto.Descripcion?.Trim();
            multa.Estado      = dto.Estado.Trim().ToLower();

            await _multaRepo.SaveChangesAsync();
            return (true, "Multa actualizada correctamente.");
        }

        // ================================================================
        // CANCELACIÓN
        // ================================================================

        public async Task<(bool exito, string mensaje)> CancelarAsync(int id, MultaCancelarDTO dto)
        {
            // 1. Ejecutar reglas de validación centralizadas
            var (esValido, mensajeValidacion) = ValidarCancelacion(dto);
            if (!esValido)
            {
                return (false, mensajeValidacion); // Corta la ejecución si falla
            }

            // 2. Invocar al repositorio (retorna el resultado y el mensaje nativo del SP)
            return await _multaRepo.CancelarConSPAsync(id, dto.MotivoCancelacion);
        }

        /// <summary>
        /// Centraliza todas las reglas de negocio previas a la ejecución del SP de cancelación.
        /// Permite añadir futuras validaciones de forma limpia sin llenar de IFs el método principal.
        /// </summary>
        private static (bool esValido, string mensaje) ValidarCancelacion(MultaCancelarDTO dto)
        {
            // Regla 1: Validar que el motivo no esté vacío ni sean puros espacios
            if (string.IsNullOrWhiteSpace(dto.MotivoCancelacion))
            {
                return (false, "El motivo de la cancelación es obligatorio y no puede estar vacío.");
            }
            
            return (true, string.Empty); // Pasó todas las validaciones
        }

        // ================================================================
        // Validaciones privadas
        // ================================================================

        private static readonly string[] TiposIncidenciaValidos =
        [
            "daño_fisico", "accidente", "infraccion_vial",
            "comportamiento_indebido", "retraso_en_pago"
        ];

        private static readonly string[] NivelesGravedadValidos =
        [
            "baja", "media", "alta"
        ];

        private static readonly string[] TiposMultaValidos =
        [
            "economica", "administrativa", "mixta"
        ];

        private static readonly string[] EstadosMultaEditables =
        [
            "pendiente", "pagada"
        ];

        private static readonly string[] TiposPenalizacionValidos =
        [
            "suspension_temporal", "bloqueo_cuenta",
            "inhabilitacion_vehiculo", "advertencia"
        ];

        private static string? ValidarCamposIncidencia(IncidenciaCreateDTO dto)
        {
            if (dto.UsuarioId <= 0) return "El usuario es obligatorio.";
            if (dto.VehiculoId <= 0) return "El vehículo es obligatorio.";
            if (string.IsNullOrWhiteSpace(dto.Tipo))
                return "El tipo de incidencia es obligatorio.";
            if (!TiposIncidenciaValidos.Contains(dto.Tipo.Trim().ToLower()))
                return $"Tipo de incidencia inválido. Valores permitidos: {string.Join(", ", TiposIncidenciaValidos)}.";
            if (string.IsNullOrWhiteSpace(dto.NivelGravedad))
                return "El nivel de gravedad es obligatorio.";
            if (!NivelesGravedadValidos.Contains(dto.NivelGravedad.Trim().ToLower()))
                return $"Nivel de gravedad inválido. Valores permitidos: {string.Join(", ", NivelesGravedadValidos)}.";
            if (string.IsNullOrWhiteSpace(dto.Descripcion))
                return "La descripción de la incidencia es obligatoria.";
            return null;
        }

        private static string? ValidarCamposMulta(MultaCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tipo))
                return "El tipo de multa es obligatorio.";
            if (!TiposMultaValidos.Contains(dto.Tipo.Trim().ToLower()))
                return $"Tipo de multa inválido. Valores permitidos: {string.Join(", ", TiposMultaValidos)}.";
            if (dto.Monto < 0)
                return "El monto no puede ser negativo.";
            return null;
        }

        private static string? ValidarCamposPenalizacion(PenalizacionCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tipo))
                return "El tipo de penalización es obligatorio.";
            if (!TiposPenalizacionValidos.Contains(dto.Tipo.Trim().ToLower()))
                return $"Tipo de penalización inválido. Valores permitidos: {string.Join(", ", TiposPenalizacionValidos)}.";
            if (string.IsNullOrWhiteSpace(dto.Motivo))
                return "El motivo de la penalización es obligatorio.";
            return null;
        }

        private static string? ValidarCamposUpdate(MultaUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tipo))
                return "El tipo de multa es obligatorio.";
            if (!TiposMultaValidos.Contains(dto.Tipo.Trim().ToLower()))
                return $"Tipo de multa inválido. Valores permitidos: {string.Join(", ", TiposMultaValidos)}.";
            if (dto.Monto < 0)
                return "El monto no puede ser negativo.";
            if (string.IsNullOrWhiteSpace(dto.Estado))
                return "El estado de la multa es obligatorio.";
            if (!EstadosMultaEditables.Contains(dto.Estado.Trim().ToLower()))
                return $"Estado inválido desde edición. Valores permitidos: {string.Join(", ", EstadosMultaEditables)}.";
            return null;
        }

        // ================================================================
        // Mapeo privado
        // ================================================================

        private static MultaResponseDTO ToResponseDTO(Multa m, Penalizacion? pen = null) => new()
        {
            IdMulta     = m.IdMulta,
            Tipo        = m.Tipo,
            Monto       = m.Monto,
            Descripcion = m.Descripcion,
            Estado      = m.Estado,
            FechaCreacion = m.FechaCreacion,

            IncidenciaId            = m.IncidenciaId,
            IncidenciaTipo          = m.Incidencia?.Tipo ?? string.Empty,
            IncidenciaNivelGravedad = m.Incidencia?.NivelGravedad ?? string.Empty,
            IncidenciaDescripcion   = m.Incidencia?.Descripcion ?? string.Empty,
            IncidenciaFechaReporte  = m.Incidencia?.FechaReporte ?? DateTime.MinValue,

            UsuarioId             = m.Incidencia?.UsuarioId ?? 0,
            UsuarioNombreCompleto = m.Incidencia?.Usuario != null
                                        ? $"{m.Incidencia.Usuario.Nombre} {m.Incidencia.Usuario.Apellido}"
                                        : string.Empty,

            VehiculoId      = m.Incidencia?.VehiculoId ?? 0,
            VehiculoPatente = m.Incidencia?.Vehiculo?.Patente ?? string.Empty,
            VehiculoMarca   = m.Incidencia?.Vehiculo?.Modelo?.Marca?.Nombre ?? string.Empty,
            VehiculoModelo  = m.Incidencia?.Vehiculo?.Modelo?.Nombre ?? string.Empty,

            IdPenalizacion          = pen?.IdPenalizacion,
            PenalizacionTipo        = pen?.Tipo,
            PenalizacionMotivo      = pen?.Motivo,
            PenalizacionEstado      = pen?.Estado,
            PenalizacionFechaInicio = pen?.FechaInicio,
            PenalizacionFechaFin    = pen?.FechaFin,
        };
    }
}