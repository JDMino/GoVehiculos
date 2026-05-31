using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;

namespace GoVehiculos.API.Services
{
    /// <summary>
    /// Gestiona todas las operaciones sobre la entidad Penalización.
    /// Incluye la creación de instancias propias del flujo de multas,
    /// delegada desde MultaService quien orquesta el proceso completo.
    ///
    /// La revocación de una penalización al cancelar una multa la gestiona
    /// MultaService directamente, ya que es parte de una misma unidad de
    /// trabajo que involucra a Multa y Penalización en conjunto.
    /// </summary>
    public class PenalizacionService
    {
        private readonly IPenalizacionRepository _repo;
        private readonly IVehiculoRepository _vehiculoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IMultaRepository _multaRepo;

        public PenalizacionService(
            IPenalizacionRepository repo,
            IVehiculoRepository vehiculoRepo,
            IUsuarioRepository usuarioRepo,
            IMultaRepository multaRepo)
        {
            _repo = repo;
            _vehiculoRepo = vehiculoRepo;
            _usuarioRepo = usuarioRepo;
            _multaRepo = multaRepo;
        }

        // ================================================================
        // CONSULTAS
        // ================================================================

        public async Task<IEnumerable<PenalizacionResponseDTO>> GetAllAsync(string? estado = null)
        {
            var lista = await _repo.GetAllAsync(estado);
            return lista.Select(ToResponseDTO);
        }

        public async Task<PenalizacionResponseDTO?> GetByIdAsync(int id)
        {
            var penalizacion = await _repo.GetByIdAsync(id);
            return penalizacion == null ? null : ToResponseDTO(penalizacion);
        }

        // ================================================================
        // CREACIÓN
        // ================================================================

        /// <summary>
        /// Construye y persiste una nueva Penalización a partir del DTO.
        /// Llamado por MultaService durante el flujo de creación completa
        /// de una multa, después de que la Multa fue persistida y su ID
        /// está disponible para asignar la FK.
        ///
        /// FechaInicio se fija aquí al momento de creación.
        /// Estado siempre "activa" al crear, no depende del frontend.
        ///
        /// Los efectos secundarios sobre Vehículo/Usuario al crear se gestionan
        /// mediante los Observadores registrados en MultaService, no aquí.
        ///
        /// Validación de FechaFin: si se provee, debe ser posterior a
        /// FechaInicio (DateTime.Now al momento de creación).
        /// </summary>
        public async Task<(bool exito, string mensaje, Penalizacion? entidad)> CrearAsync(
            PenalizacionCreateDTO dto,
            int multaId)
        {
            var fechaInicio = DateTime.Now;

            if (dto.FechaFin.HasValue && dto.FechaFin.Value <= fechaInicio)
                return (false, "La fecha de fin debe ser posterior a la fecha de inicio de la penalización.", null);

            var penalizacion = new Penalizacion
            {
                MultaId = multaId,
                Tipo = dto.Tipo.Trim().ToLower(),
                Motivo = dto.Motivo.Trim(),
                FechaInicio = fechaInicio,
                FechaFin = dto.FechaFin,
                Estado = "activa"
            };

            await _repo.AddAsync(penalizacion);
            await _repo.SaveChangesAsync();

            return (true, string.Empty, penalizacion);
        }

        // ================================================================
        // ACTUALIZACIÓN
        // ================================================================

