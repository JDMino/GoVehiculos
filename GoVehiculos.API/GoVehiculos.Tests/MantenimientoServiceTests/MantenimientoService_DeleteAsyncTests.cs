using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.DeleteAsync(int id)
///     → Task&lt;bool&gt;
///
/// Comportamiento esperado:
///   Busca la entidad con GetByIdAsync. Si existe invoca DeleteAsync y
///   SaveChangesAsync sobre el repositorio y devuelve true.
///   Si no existe devuelve false sin persistir.
///
/// Ningún test toca Entity Framework ni base de datos real.
/// </summary>
public class MantenimientoService_DeleteAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_DeleteAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Existencia
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MantenimientoExistente_RetornaTrue()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 1);
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var resultado = await _sut.DeleteAsync(1);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task MantenimientoNoExiste_RetornaFalse()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        var resultado = await _sut.DeleteAsync(999);

        // Assert
        resultado.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────
    // Invocación del repositorio
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MantenimientoExistente_InvocaDeleteAsyncConElIdCorrecto()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 5);
        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(5);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(5), Times.Once);
    }

    [Fact]
    public async Task MantenimientoExistente_InvocaSaveChangesUnaVez()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 5);
        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(5);

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MantenimientoNoExiste_NoInvocaDeleteAsync()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        await _sut.DeleteAsync(999);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task MantenimientoNoExiste_NoInvocaSaveChanges()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        await _sut.DeleteAsync(999);

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
