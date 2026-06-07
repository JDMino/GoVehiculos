// =============================================================================
// GoVehiculos.Tests/Services/IncidenciaServiceTests/GetByIdAsyncTests.cs
// Tests unitarios para IncidenciaService.GetByIdAsync()
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.IncidenciaServiceTests;

public class GetByIdAsyncTests
{
    private readonly Mock<IIncidenciaRepository> _repoMock        = new();
    private readonly Mock<IVehiculoRepository>   _vehiculoRepoMock = new();
    private readonly Mock<IUsuarioRepository>    _usuarioRepoMock  = new();

    private IncidenciaService CrearSut() =>
        new(_repoMock.Object, _vehiculoRepoMock.Object, _usuarioRepoMock.Object);

    // ── Caso exitoso ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_IncidenciaExiste_RetornaDTO()
    {
        // Arrange
        var incidencia = ModelBuilders.Incidencia(id: 5, tipo: "daño_fisico", gravedad: "alta");
        _repoMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(incidencia);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByIdAsync(5);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.IdIncidencia.Should().Be(5);
        resultado.Tipo.Should().Be("daño_fisico");
        resultado.NivelGravedad.Should().Be("alta");
        resultado.UsuarioNombreCompleto.Should().Be("Juan Perez");
        resultado.VehiculoMarca.Should().Be("Chevrolet");
    }

    // ── Caso de error: no existe ──────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_IncidenciaNoExiste_RetornaNull()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Incidencia?)null);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByIdAsync(999);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_LlamaAlRepositorioConElIdCorrecto()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(42))
            .ReturnsAsync((Incidencia?)null);

        var sut = CrearSut();

        // Act
        await sut.GetByIdAsync(42);

        // Assert
        _repoMock.Verify(r => r.GetByIdAsync(42), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_MapeaFechaReporteCorrectamente()
    {
        // Arrange
        var fecha = new DateTime(2024, 6, 15, 10, 30, 0);
        var incidencia = ModelBuilders.Incidencia(id: 1);
        incidencia.FechaReporte = fecha;

        _repoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(incidencia);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetByIdAsync(1);

        // Assert
        resultado!.FechaReporte.Should().Be(fecha);
    }
}
