// =============================================================================
// GoVehiculos.Tests/Services/MultaServiceTests/GetByIdAsyncTests.cs
// CORRECCIÓN: eliminados Mock<IncidenciaService> y Mock<PenalizacionService>.
// Se usan instancias REALES construidas con sus repositorios mockeados.
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Observers;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.MultaServiceTests;

public class GetByIdAsyncTests
{
    private readonly Mock<IMultaRepository>        _multaRepoMock        = new();
    private readonly Mock<IPenalizacionRepository> _penalizacionRepoMock = new();
    private readonly Mock<IVehiculoRepository>     _vehiculoRepoMock     = new();
    private readonly Mock<IUsuarioRepository>      _usuarioRepoMock      = new();
    private readonly Mock<IIncidenciaRepository>   _incidenciaRepoMock   = new();

    private MultaService CrearSut() =>
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
            Enumerable.Empty<IMultaObserver>());

    [Fact]
    public async Task GetByIdAsync_MultaExiste_RetornaDTO()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 7, estado: "pagada", tipo: "administrativa", monto: 1500m);
        _multaRepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(multa);
        _penalizacionRepoMock.Setup(r => r.GetByMultaIdAsync(7)).ReturnsAsync((Penalizacion?)null);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByIdAsync(7);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.IdMulta.Should().Be(7);
        resultado.Estado.Should().Be("pagada");
        resultado.Tipo.Should().Be("administrativa");
        resultado.Monto.Should().Be(1500m);
    }

    [Fact]
    public async Task GetByIdAsync_MultaNoExiste_RetornaNull()
    {
        // Arrange
        _multaRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Multa?)null);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByIdAsync(999);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_LlamaRepositorioConIdCorrecto()
    {
        // Arrange
        _multaRepoMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync((Multa?)null);

        var sut = CrearSut();

        // Act
        await sut.GetByIdAsync(15);

        // Assert
        _multaRepoMock.Verify(r => r.GetByIdAsync(15), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ConPenalizacion_MapeaDatosDePenalizacion()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 3);
        var pen   = ModelBuilders.Penalizacion(id: 55, multaId: 3, tipo: "suspension_temporal", estado: "cumplida");
        pen.FechaFin = new DateTime(2025, 1, 1);

        _multaRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(multa);
        _penalizacionRepoMock.Setup(r => r.GetByMultaIdAsync(3)).ReturnsAsync(pen);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByIdAsync(3);

        // Assert
        resultado!.IdPenalizacion.Should().Be(55);
        resultado.PenalizacionTipo.Should().Be("suspension_temporal");
        resultado.PenalizacionEstado.Should().Be("cumplida");
        resultado.PenalizacionFechaFin.Should().Be(new DateTime(2025, 1, 1));
    }

    [Fact]
    public async Task GetByIdAsync_SinPenalizacion_CamposPenalizacionSonNull()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 4);
        _multaRepoMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(multa);
        _penalizacionRepoMock.Setup(r => r.GetByMultaIdAsync(4)).ReturnsAsync((Penalizacion?)null);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByIdAsync(4);

        // Assert
        resultado!.IdPenalizacion.Should().BeNull();
        resultado.PenalizacionTipo.Should().BeNull();
        resultado.PenalizacionEstado.Should().BeNull();
    }
}
