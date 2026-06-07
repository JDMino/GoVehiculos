using FluentAssertions;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.GetContadorNuevasTerminadasAsync()
///     → Task&lt;int&gt;
///
/// Comportamiento esperado:
///   El service delega directamente a IMantenimientoRepository.ContarTerminadosAsync
///   y propaga el valor devuelto sin transformación. Ningún otro repositorio
///   ni efecto secundario deben producirse.
/// </summary>
public class MantenimientoService_GetContadorNuevasTerminadasAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_GetContadorNuevasTerminadasAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    [Fact]
    public async Task CuandoHayOrdenesTerminadas_RetornaLaCantidadCorrecta()
    {
        // Arrange
        const int cantidadEsperada = 7;

        _repoMock
            .Setup(r => r.ContarTerminadosAsync())
            .ReturnsAsync(cantidadEsperada);

        // Act
        var resultado = await _sut.GetContadorNuevasTerminadasAsync();

        // Assert
        resultado.Should().Be(cantidadEsperada);
    }

    [Fact]
    public async Task CuandoNoHayOrdenesTerminadas_RetornaCero()
    {
        // Arrange
        _repoMock
            .Setup(r => r.ContarTerminadosAsync())
            .ReturnsAsync(0);

        // Act
        var resultado = await _sut.GetContadorNuevasTerminadasAsync();

        // Assert
        resultado.Should().Be(0);
    }

    [Fact]
    public async Task InvocaExactamenteUnaVezElRepositorio()
    {
        // Arrange
        _repoMock
            .Setup(r => r.ContarTerminadosAsync())
            .ReturnsAsync(5);

        // Act
        await _sut.GetContadorNuevasTerminadasAsync();

        // Assert
        _repoMock.Verify(r => r.ContarTerminadosAsync(), Times.Once);
    }

    [Fact]
    public async Task NoInvocaNingunOtroMetodoDelRepositorio()
    {
        // Arrange
        _repoMock
            .Setup(r => r.ContarTerminadosAsync())
            .ReturnsAsync(3);

        // Act
        await _sut.GetContadorNuevasTerminadasAsync();

        // Assert
        _repoMock.Verify(r => r.ContarTerminadosAsync(), Times.Once);
        _repoMock.VerifyNoOtherCalls();
        _vehiculoRepoMock.VerifyNoOtherCalls();
    }
}