        /// <summary>
        /// Actualiza los campos editables de una Penalización existente y aplica
        /// los efectos secundarios correspondientes al nuevo tipo seleccionado.
        ///
        /// A diferencia de la creación (donde los efectos los gestionan los
        /// Observadores del patrón Observer), en la edición los efectos se aplican
        /// directamente aquí porque:
        ///   1. El evento que dispara los observadores es "multa creada", no "penalización editada".
        ///   2. PenalizacionService ya tiene acceso a los repositorios necesarios.
        ///   3. Centralizar la lógica aquí evita que la edición quede sin efecto.
        ///
        /// Efectos secundarios por tipo:
        ///   bloqueo_cuenta          → Usuario.Bloqueado = true
        ///   inhabilitacion_vehiculo → Vehiculo.Estado = "fuera_de_servicio"
        ///
        /// Nota: los efectos secundarios solo se aplican si el tipo cambió
        /// respecto al valor anterior, evitando operaciones redundantes sobre
        /// Vehículo/Usuario cuando solo se edita el motivo o la fecha.
        ///
        /// No permite pasar el estado a "revocada": ese estado solo se asigna
        /// al cancelar la multa padre mediante MultaService.CancelarAsync.
        ///
        /// Validación de FechaFin: si se provee, debe ser posterior a
        /// FechaInicio de la penalización existente.
        /// </summary>
        public async Task<(bool exito, string mensaje)> UpdateAsync(int id, PenalizacionUpdateDTO dto)
        {
            var errorCampos = ValidarCamposUpdate(dto);
            if (errorCampos != null) return (false, errorCampos);

            var penalizacion = await _repo.GetByIdAsync(id);
            if (penalizacion == null) return (false, "Penalización no encontrada.");

            if (penalizacion.Estado == "revocada")
                return (false, "Una penalización revocada no puede modificarse.");

            if (dto.Estado.Trim().ToLower() == "revocada")
                return (false, "El estado 'revocada' solo se asigna al cancelar la multa asociada.");

            if (dto.FechaFin.HasValue && dto.FechaFin.Value <= penalizacion.FechaInicio)
                return (false, $"La fecha de fin debe ser posterior a la fecha de inicio " +
                               $"({penalizacion.FechaInicio:dd/MM/yyyy HH:mm}).");

            var tipoAnterior = penalizacion.Tipo;
            var tipoNuevo = dto.Tipo.Trim().ToLower();

            // Actualizar campos de la penalización
            penalizacion.Tipo = tipoNuevo;
            penalizacion.Motivo = dto.Motivo.Trim();
            penalizacion.FechaFin = dto.FechaFin;
            penalizacion.Estado = dto.Estado.Trim().ToLower();

            // ── Efectos secundarios al editar ────────────────────────────
            // Solo se aplican si el tipo cambió, para evitar operaciones
            // redundantes sobre Vehículo/Usuario cuando el tipo no varía.
            if (tipoNuevo != tipoAnterior)
            {
                // Obtener el vehículo y usuario desde la multa vinculada
                // para poder aplicar los efectos secundarios correspondientes.
                var multa = penalizacion.MultaId.HasValue
                    ? await _multaRepo.GetByIdAsync(penalizacion.MultaId.Value)
                    : null;

                if (multa?.Incidencia != null)
                {
                    if (tipoNuevo == "inhabilitacion_vehiculo")
                    {
                        var vehiculo = await _vehiculoRepo.GetByIdSimpleAsync(multa.Incidencia.VehiculoId);
                        if (vehiculo != null)
                            vehiculo.Estado = "fuera_de_servicio";
                    }

                    if (tipoNuevo == "bloqueo_cuenta")
                    {
                        var usuario = await _usuarioRepo.GetByIdSimpleAsync(multa.Incidencia.UsuarioId);
                        if (usuario != null)
                            usuario.Bloqueado = true;
                    }
                }
            }

            await _repo.SaveChangesAsync();
            return (true, "Penalización actualizada correctamente.");
        }

        // ================================================================
        // Validaciones privadas
        // ================================================================

        private static readonly string[] TiposValidos =
        [
            "suspension_temporal", "bloqueo_cuenta",
            "inhabilitacion_vehiculo", "advertencia"
        ];

        private static readonly string[] EstadosEditables =
        [
            "activa", "cumplida"
        ];

        private static string? ValidarCamposUpdate(PenalizacionUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tipo))
                return "El tipo de penalización es obligatorio.";
            if (!TiposValidos.Contains(dto.Tipo.Trim().ToLower()))
                return $"Tipo de penalización inválido. Valores permitidos: {string.Join(", ", TiposValidos)}.";

            if (string.IsNullOrWhiteSpace(dto.Motivo))
                return "El motivo es obligatorio.";

            if (string.IsNullOrWhiteSpace(dto.Estado))
                return "El estado de la penalización es obligatorio.";
            if (!EstadosEditables.Contains(dto.Estado.Trim().ToLower()) &&
                dto.Estado.Trim().ToLower() != "revocada")
                return $"Estado inválido. Valores permitidos desde edición: {string.Join(", ", EstadosEditables)}.";

            return null;
        }

        // ================================================================
        // Mapeo privado
        // ================================================================

        private static PenalizacionResponseDTO ToResponseDTO(Penalizacion p) => new()
        {
            IdPenalizacion = p.IdPenalizacion,
            MultaId = p.MultaId,
            Tipo = p.Tipo,
            Motivo = p.Motivo,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            Estado = p.Estado
        };
    }
}