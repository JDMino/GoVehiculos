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
    /// MultaService actúa como Sujeto del patrón. Mantiene una lista de
    /// observadores registrados y los notifica una vez que la multa completa
    /// fue persistida. Cada observador decide de forma autónoma si debe actuar
    /// según los tipos recibidos, aplicando su efecto secundario sin que el
    /// servicio conozca los detalles de ninguno.
    ///
    /// Esto reemplaza los ifs hardcodeados de efectos secundarios que existían
    /// anteriormente. Agregar un nuevo efecto (ej: enviar un email) implica
    /// solo crear un nuevo observador y registrarlo en Program.cs, sin tocar
    /// este archivo (Open/Closed Principle).
    ///
    /// La delegación de construcción de entidades a sus servicios propios
    /// (IncidenciaService.CrearAsync, PenalizacionService.CrearAsync) mantiene
    /// la separación de responsabilidades: cada servicio conoce cómo construir
    /// y persistir su propia entidad, y MultaService solo orquesta el flujo.
    /// </summary>
    public class MultaService
    {
        private readonly IMultaRepository _multaRepo;
        private readonly IPenalizacionRepository _penalizacionRepo;
        private readonly IVehiculoRepository _vehiculoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IncidenciaService _incidenciaService;
        private readonly PenalizacionService _penalizacionService;

        // ── PATRÓN OBSERVADOR — Lista de observadores registrados ────────
        private readonly List<IMultaObserver> _observadores;

        public MultaService(
            IMultaRepository multaRepo,
            IPenalizacionRepository penalizacionRepo,
            IVehiculoRepository vehiculoRepo,
            IUsuarioRepository usuarioRepo,
            IncidenciaService incidenciaService,
            PenalizacionService penalizacionService,
            IEnumerable<IMultaObserver> observadores)
        {
            _multaRepo = multaRepo;
            _penalizacionRepo = penalizacionRepo;
            _vehiculoRepo = vehiculoRepo;
            _usuarioRepo = usuarioRepo;
            _incidenciaService = incidenciaService;
            _penalizacionService = penalizacionService;
            _observadores = observadores.ToList();
        }

        // ================================================================
        // PATRÓN OBSERVADOR — Métodos del Sujeto
        // ================================================================

        /// <summary>
        /// Registra un observador en la lista del sujeto.
        /// En este proyecto los observadores se inyectan por DI en el constructor,
        /// pero este método permite registros dinámicos si fuera necesario en el futuro.
        /// </summary>
        public void Registrar(IMultaObserver observador)
            => _observadores.Add(observador);

        /// <summary>
        /// Elimina un observador de la lista del sujeto.
        /// </summary>
        public void Eliminar(IMultaObserver observador)
            => _observadores.Remove(observador);

        /// <summary>
        /// Notifica a todos los observadores registrados pasándoles los datos
        /// del evento "multa creada". Cada observador decide internamente
        /// si debe actuar según los tipos recibidos.
        /// </summary>
        private async Task NotificarAsync(
            string tipoIncidencia,
            string tipoPenalizacion,
            int vehiculoId,
            int usuarioId)
        {
            foreach (var observador in _observadores)
                await observador.ActualizarAsync(tipoIncidencia, tipoPenalizacion, vehiculoId, usuarioId);
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        /// <summary>
        /// Devuelve todas las multas con sus penalizaciones resueltas en una
        /// sola query extra (no N+1). El repositorio carga en lote todas las
        /// penalizaciones de los IDs involucrados y el servicio las indexa
        /// en un diccionario para el mapeo.
        /// </summary>
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

        /// <summary>
        /// Devuelve una multa por ID con su penalización asociada resuelta.
        /// La penalización se busca con GetByMultaIdAsync (query separada)
        /// porque EF no puede hacer Include desde Multa hacia Penalización
        /// dado que la FK está en la dirección inversa.
        /// </summary>
        public async Task<MultaResponseDTO?> GetByIdAsync(int id)
        {
            var multa = await _multaRepo.GetByIdAsync(id);
            if (multa == null) return null;

            var pen = await _penalizacionRepo.GetByMultaIdAsync(id);
            return ToResponseDTO(multa, pen);
        }

        // ================================================================
        // CREACIÓN COMPLETA — Flujo de las 3 entidades con Observer
        // ================================================================

        /// <summary>
        /// Crea una multa completa (Incidencia + Multa + Penalización) en una
        /// secuencia de persistencias que respeta las dependencias de FK,
        /// delegando la construcción de cada entidad a su servicio propio.
        ///
        /// Una vez persistidas las tres entidades, notifica a los observadores
        /// registrados para que apliquen los efectos secundarios correspondientes
        /// (actualizar estado mecánico, bloquear usuario, inhabilitar vehículo).
        /// </summary>
        public async Task<(bool exito, string mensaje, MultaResponseDTO? dto)> CrearMultaCompletaAsync(
            IncidenciaCreateDTO incidenciaDto,
            MultaCreateDTO multaDto,
            PenalizacionCreateDTO penalizacionDto)
        {
            // ── Validaciones de campos ───────────────────────────────────
            var errorIncidencia = ValidarCamposIncidencia(incidenciaDto);
            if (errorIncidencia != null) return (false, errorIncidencia, null);

            var errorMulta = ValidarCamposMulta(multaDto);
            if (errorMulta != null) return (false, errorMulta, null);

            var errorPenalizacion = ValidarCamposPenalizacion(penalizacionDto);
            if (errorPenalizacion != null) return (false, errorPenalizacion, null);

            // ── Verificaciones de existencia ─────────────────────────────
            var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(incidenciaDto.VehiculoId);
            if (vehiculo == null)
                return (false, "El vehículo indicado no existe.", null);

            var usuario = await _usuarioRepo.GetByIdSimpleAsync(incidenciaDto.UsuarioId);
            if (usuario == null)
                return (false, "El usuario indicado no existe.", null);

            // ── Paso 1: IncidenciaService construye y persiste la Incidencia ──
            // Devuelve la entidad con IdIncidencia generado por la BD.
            var incidencia = await _incidenciaService.CrearAsync(incidenciaDto);

            // ── Paso 2: MultaService construye y persiste la Multa ────────
            // Asigna la FK con el ID recién generado.
            var multa = new Multa
            {
                IncidenciaId = incidencia.IdIncidencia,
                Tipo = multaDto.Tipo.Trim().ToLower(),
                Monto = multaDto.Monto,
                Descripcion = multaDto.Descripcion?.Trim(),
                Estado = "pendiente",
                FechaCreacion = DateTime.Now
            };
            await _multaRepo.AddAsync(multa);
            await _multaRepo.SaveChangesAsync();

            // ── Paso 3: PenalizacionService construye y persiste la Penalización ──
            // Asigna la FK con el ID de la Multa recién generado.
            var (exitoPen, mensajePen, _) = await _penalizacionService.CrearAsync(penalizacionDto, multa.IdMulta);
            if (!exitoPen) return (false, mensajePen, null);

            // ── Paso 4: Notificar a los observadores (efectos secundarios) ──
            // PATRÓN OBSERVADOR — Notificación:
            // El sujeto no sabe qué observadores actuarán ni cómo. Solo pasa
            // los datos del evento. Cada observador decide de forma autónoma.
            await NotificarAsync(
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

        /// <summary>
        /// Actualiza los campos editables de una Multa existente.
        /// No permite pasar el estado a "cancelada" desde este método.
        /// </summary>
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

            multa.Tipo = dto.Tipo.Trim().ToLower();
            multa.Monto = dto.Monto;
            multa.Descripcion = dto.Descripcion?.Trim();
            multa.Estado = dto.Estado.Trim().ToLower();

            await _multaRepo.SaveChangesAsync();
            return (true, "Multa actualizada correctamente.");
        }

        // ================================================================
        // CANCELACIÓN
        // ================================================================

        /// <summary>
        /// Cancela una multa y revoca su penalización asociada.
        /// Esta es la única vía para establecer multa.estado = "cancelada".
        ///
        /// Efectos deliberadamente NO revertidos por los observadores:
        ///   - Vehiculo.EstadoMecanico: requiere una orden de mantenimiento.
        ///   - Usuario.Bloqueado: requiere intervención explícita del administrador.
        /// </summary>
        public async Task<(bool exito, string mensaje)> CancelarAsync(int id, MultaCancelarDTO dto)
        {
            var multa = await _multaRepo.GetByIdSimpleAsync(id);
            if (multa == null) return (false, "Multa no encontrada.");

            if (multa.Estado == "cancelada")
                return (false, "La multa ya fue cancelada anteriormente.");

            var motivoTexto = string.IsNullOrWhiteSpace(dto.MotivoCancelacion)
                ? string.Empty
                : $" | CANCELADA: {dto.MotivoCancelacion.Trim()}";

            multa.Estado = "cancelada";
            multa.Descripcion = (multa.Descripcion ?? string.Empty) + motivoTexto;

            var penalizacion = await _penalizacionRepo.GetByMultaIdAsync(id);
            if (penalizacion != null)
                penalizacion.Estado = "revocada";

            await _multaRepo.SaveChangesAsync();
            return (true, "Multa cancelada y penalización revocada correctamente.");
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
            IdMulta = m.IdMulta,
            Tipo = m.Tipo,
            Monto = m.Monto,
            Descripcion = m.Descripcion,
            Estado = m.Estado,
            FechaCreacion = m.FechaCreacion,

            IncidenciaId = m.IncidenciaId,
            IncidenciaTipo = m.Incidencia?.Tipo ?? string.Empty,
            IncidenciaNivelGravedad = m.Incidencia?.NivelGravedad ?? string.Empty,
            IncidenciaDescripcion = m.Incidencia?.Descripcion ?? string.Empty,
            IncidenciaFechaReporte = m.Incidencia?.FechaReporte ?? DateTime.MinValue,

            UsuarioId = m.Incidencia?.UsuarioId ?? 0,
            UsuarioNombreCompleto = m.Incidencia?.Usuario != null
                                        ? $"{m.Incidencia.Usuario.Nombre} {m.Incidencia.Usuario.Apellido}"
                                        : string.Empty,

            VehiculoId = m.Incidencia?.VehiculoId ?? 0,
            VehiculoPatente = m.Incidencia?.Vehiculo?.Patente ?? string.Empty,
            VehiculoMarca = m.Incidencia?.Vehiculo?.Modelo?.Marca?.Nombre ?? string.Empty,
            VehiculoModelo = m.Incidencia?.Vehiculo?.Modelo?.Nombre ?? string.Empty,

            IdPenalizacion = pen?.IdPenalizacion,
            PenalizacionTipo = pen?.Tipo,
            PenalizacionMotivo = pen?.Motivo,
            PenalizacionEstado = pen?.Estado,
            PenalizacionFechaInicio = pen?.FechaInicio,
            PenalizacionFechaFin = pen?.FechaFin,
        };
    }
}