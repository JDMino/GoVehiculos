using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para la ESTRATEGIA "finalizar" dentro de:
///   MantenimientoService.EjecutarAccionAsync(id, empleadoId, "finalizar", contexto)
///     → Task&lt;(bool exito, string mensaje)&gt;
///
/// FinalizarStrategy (NecesitaVehiculo = true):
///   - Requiere EmpleadoId == el empleado que finaliza
///   - Requiere estado == "iniciado"
///   - FechaRealizacion no puede ser anterior a FechaProgramada
///   - Descripcion, RealizadoPor: obligatorios
///   - Costo: no puede ser negativo
///   - Contexto tipado como MantenimientoFinalizarDTO (null → error)
///   - Si todo ok → estado = "finalizado", vehiculo.EstadoMecanico = "bueno"
/// </summary>
public class MantenimientoService_EjecutarAccion_FinalizarStrategyTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_EjecutarAccion_FinalizarStrategyTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Camino feliz
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EstadoIniciado_DatosValidos_RetornaTrue()
    {
        // Arrange
        const int empleadoId = 2;
        // FechaProgramada en el pasado para que FechaRealizacion = hoy sea válida
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId: empleadoId, conVehiculo: true,
            fechaProg: DateOnly.FromDateTime(DateTime.Today.AddDays(-3)));

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var contexto = ModelBuilderMantenimiento.FinalizarDTO();

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", contexto);

        // Assert
        exito.Should().BeTrue();
    }

    [Fact]
    public async Task Exito_MutaEstadoAFinalizado()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId: empleadoId, conVehiculo: true,
            fechaProg: DateOnly.FromDateTime(DateTime.Today.AddDays(-3)));

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", ModelBuilderMantenimiento.FinalizarDTO());

        // Assert
        mant.Estado.Should().Be("finalizado");
    }

    [Fact]
    public async Task Exito_MutaEstadoMecanicoDelVehiculoABueno()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId: empleadoId, conVehiculo: true,
            fechaProg: DateOnly.FromDateTime(DateTime.Today.AddDays(-5)));

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", ModelBuilderMantenimiento.FinalizarDTO());

        // Assert — FinalizarStrategy pone EstadoMecanico = "bueno"
        mant.Vehiculo!.EstadoMecanico.Should().Be("bueno");
    }

    [Fact]
    public async Task Exito_ActualizaCamposDelContexto()
    {
        // Arrange
        const int empleadoId = 2;
        var hoy  = DateOnly.FromDateTime(DateTime.Today);
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId: empleadoId, conVehiculo: true,
            fechaProg: hoy.AddDays(-2));

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var contexto = ModelBuilderMantenimiento.FinalizarDTO(
            descripcion:     "Frenos reemplazados",
            realizadoPor:    "Taller XYZ",
            costo:           4_500,
            fechaRealizacion: hoy);

        // Act
        await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", contexto);

        // Assert
        mant.Descripcion.Should().Be("Frenos reemplazados");
        mant.RealizadoPor.Should().Be("Taller XYZ");
        mant.Costo.Should().Be(4_500);
        mant.FechaRealizacion.Should().Be(hoy);
    }

    // ────────────────────────────────────────────────────────────────
    // Permiso
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EmpleadoDistinto_RetornaFalseConMensajePermiso()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(empleadoId: 5, conVehiculo: true);
        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(
            1, 99, "finalizar", ModelBuilderMantenimiento.FinalizarDTO());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("permiso");
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
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            empleadoId: empleadoId, estadoMantenimiento: estado);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar", ModelBuilderMantenimiento.FinalizarDTO());

        // Assert
        exito.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────
    // Validaciones del contexto (MantenimientoFinalizarDTO)
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ContextoNull_RetornaFalse()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(empleadoId: empleadoId, conVehiculo: true);
        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", null);

        // Assert
        exito.Should().BeFalse();
    }

    [Fact]
    public async Task DescripcionVacia_RetornaFalse()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(empleadoId: empleadoId, conVehiculo: true);
        mant.FechaProgramada = null;
        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar", ModelBuilderMantenimiento.FinalizarDTO(descripcion: ""));

        // Assert
        exito.Should().BeFalse();
    }

    [Fact]
    public async Task RealizadoPorVacio_RetornaFalse()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(empleadoId: empleadoId, conVehiculo: true);
        mant.FechaProgramada = null;
        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar", ModelBuilderMantenimiento.FinalizarDTO(realizadoPor: ""));

        // Assert
        exito.Should().BeFalse();
    }

    [Fact]
    public async Task CostoNegativo_RetornaFalse()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(empleadoId: empleadoId, conVehiculo: true);
        mant.FechaProgramada = null;
        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar", ModelBuilderMantenimiento.FinalizarDTO(costo: -1));

        // Assert
        exito.Should().BeFalse();
    }

    [Fact]
    public async Task FechaRealizacionAnteriorAFechaProgramada_RetornaFalseConMensajeAnterior()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId: empleadoId, conVehiculo: true,
            // FechaProgramada = dentro de 5 días → FechaRealizacion hoy es anterior
            fechaProg: DateOnly.FromDateTime(DateTime.Today.AddDays(5)));

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        var contexto = ModelBuilderMantenimiento.FinalizarDTO(
            fechaRealizacion: DateOnly.FromDateTime(DateTime.Today));

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", contexto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("anterior");
    }

    [Fact]
    public async Task FechaRealizacionIgualAFechaProgramada_EsValido()
    {
        // Arrange
        const int empleadoId = 2;
        var fechaProg = DateOnly.FromDateTime(DateTime.Today);

        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId: empleadoId, conVehiculo: true, fechaProg: fechaProg);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // FechaRealizacion == FechaProgramada → no es anterior → válido
        var contexto = ModelBuilderMantenimiento.FinalizarDTO(fechaRealizacion: fechaProg);

        // Act
        var (exito, _) = await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", contexto);

        // Assert
        exito.Should().BeTrue();
    }
}
