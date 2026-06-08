// =============================================================================
// GoVehiculos.Tests/Services/MultaServiceTests/CrearMultaCompletaAsyncTests.cs
// Alineado con la planilla de pruebas unitarias — PDF versión final.
// CMC-01 a CMC-21.
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Observers;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.MultaServiceTests;

public class CrearMultaCompletaAsyncTests
{
    // ── Repositorios para MultaService ────────────────────────────────────
    private readonly Mock<IMultaRepository>        _multaRepoMock        = new();
    private readonly Mock<IPenalizacionRepository> _penalizacionRepoMock = new();
    private readonly Mock<IVehiculoRepository>     _vehiculoRepoMock     = new();
    private readonly Mock<IUsuarioRepository>      _usuarioRepoMock      = new();

    // ── Repositorio para IncidenciaService (instancia real) ──────────────
    private readonly Mock<IIncidenciaRepository>   _incidenciaRepoMock   = new();

    // ── Construcción del SUT ──────────────────────────────────────────────
    // IncidenciaService y PenalizacionService son instancias REALES porque
    // sus métodos no son virtuales: no se puede hacer Setup() sobre ellos.
    private MultaService CrearSut(IEnumerable<IMultaObserver>? observadores = null) =>
        new(_multaRepoMock.Object,
            _penalizacionRepoMock.Object,
            _vehiculoRepoMock.Object,
            _usuarioRepoMock.Object,
            new IncidenciaService(
                _incidenciaRepoMock.Object,
                _vehiculoRepoMock.Object,
                _usuarioRepoMock.Object),
            new PenalizacionService(
                _penalizacionRepoMock.Object,
                _vehiculoRepoMock.Object,
                _usuarioRepoMock.Object,
                _multaRepoMock.Object),
            observadores ?? Enumerable.Empty<IMultaObserver>());

    // ── DTOs base ────────────────────────────────────────────────────────

    private static IncidenciaCreateDTO IncidenciaDto(
        string tipo     = "accidente",
        string gravedad = "media",
        int    usuario  = 1,
        int    vehiculo = 1) => new()
    {
        UsuarioId     = usuario,
        VehiculoId    = vehiculo,
        Tipo          = tipo,
        NivelGravedad = gravedad,
        Descripcion   = "Descripción de incidencia"
    };

    private static MultaCreateDTO MultaDto(
        string  tipo  = "economica",
        decimal monto = 5000m) => new()
    {
        Tipo        = tipo,
        Monto       = monto,
        Descripcion = "Descripción de multa"
    };

    private static PenalizacionCreateDTO PenalizacionDto(
        string    tipo     = "advertencia",
        string    motivo   = "Primera falta",
        DateTime? fechaFin = null) => new()
    {
        Tipo     = tipo,
        Motivo   = motivo,
        FechaFin = fechaFin
    };

    // ── Setup del flujo exitoso ───────────────────────────────────────────
    private void ConfigurarFlujoCorrecto(int vehiculoId = 1, int usuarioId = 1)
    {
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(vehiculoId))
            .ReturnsAsync(ModelBuilders.Vehiculo(vehiculoId));
        _usuarioRepoMock
            .Setup(r => r.GetByIdSimpleAsync(usuarioId))
            .ReturnsAsync(ModelBuilders.Usuario(usuarioId));

