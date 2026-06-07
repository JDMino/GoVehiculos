using FluentAssertions;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para la ESTRATEGIA "cancelar" dentro de:
///   MantenimientoService.EjecutarAccionAsync(id, empleadoId, "cancelar", contexto)
///     → Task&lt;(bool exito, string mensaje)&gt;
///
/// CancelarStrategy (NecesitaVehiculo = false):
///   - Requiere EmpleadoId == el empleado que cancela
///   - Requiere estado == "iniciado"  (solo se puede cancelar lo que ya inició)
///   - Contexto tipado como MantenimientoCancelarDTO (null → error)
///   - Descripcion del DTO: obligatoria (motivo de cancelación)
///   - Si todo ok → estado = "cancelado", descripcion = dto.Descripcion
/// </summary>
public class MantenimientoService_EjecutarAccion_CancelarStrategyTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_EjecutarAccion_CancelarStrategyTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Camino feliz
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EstadoIniciado_EmpleadoCorrecto_DatosValidos_RetornaTrue()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "iniciado");

        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var contexto = ModelBuilderMantenimiento.CancelarDTO();

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(1, empleadoId, "cancelar", contexto);

        // Assert
        exito.Should().BeTrue();
    }

    [Fact]
    public async Task Exito_MutaEstadoACancelado()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "iniciado");

        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "cancelar", ModelBuilderMantenimiento.CancelarDTO());

        // Assert
        mant.Estado.Should().Be("cancelado");
    }

    [Fact]
    public async Task Exito_ActualizaDescripcionConElMotivoDelContexto()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "iniciado");

        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var contexto = new MantenimientoCancelarDTO { Descripcion = "Falta de repuestos críticos" };

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "cancelar", contexto);

        // Assert
        mant.Descripcion.Should().Be("Falta de repuestos críticos");
    }

    [Fact]
    public async Task Exito_LlamaSaveChangesUnaVez()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "iniciado");

        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "cancelar", ModelBuilderMantenimiento.CancelarDTO());

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // Permiso — empleado distinto
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EmpleadoDistinto_RetornaFalseConMensajePermiso()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: 5, estado: "iniciado");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(
            1, 99, "cancelar", ModelBuilderMantenimiento.CancelarDTO());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("permiso");
    }

    [Fact]
    public async Task EmpleadoDistinto_NoMutaElEstado()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: 5, estado: "iniciado");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act
        await _sut.EjecutarAccionAsync(1, 99, "cancelar", ModelBuilderMantenimiento.CancelarDTO());

        // Assert — estado sin cambio
        mant.Estado.Should().Be("iniciado");
    }

    // ────────────────────────────────────────────────────────────────
    // Estado no permitido
    // ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("pendiente")]
    [InlineData("finalizado")]
    [InlineData("cancelado")]
    public async Task EstadoDistintoDeIniciado_RetornaFalse(string estado)
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: estado);
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "cancelar", ModelBuilderMantenimiento.CancelarDTO());

        // Assert
        exito.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────
    // Validaciones del contexto (MantenimientoCancelarDTO)
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ContextoNull_RetornaFalse()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "iniciado");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act — sin contexto la estrategia no puede hacer el cast a MantenimientoCancelarDTO
        var (exito, _) = await _sut.EjecutarAccionAsync(1, empleadoId, "cancelar", null);

        // Assert
        exito.Should().BeFalse();
    }

    [Fact]
    public async Task DescripcionVacia_RetornaFalse()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "iniciado");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        var contexto = new MantenimientoCancelarDTO { Descripcion = "" };

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(1, empleadoId, "cancelar", contexto);

        // Assert
        exito.Should().BeFalse();
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public async Task DescripcionEnBlancoOSoloEspacios_RetornaFalse(string descripcion)
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: empleadoId, estado: "iniciado");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        var contexto = new MantenimientoCancelarDTO { Descripcion = descripcion };

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(1, empleadoId, "cancelar", contexto);

        // Assert
        exito.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────
    // Persistencia — fallo no persiste
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CuandoFalla_NoLlamaSaveChanges()
    {
        // Arrange — empleado distinto fuerza el fallo
        var mant = ModelBuilderMantenimiento.Mantenimiento(empleadoId: 5, estado: "iniciado");
        _repoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(mant);

        // Act
        await _sut.EjecutarAccionAsync(1, 99, "cancelar", ModelBuilderMantenimiento.CancelarDTO());

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
