using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.DisponibilizarVehiculoAsync(int idMantenimiento)
///     → Task&lt;(bool exito, string mensaje)&gt;
///
/// Flujo real del service:
///   1. GetByIdConVehiculoAsync → debe existir
///   2. mantenimiento.Estado == "finalizado"
///   3. mantenimiento.Vehiculo != null
///   4. mantenimiento.Disponibilizado == false  (idempotencia bloqueada)
///   5. vehiculo.Estado = "disponible"
///   6. mantenimiento.Disponibilizado = true
///   7. SaveChangesAsync
///
/// Ningún test toca Entity Framework ni base de datos real.
/// </summary>
public class MantenimientoService_DisponibilizarVehiculoAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_DisponibilizarVehiculoAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Camino feliz
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task OrdenFinalizadaNoDisponibilizada_RetornaExitoTrue()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: "finalizado",
            estadoVehiculo:      "mantenimiento",
            disponibilizado:     false);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var (exito, mensaje) = await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Contain("disponibilizado");
    }

    [Fact]
    public async Task Exito_MutaVehiculoEstadoADisponible()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: "finalizado",
            estadoVehiculo:      "mantenimiento",
            disponibilizado:     false);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        mant.Vehiculo!.Estado.Should().Be("disponible");
    }

    [Fact]
    public async Task Exito_MarcaDisponibilizadoEnTrue()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: "finalizado",
            disponibilizado:     false);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        mant.Disponibilizado.Should().BeTrue();
    }

    [Fact]
    public async Task Exito_LlamaSaveChangesUnaVez()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: "finalizado",
            disponibilizado:     false);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SiempreUsaGetByIdConVehiculoAsync_NuncaGetByIdSimpleNiGetById()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: "finalizado",
            disponibilizado:     false);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(5)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.DisponibilizarVehiculoAsync(5);

        // Assert
        _repoMock.Verify(r => r.GetByIdConVehiculoAsync(5),          Times.Once);
        _repoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()),       Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // Orden no encontrada
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task OrdenNoExiste_RetornaFalseConMensajeEncontrada()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdConVehiculoAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        var (exito, mensaje) = await _sut.DisponibilizarVehiculoAsync(999);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("encontrada");
    }

    [Fact]
    public async Task OrdenNoExiste_NoLlamaSaveChanges()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdConVehiculoAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        await _sut.DisponibilizarVehiculoAsync(999);

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // Estado distinto de "finalizado"
    // ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("pendiente")]
    [InlineData("iniciado")]
    [InlineData("cancelado")]
    public async Task EstadoNoFinalizado_RetornaFalseConMensajeFinalizada(string estado)
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: estado,
            disponibilizado:     false);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("finalizada");
    }

    [Theory]
    [InlineData("pendiente")]
    [InlineData("iniciado")]
    [InlineData("cancelado")]
    public async Task EstadoNoFinalizado_NoLlamaSaveChanges(string estado)
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: estado,
            disponibilizado:     false);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // Navegación a Vehiculo es null
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VehiculoNavegacionNull_RetornaFalseConMensajeVehiculo()
    {
        // Arrange — orden finalizada pero sin la navegación cargada
        var mant = ModelBuilderMantenimiento.Mantenimiento(
            id:              1,
            estado:          "finalizado",
            disponibilizado: false);
        // mant.Vehiculo queda null (valor por defecto del builder)

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("vehículo");
    }

    [Fact]
    public async Task VehiculoNavegacionNull_NoLlamaSaveChanges()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.Mantenimiento(
            estado:          "finalizado",
            disponibilizado: false);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // Idempotencia — ya fue disponibilizada antes
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task YaDisponibilizada_RetornaFalseConMensajeAnteriormente()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: "finalizado",
            disponibilizado:     true);     // <-- ya fue procesada

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("anteriormente");
    }

    [Fact]
    public async Task YaDisponibilizada_NoVuelveAMutarElVehiculo()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: "finalizado",
            estadoVehiculo:      "disponible",
            disponibilizado:     true);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        string estadoOriginal = mant.Vehiculo!.Estado;

        // Act
        await _sut.DisponibilizarVehiculoAsync(1);

        // Assert — estado sin segunda mutación
        mant.Vehiculo.Estado.Should().Be(estadoOriginal);
    }

    [Fact]
    public async Task YaDisponibilizada_NoLlamaSaveChanges()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: "finalizado",
            disponibilizado:     true);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        await _sut.DisponibilizarVehiculoAsync(1);

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // Verificación de que el ID se pasa correctamente al repositorio
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PropagaElIdCorrectoAGetByIdConVehiculoAsync()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdConVehiculoAsync(42))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        await _sut.DisponibilizarVehiculoAsync(42);

        // Assert
        _repoMock.Verify(r => r.GetByIdConVehiculoAsync(42), Times.Once);
        _repoMock.Verify(r => r.GetByIdConVehiculoAsync(It.Is<int>(x => x != 42)), Times.Never);
    }
}
