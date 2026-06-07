using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.CreateAsync(MantenimientoCreateDTO dto)
///     → Task&lt;(bool exito, string mensaje, MantenimientoResponseDTO? dto)&gt;
///
/// Flujo real del service:
///   1. ValidarCamposCreate (validaciones locales, sin BD)
///   2. repo.CrearConSPAsync (procedimiento almacenado — mockeado)
///   3. GetByIdAsync (recarga la entidad creada con navegaciones)
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
            .ReturnsAsync((true, "Orden creada correctamente.", idMantenimiento));

        _repoMock
            .Setup(r => r.GetByIdAsync(idMantenimiento))
            .ReturnsAsync(ModelBuilderMantenimiento.MantenimientoConVehiculo(id: idMantenimiento));
    }

    // ────────────────────────────────────────────────────────────────
    // Camino feliz
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DatosValidos_RetornaExitoTrue()
    {
        // Arrange
        ConfigurarSpExitoso();
        var dto = ModelBuilderMantenimiento.CreateDTO();

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().NotBeEmpty();
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task DatosValidos_DTOResultadoTieneElIdGeneradoPorElSP()
    {
        // Arrange
        ConfigurarSpExitoso(idMantenimiento: 42);
        var dto = ModelBuilderMantenimiento.CreateDTO();

        // Act
        var (_, _, resultado) = await _sut.CreateAsync(dto);

        // Assert
        resultado!.IdMantenimiento.Should().Be(42);
    }

    [Fact]
    public async Task DatosValidos_InvocaElSpConTodosLosParametrosCorrectos()
    {
        // Arrange
        ConfigurarSpExitoso();
        var manana = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var dto = ModelBuilderMantenimiento.CreateDTO(
            vehiculoId:      3,
            empleadoId:      8,
            tipo:            "correctivo",
            descripcion:     "Cambio de frenos",
            prioridad:       "alta",
            fechaProgramada: manana);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        _repoMock.Verify(r => r.CrearConSPAsync(
            3, 8, "correctivo", "Cambio de frenos", "alta", manana), Times.Once);
    }

    [Fact]
    public async Task DatosValidos_DespuesDelSP_InvocaGetByIdAsyncParaRecargarElDTO()
    {
        // Arrange
        ConfigurarSpExitoso(idMantenimiento: 7);
        var dto = ModelBuilderMantenimiento.CreateDTO();

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        _repoMock.Verify(r => r.GetByIdAsync(7), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // Validaciones locales — Tipo
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TipoVacio_RetornaExitoFalseConMensaje()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(tipo: "");

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().NotBeEmpty();
        resultado.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TipoEnBlancoOSoloEspacios_RetornaFalse(string tipo)
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(tipo: tipo);

        // Act
        var (exito, _, _) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────
    // Validaciones locales — Descripcion
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DescripcionVacia_RetornaFalse()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(descripcion: "");

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().NotBeEmpty();
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // Validaciones locales — Prioridad
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PrioridadVacia_RetornaFalse()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(prioridad: "");

        // Act
        var (exito, mensaje, _) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().NotBeEmpty();
    }

    // ────────────────────────────────────────────────────────────────
    // Validaciones locales — FechaProgramada
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task FechaProgramadaEnElPasado_RetornaFalse()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(
            fechaProgramada: DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().NotBeEmpty();
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task FechaProgramadaHoy_EsValida_NoFallaValidacion()
    {
        // Arrange — la validación es "< hoy", así que hoy mismo es válido
        ConfigurarSpExitoso();
        var dto = ModelBuilderMantenimiento.CreateDTO(
            fechaProgramada: DateOnly.FromDateTime(DateTime.Today));

        // Act
        var (exito, _, _) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeTrue();
    }

    // ────────────────────────────────────────────────────────────────
    // Validaciones locales — EmpleadoId
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EmpleadoIdCero_RetornaFalseConMensajeQueContieneEmpleado()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(empleadoId: 0);

        // Act
        var (exito, mensaje, _) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("empleado");
    }

    [Fact]
    public async Task EmpleadoIdNegativo_RetornaFalse()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.CreateDTO(empleadoId: -5);

        // Act
        var (exito, _, _) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────
    // SP retorna error
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CuandoSpRetornaError_RetornaExitoFalseConMensajeDelSP()
    {
        // Arrange
        _repoMock
            .Setup(r => r.CrearConSPAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync((false, "El vehículo ya tiene una orden activa.", 0));

        var dto = ModelBuilderMantenimiento.CreateDTO();

        // Act
        var (exito, mensaje, resultado) = await _sut.CreateAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("activa");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // Cortocircuito — validación local impide llamar al SP
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CuandoFallaValidacionLocal_NoInvocaElSP()
    {
        // Arrange — tipo vacío dispara la validación local antes del SP
        var dto = ModelBuilderMantenimiento.CreateDTO(tipo: "");

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        _repoMock.Verify(r => r.CrearConSPAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task CuandoSpRetornaError_NoInvocaGetByIdAsync()
    {
        // Arrange
        _repoMock
            .Setup(r => r.CrearConSPAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync((false, "Error del SP.", 0));

        var dto = ModelBuilderMantenimiento.CreateDTO();

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }
}
