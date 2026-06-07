// =============================================================================
// GoVehiculos.Tests/Services/MultaServiceTests/UpdateAsyncTests.cs
//
// CORRECCIÓN PROBLEMA 1: eliminados Mock<IncidenciaService> y Mock<PenalizacionService>.
// Se usan instancias REALES construidas con sus repositorios mockeados.
//
// CORRECCIÓN PROBLEMA 2: el test UpdateAsync_IntentoCambiarEstadoACancelada_RetornaFalse
// esperaba mensaje.Should().Contain("endpoint dedicado") pero el flujo real es:
//
//   UpdateAsync llama ValidarCamposUpdate(dto) PRIMERO.
//   ValidarCamposUpdate comprueba si dto.Estado está en EstadosMultaEditables
//   = ["pendiente", "pagada"].
//   "cancelada" NO está → retorna "Estado inválido desde edición. Valores permitidos: pendiente, pagada."
//   El método retorna ANTES de llegar al check explícito de dto.Estado == "cancelada".
//
// El test fue ajustado para validar el mensaje real que devuelve el servicio.
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Observers;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.MultaServiceTests;

public class UpdateAsyncTests
{
    private readonly Mock<IMultaRepository>        _multaRepoMock        = new();
    private readonly Mock<IPenalizacionRepository> _penalizacionRepoMock = new();
    private readonly Mock<IVehiculoRepository>     _vehiculoRepoMock     = new();
    private readonly Mock<IUsuarioRepository>      _usuarioRepoMock      = new();
    private readonly Mock<IIncidenciaRepository>   _incidenciaRepoMock   = new();

    private MultaService CrearSut() =>
        new(_multaRepoMock.Object,
            _penalizacionRepoMock.Object,
            _vehiculoRepoMock.Object,
            _usuarioRepoMock.Object,
            new IncidenciaService(
                _incidenciaRepoMock.Object,
                _vehiculoRepoMock.Object,
                _usuarioRepoMock.Object),
            new PenalizacionService(
                _penalizacionRepoMock.Object,
                _vehiculoRepoMock.Object,
                _usuarioRepoMock.Object,
                _multaRepoMock.Object),
            Enumerable.Empty<IMultaObserver>());

    private static MultaUpdateDTO DtoValido(
        string  tipo   = "economica",
        string  estado = "pendiente",
        decimal monto  = 1000m) => new()
    {
        Tipo        = tipo,
        Monto       = monto,
        Descripcion = "Descripción actualizada",
        Estado      = estado
    };

    // ── Casos exitosos ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_DatosValidos_RetornaExitoTrue()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "pendiente");
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);
        _multaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, DtoValido());

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Multa actualizada correctamente.");
    }

    [Fact]
    public async Task UpdateAsync_DatosValidos_ActualizaLaEntidad()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "pendiente", tipo: "economica", monto: 100m);
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);
        _multaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new MultaUpdateDTO
        {
            Tipo        = "administrativa",
            Monto       = 2500m,
            Descripcion = "Nueva descripción",
            Estado      = "pagada"
        };
        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert
        multa.Tipo.Should().Be("administrativa");
        multa.Monto.Should().Be(2500m);
        multa.Estado.Should().Be("pagada");
        multa.Descripcion.Should().Be("Nueva descripción");
    }

    [Fact]
    public async Task UpdateAsync_LlamaSaveChanges()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1);
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);
        _multaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, DtoValido());

        // Assert
        _multaRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("pendiente")]
    [InlineData("pagada")]
    public async Task UpdateAsync_EstadosEditablesValidos_RetornaExitoTrue(string estado)
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "pendiente");
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);
        _multaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.UpdateAsync(1, DtoValido(estado: estado));

        // Assert
        exito.Should().BeTrue();
    }

    // ── Casos de error ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_MultaNoExiste_RetornaFalse()
    {
        // Arrange
        _multaRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync((Multa?)null);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(999, DtoValido());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Multa no encontrada.");
    }

    [Fact]
    public async Task UpdateAsync_MultaCancelada_RetornaFalse()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "cancelada");
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, DtoValido());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Una multa cancelada no puede modificarse.");
    }

    [Fact]
    public async Task UpdateAsync_IntentoCambiarEstadoACancelada_RetornaFalse()
    {
        // CORRECCIÓN PROBLEMA 2:
        // ValidarCamposUpdate se ejecuta ANTES de cargar la multa de la BD.
        // EstadosMultaEditables = ["pendiente", "pagada"].
        // "cancelada" no está en esa lista → la validación retorna primero con:
        // "Estado inválido desde edición. Valores permitidos: pendiente, pagada."
        // El servicio NUNCA llega al check explícito `if (dto.Estado == "cancelada")`.
        // El test original esperaba "endpoint dedicado" → era incorrecto.

        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, DtoValido(estado: "cancelada"));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Estado inválido desde edición. Valores permitidos: pendiente, pagada.");
    }

    [Fact]
    public async Task UpdateAsync_TipoInvalido_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, DtoValido(tipo: "tipo_invalido"));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de multa inválido");
    }

    [Fact]
    public async Task UpdateAsync_MontoNegativo_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo = "economica", Monto = -1m, Estado = "pendiente"
        });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El monto no puede ser negativo.");
    }

    [Fact]
    public async Task UpdateAsync_EstadoInvalido_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, DtoValido(estado: "estado_invalido"));

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Estado inválido desde edición");
    }

    [Fact]
    public async Task UpdateAsync_NoLlamaSaveChanges_CuandoFalla()
    {
        // Arrange — la validación falla antes de llegar al repo
        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, DtoValido(tipo: "tipo_invalido"));

        // Assert
        _multaRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
