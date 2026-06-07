// =============================================================================
// GoVehiculos.Tests/Services/IncidenciaServiceTests/UpdateAsyncTests.cs
// Tests unitarios para IncidenciaService.UpdateAsync()
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.IncidenciaServiceTests;

public class UpdateAsyncTests
{
    private readonly Mock<IIncidenciaRepository> _repoMock        = new();
    private readonly Mock<IVehiculoRepository>   _vehiculoRepoMock = new();
    private readonly Mock<IUsuarioRepository>    _usuarioRepoMock  = new();

    private IncidenciaService CrearSut() =>
        new(_repoMock.Object, _vehiculoRepoMock.Object, _usuarioRepoMock.Object);

    // ── Casos exitosos ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_DatosValidos_RetornaExitoTrue()
    {
        // Arrange
        var incidencia = ModelBuilders.Incidencia(id: 1, tipo: "accidente", gravedad: "media");
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "infraccion_vial",
            NivelGravedad = "alta",
            Descripcion   = "Descripción actualizada"
        };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(incidencia);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Incidencia actualizada correctamente.");
    }

    [Fact]
    public async Task UpdateAsync_DatosValidos_ActualizaLaEntidad()
    {
        // Arrange
        var incidencia = ModelBuilders.Incidencia(id: 1, tipo: "accidente", gravedad: "media");
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "comportamiento_indebido",
            NivelGravedad = "baja",
            Descripcion   = "Nueva descripción"
        };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(incidencia);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert — la entidad fue modificada antes de SaveChanges
        incidencia.Tipo.Should().Be("comportamiento_indebido");
        incidencia.NivelGravedad.Should().Be("baja");
        incidencia.Descripcion.Should().Be("Nueva descripción");
    }

    [Fact]
    public async Task UpdateAsync_LlamaSaveChangesAsync_CuandoEsExitoso()
    {
        // Arrange
        var incidencia = ModelBuilders.Incidencia(id: 1);
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "accidente",
            NivelGravedad = "media",
            Descripcion   = "Desc"
        };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(incidencia);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── Efecto secundario: daño_fisico → EstadoMecanico = "malo" ─────────

    [Fact]
    public async Task UpdateAsync_CambioADanioFisico_ActualizaEstadoMecanicoAMalo()
    {
        // Arrange — tipo anterior ≠ daño_fisico, nuevo = daño_fisico
        var incidencia = ModelBuilders.Incidencia(id: 1, vehiculoId: 5, tipo: "accidente");
        var vehiculo   = ModelBuilders.Vehiculo(id: 5, estadoMecanico: "bueno");

        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "daño_fisico",
            NivelGravedad = "alta",
            Descripcion   = "Golpe en la puerta"
        };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(incidencia);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(5))
            .ReturnsAsync(vehiculo);

        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert — efecto secundario aplicado
        vehiculo.EstadoMecanico.Should().Be("malo");
    }

    [Fact]
    public async Task UpdateAsync_TipoYaEraDanioFisico_NoLlamaAlVehiculoRepo()
    {
        // Arrange — tipo anterior == daño_fisico → no debe disparar efecto secundario
        var incidencia = ModelBuilders.Incidencia(id: 1, vehiculoId: 5, tipo: "daño_fisico");
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "daño_fisico",   // sin cambio de tipo
            NivelGravedad = "alta",
            Descripcion   = "Actualización de descripción"
        };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(incidencia);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert — el repo de vehículo NO debe ser consultado
        _vehiculoRepoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
    }

    // ── Casos de error: incidencia no encontrada ──────────────────────────

    [Fact]
    public async Task UpdateAsync_IncidenciaNoExiste_RetornaFalse()
    {
        // Arrange
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "accidente",
            NivelGravedad = "media",
            Descripcion   = "Desc"
        };
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Incidencia?)null);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(999, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Incidencia no encontrada.");
    }

    [Fact]
    public async Task UpdateAsync_IncidenciaNoExiste_NoLlamaSaveChanges()
    {
        // Arrange
        var dto = new IncidenciaUpdateDTO
        {
            Tipo = "accidente", NivelGravedad = "media", Descripcion = "X"
        };
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Incidencia?)null);

        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ── Validaciones de campos ────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task UpdateAsync_TipoVacioONulo_RetornaFalse(string? tipo)
    {
        // Arrange
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = tipo!,
            NivelGravedad = "media",
            Descripcion   = "Desc"
        };

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo de incidencia es obligatorio.");
    }

    [Fact]
    public async Task UpdateAsync_TipoInvalido_RetornaFalse()
    {
        // Arrange
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "tipo_inexistente",
            NivelGravedad = "media",
            Descripcion   = "Desc"
        };

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de incidencia inválido");
    }

    [Theory]
    [InlineData("daño_fisico")]
    [InlineData("accidente")]
    [InlineData("infraccion_vial")]
    [InlineData("comportamiento_indebido")]
    [InlineData("retraso_en_pago")]
    public async Task UpdateAsync_TodosLosTiposValidos_PasanValidacion(string tipo)
    {
        // Arrange
        var incidencia = ModelBuilders.Incidencia(id: 1, tipo: "accidente");
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = tipo,
            NivelGravedad = "media",
            Descripcion   = "Desc"
        };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(incidencia);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        // Si tipo == daño_fisico y cambia, GetByIdSimpleAsync es llamado
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync(ModelBuilders.Vehiculo());

        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.UpdateAsync(1, dto);

        // Assert — no debe fallar por validación de tipo
        exito.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_NivelGravedadVacio_RetornaFalse(string gravedad)
    {
        // Arrange
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "accidente",
            NivelGravedad = gravedad,
            Descripcion   = "Desc"
        };

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El nivel de gravedad es obligatorio.");
    }

    [Fact]
    public async Task UpdateAsync_NivelGravedadInvalido_RetornaFalse()
    {
        // Arrange
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "accidente",
            NivelGravedad = "extrema",
            Descripcion   = "Desc"
        };

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Nivel de gravedad inválido");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_DescripcionVacia_RetornaFalse(string descripcion)
    {
        // Arrange
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "accidente",
            NivelGravedad = "media",
            Descripcion   = descripcion
        };

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La descripción es obligatoria.");
    }

    // ── Casos borde ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_DanioFisicoVehiculoNoEncontrado_SigueSinError()
    {
        // Arrange — efecto secundario sin vehículo en BD → debe continuar sin romper
        var incidencia = ModelBuilders.Incidencia(id: 1, vehiculoId: 99, tipo: "accidente");
        var dto = new IncidenciaUpdateDTO
        {
            Tipo          = "daño_fisico",
            NivelGravedad = "alta",
            Descripcion   = "Prueba"
        };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(incidencia);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(99))
            .ReturnsAsync((Vehiculo?)null);

        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.UpdateAsync(1, dto);

        // Assert — el servicio no debe lanzar excepción
        exito.Should().BeTrue();
    }
}
