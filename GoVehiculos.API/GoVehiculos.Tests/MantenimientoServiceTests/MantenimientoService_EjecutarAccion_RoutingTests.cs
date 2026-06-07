using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para el routing del PATRÓN STRATEGY en:
///   MantenimientoService.EjecutarAccionAsync(int id, int empleadoId, string accion, object? contexto)
///     → Task&lt;(bool exito, string mensaje)&gt;
///
/// Cubre los casos que son responsabilidad del service como coordinador,
/// ANTES de delegar en una estrategia concreta:
///   - Acción no registrada en el diccionario
///   - Selección correcta de query (GetByIdSimpleAsync vs GetByIdConVehiculoAsync)
///     según IAccionMantenimientoStrategy.NecesitaVehiculo
///   - Mantenimiento no encontrado en cada variante de query
///
/// Las reglas de negocio internas de cada estrategia se testean en sus
/// archivos propios (IniciarStrategy, FinalizarStrategy, CancelarStrategy).
/// </summary>
public class MantenimientoService_EjecutarAccionAsync_RoutingTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_EjecutarAccionAsync_RoutingTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Acción desconocida o mal escrita
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AccionNoRegistrada_RetornaFalseConMensajeQueContieneElNombre()
    {
        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(1, 1, "volcar");

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("volcar");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("INICIAR")]       // el diccionario usa lower-case exacto
    [InlineData("Finalizar")]
    [InlineData("CANCELAR")]
    public async Task AccionConCasoErroneoOVacia_RetornaFalse(string accion)
    {
        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(1, 1, accion);

        // Assert
        exito.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────
    // Mantenimiento no encontrado — estrategias sin vehículo
    // ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("iniciar")]
    [InlineData("cancelar")]
    public async Task MantenimientoNoExiste_EstrategiaSinVehiculo_RetornaFalseConMensajeEncontrado(
        string accion)
    {
        // Arrange — IniciarStrategy y CancelarStrategy usan GetByIdSimpleAsync
        _repoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(999, 1, accion);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("encontrado");
    }

    // ────────────────────────────────────────────────────────────────
    // Mantenimiento no encontrado — FinalizarStrategy (NecesitaVehiculo = true)
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MantenimientoNoExiste_EstrategiaConVehiculo_RetornaFalseConMensajeEncontrado()
    {
        // Arrange — FinalizarStrategy usa GetByIdConVehiculoAsync
        _repoMock
            .Setup(r => r.GetByIdConVehiculoAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        var contexto = ModelBuilderMantenimiento.FinalizarDTO();

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(999, 1, "finalizar", contexto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("encontrado");
    }

    // ────────────────────────────────────────────────────────────────
    // Verificación de query según NecesitaVehiculo
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Iniciar_UsaGetByIdSimpleAsync_NuncaGetByIdConVehiculo()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "pendiente");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "iniciar");

        // Assert
        _repoMock.Verify(r => r.GetByIdSimpleAsync(1), Times.Once);
        _repoMock.Verify(r => r.GetByIdConVehiculoAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Cancelar_UsaGetByIdSimpleAsync_NuncaGetByIdConVehiculo()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "iniciado");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var contexto = ModelBuilderMantenimiento.CancelarDTO();

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "cancelar", contexto);

        // Assert
        _repoMock.Verify(r => r.GetByIdSimpleAsync(1), Times.Once);
        _repoMock.Verify(r => r.GetByIdConVehiculoAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Finalizar_UsaGetByIdConVehiculoAsync_NuncaGetByIdSimple()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(empleadoId: empleadoId, conVehiculo: true);
        mant.FechaProgramada = DateOnly.MinValue;   // sin restricción de fecha

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var contexto = ModelBuilderMantenimiento.FinalizarDTO();

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", contexto);

        // Assert
        _repoMock.Verify(r => r.GetByIdConVehiculoAsync(1), Times.Once);
        _repoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // SaveChanges solo cuando la estrategia tiene éxito
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CuandoEstrategiaFalla_NoInvocaSaveChanges()
    {
        // Arrange — empleado distinto, IniciarStrategy rechaza
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: 5, estado: "pendiente");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act
        await _sut.EjecutarAccionAsync(1, 99, "iniciar");

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CuandoEstrategiaTieneExito_InvocaSaveChangesUnaVez()
    {
        // Arrange — condiciones válidas para IniciarStrategy
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "pendiente");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "iniciar");

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
