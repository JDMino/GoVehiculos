// =============================================================================
// GoVehiculos.Tests/Services/PenalizacionServiceTests/GetByIdAsyncTests.cs
// Tests unitarios para PenalizacionService.GetByIdAsync()
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.PenalizacionServiceTests;

public class GetByIdAsyncTests
{
    private readonly Mock<IPenalizacionRepository> _repoMock        = new();
    private readonly Mock<IVehiculoRepository>     _vehiculoRepoMock = new();
    private readonly Mock<IUsuarioRepository>      _usuarioRepoMock  = new();
    private readonly Mock<IMultaRepository>        _multaRepoMock    = new();

    private PenalizacionService CrearSut() =>
        new(_repoMock.Object, _vehiculoRepoMock.Object, _usuarioRepoMock.Object, _multaRepoMock.Object);

    [Fact]
    public async Task GetByIdAsync_PenalizacionExiste_RetornaDTO()
    {
        // Arrange
        var pen = ModelBuilders.Penalizacion(id: 5, tipo: "bloqueo_cuenta", estado: "activa");
        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(pen);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByIdAsync(5);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.IdPenalizacion.Should().Be(5);
        resultado.Tipo.Should().Be("bloqueo_cuenta");
        resultado.Estado.Should().Be("activa");
    }

    [Fact]
    public async Task GetByIdAsync_PenalizacionNoExiste_RetornaNull()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Penalizacion?)null);

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
        _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Penalizacion?)null);

        var sut = CrearSut();

        // Act
        await sut.GetByIdAsync(10);

        // Assert
        _repoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_FechaFinNula_RetornaDtoConFechaFinNull()
    {
        // Arrange
        var pen = ModelBuilders.Penalizacion(id: 1);
        pen.FechaFin = null;
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByIdAsync(1);

        // Assert
        resultado!.FechaFin.Should().BeNull();
    }
}
