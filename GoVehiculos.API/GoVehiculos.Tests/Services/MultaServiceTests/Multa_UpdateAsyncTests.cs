// =============================================================================
// GoVehiculos.Tests/Services/MultaServiceTests/UpdateAsyncTests.cs
// Alineado con la planilla de pruebas unitarias — PDF versión final.
// UA-01 a UA-12. UA-13 y UA-14 eliminados por no estar en la planilla.
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

    // ── UA-01 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA01_ActualizacionValidaMultaPendiente_RetornaExitoTrue()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "pendiente");
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);
        _multaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo        = "economica",
            Monto       = 3000m,
            Descripcion = "Actualización",
            Estado      = "pendiente"
        });

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Multa actualizada correctamente.");
    }

    // ── UA-02 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA02_CambioDeEstadoDePendienteAPagada_ActualizaEntidad()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "pendiente", tipo: "economica", monto: 100m);
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);
        _multaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo        = "administrativa",
            Monto       = 1500m,
            Descripcion = "Nueva desc",
            Estado      = "pagada"
        });

        // Assert
        exito.Should().BeTrue();
        multa.Tipo.Should().Be("administrativa");
        multa.Monto.Should().Be(1500m);
        multa.Estado.Should().Be("pagada");
    }

    // ── UA-03 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA03_TipoMultaMixta_EsValido()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "pendiente");
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);
        _multaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo        = "mixta",
            Monto       = 2000m,
            Descripcion = "X",
            Estado      = "pendiente"
        });

        // Assert
        exito.Should().BeTrue();
    }

    // ── UA-04 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA04_MultaNoExiste_RetornaFalse()
    {
        // Arrange
        _multaRepoMock
            .Setup(r => r.GetByIdSimpleAsync(999))
            .ReturnsAsync((Multa?)null);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(999, new MultaUpdateDTO
        {
            Tipo   = "economica",
            Monto  = 100m,
            Estado = "pendiente"
        });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Multa no encontrada.");
    }

    // ── UA-05 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA05_MultaYaCancelada_RetornaFalse()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "cancelada");
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo   = "economica",
            Monto  = 100m,
            Estado = "pendiente"
        });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Una multa cancelada no puede modificarse.");
    }

    // ── UA-06 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA06_IntentoCambiarEstadoACanceladaPorEdicionDirecta_RetornaFalse()
    {
        // Arrange
        // ValidarCamposUpdate se ejecuta ANTES de consultar la BD.
        // "cancelada" no está en EstadosMultaEditables = ["pendiente", "pagada"]
        // → retorna el mensaje de validación sin llegar al check explícito.
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo   = "economica",
            Monto  = 100m,
            Estado = "cancelada"
        });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Estado inválido desde edición. Valores permitidos: pendiente, pagada.");
    }

    // ── UA-07 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA07_TipoMultaVacio_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo   = "",
            Monto  = 100m,
            Estado = "pendiente"
        });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo de multa es obligatorio.");
    }

    // ── UA-08 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA08_TipoMultaFueraDeValoresPermitidos_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo   = "tipo_invalido",
            Monto  = 100m,
            Estado = "pendiente"
        });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de multa inválido. Valores permitidos: economica, administrativa, mixta.");
    }

    // ── UA-09 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA09_MontoNegativo_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo   = "economica",
            Monto  = -500m,
            Estado = "pendiente"
        });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El monto no puede ser negativo.");
    }

    // ── UA-10 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA10_MontoCero_EsValido()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "pendiente");
        _multaRepoMock.Setup(r => r.GetByIdSimpleAsync(1)).ReturnsAsync(multa);
        _multaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo   = "economica",
            Monto  = 0m,
            Estado = "pendiente"
        });

        // Assert
        exito.Should().BeTrue();
    }

    // ── UA-11 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA11_EstadoVacio_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo   = "economica",
            Monto  = 100m,
            Estado = ""
        });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El estado de la multa es obligatorio.");
    }

    // ── UA-12 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UA12_EstadoFueraDeValoresPermitidos_RetornaFalse()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, new MultaUpdateDTO
        {
            Tipo   = "economica",
            Monto  = 100m,
            Estado = "estado_invalido"
        });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Estado inválido desde edición. Valores permitidos: pendiente, pagada.");
    }
}