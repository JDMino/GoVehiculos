// =============================================================================
// GoVehiculos.Tests/Services/PenalizacionServiceTests/CrearAsyncTests.cs
// Tests unitarios para PenalizacionService.CrearAsync()
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;

namespace GoVehiculos.Tests.Services.PenalizacionServiceTests;

public class CrearAsyncTests
{
    private readonly Mock<IPenalizacionRepository> _repoMock        = new();
    private readonly Mock<IVehiculoRepository>     _vehiculoRepoMock = new();
    private readonly Mock<IUsuarioRepository>      _usuarioRepoMock  = new();
    private readonly Mock<IMultaRepository>        _multaRepoMock    = new();

    private PenalizacionService CrearSut() =>
        new(_repoMock.Object, _vehiculoRepoMock.Object, _usuarioRepoMock.Object, _multaRepoMock.Object);

    // ── Casos exitosos ────────────────────────────────────────────────────

    [Fact]
    public async Task CrearAsync_DatosValidos_RetornaExitoTrueYEntidad()
    {
        // Arrange
        var dto = new PenalizacionCreateDTO
        {
            Tipo    = "advertencia",
            Motivo  = "Conducta inapropiada",
            FechaFin = null
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Penalizacion>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, mensaje, entidad) = await sut.CrearAsync(dto, multaId: 10);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().BeEmpty();
        entidad.Should().NotBeNull();
        entidad!.MultaId.Should().Be(10);
        entidad.Tipo.Should().Be("advertencia");
        entidad.Motivo.Should().Be("Conducta inapropiada");
        entidad.Estado.Should().Be("activa");
    }

    [Fact]
    public async Task CrearAsync_EstadoSiempreActiva_IndependientementeDelDTO()
    {
        // Arrange — el estado "activa" lo fija el servicio, nunca el DTO
        var dto = new PenalizacionCreateDTO
        {
            Tipo   = "suspension_temporal",
            Motivo = "Test"
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Penalizacion>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (_, _, entidad) = await sut.CrearAsync(dto, multaId: 1);

        // Assert
        entidad!.Estado.Should().Be("activa");
    }

    [Fact]
    public async Task CrearAsync_LlamaAddAsyncYSaveChangesAsync()
    {
        // Arrange
        var dto = new PenalizacionCreateDTO { Tipo = "advertencia", Motivo = "Test" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Penalizacion>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        await sut.CrearAsync(dto, multaId: 5);

        // Assert
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Penalizacion>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_FechaInicioEsEstablecidaAlMomentoDeCreacion()
    {
        // Arrange
        var antes = DateTime.Now.AddSeconds(-1);
        var dto   = new PenalizacionCreateDTO { Tipo = "advertencia", Motivo = "Test" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Penalizacion>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (_, _, entidad) = await sut.CrearAsync(dto, multaId: 1);
        var despues = DateTime.Now.AddSeconds(1);

        // Assert
        entidad!.FechaInicio.Should().BeAfter(antes).And.BeBefore(despues);
    }

    [Fact]
    public async Task CrearAsync_NormalizaTipoALowercase()
    {
        // Arrange
        var dto = new PenalizacionCreateDTO { Tipo = "ADVERTENCIA", Motivo = "  Motivo  " };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Penalizacion>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (_, _, entidad) = await sut.CrearAsync(dto, multaId: 1);

        // Assert
        entidad!.Tipo.Should().Be("advertencia");
        entidad.Motivo.Should().Be("Motivo");
    }

    [Fact]
    public async Task CrearAsync_ConFechaFin_LaAsignaEnLaEntidad()
    {
        // Arrange
        var fechaFin = DateTime.Now.AddMonths(3);
        var dto = new PenalizacionCreateDTO
        {
            Tipo     = "suspension_temporal",
            Motivo   = "Test",
            FechaFin = fechaFin
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Penalizacion>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, _, entidad) = await sut.CrearAsync(dto, multaId: 1);

        // Assert
        exito.Should().BeTrue();
        entidad!.FechaFin.Should().Be(fechaFin);
    }

    // ── Validación: FechaFin no puede ser anterior a FechaInicio ─────────

    [Fact]
    public async Task CrearAsync_FechaFinAnteriorAHoy_RetornaFalse()
    {
        // Arrange — fecha de fin en el pasado
        var dto = new PenalizacionCreateDTO
        {
            Tipo     = "suspension_temporal",
            Motivo   = "Test",
            FechaFin = DateTime.Now.AddDays(-1)
        };

        var sut = CrearSut();

        // Act
        var (exito, mensaje, entidad) = await sut.CrearAsync(dto, multaId: 1);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("fecha de fin debe ser posterior");
        entidad.Should().BeNull();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Penalizacion>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_FechaFinIgualAHoy_RetornaFalse()
    {
        // Arrange — exactamente igual a DateTime.Now (<=)
        var dto = new PenalizacionCreateDTO
        {
            Tipo     = "suspension_temporal",
            Motivo   = "Test",
            FechaFin = DateTime.Now.AddSeconds(-5)   // asegura que es <= FechaInicio
        };

        var sut = CrearSut();

        // Act
        var (exito, _, _) = await sut.CrearAsync(dto, multaId: 1);

        // Assert
        exito.Should().BeFalse();
    }

    [Fact]
    public async Task CrearAsync_SinFechaFin_NoValidaFechaFin()
    {
        // Arrange — FechaFin null → no debe validar
        var dto = new PenalizacionCreateDTO
        {
            Tipo     = "advertencia",
            Motivo   = "Sin fecha",
            FechaFin = null
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Penalizacion>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, _, _) = await sut.CrearAsync(dto, multaId: 1);

        // Assert
        exito.Should().BeTrue();
    }
}
