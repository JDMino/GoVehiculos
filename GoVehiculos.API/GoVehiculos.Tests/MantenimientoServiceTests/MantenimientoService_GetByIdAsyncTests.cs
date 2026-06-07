using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.GetByIdAsync(int id)
///     → Task&lt;MantenimientoResponseDTO?&gt;
///
/// Comportamiento esperado:
///   Recupera una entidad por ID desde el repositorio y la mapea a DTO.
///   Si no existe devuelve null. Invoca solo GetByIdAsync del repositorio.
/// </summary>
public class MantenimientoService_GetByIdAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_GetByIdAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Existencia y null
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CuandoExiste_RetornaDTOConIdCorrecto()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 10);

        _repoMock
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(mant);

        // Act
        var resultado = await _sut.GetByIdAsync(10);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.IdMantenimiento.Should().Be(10);
    }

    [Fact]
    public async Task CuandoNoExiste_RetornaNull()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        var resultado = await _sut.GetByIdAsync(999);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task InvocaElRepositorioUnaVezConElIdCorrecto()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(7))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        await _sut.GetByIdAsync(7);

        // Assert
        _repoMock.Verify(r => r.GetByIdAsync(7), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // Mapeo completo de campos
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MapeaTodosLosCamposDelVehiculo()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(
            patente: "BB111CC",
            estado:  "mantenimiento",
            estadoMecanico: "malo");

        var mant = ModelBuilderMantenimiento.Mantenimiento(id: 5);
        mant.Vehiculo   = vehiculo;
        mant.VehiculoId = vehiculo.IdVehiculo;

        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(mant);

        // Act
        var dto = await _sut.GetByIdAsync(5);

        // Assert
        dto!.VehiculoId.Should().Be(vehiculo.IdVehiculo);
        dto.VehiculoPatente.Should().Be("BB111CC");
        dto.VehiculoEstado.Should().Be("mantenimiento");
        dto.VehiculoMarca.Should().Be("Chevrolet");
        dto.VehiculoModelo.Should().Be("Corsa");
    }

    [Fact]
    public async Task MapeaTodosLosCamposDelEmpleado()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 5);
        mant.EmpleadoId = 3;
        mant.Empleado   = ModelBuilderMantenimiento.Empleado(id: 3, nombre: "Ana", apellido: "López");

        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(mant);

        // Act
        var dto = await _sut.GetByIdAsync(5);

        // Assert
        dto!.EmpleadoId.Should().Be(3);
        dto.EmpleadoNombre.Should().Be("Ana López");
    }

    [Fact]
    public async Task MapeaTodosLosCamposPropiasDelMantenimiento()
    {
        // Arrange
        var hoy  = DateOnly.FromDateTime(DateTime.Today);
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            id: 5, estadoMantenimiento: "finalizado");
        mant.Tipo             = "correctivo";
        mant.Descripcion      = "Cambio de aceite";
        mant.Prioridad        = "alta";
        mant.Costo            = 1_500;
        mant.RealizadoPor     = "Taller Norte";
        mant.FechaRealizacion = hoy;
        mant.Disponibilizado  = true;

        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(mant);

        // Act
        var dto = await _sut.GetByIdAsync(5);

        // Assert
        dto!.Tipo.Should().Be("correctivo");
        dto.Descripcion.Should().Be("Cambio de aceite");
        dto.Estado.Should().Be("finalizado");
        dto.Prioridad.Should().Be("alta");
        dto.Costo.Should().Be(1_500);
        dto.RealizadoPor.Should().Be("Taller Norte");
        dto.FechaRealizacion.Should().Be(hoy);
        dto.Disponibilizado.Should().BeTrue();
    }

    [Fact]
    public async Task EmpleadoNombreEsNull_CuandoEmpleadoNavegacionEsNull()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 3);
        mant.Empleado = null;

        _repoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(mant);

        // Act
        var dto = await _sut.GetByIdAsync(3);

        // Assert
        dto!.EmpleadoNombre.Should().BeNull();
    }
}
