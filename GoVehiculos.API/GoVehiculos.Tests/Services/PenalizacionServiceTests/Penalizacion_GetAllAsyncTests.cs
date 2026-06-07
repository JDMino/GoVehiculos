// =============================================================================
// GoVehiculos.Tests/Services/PenalizacionServiceTests/GetAllAsyncTests.cs
// Tests unitarios para PenalizacionService.GetAllAsync()
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.PenalizacionServiceTests;

public class GetAllAsyncTests
{
    private readonly Mock<IPenalizacionRepository> _repoMock        = new();
    private readonly Mock<IVehiculoRepository>     _vehiculoRepoMock = new();
    private readonly Mock<IUsuarioRepository>      _usuarioRepoMock  = new();
    private readonly Mock<IMultaRepository>        _multaRepoMock    = new();

    private PenalizacionService CrearSut() =>
        new(_repoMock.Object, _vehiculoRepoMock.Object, _usuarioRepoMock.Object, _multaRepoMock.Object);

    [Fact]
    public async Task GetAllAsync_SinFiltro_RetornaTodasLasPenalizaciones()
    {
        // Arrange
        var penalizaciones = new List<Penalizacion>
        {
            ModelBuilders.Penalizacion(id: 1, tipo: "advertencia",           estado: "activa"),
            ModelBuilders.Penalizacion(id: 2, tipo: "bloqueo_cuenta",        estado: "cumplida"),
            ModelBuilders.Penalizacion(id: 3, tipo: "inhabilitacion_vehiculo", estado: "revocada")
        };
        _repoMock.Setup(r => r.GetAllAsync(null)).ReturnsAsync(penalizaciones);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync(null);

        // Assert
        var lista = resultado.ToList();
        lista.Should().HaveCount(3);
        lista[0].IdPenalizacion.Should().Be(1);
        lista[1].IdPenalizacion.Should().Be(2);
        lista[2].IdPenalizacion.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_ConFiltroEstado_PasaFiltroAlRepositorio()
    {
        // Arrange
        var penalizaciones = new List<Penalizacion>
        {
            ModelBuilders.Penalizacion(id: 1, estado: "activa")
        };
        _repoMock.Setup(r => r.GetAllAsync("activa")).ReturnsAsync(penalizaciones);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync("activa");

        // Assert
        resultado.Should().HaveCount(1);
        _repoMock.Verify(r => r.GetAllAsync("activa"), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_SinPenalizaciones_RetornaColeccionVacia()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<string?>())).ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_MapeaCamposCorrectos()
    {
        // Arrange
        var fechaInicio = new DateTime(2024, 1, 10);
        var fechaFin    = new DateTime(2024, 3, 10);
        var pen = new Penalizacion
        {
            IdPenalizacion = 7,
            MultaId        = 3,
            Tipo           = "suspension_temporal",
            Motivo         = "Uso indebido del vehículo",
            FechaInicio    = fechaInicio,
            FechaFin       = fechaFin,
            Estado         = "cumplida"
        };
        _repoMock.Setup(r => r.GetAllAsync(null)).ReturnsAsync(new List<Penalizacion> { pen });

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync(null);

        // Assert
        var dto = resultado.Single();
        dto.IdPenalizacion.Should().Be(7);
        dto.MultaId.Should().Be(3);
        dto.Tipo.Should().Be("suspension_temporal");
        dto.Motivo.Should().Be("Uso indebido del vehículo");
        dto.FechaInicio.Should().Be(fechaInicio);
        dto.FechaFin.Should().Be(fechaFin);
        dto.Estado.Should().Be("cumplida");
    }
}
