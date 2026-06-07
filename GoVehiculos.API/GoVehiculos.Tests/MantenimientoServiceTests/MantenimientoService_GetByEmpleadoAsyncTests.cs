using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.GetByEmpleadoAsync(int empleadoId)
///     → Task&lt;IEnumerable&lt;MantenimientoResponseDTO&gt;&gt;
///
/// Comportamiento esperado:
///   Recupera y mapea todas las órdenes asignadas al empleado indicado.
///   Propaga el empleadoId al repositorio sin transformarlo.
/// </summary>
public class MantenimientoService_GetByEmpleadoAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_GetByEmpleadoAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    [Fact]
    public async Task CuandoEmpleadoTieneOrdenes_RetornaTodas()
    {
        // Arrange
        const int empleadoId = 3;
        var lista = new List<Mantenimiento>
        {
            ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 1, empleadoId: empleadoId),
            ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 2, empleadoId: empleadoId)
        };

        _repoMock
            .Setup(r => r.GetByEmpleadoAsync(empleadoId))
            .ReturnsAsync(lista);

        // Act
        var resultado = await _sut.GetByEmpleadoAsync(empleadoId);

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task CuandoEmpleadoNoTieneOrdenes_RetornaEnumerableVacio()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByEmpleadoAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Mantenimiento>());

        // Act
        var resultado = await _sut.GetByEmpleadoAsync(99);

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task PropagaElEmpleadoIdAlRepositorioSinTransformar()
    {
        // Arrange
        const int empleadoId = 15;
        _repoMock
            .Setup(r => r.GetByEmpleadoAsync(empleadoId))
            .ReturnsAsync(new List<Mantenimiento>());

        // Act
        await _sut.GetByEmpleadoAsync(empleadoId);

        // Assert
        _repoMock.Verify(r => r.GetByEmpleadoAsync(empleadoId), Times.Once);
        _repoMock.Verify(r => r.GetByEmpleadoAsync(It.Is<int>(x => x != empleadoId)), Times.Never);
    }

    [Fact]
    public async Task InvocaElRepositorioExactamenteUnaVez()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByEmpleadoAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Mantenimiento>());

        // Act
        await _sut.GetByEmpleadoAsync(5);

        // Assert
        _repoMock.Verify(r => r.GetByEmpleadoAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task MapeaCorrectamenteElIdMantenimiento()
    {
        // Arrange
        const int empleadoId = 3;
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 77, empleadoId: empleadoId);

        _repoMock
            .Setup(r => r.GetByEmpleadoAsync(empleadoId))
            .ReturnsAsync(new List<Mantenimiento> { mant });

        // Act
        var dto = (await _sut.GetByEmpleadoAsync(empleadoId)).Single();

        // Assert
        dto.IdMantenimiento.Should().Be(77);
    }
}
