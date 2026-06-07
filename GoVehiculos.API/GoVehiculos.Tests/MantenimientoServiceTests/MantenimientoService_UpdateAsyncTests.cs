using FluentAssertions;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.UpdateAsync(int id, MantenimientoUpdateDTO dto)
///     → Task&lt;bool&gt;
///
/// Comportamiento esperado:
///   Recupera la entidad con GetByIdAsync (incluye navegaciones),
///   aplica todos los cambios del DTO directamente sobre la entidad en memoria
///   y llama a SaveChangesAsync. Si no existe devuelve false sin persistir.
///
/// Ningún test toca Entity Framework ni base de datos real.
/// </summary>
public class MantenimientoService_UpdateAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_UpdateAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Existencia
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MantenimientoExistente_RetornaTrue()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo(id: 1);
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var resultado = await _sut.UpdateAsync(1, ModelBuilderMantenimiento.UpdateDTO());

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task MantenimientoNoExiste_RetornaFalse()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        var resultado = await _sut.UpdateAsync(999, ModelBuilderMantenimiento.UpdateDTO());

        // Assert
        resultado.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────
    // Mutación de campos
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ActualizaTodosLosCamposEditablesEnLaEntidad()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo();
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var hoy    = DateOnly.FromDateTime(DateTime.Today);
        var manana = hoy.AddDays(1);

        var dto = new MantenimientoUpdateDTO
        {
            Estado           = "iniciado",
            Prioridad        = "alta",
            Descripcion      = "Descripción actualizada",
            EmpleadoId       = 5,
            Costo            = 3_000,
            RealizadoPor     = "Taller ABC",
            FechaProgramada  = manana,
            FechaRealizacion = hoy
        };

        // Act
        await _sut.UpdateAsync(1, dto);

        // Assert — verificación sobre la entidad mutada en memoria
        mant.Estado.Should().Be("iniciado");
        mant.Prioridad.Should().Be("alta");
        mant.Descripcion.Should().Be("Descripción actualizada");
        mant.EmpleadoId.Should().Be(5);
        mant.Costo.Should().Be(3_000);
        mant.RealizadoPor.Should().Be("Taller ABC");
        mant.FechaProgramada.Should().Be(manana);
        mant.FechaRealizacion.Should().Be(hoy);
    }

    [Fact]
    public async Task EmpleadoIdNullable_SeAsignaCorectamenteComoNull()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo();
        mant.EmpleadoId = 2;   // tiene empleado previo

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = ModelBuilderMantenimiento.UpdateDTO(empleadoId: null);

        // Act
        await _sut.UpdateAsync(1, dto);

        // Assert
        mant.EmpleadoId.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // Persistencia
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MantenimientoExistente_LlamaSaveChangesExactamenteUnaVez()
    {
        // Arrange
        var mant = ModelBuilderMantenimiento.MantenimientoConVehiculo();
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(mant);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(1, ModelBuilderMantenimiento.UpdateDTO());

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MantenimientoNoExiste_NoLlamaSaveChanges()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        await _sut.UpdateAsync(999, ModelBuilderMantenimiento.UpdateDTO());

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // Query utilizada
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UsaGetByIdAsync_NoGetByIdSimple()
    {
        // Arrange — UpdateAsync carga con navegaciones (GetByIdAsync),
        // no con la versión simple, para poder devolver el DTO con datos completos.
        _repoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Mantenimiento?)null);

        // Act
        await _sut.UpdateAsync(1, ModelBuilderMantenimiento.UpdateDTO());

        // Assert
        _repoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        _repoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
    }
}
