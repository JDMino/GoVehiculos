using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.CreateAsync(MantenimientoCreateDTO dto)
///     → Task&lt;(bool exito, string mensaje, MantenimientoResponseDTO? dto)&gt;
///
/// Casos cubiertos (alineados con planilla de pruebas):
///   CA01 — Datos válidos, SP exitoso
///   CA05 — Tipo vacío
///   CA06 — Tipo con solo espacios
///   CA07 — Descripción vacía
///   CA08 — Prioridad vacía
///   CA09 — FechaProgramada en el pasado
///   CA10 — FechaProgramada igual a hoy (límite válido)
///   CA11 — EmpleadoId = 0
///   CA12 — EmpleadoId negativo
///   CA13 — SP retorna error de negocio
///
/// Ningún test toca Entity Framework ni base de datos real.
/// </summary>
public class MantenimientoService_CreateAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_CreateAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Helper privado — configura el SP y la recarga de la orden
    // ────────────────────────────────────────────────────────────────

    private void ConfigurarSpExitoso(int idMantenimiento = 1)
    {
        _repoMock
            .Setup(r => r.CrearConSPAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync((true, "Orden de mantenimiento creada correctamente.", idMantenimiento));

        _repoMock
            .Setup(r => r.GetByIdAsync(idMantenimiento))
            .ReturnsAsync(ModelBuilderMantenimiento.MantenimientoConVehiculo(id: idMantenimiento));
    }

    // ────────────────────────────────────────────────────────────────
    // CA01 — Datos completamente válidos, SP exitoso
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA01_DatosCompletamenteValidos_SpExitoso_RetornaExitoYMensajeCorrecto()
    {
        // Arrange
        ConfigurarSpExitoso(idMantenimiento: 1);

        var dto = ModelBuilderMantenimiento.CreateDTO(
            vehiculoId:      1,
            empleadoId:      1,
            tipo:            "correctivo",
            descripcion:     "Cambio de frenos",
            prioridad:       "alta",
            fechaProgramada: DateOnly.FromDateTime(DateTime.Today));

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Orden de mantenimiento creada correctamente.");
        resultado.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // CA05 — Tipo vacío
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA05_TipoVacio_RetornaFalseConMensajeObligatorio()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(tipo: "");

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo de mantenimiento es obligatorio.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // CA06 — Tipo con solo espacios
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA06_TipoConSoloEspacios_RetornaFalseConMensajeObligatorio()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(tipo: " ");

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo de mantenimiento es obligatorio.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // CA07 — Descripción vacía
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA07_DescripcionVacia_RetornaFalseConMensajeObligatorio()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(descripcion: "");

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La descripción es obligatoria.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // CA08 — Prioridad vacía
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA08_PrioridadVacia_RetornaFalseConMensajeObligatorio()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(prioridad: "");

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La prioridad es obligatoria.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // CA09 — FechaProgramada en el pasado
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA09_FechaProgramadaEnElPasado_RetornaFalseConMensajeFecha()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(
            fechaProgramada: DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La fecha programada no puede ser anterior a hoy.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // CA10 — FechaProgramada igual a hoy (límite válido)
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA10_FechaProgramadaIgualAHoy_EsValidaYRetornaExito()
    {
        // Arrange — la validación es estrictamente "< hoy", por lo que hoy mismo es válido
        ConfigurarSpExitoso(idMantenimiento: 1);

        var dto = ModelBuilderMantenimiento.CreateDTO(
            fechaProgramada: DateOnly.FromDateTime(DateTime.Today));

        // Act
        var (exito, mensaje, _) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Orden de mantenimiento creada correctamente.");
    }

    // ────────────────────────────────────────────────────────────────
    // CA11 — EmpleadoId = 0
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA11_EmpleadoIdCero_RetornaFalseConMensajeEmpleado()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(empleadoId: 0);

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El empleado asignado es obligatorio. Debe seleccionar un empleado válido.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // CA12 — EmpleadoId negativo
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA12_EmpleadoIdNegativo_RetornaFalseConMensajeEmpleado()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(empleadoId: -5);

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El empleado asignado es obligatorio. Debe seleccionar un empleado válido.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // CA13 — SP retorna error de negocio
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA13_SpRetornaError_RetornaFalseConMensajeDelSP()
    {
        // Arrange
        _repoMock
            .Setup(r => r.CrearConSPAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync((false, "El vehículo ya tiene una orden de mantenimiento activa.", 0));

        var dto = ModelBuilderMantenimiento.CreateDTO();

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El vehículo ya tiene una orden de mantenimiento activa.");
        resultado.Should().BeNull();
    }
}