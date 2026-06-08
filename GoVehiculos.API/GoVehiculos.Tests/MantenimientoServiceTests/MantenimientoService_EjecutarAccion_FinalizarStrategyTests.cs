using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services;

/// <summary>
/// Tests unitarios para la ESTRATEGIA "finalizar" dentro de:
///   MantenimientoService.EjecutarAccionAsync(id, empleadoId, "finalizar", contexto)
///     → Task&lt;(bool exito, string mensaje)&gt;
///
/// Casos cubiertos (alineados con planilla de pruebas):
///   EF01 — Todos los datos válidos
///   EF05 — Empleado que ejecuta no es el dueño de la orden
///   EF06 — Estado del mantenimiento es "pendiente"
///   EF07 — Estado del mantenimiento es "finalizado"
///   EF08 — Estado del mantenimiento es "cancelado"
///   EF10 — Descripción vacía en el DTO
///   EF11 — RealizadoPor vacío en el DTO
///   EF12 — Costo negativo
///   EF13 — FechaRealizacion anterior a FechaProgramada
///   EF14 — FechaRealizacion igual a FechaProgramada (límite válido)
///
/// FinalizarStrategy (NecesitaVehiculo = true): usa GetByIdConVehiculoAsync.
/// Ningún test toca Entity Framework ni base de datos real.
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
    // EF01 — Todos los datos válidos
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF01_TodosLosDatosValidos_RetornaExitoYMensajeCorrecto()
    {
        // Arrange
        const int empleadoId = 2;

        // FechaProgramada hace 3 días para que FechaRealizacion = hoy sea válida
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            id:          1,
            empleadoId:  empleadoId,
            conVehiculo: true,
            fechaProg:   DateOnly.FromDateTime(DateTime.Today.AddDays(-3)));

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var contexto = ModelBuilderMantenimiento.FinalizarDTO(
            descripcion:      "Trabajo completado",
            realizadoPor:     "Taller Sur",
            costo:            2_000,
            fechaRealizacion: DateOnly.FromDateTime(DateTime.Today));

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", contexto);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Mantenimiento finalizado correctamente.");
    }

    // ────────────────────────────────────────────────────────────────
    // EF05 — Empleado que ejecuta no es el dueño de la orden
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF05_EmpleadoNoEsDuenio_RetornaFalseConMensajePermiso()
    {
        // Arrange — la orden le pertenece al empleado 5, pero quien intenta ejecutarla es el 99
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId:  5,
            conVehiculo: true);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        var contexto = ModelBuilderMantenimiento.FinalizarDTO();

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(1, 99, "finalizar", contexto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("No tenés permiso para operar este mantenimiento.");
    }

    // ────────────────────────────────────────────────────────────────
    // EF06 — Estado del mantenimiento es "pendiente"
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF06_EstadoPendiente_RetornaFalseConMensajeEstado()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            empleadoId:          empleadoId,
            estadoMantenimiento: "pendiente");

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar", ModelBuilderMantenimiento.FinalizarDTO());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El mantenimiento no puede finalizarse porque está en estado 'pendiente'.");
    }

    // ────────────────────────────────────────────────────────────────
    // EF07 — Estado del mantenimiento es "finalizado"
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF07_EstadoFinalizado_RetornaFalseConMensajeEstado()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            empleadoId:          empleadoId,
            estadoMantenimiento: "finalizado");

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar", ModelBuilderMantenimiento.FinalizarDTO());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El mantenimiento no puede finalizarse porque está en estado 'finalizado'.");
    }

    // ────────────────────────────────────────────────────────────────
    // EF08 — Estado del mantenimiento es "cancelado"
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF08_EstadoCancelado_RetornaFalseConMensajeEstado()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            empleadoId:          empleadoId,
            estadoMantenimiento: "cancelado");

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar", ModelBuilderMantenimiento.FinalizarDTO());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El mantenimiento no puede finalizarse porque está en estado 'cancelado'.");
    }

    // ────────────────────────────────────────────────────────────────
    // EF10 — Descripción vacía en el DTO
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF10_DescripcionVacia_RetornaFalseConMensajeObligatorio()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId:  empleadoId,
            conVehiculo: true);
        mant.FechaProgramada = null;

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar",
            ModelBuilderMantenimiento.FinalizarDTO(descripcion: ""));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La descripción es obligatoria.");
    }

    // ────────────────────────────────────────────────────────────────
    // EF11 — RealizadoPor vacío en el DTO
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF11_RealizadoPorVacio_RetornaFalseConMensajeObligatorio()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId:  empleadoId,
            conVehiculo: true);
        mant.FechaProgramada = null;

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar",
            ModelBuilderMantenimiento.FinalizarDTO(realizadoPor: ""));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Debe indicar quién realizó el trabajo.");
    }

    // ────────────────────────────────────────────────────────────────
    // EF12 — Costo negativo
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF12_CostoNegativo_RetornaFalseConMensajeCosto()
    {
        // Arrange
        const int empleadoId = 2;
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId:  empleadoId,
            conVehiculo: true);
        mant.FechaProgramada = null;

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(
            1, empleadoId, "finalizar",
            ModelBuilderMantenimiento.FinalizarDTO(costo: -1));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El costo no puede ser negativo.");
    }

    // ────────────────────────────────────────────────────────────────
    // EF13 — FechaRealizacion anterior a FechaProgramada
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF13_FechaRealizacionAnteriorAFechaProgramada_RetornaFalseConMensajeFecha()
    {
        // Arrange
        const int empleadoId = 2;

        // FechaProgramada dentro de 5 días → FechaRealizacion hoy es anterior
        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId:  empleadoId,
            conVehiculo: true,
            fechaProg:   DateOnly.FromDateTime(DateTime.Today.AddDays(5)));

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);

        var contexto = ModelBuilderMantenimiento.FinalizarDTO(
            fechaRealizacion: DateOnly.FromDateTime(DateTime.Today));

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", contexto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("La fecha de realización no puede ser anterior a la fecha programada");
    }

    // ────────────────────────────────────────────────────────────────
    // EF14 — FechaRealizacion igual a FechaProgramada (límite válido)
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EF14_FechaRealizacionIgualAFechaProgramada_EsValidaYRetornaExito()
    {
        // Arrange
        const int empleadoId = 2;
        var fechaProg = DateOnly.FromDateTime(DateTime.Today);

        var mant = ModelBuilderMantenimiento.MantenimientoIniciado(
            empleadoId:  empleadoId,
            conVehiculo: true,
            fechaProg:   fechaProg);

        _repoMock.Setup(r => r.GetByIdConVehiculoAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // FechaRealizacion == FechaProgramada → no es anterior → válido
        var contexto = ModelBuilderMantenimiento.FinalizarDTO(fechaRealizacion: fechaProg);

        // Act
        var (exito, mensaje) = await _sut.EjecutarAccionAsync(1, empleadoId, "finalizar", contexto);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Mantenimiento finalizado correctamente.");
    }
}