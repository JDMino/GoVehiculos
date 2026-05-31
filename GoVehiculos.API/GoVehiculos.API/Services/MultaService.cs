using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Observers;
using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Services
{
    /// <summary>
    /// Orquesta todas las operaciones del módulo de multas.
    ///
    /// PATRÓN OBSERVADOR — Sujeto:
    /// MultaService mantiene su rol de sujeto intacto. Los observadores
    /// se notifican al crear una multa exactamente igual que antes.
    /// El SP reemplaza únicamente la cancelación, que no tenía observadores
    /// asociados (la cancelación no dispara efectos secundarios sobre
    /// otras entidades por decisión de negocio documentada).
    /// </summary>
    public class MultaService
    {
        private readonly IMultaRepository        _multaRepo;
        private readonly IPenalizacionRepository _penalizacionRepo;
        private readonly IVehiculoRepository     _vehiculoRepo;
        private readonly IUsuarioRepository      _usuarioRepo;
        private readonly IncidenciaService       _incidenciaService;
        private readonly PenalizacionService     _penalizacionService;

        private readonly List<IMultaObserver> _observadores;

        public MultaService(
            IMultaRepository        multaRepo,
            IPenalizacionRepository penalizacionRepo,
            IVehiculoRepository     vehiculoRepo,
            IUsuarioRepository      usuarioRepo,
            IncidenciaService       incidenciaService,
            PenalizacionService     penalizacionService,
            IEnumerable<IMultaObserver> observadores)
        {
            _multaRepo           = multaRepo;
            _penalizacionRepo    = penalizacionRepo;
            _vehiculoRepo        = vehiculoRepo;
            _usuarioRepo         = usuarioRepo;
            _incidenciaService   = incidenciaService;
            _penalizacionService = penalizacionService;
            _observadores        = observadores.ToList();
        }

        // ================================================================
        // PATRÓN OBSERVADOR — Métodos del Sujeto (sin cambios)
        // ================================================================

        public void Registrar(IMultaObserver observador)
            => _observadores.Add(observador);

        public void Eliminar(IMultaObserver observador)
            => _observadores.Remove(observador);

        private async Task NotificarAsync(
            string tipoIncidencia,
            string tipoPenalizacion,
            int    vehiculoId,
            int    usuarioId)
        {
            foreach (var observador in _observadores)
                await observador.ActualizarAsync(tipoIncidencia, tipoPenalizacion, vehiculoId, usuarioId);
        }

        // ================================================================
        // CONSULTAS (sin cambios)
        // ================================================================

        public async Task<IEnumerable<MultaResponseDTO>> GetAllAsync(
            string? estado         = null,
            string? tipoIncidencia = null,
            string? nivelGravedad  = null)
        {
            var lista = await _multaRepo.GetAllAsync(estado, tipoIncidencia, nivelGravedad);

            var multaIds      = lista.Select(m => m.IdMulta).ToList();
            var pens          = await _penalizacionRepo.GetByMultaIdsAsync(multaIds);
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

        // ================================================================
        // CREACIÓN COMPLETA — Observer intacto (sin cambios)
        // ================================================================

        public async Task<(bool exito, string mensaje, MultaResponseDTO? dto)> CrearMultaCompletaAsync(
            IncidenciaCreateDTO   incidenciaDto,
            MultaCreateDTO        multaDto,
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

            await NotificarAsync(
                incidenciaDto.Tipo.Trim().ToLower(),
                penalizacionDto.Tipo.Trim().ToLower(),
                incidenciaDto.VehiculoId,
                incidenciaDto.UsuarioId);

            var resultado = await GetByIdAsync(multa.IdMulta);
            return (true, "Multa creada correctamente.", resultado);
        }

        // ================================================================
        // ACTUALIZACIÓN (sin cambios)
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
        // CANCELACIÓN — con SP
        //
        // ANTES: este método cargaba la multa con GetByIdSimpleAsync,
        // modificaba Estado y Descripcion en memoria, luego llamaba
        // PenalizacionRepository.GetByMultaIdAsync() para cargar la
        // penalización y modificaba su Estado en memoria, y finalmente
        // SaveChangesAsync() emitía los dos UPDATE como statements
        // separados. En total: 2 queries + 1 SaveChanges.
        //
        // DESPUÉS: delega directamente a _multaRepo.CancelarConSPAsync().
        // El SP verifica existencia y estado previo, construye la
        // descripción con el motivo, y actualiza Multa y Penalizacion
        // en una única transacción atómica. El service queda como un
        // método delgado que solo valida campos y delega.
        // En total: 1 llamada a la BD.
        // ================================================================

        public async Task<(bool exito, string mensaje)> CancelarAsync(int id, MultaCancelarDTO dto)
        {
            return await _multaRepo.CancelarConSPAsync(id, dto.MotivoCancelacion);
        }

        // ================================================================
        // Validaciones privadas (sin cambios)
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
            if (dto.UsuarioId  <= 0) return "El usuario es obligatorio.";
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
        // Mapeo privado (sin cambios)
        // ================================================================

        private static MultaResponseDTO ToResponseDTO(Multa m, Penalizacion? pen = null) => new()
        {
            IdMulta       = m.IdMulta,
            Tipo          = m.Tipo,
            Monto         = m.Monto,
            Descripcion   = m.Descripcion,
            Estado        = m.Estado,
            FechaCreacion = m.FechaCreacion,

            IncidenciaId            = m.IncidenciaId,
            IncidenciaTipo          = m.Incidencia?.Tipo              ?? string.Empty,
            IncidenciaNivelGravedad = m.Incidencia?.NivelGravedad     ?? string.Empty,
            IncidenciaDescripcion   = m.Incidencia?.Descripcion       ?? string.Empty,
            IncidenciaFechaReporte  = m.Incidencia?.FechaReporte      ?? DateTime.MinValue,

            UsuarioId             = m.Incidencia?.UsuarioId ?? 0,
            UsuarioNombreCompleto = m.Incidencia?.Usuario != null
                                        ? $"{m.Incidencia.Usuario.Nombre} {m.Incidencia.Usuario.Apellido}"
                                        : string.Empty,

            VehiculoId      = m.Incidencia?.VehiculoId                      ?? 0,
            VehiculoPatente = m.Incidencia?.Vehiculo?.Patente               ?? string.Empty,
            VehiculoMarca   = m.Incidencia?.Vehiculo?.Modelo?.Marca?.Nombre  ?? string.Empty,
            VehiculoModelo  = m.Incidencia?.Vehiculo?.Modelo?.Nombre         ?? string.Empty,

            IdPenalizacion          = pen?.IdPenalizacion,
            PenalizacionTipo        = pen?.Tipo,
            PenalizacionMotivo      = pen?.Motivo,
            PenalizacionEstado      = pen?.Estado,
            PenalizacionFechaInicio = pen?.FechaInicio,
            PenalizacionFechaFin    = pen?.FechaFin,
        };
    }
}