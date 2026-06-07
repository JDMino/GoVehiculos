using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.GetAllAsync(string? estado)
///     → Task&lt;IEnumerable&lt;MantenimientoResponseDTO&gt;&gt;
///
/// Comportamiento esperado:
///   Recupera la lista desde el repositorio, aplica el filtro por estado
///   si se proporciona, y mapea cada entidad a su DTO de respuesta.
///   No invoca EF ni base de datos directamente.
/// </summary>
public class MantenimientoService_GetAllAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_GetAllAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Casos de cantidad y filtrado
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SinFiltro_RetornaTodosLosDTOs()
    {
        // Arrange
        var lista = new List<Mantenimiento>
        {
            ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 1),
            ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 2)
        };

        _repoMock
            .Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(lista);

        // Act
        var resultado = await _sut.GetAllAsync(null);

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ConFiltroEstado_PropagaElFiltroAlRepositorio()
    {
        // Arrange
        const string estado = "finalizado";
        var lista = new List<Mantenimiento>
        {
            ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 1, estadoMantenimiento: estado)
        };

        _repoMock
            .Setup(r => r.GetAllAsync(estado))
            .ReturnsAsync(lista);

        // Act
        var resultado = await _sut.GetAllAsync(estado);

        // Assert
        resultado.Should().HaveCount(1);
        _repoMock.Verify(r => r.GetAllAsync(estado), Times.Once);
    }

    [Fact]
    public async Task CuandoListaEstaVacia_RetornaEnumerableVacio()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Mantenimiento>());

        // Act
        var resultado = await _sut.GetAllAsync(null);

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task InvocaElRepositorioUnaVezConElParamCorrecto()
    {
        // Arrange
        const string estado = "pendiente";
        _repoMock
            .Setup(r => r.GetAllAsync(estado))
            .ReturnsAsync(new List<Mantenimiento>());

        // Act
        await _sut.GetAllAsync(estado);

        // Assert
        _repoMock.Verify(r => r.GetAllAsync(estado), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // Mapeo de campos del DTO
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MapeaIdMantenimientoYVehiculoId_Correctamente()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 42);
        mant.VehiculoId = 7;

        _repoMock
            .Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Mantenimiento> { mant });

        // Act
        var dto = (await _sut.GetAllAsync(null)).Single();

        // Assert
        dto.IdMantenimiento.Should().Be(42);
        dto.VehiculoId.Should().Be(7);
    }

    [Fact]
    public async Task MapeaVehiculoPatenteYMarcaYModelo_CuandoNavegacionPresente()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(patente: "ZZ999ZZ");
        var mant     = ModelBuilderMantenimiento.Mantenimiento();
        mant.Vehiculo = vehiculo;

        _repoMock
            .Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Mantenimiento> { mant });

        // Act
        var dto = (await _sut.GetAllAsync(null)).Single();

        // Assert
        dto.VehiculoPatente.Should().Be("ZZ999ZZ");
        dto.VehiculoMarca.Should().Be("Chevrolet");
        dto.VehiculoModelo.Should().Be("Corsa");
    }

    [Fact]
    public async Task MapeaVehiculoEstado_CuandoNavegacionPresente()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(estado: "mantenimiento");
        var mant     = ModelBuilderMantenimiento.Mantenimiento();
        mant.Vehiculo = vehiculo;

        _repoMock
            .Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Mantenimiento> { mant });

        // Act
        var dto = (await _sut.GetAllAsync(null)).Single();

        // Assert
        dto.VehiculoEstado.Should().Be("mantenimiento");
    }

    [Fact]
    public async Task MapeaEmpleadoNombreCompleto_CuandoNavegacionPresente()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo();
        mant.Empleado = ModelBuilderMantenimiento.Empleado(nombre: "Carlos", apellido: "García");

        _repoMock
            .Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Mantenimiento> { mant });

        // Act
        var dto = (await _sut.GetAllAsync(null)).Single();

        // Assert
        dto.EmpleadoNombre.Should().Be("Carlos García");
    }

    [Fact]
    public async Task EmpleadoNombreEsNull_CuandoNavegacionEmpleadoEsNull()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo();
        mant.Empleado = null;

        _repoMock
            .Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Mantenimiento> { mant });

        // Act
        var dto = (await _sut.GetAllAsync(null)).Single();

        // Assert
        dto.EmpleadoNombre.Should().BeNull();
    }

    [Fact]
    public async Task MapeaCamposProposDelMantenimiento_Correctamente()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(
            estadoMantenimiento: "iniciado");
        mant.Tipo         = "correctivo";
        mant.Descripcion  = "Cambio de aceite";
        mant.Prioridad    = "alta";
        mant.Costo        = 3_500;
        mant.RealizadoPor = "Taller ABC";

        _repoMock
            .Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Mantenimiento> { mant });

        // Act
        var dto = (await _sut.GetAllAsync(null)).Single();

        // Assert
        dto.Tipo.Should().Be("correctivo");
        dto.Descripcion.Should().Be("Cambio de aceite");
        dto.Estado.Should().Be("iniciado");
        dto.Prioridad.Should().Be("alta");
        dto.Costo.Should().Be(3_500);
        dto.RealizadoPor.Should().Be("Taller ABC");
    }

    [Fact]
    public async Task MapeaDisponibilizado_Correctamente()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(disponibilizado: true);

        _repoMock
            .Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Mantenimiento> { mant });

        // Act
        var dto = (await _sut.GetAllAsync(null)).Single();

        // Assert
        dto.Disponibilizado.Should().BeTrue();
    }
}
