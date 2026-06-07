// =============================================================================
// GoVehiculos.Tests/Services/MultaServiceTests/CancelarAsyncTests.cs
// CORRECCIÓN: eliminados Mock<IncidenciaService> y Mock<PenalizacionService>.
// Se usan instancias REALES construidas con sus repositorios mockeados.
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

    // ── Casos exitosos ────────────────────────────────────────────────────

    [Fact]
    public async Task CancelarAsync_MotivoValido_DelegaAlSPYRetornaSuResultado()
    {
        // Arrange
        var dto = new MultaCancelarDTO { MotivoCancelacion = "Error humano en el registro" };
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(5, "Error humano en el registro"))
            .ReturnsAsync((true, "Multa cancelada correctamente."));

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.CancelarAsync(5, dto);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Multa cancelada correctamente.");
    }

    [Fact]
    public async Task CancelarAsync_MotivoValido_LlamaCancelarConSPAsyncConParametrosCorrectos()
    {
        // Arrange
        var dto = new MultaCancelarDTO { MotivoCancelacion = "Motivo de prueba" };
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((true, "OK"));

        var sut = CrearSut();

        // Act
        await sut.CancelarAsync(7, dto);

        // Assert
        _multaRepoMock.Verify(r => r.CancelarConSPAsync(7, "Motivo de prueba"), Times.Once);
    }

    [Fact]
    public async Task CancelarAsync_SPRetornaFalse_PropagaElFalso()
    {
        // Arrange — el SP falla si la multa ya está cancelada o no existe
        var dto = new MultaCancelarDTO { MotivoCancelacion = "Motivo" };
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((false, "La multa no existe o ya fue cancelada."));

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.CancelarAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La multa no existe o ya fue cancelada.");
    }

    // ── Validaciones previas al SP ────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CancelarAsync_MotivoVacioOEspacios_RetornaFalseSinLlamarAlSP(string motivo)
    {
        // Arrange
        var dto = new MultaCancelarDTO { MotivoCancelacion = motivo };
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.CancelarAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El motivo de la cancelación es obligatorio y no puede estar vacío.");
        _multaRepoMock.Verify(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CancelarAsync_MotivoNulo_RetornaFalseSinLlamarAlSP()
    {
        // Arrange
        var dto = new MultaCancelarDTO { MotivoCancelacion = null! };
        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.CancelarAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        _multaRepoMock.Verify(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CancelarAsync_PropagaMensajeExactoDelSP()
    {
        // Arrange — el SP genera mensajes con lógica propia en SQL Server
        var mensajeSP = "Multa #3 cancelada. Penalización #8 revocada.";
        var dto = new MultaCancelarDTO { MotivoCancelacion = "Motivo válido" };
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((true, mensajeSP));

        var sut = CrearSut();

        // Act
        var (_, mensaje) = await sut.CancelarAsync(3, dto);

        // Assert — el mensaje es el generado por el SP, no uno propio del servicio
        mensaje.Should().Be(mensajeSP);
    }

    [Fact]
    public async Task CancelarAsync_MotivoDeUnCaracter_PasaValidacionYLlamaSP()
    {
        // Arrange
        var dto = new MultaCancelarDTO { MotivoCancelacion = "X" };
        _multaRepoMock
            .Setup(r => r.CancelarConSPAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((true, "OK"));

        var sut = CrearSut();

        // Act
        var (exito, _) = await sut.CancelarAsync(1, dto);

        // Assert
        exito.Should().BeTrue();
        _multaRepoMock.Verify(r => r.CancelarConSPAsync(1, "X"), Times.Once);
    }
}