        _incidenciaRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Incidencia>()))
            .Returns(Task.CompletedTask);
        _incidenciaRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _multaRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Multa>()))
            .Returns(Task.CompletedTask);
        _multaRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _penalizacionRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Penalizacion>()))
            .Returns(Task.CompletedTask);
        _penalizacionRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _multaRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Multa());
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Penalizacion?)null);
    }

    // ── CMC-01 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC01_DatosValidosEnLosTresDTOs_RetornaExitoTrue()
    {
        // Arrange
        ConfigurarFlujoCorrecto();
        var sut = CrearSut();

        // Act
        var (exito, mensaje, dto) = await sut.CrearMultaCompletaAsync(
            new IncidenciaCreateDTO
            {
                UsuarioId     = 1,
                VehiculoId    = 1,
                Tipo          = "accidente",
                NivelGravedad = "media",
                Descripcion   = "Choque"
            },
            new MultaCreateDTO
            {
                Tipo        = "economica",
                Monto       = 5000m,
                Descripcion = "Multa por choque"
            },
            new PenalizacionCreateDTO
            {
                Tipo     = "advertencia",
                Motivo   = "Primera falta",
                FechaFin = null
            });

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Multa creada correctamente.");
        dto.Should().NotBeNull();
    }

    // ── CMC-02 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC02_UsuarioIdIgualACero_RetornaFalse()
    {
        // Arrange
        var incDto = IncidenciaDto();
        incDto.UsuarioId = 0;
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            incDto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El usuario es obligatorio.");
    }

    // ── CMC-03 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC03_VehiculoIdIgualACero_RetornaFalse()
    {
        // Arrange
        var incDto = IncidenciaDto();
        incDto.VehiculoId = 0;
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            incDto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El vehículo es obligatorio.");
    }

    // ── CMC-04 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC04_TipoIncidenciaVacio_RetornaFalse()
    {
        // Arrange
        var incDto = IncidenciaDto(tipo: "");
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            incDto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo de incidencia es obligatorio.");
    }

    // ── CMC-05 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC05_TipoIncidenciaFueraDeValoresPermitidos_RetornaFalse()
    {
        // Arrange
        var incDto = IncidenciaDto(tipo: "tipo_inexistente");
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            incDto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de incidencia inválido. Valores permitidos: daño_fisico, accidente, infraccion_vial, comportamiento_indebido, retraso_en_pago.");
    }

    // ── CMC-06 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC06_NivelGravedadFueraDeValoresPermitidos_RetornaFalse()
    {
        // Arrange
        var incDto = IncidenciaDto(gravedad: "extrema");
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            incDto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Nivel de gravedad inválido. Valores permitidos: baja, media, alta.");
    }

    // ── CMC-07 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC07_DescripcionIncidenciaVacia_RetornaFalse()
    {
        // Arrange
        var incDto = new IncidenciaCreateDTO
        {
            UsuarioId     = 1,
            VehiculoId    = 1,
            Tipo          = "accidente",
            NivelGravedad = "media",
            Descripcion   = ""
        };
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            incDto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La descripción de la incidencia es obligatoria.");
    }

    // ── CMC-08 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC08_TipoMultaVacio_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(tipo: ""), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo de multa es obligatorio.");
    }

    // ── CMC-09 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC09_TipoMultaFueraDeValoresPermitidos_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(tipo: "tipo_invalido"), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de multa inválido. Valores permitidos: economica, administrativa, mixta.");
    }

    // ── CMC-10 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC10_MontoMultaNegativo_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(monto: -1m), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El monto no puede ser negativo.");
    }

    // ── CMC-11 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC11_MontoIgualACeroParaMultaAdministrativa_EsValido()
    {
        // Arrange
        ConfigurarFlujoCorrecto();
        var sut = CrearSut();

        // Act
        var (exito, _, dto) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(),
            MultaDto(tipo: "administrativa", monto: 0m),
            PenalizacionDto());

        // Assert
        exito.Should().BeTrue();
        dto.Should().NotBeNull();
    }

    // ── CMC-12 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC12_TipoPenalizacionVacio_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto(tipo: ""));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo de penalización es obligatorio.");
    }

    // ── CMC-13 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC13_TipoPenalizacionFueraDeValoresPermitidos_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto(tipo: "tipo_invalido"));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de penalización inválido. Valores permitidos: suspension_temporal, bloqueo_cuenta, inhabilitacion_vehiculo, advertencia.");
    }

    // ── CMC-14 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC14_MotivoPenalizacionVacio_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto(motivo: ""));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El motivo de la penalización es obligatorio.");
    }

    // ── CMC-15 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC15_UsuarioNoExisteEnBD_RetornaFalse()
    {
        // Arrange
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Vehiculo());
        _usuarioRepoMock
            .Setup(r => r.GetByIdSimpleAsync(99))
            .ReturnsAsync((Usuario?)null);

        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(usuario: 99), MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El usuario indicado no existe.");
    }

    // ── CMC-16 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC16_VehiculoNoExisteEnBD_RetornaFalse()
    {
        // Arrange
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(99))
            .ReturnsAsync((Vehiculo?)null);
        _usuarioRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Usuario());

        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(vehiculo: 99), MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El vehículo indicado no existe.");
    }

    // ── CMC-17 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC17_FechaFinPenalizacionAnteriorAFechaActual_RetornaFalse()
    {
        // Arrange
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Vehiculo());
        _usuarioRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Usuario());
        _incidenciaRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Incidencia>()))
            .Returns(Task.CompletedTask);
        _incidenciaRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _multaRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Multa>()))
            .Returns(Task.CompletedTask);
        _multaRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(),
            MultaDto(),
            PenalizacionDto(tipo: "suspension_temporal", fechaFin: DateTime.Now.AddDays(-1)));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("La fecha de fin debe ser posterior a la fecha de inicio de la penalización");
    }

    // ── CMC-18 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC18_CreacionExitosaConDosObservadores_AmbosNotificados()
    {
        // Arrange
        var obs1 = new Mock<IMultaObserver>();
        var obs2 = new Mock<IMultaObserver>();
        obs1.Setup(o => o.ActualizarAsync(It.IsAny<MultaAbs>())).Returns(Task.CompletedTask);
        obs2.Setup(o => o.ActualizarAsync(It.IsAny<MultaAbs>())).Returns(Task.CompletedTask);

        ConfigurarFlujoCorrecto();
        var sut = CrearSut(new[] { obs1.Object, obs2.Object });

        // Act
        var (exito, _, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeTrue();
        obs1.Verify(o => o.ActualizarAsync(It.IsAny<MultaAbs>()), Times.Once);
        obs2.Verify(o => o.ActualizarAsync(It.IsAny<MultaAbs>()), Times.Once);
    }

    // ── CMC-19 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC19_ObservadorRecibeSubjectConDatosCorrectos()
    {
        // Arrange
        MultaService? capturedSubject = null;
        var observadorMock = new Mock<IMultaObserver>();
        observadorMock
            .Setup(o => o.ActualizarAsync(It.IsAny<MultaAbs>()))
            .Callback<MultaAbs>(subj => capturedSubject = subj as MultaService)
            .Returns(Task.CompletedTask);

        ConfigurarFlujoCorrecto(vehiculoId: 1, usuarioId: 1);
        var sut = CrearSut(new[] { observadorMock.Object });

        // Act
        var (exito, _, _) = await sut.CrearMultaCompletaAsync(
            new IncidenciaCreateDTO
            {
                UsuarioId     = 1,
                VehiculoId    = 1,
                Tipo          = "daño_fisico",
                NivelGravedad = "alta",
                Descripcion   = "X"
            },
            MultaDto(),
            PenalizacionDto(tipo: "bloqueo_cuenta"));

        // Assert
        exito.Should().BeTrue();
        capturedSubject.Should().NotBeNull();
        capturedSubject!.TipoIncidencia.Should().Be("daño_fisico");
        capturedSubject.TipoPenalizacion.Should().Be("bloqueo_cuenta");
    }

    // ── CMC-20 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC20_FallaPenalizacionPorFechaFin_ObservadorNoEsNotificado()
    {
        // Arrange
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Vehiculo());
        _usuarioRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Usuario());
        _incidenciaRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Incidencia>()))
            .Returns(Task.CompletedTask);
        _incidenciaRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _multaRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Multa>()))
            .Returns(Task.CompletedTask);
        _multaRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var observadorMock = new Mock<IMultaObserver>();
        var sut = CrearSut(new[] { observadorMock.Object });

        // Act
        var (exito, _, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(),
            MultaDto(),
            PenalizacionDto(fechaFin: DateTime.Now.AddDays(-1)));

        // Assert
        exito.Should().BeFalse();
        observadorMock.Verify(o => o.ActualizarAsync(It.IsAny<MultaAbs>()), Times.Never);
    }

    // ── CMC-21 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CMC21_MultaSiemprePersistidaConEstadoPendiente()
    {
        // Arrange
        Multa? multaCapturada = null;
        ConfigurarFlujoCorrecto();
        _multaRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Multa>()))
            .Callback<Multa>(m => multaCapturada = m)
            .Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, _, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(),
            new MultaCreateDTO { Tipo = "economica", Monto = 5000m },
            PenalizacionDto());

        // Assert
        exito.Should().BeTrue();
        multaCapturada.Should().NotBeNull();
        multaCapturada!.Estado.Should().Be("pendiente");
    }
}