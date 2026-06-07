// =============================================================================
// GoVehiculos.Tests/Services/MultaServiceTests/CrearMultaCompletaAsyncTests.cs
//
// CORRECCIÓN PROBLEMA 1:
// Los tests originales hacían Mock<IncidenciaService> y Mock<PenalizacionService>
// y luego Setup() sobre CrearAsync(), que NO es virtual → NotSupportedException.
//
// SOLUCIÓN: se usan instancias REALES de IncidenciaService y PenalizacionService,
// cada una inyectada con sus propios mocks de repositorio. Así los tests controlan
// el comportamiento mockeando IIncidenciaRepository e IPenalizacionRepository
// directamente, sin tocar métodos no virtuales.
//
// Incluye validación del PATRÓN OBSERVADOR (Observer).
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

    // ── Repositorios para IncidenciaService (instancia real) ─────────────
    // IncidenciaService recibe IVehiculoRepository e IUsuarioRepository que
    // son los mismos mocks que MultaService, más su propio IIncidenciaRepository.
    private readonly Mock<IIncidenciaRepository>   _incidenciaRepoMock   = new();

    // ── Construcción del SUT ──────────────────────────────────────────────
    // IncidenciaService y PenalizacionService son instancias REALES.
    // Sus métodos no son virtuales: no se puede hacer Setup() sobre ellos.
    // El control del comportamiento se ejerce mockeando sus repositorios.
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

    // ── DTOs válidos por defecto ──────────────────────────────────────────

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
        string tipo   = "advertencia",
        string motivo = "Motivo de prueba") => new()
    {
        Tipo     = tipo,
        Motivo   = motivo,
        FechaFin = null
    };

    // ── Setup del flujo exitoso completo ──────────────────────────────────
    // Configura todos los repositorios para que el camino feliz funcione.
    // IncidenciaService.CrearAsync → usa _incidenciaRepoMock
    // PenalizacionService.CrearAsync → usa _penalizacionRepoMock
    // MultaService → usa _multaRepoMock, _vehiculoRepoMock, _usuarioRepoMock
    private void ConfigurarFlujoCorrecto(int vehiculoId = 1, int usuarioId = 1)
    {
        // Validaciones de existencia en MultaService
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(vehiculoId))
            .ReturnsAsync(ModelBuilders.Vehiculo(vehiculoId));
        _usuarioRepoMock
            .Setup(r => r.GetByIdSimpleAsync(usuarioId))
            .ReturnsAsync(ModelBuilders.Usuario(usuarioId));

        // IncidenciaService.CrearAsync internamente llama a _incidenciaRepoMock
        _incidenciaRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Incidencia>()))
            .Returns(Task.CompletedTask);
        _incidenciaRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // MultaService persiste la Multa
        _multaRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Multa>()))
            .Returns(Task.CompletedTask);
        _multaRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // PenalizacionService.CrearAsync internamente llama a _penalizacionRepoMock
        _penalizacionRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Penalizacion>()))
            .Returns(Task.CompletedTask);
        _penalizacionRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // GetByIdAsync final para construir la respuesta
        _multaRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Multa());
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Penalizacion?)null);
    }

    // ── Casos exitosos ────────────────────────────────────────────────────

    [Fact]
    public async Task CrearMultaCompleta_DatosValidos_RetornaExitoTrue()
    {
        // Arrange
        ConfigurarFlujoCorrecto();
        var sut = CrearSut();

        // Act
        var (exito, mensaje, dto) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Multa creada correctamente.");
        dto.Should().NotBeNull();
    }

    [Fact]
    public async Task CrearMultaCompleta_DatosValidos_LlamaAddAsyncDeIncidenciaRepo()
    {
        // Arrange
        ConfigurarFlujoCorrecto();
        var sut = CrearSut();

        // Act
        await sut.CrearMultaCompletaAsync(IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert — IncidenciaService.CrearAsync internamente llama a _incidenciaRepoMock
        _incidenciaRepoMock.Verify(r => r.AddAsync(It.IsAny<Incidencia>()), Times.Once);
        _incidenciaRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CrearMultaCompleta_DatosValidos_LlamaAddAsyncDePenalizacionRepo()
    {
        // Arrange
        ConfigurarFlujoCorrecto();
        var sut = CrearSut();

        // Act
        await sut.CrearMultaCompletaAsync(IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert — PenalizacionService.CrearAsync internamente llama a _penalizacionRepoMock
        _penalizacionRepoMock.Verify(r => r.AddAsync(It.IsAny<Penalizacion>()), Times.Once);
    }

    [Fact]
    public async Task CrearMultaCompleta_MultaEstadoSiemprePendiente()
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
        await sut.CrearMultaCompletaAsync(IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert — el estado siempre es fijado por el servicio
        multaCapturada.Should().NotBeNull();
        multaCapturada!.Estado.Should().Be("pendiente");
    }

    // ── PATRÓN OBSERVADOR — Comportamiento del Subject ────────────────────

    [Fact]
    public async Task CrearMultaCompleta_NotificaObservadoresRegistrados()
    {
        // Arrange
        var observadorMock = new Mock<IMultaObserver>();
        observadorMock
            .Setup(o => o.ActualizarAsync(It.IsAny<MultaAbs>()))
            .Returns(Task.CompletedTask);

        ConfigurarFlujoCorrecto();
        var sut = CrearSut(new[] { observadorMock.Object });

        // Act
        await sut.CrearMultaCompletaAsync(IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert — el observador debe ser notificado exactamente una vez
        observadorMock.Verify(o => o.ActualizarAsync(It.IsAny<MultaAbs>()), Times.Once);
    }

    [Fact]
    public async Task CrearMultaCompleta_NotificaMultiplesObservadores()
    {
        // Arrange
        var obs1 = new Mock<IMultaObserver>();
        var obs2 = new Mock<IMultaObserver>();
        obs1.Setup(o => o.ActualizarAsync(It.IsAny<MultaAbs>())).Returns(Task.CompletedTask);
        obs2.Setup(o => o.ActualizarAsync(It.IsAny<MultaAbs>())).Returns(Task.CompletedTask);

        ConfigurarFlujoCorrecto();
        var sut = CrearSut(new[] { obs1.Object, obs2.Object });

        // Act
        await sut.CrearMultaCompletaAsync(IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert — ambos observadores son notificados
        obs1.Verify(o => o.ActualizarAsync(It.IsAny<MultaAbs>()), Times.Once);
        obs2.Verify(o => o.ActualizarAsync(It.IsAny<MultaAbs>()), Times.Once);
    }

    [Fact]
    public async Task CrearMultaCompleta_SetaEstadoDelSubjeto_AntesDeNotificar()
    {
        // Arrange — verifica que TipoIncidencia, TipoPenalizacion, VehiculoId y UsuarioId
        // son correctos en el Subject cuando el observador los consulta via cast
        MultaService? capturedSubject = null;
        var observadorMock = new Mock<IMultaObserver>();
        observadorMock
            .Setup(o => o.ActualizarAsync(It.IsAny<MultaAbs>()))
            .Callback<MultaAbs>(subj => capturedSubject = subj as MultaService)
            .Returns(Task.CompletedTask);

        ConfigurarFlujoCorrecto(vehiculoId: 3, usuarioId: 7);
        var sut = CrearSut(new[] { observadorMock.Object });

        // Act
        await sut.CrearMultaCompletaAsync(
            IncidenciaDto(tipo: "daño_fisico", vehiculo: 3, usuario: 7),
            MultaDto(),
            PenalizacionDto(tipo: "bloqueo_cuenta"));

        // Assert — el estado del sujeto fue correctamente seteado
        capturedSubject.Should().NotBeNull();
        capturedSubject!.TipoIncidencia.Should().Be("daño_fisico");
        capturedSubject.TipoPenalizacion.Should().Be("bloqueo_cuenta");
        capturedSubject.VehiculoId.Should().Be(3);
        capturedSubject.UsuarioId.Should().Be(7);
    }

    [Fact]
    public async Task CrearMultaCompleta_SinObservadores_NoLanzaExcepcion()
    {
        // Arrange
        ConfigurarFlujoCorrecto();
        var sut = CrearSut(Enumerable.Empty<IMultaObserver>());

        // Act
        Func<Task> acto = () => sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert
        await acto.Should().NotThrowAsync();
    }

    // ── Casos de error: validaciones de incidencia ────────────────────────

    [Fact]
    public async Task CrearMultaCompleta_UsuarioIdCero_RetornaFalse()
    {
        // Arrange
        var dto = IncidenciaDto();
        dto.UsuarioId = 0;
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(dto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El usuario es obligatorio.");
    }

    [Fact]
    public async Task CrearMultaCompleta_VehiculoIdCero_RetornaFalse()
    {
        // Arrange
        var dto = IncidenciaDto();
        dto.VehiculoId = 0;
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(dto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El vehículo es obligatorio.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task CrearMultaCompleta_TipoIncidenciaVacio_RetornaFalse(string tipo)
    {
        // Arrange
        var dto = IncidenciaDto(tipo: tipo);
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(dto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo de incidencia es obligatorio.");
    }

    [Fact]
    public async Task CrearMultaCompleta_TipoIncidenciaInvalido_RetornaFalse()
    {
        // Arrange
        var dto = IncidenciaDto(tipo: "tipo_invalido");
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(dto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de incidencia inválido");
    }

    [Theory]
    [InlineData("daño_fisico")]
    [InlineData("accidente")]
    [InlineData("infraccion_vial")]
    [InlineData("comportamiento_indebido")]
    [InlineData("retraso_en_pago")]
    public async Task CrearMultaCompleta_TodosLosTiposIncidenciaValidos_PasanValidacion(string tipo)
    {
        // Arrange
        ConfigurarFlujoCorrecto();
        var sut = CrearSut();

        // Act
        var (exito, _, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(tipo: tipo), MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeTrue();
    }

    [Fact]
    public async Task CrearMultaCompleta_NivelGravedadInvalido_RetornaFalse()
    {
        // Arrange
        var dto = IncidenciaDto(gravedad: "extrema");
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(dto, MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Nivel de gravedad inválido");
    }

    // ── Casos de error: validaciones de multa ─────────────────────────────

    [Fact]
    public async Task CrearMultaCompleta_TipoMultaInvalido_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(tipo: "tipo_invalido"), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de multa inválido");
    }

    [Fact]
    public async Task CrearMultaCompleta_MontoNegativo_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(monto: -100m), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El monto no puede ser negativo.");
    }

    [Fact]
    public async Task CrearMultaCompleta_MontoCero_EsValido()
    {
        // Arrange — monto 0 es válido (multa administrativa)
        ConfigurarFlujoCorrecto();
        var sut = CrearSut();

        // Act
        var (exito, _, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(tipo: "administrativa", monto: 0m), PenalizacionDto());

        // Assert
        exito.Should().BeTrue();
    }

    // ── Casos de error: validaciones de penalización ──────────────────────

    [Fact]
    public async Task CrearMultaCompleta_TipoPenalizacionInvalido_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto(tipo: "tipo_invalido"));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de penalización inválido");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task CrearMultaCompleta_MotivoPenalizacionVacio_RetornaFalse(string motivo)
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto(motivo: motivo));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El motivo de la penalización es obligatorio.");
    }

    // ── Casos de error: entidades no encontradas ──────────────────────────

    [Fact]
    public async Task CrearMultaCompleta_VehiculoNoExiste_RetornaFalse()
    {
        // Arrange
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync((Vehiculo?)null);
        _usuarioRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Usuario());

        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El vehículo indicado no existe.");
    }

    [Fact]
    public async Task CrearMultaCompleta_UsuarioNoExiste_RetornaFalse()
    {
        // Arrange
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Vehiculo());
        _usuarioRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync((Usuario?)null);

        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), PenalizacionDto());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El usuario indicado no existe.");
    }

    [Fact]
    public async Task CrearMultaCompleta_FechaFinPenalizacionEnElPasado_RetornaFalse()
    {
        // Arrange — PenalizacionService.CrearAsync valida FechaFin <= FechaInicio
        // Controlamos esto desde el DTO de penalización con FechaFin en el pasado.
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Vehiculo());
        _usuarioRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Usuario());

        // IncidenciaService.CrearAsync necesita sus repos para persistir
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

        // FechaFin en el pasado → PenalizacionService.CrearAsync retorna false
        var penDto = new PenalizacionCreateDTO
        {
            Tipo     = "advertencia",
            Motivo   = "Motivo",
            FechaFin = DateTime.Now.AddDays(-1)
        };

        var sut = CrearSut();

        // Act
        var (exito, mensaje, _) = await sut.CrearMultaCompletaAsync(
            IncidenciaDto(), MultaDto(), penDto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("fecha de fin debe ser posterior");
    }

    [Fact]
    public async Task CrearMultaCompleta_FallaPenalizacion_NoNotificaObservadores()
    {
        // Arrange — misma situación: FechaFin inválida causa que PenalizacionService
        // retorne false antes de que MultaService llegue a NotificarAsync.
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

        var penDto = new PenalizacionCreateDTO
        {
            Tipo     = "advertencia",
            Motivo   = "Motivo",
            FechaFin = DateTime.Now.AddDays(-1)
        };

        var observadorMock = new Mock<IMultaObserver>();
        var sut = CrearSut(new[] { observadorMock.Object });

        // Act
        await sut.CrearMultaCompletaAsync(IncidenciaDto(), MultaDto(), penDto);

        // Assert — si falla antes de NotificarAsync, los observadores NO son llamados
        observadorMock.Verify(o => o.ActualizarAsync(It.IsAny<MultaAbs>()), Times.Never);
    }
}
