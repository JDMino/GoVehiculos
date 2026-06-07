using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para la ESTRATEGIA "iniciar" dentro de:
///   MantenimientoService.EjecutarAccionAsync(id, empleadoId, "iniciar", contexto)
///     → Task&lt;(bool exito, string mensaje)&gt;
///
/// IniciarStrategy (NecesitaVehiculo = false):
///   - Requiere EmpleadoId == el empleado que inicia
///   - Requiere estado == "pendiente"
///   - Si todo ok → estado = "iniciado"
/// </summary>
public class MantenimientoService_EjecutarAccion_IniciarStrategyTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_EjecutarAccion_IniciarStrategyTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Camino feliz
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EstadoPendiente_EmpleadoCorrecto_RetornaTrue()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "pendiente");

        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(1, empleadoId, "iniciar");

        // Assert
        exito.Should().BeTrue();
    }

    [Fact]
    public async Task EstadoPendiente_EmpleadoCorrecto_MutaEstadoAIniciado()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "pendiente");

        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "iniciar");

        // Assert
        mant.Estado.Should().Be("iniciado");
    }

    // ────────────────────────────────────────────────────────────────
    // Permiso — empleado distinto
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EmpleadoDistinto_RetornaFalseConMensajePermiso()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: 5, estado: "pendiente");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(1, 99, "iniciar");

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("permiso");
    }

    // ────────────────────────────────────────────────────────────────
    // Estado no permitido
    // ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("iniciado")]
    [InlineData("finalizado")]
    [InlineData("cancelado")]
    public async Task EstadoDistintoDePendiente_RetornaFalse(string estado)
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: estado);
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(1, empleadoId, "iniciar");

        // Assert
        exito.Should().BeFalse();
    }

    [Fact]
    public async Task CuandoFalla_EstadoNoDebeCambiar()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "finalizado");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "iniciar");

        // Assert
        mant.Estado.Should().Be("finalizado");   // sin cambio
    }

    // ────────────────────────────────────────────────────────────────
    // Persistencia
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CuandoExito_LlamaSaveChangesUnaVez()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "pendiente");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "iniciar");

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CuandoFalla_NoLlamaSaveChanges()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: 5, estado: "pendiente");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act
        await _sut.EjecutarAccionAsync(1, 99, "iniciar");

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
