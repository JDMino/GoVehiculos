// =============================================================================
// GoVehiculos.Tests/Services/MultaServiceTests/CancelarAsyncTests.cs
// Alineado con la planilla de pruebas unitarias — PDF versión final.
// CA-01 a CA-07. CA-08 eliminado por no estar en la planilla.
//
// CA-01: el mensaje esperado es el que devuelve el SP configurado en el mock.
// CA-03: se separa en dos tests (CA03a y CA03b) para cubrir los dos mensajes
//        posibles que el SP puede devolver según la planilla.
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Observers;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;

namespace GoVehiculos.Tests.Services.MultaServiceTests;

public class CancelarAsyncTests
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

    // ── CA-01 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA01_MotivoValidoSPCancelaExitosamente_RetornaExitoTrue()
    {
        // Arrange
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(5, "Error en el registro"))
            .ReturnsAsync((true, "Multa cancelada y penalización revocada correctamente."));

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.CancelarAsync(5,
            new MultaCancelarDTO { MotivoCancelacion = "Error en el registro" });

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Multa cancelada y penalización revocada correctamente.");
    }

    // ── CA-02 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA02_SPInvocadoConIdYMotivoCorrectos()
    {
        // Arrange
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((true, "OK"));

        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.CancelarAsync(7,
            new MultaCancelarDTO { MotivoCancelacion = "Motivo de prueba" });

        // Assert
        exito.Should().BeTrue();
        _multaRepoMock.Verify(
            r => r.CancelarConSPAsync(7, "Motivo de prueba"),
            Times.Once);
    }

    // ── CA-03a ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA03a_SPRechazaPorqueLaMultaNoExiste_RetornaFalse()
    {
        // Arrange
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((false, "La multa indicada no existe."));

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.CancelarAsync(1,
            new MultaCancelarDTO { MotivoCancelacion = "Motivo" });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La multa indicada no existe.");
    }

    // ── CA-03b ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA03b_SPRechazaPorqueMultaYaFueCancelada_RetornaFalse()
    {
        // Arrange
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((false, "La multa ya fue cancelada anteriormente."));

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.CancelarAsync(1,
            new MultaCancelarDTO { MotivoCancelacion = "Motivo" });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La multa ya fue cancelada anteriormente.");
    }

    // ── CA-04 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA04_MotivoCancelacionVacio_RetornaFalseSinLlamarAlSP()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.CancelarAsync(1,
            new MultaCancelarDTO { MotivoCancelacion = "" });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El motivo de la cancelación es obligatorio y no puede estar vacío.");
        _multaRepoMock.Verify(
            r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    // ── CA-05 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA05_MotivoCompuestoSoloDeEspacios_RetornaFalseSinLlamarAlSP()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.CancelarAsync(1,
            new MultaCancelarDTO { MotivoCancelacion = "   " });

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El motivo de la cancelación es obligatorio y no puede estar vacío.");
        _multaRepoMock.Verify(
            r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    // ── CA-06 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA06_MotivoNulo_RetornaFalseSinLlamarAlSP()
    {
        // Arrange
        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.CancelarAsync(1,
            new MultaCancelarDTO { MotivoCancelacion = null! });

        // Assert
        exito.Should().BeFalse();
        _multaRepoMock.Verify(
            r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    // ── CA-07 ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CA07_MotivoDeUnCaracter_SPInvocadoConMotivoCorrecto()
    {
        // Arrange
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((true, "OK"));

        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.CancelarAsync(1,
            new MultaCancelarDTO { MotivoCancelacion = "X" });

        // Assert
        exito.Should().BeTrue();
        _multaRepoMock.Verify(
            r => r.CancelarConSPAsync(1, "X"),
            Times.Once);
    }
}