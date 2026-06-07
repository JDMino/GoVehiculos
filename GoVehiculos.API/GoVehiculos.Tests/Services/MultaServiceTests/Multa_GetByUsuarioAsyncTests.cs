// =============================================================================
// GoVehiculos.Tests/Services/MultaServiceTests/GetByUsuarioAsyncTests.cs
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

public class GetByUsuarioAsyncTests
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
    public async Task GetByUsuarioAsync_UsuarioConMultas_RetornaLista()
    {
        // Arrange
        var multas = new List<Multa>
        {
            ModelBuilders.Multa(id: 1, estado: "pendiente"),
            ModelBuilders.Multa(id: 2, estado: "pagada")
        };
        _multaRepoMock.Setup(r => r.GetByUsuarioIdAsync(10, null)).ReturnsAsync(multas);
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByUsuarioAsync(10, null);

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByUsuarioAsync_ConFiltroEstado_PasaFiltroAlRepo()
    {
        // Arrange
        _multaRepoMock.Setup(r => r.GetByUsuarioIdAsync(5, "pendiente"))
            .ReturnsAsync(new List<Multa>());
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        await sut.GetByUsuarioAsync(5, "pendiente");

        // Assert
        _multaRepoMock.Verify(r => r.GetByUsuarioIdAsync(5, "pendiente"), Times.Once);
    }

    [Fact]
    public async Task GetByUsuarioAsync_UsuarioSinMultas_RetornaVacio()
    {
        // Arrange
        _multaRepoMock.Setup(r => r.GetByUsuarioIdAsync(99, null))
            .ReturnsAsync(new List<Multa>());
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByUsuarioAsync(99, null);

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUsuarioAsync_UsaGetByMultaIdsAsyncParaEvitarN1()
    {
        // Arrange
        var multas = new List<Multa>
        {
            ModelBuilders.Multa(id: 1),
            ModelBuilders.Multa(id: 2)
        };
        _multaRepoMock.Setup(r => r.GetByUsuarioIdAsync(3, null)).ReturnsAsync(multas);
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        await sut.GetByUsuarioAsync(3, null);

        // Assert — una sola query batch, nunca la individual
        _penalizacionRepoMock.Verify(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()), Times.Once);
        _penalizacionRepoMock.Verify(r => r.GetByMultaIdAsync(It.IsAny<int>()), Times.Never);
    }
}
