using FluentAssertions;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.GetContadorEmpleadoAsync(int empleadoId)
///     → Task&lt;int&gt;
///
/// Comportamiento esperado:
///   El service delega directamente a IMantenimientoRepository.ContarPendientesPorEmpleadoAsync
///   y propaga el valor devuelto sin transformación. Ningún otro repositorio
///   ni efecto secundario deben producirse.
/// </summary>
public class MantenimientoService_GetContadorEmpleadoAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_GetContadorEmpleadoAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    [Fact]
    public async Task CuandoHayOrdenesActivas_RetornaLaCantidadCorrecta()
    {
        // Arrange
        const int empleadoId        = 5;
        const int cantidadEsperada  = 3;

        _repoMock
            .Setup(r => r.ContarPendientesPorEmpleadoAsync(empleadoId))
            .ReturnsAsync(cantidadEsperada);

        // Act
        var resultado = await _sut.GetContadorEmpleadoAsync(empleadoId);

        // Assert
        resultado.Should().Be(cantidadEsperada);
    }

    [Fact]
    public async Task CuandoNoHayOrdenes_RetornaCero()
    {
        // Arrange
        const int empleadoId = 99;

        _repoMock
            .Setup(r => r.ContarPendientesPorEmpleadoAsync(empleadoId))
            .ReturnsAsync(0);

        // Act
        var resultado = await _sut.GetContadorEmpleadoAsync(empleadoId);

        // Assert
        resultado.Should().Be(0);
    }

    [Theory]
    [InlineData(1,  1)]
    [InlineData(7,  10)]
    [InlineData(42, 0)]
    public async Task PropagaElValorDelRepositorioParaDiversosEmpleados(
        int empleadoId, int cantidadEsperada)
    {
        // Arrange
        _repoMock
            .Setup(r => r.ContarPendientesPorEmpleadoAsync(empleadoId))
            .ReturnsAsync(cantidadEsperada);

        // Act
        var resultado = await _sut.GetContadorEmpleadoAsync(empleadoId);

        // Assert
        resultado.Should().Be(cantidadEsperada);
    }

    [Fact]
    public async Task InvocaExactamenteUnaVezElRepositorioConElEmpleadoIdCorrecto()
    {
        // Arrange
        const int empleadoId = 7;

        _repoMock
            .Setup(r => r.ContarPendientesPorEmpleadoAsync(empleadoId))
            .ReturnsAsync(2);

        // Act
        await _sut.GetContadorEmpleadoAsync(empleadoId);

        // Assert
        _repoMock.Verify(r => r.ContarPendientesPorEmpleadoAsync(empleadoId), Times.Once);
    }

    [Fact]
    public async Task NoInvocaNingunOtroMetodoDelRepositorio()
    {
        // Arrange
        _repoMock
            .Setup(r => r.ContarPendientesPorEmpleadoAsync(It.IsAny<int>()))
            .ReturnsAsync(1);

        // Act
        await _sut.GetContadorEmpleadoAsync(1);

        // Assert — solo debe haberse llamado ContarPendientesPorEmpleadoAsync
        _repoMock.Verify(r => r.ContarPendientesPorEmpleadoAsync(It.IsAny<int>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
        _vehiculoRepoMock.VerifyNoOtherCalls();
    }
}
