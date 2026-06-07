// =============================================================================
// GoVehiculos.Tests/Services/PenalizacionServiceTests/UpdateAsyncTests.cs
// Tests unitarios para PenalizacionService.UpdateAsync()
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.PenalizacionServiceTests;

public class UpdateAsyncTests
{
    private readonly Mock<IPenalizacionRepository> _repoMock        = new();
    private readonly Mock<IVehiculoRepository>     _vehiculoRepoMock = new();
    private readonly Mock<IUsuarioRepository>      _usuarioRepoMock  = new();
    private readonly Mock<IMultaRepository>        _multaRepoMock    = new();

    private PenalizacionService CrearSut() =>
        new(_repoMock.Object, _vehiculoRepoMock.Object, _usuarioRepoMock.Object, _multaRepoMock.Object);

    private static PenalizacionUpdateDTO DtoValido(
        string tipo   = "advertencia",
        string estado = "activa") => new()
    {
        Tipo    = tipo,
        Motivo  = "Motivo actualizado",
        Estado  = estado,
        FechaFin = null
    };

    // ── Casos exitosos ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_DatosValidos_RetornaExitoTrue()
    {
        // Arrange
        var pen = ModelBuilders.Penalizacion(id: 1, tipo: "advertencia", estado: "activa");
        pen.FechaInicio = DateTime.Now.AddDays(-5);

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, DtoValido("advertencia", "cumplida"));

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Penalización actualizada correctamente.");
    }

    [Fact]
    public async Task UpdateAsync_DatosValidos_ActualizaLaEntidad()
    {
        // Arrange
        var pen = ModelBuilders.Penalizacion(id: 1, tipo: "advertencia", estado: "activa");
        pen.FechaInicio = DateTime.Now.AddDays(-5);
        var dto = new PenalizacionUpdateDTO
        {
            Tipo    = "suspension_temporal",
            Motivo  = "Nuevo motivo",
            Estado  = "cumplida",
            FechaFin = null
        };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert
        pen.Tipo.Should().Be("suspension_temporal");
        pen.Motivo.Should().Be("Nuevo motivo");
        pen.Estado.Should().Be("cumplida");
    }

    [Fact]
    public async Task UpdateAsync_LlamaSaveChangesAsync()
    {
        // Arrange
        var pen = ModelBuilders.Penalizacion(id: 1);
        pen.FechaInicio = DateTime.Now.AddDays(-5);
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, DtoValido());

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── Efecto secundario: bloqueo_cuenta ────────────────────────────────

    [Fact]
    public async Task UpdateAsync_CambioABloqueoCuenta_BloqueaAlUsuario()
    {
        // Arrange — tipo anterior != bloqueo_cuenta → debe aplicar efecto
        var pen = ModelBuilders.Penalizacion(id: 1, tipo: "advertencia", estado: "activa");
        pen.MultaId     = 10;
        pen.FechaInicio = DateTime.Now.AddDays(-5);

        var incidencia = ModelBuilders.Incidencia(usuarioId: 3, vehiculoId: 4);
        var multa      = ModelBuilders.Multa(id: 10, incidenciaId: 1);
        multa.Incidencia = incidencia;

        var usuario = ModelBuilders.Usuario(id: 3, bloqueado: false);

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _multaRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(multa);
        _usuarioRepoMock.Setup(r => r.GetByIdSimpleAsync(3)).ReturnsAsync(usuario);

        var dto = new PenalizacionUpdateDTO
        {
            Tipo   = "bloqueo_cuenta",
            Motivo = "Test",
            Estado = "activa"
        };
        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert — efecto secundario aplicado
        usuario.Bloqueado.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_CambioAInhabilitacionVehiculo_PasaVehiculoAFueraDeServicio()
    {
        // Arrange
        var pen = ModelBuilders.Penalizacion(id: 1, tipo: "advertencia", estado: "activa");
        pen.MultaId     = 20;
        pen.FechaInicio = DateTime.Now.AddDays(-5);

        var incidencia = ModelBuilders.Incidencia(usuarioId: 1, vehiculoId: 7);
        var multa      = ModelBuilders.Multa(id: 20, incidenciaId: 1);
        multa.Incidencia = incidencia;

        var vehiculo = ModelBuilders.Vehiculo(id: 7, estado: "disponible");

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _multaRepoMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(multa);
        _vehiculoRepoMock.Setup(r => r.GetByIdSimpleAsync(7)).ReturnsAsync(vehiculo);

        var dto = new PenalizacionUpdateDTO
        {
            Tipo   = "inhabilitacion_vehiculo",
            Motivo = "Test",
            Estado = "activa"
        };
        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert
        vehiculo.Estado.Should().Be("fuera_de_servicio");
    }

    [Fact]
    public async Task UpdateAsync_TipoNoCanbia_NoAplicaEfectoSecundario()
    {
        // Arrange — mismo tipo → no debe consultar multa/usuario/vehículo
        var pen = ModelBuilders.Penalizacion(id: 1, tipo: "advertencia", estado: "activa");
        pen.FechaInicio = DateTime.Now.AddDays(-5);

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new PenalizacionUpdateDTO
        {
            Tipo   = "advertencia",   // igual que antes
            Motivo = "Motivo nuevo",
            Estado = "cumplida"
        };
        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, dto);

        // Assert — no consulta multa, usuario ni vehículo
        _multaRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _usuarioRepoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
        _vehiculoRepoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
    }

    // ── Casos de error ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PenalizacionNoExiste_RetornaFalse()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Penalizacion?)null);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(999, DtoValido());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Penalización no encontrada.");
    }

    [Fact]
    public async Task UpdateAsync_PenalizacionRevocada_RetornaFalse()
    {
        // Arrange
        var pen = ModelBuilders.Penalizacion(id: 1, estado: "revocada");
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);

        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, DtoValido());

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Una penalización revocada no puede modificarse.");
    }

    [Fact]
    public async Task UpdateAsync_IntentoCambiarEstadoARevocada_RetornaFalse()
    {
        // Arrange
        var pen = ModelBuilders.Penalizacion(id: 1, estado: "activa");
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);

        var dto = DtoValido(estado: "revocada");
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("revocada");
    }

    [Fact]
    public async Task UpdateAsync_FechaFinAnteriorAFechaInicio_RetornaFalse()
    {
        // Arrange
        var pen = ModelBuilders.Penalizacion(id: 1, estado: "activa");
        pen.FechaInicio = DateTime.Now;

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pen);

        var dto = new PenalizacionUpdateDTO
        {
            Tipo     = "advertencia",
            Motivo   = "Test",
            Estado   = "activa",
            FechaFin = DateTime.Now.AddDays(-1)
        };
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("fecha de fin debe ser posterior");
    }

    // ── Validaciones de campos ────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_TipoVacio_RetornaFalse(string tipo)
    {
        // Arrange
        var dto = new PenalizacionUpdateDTO { Tipo = tipo, Motivo = "X", Estado = "activa" };
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo de penalización es obligatorio.");
    }

    [Fact]
    public async Task UpdateAsync_TipoInvalido_RetornaFalse()
    {
        // Arrange
        var dto = new PenalizacionUpdateDTO { Tipo = "tipo_inexistente", Motivo = "X", Estado = "activa" };
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Tipo de penalización inválido");
    }

    [Theory]
    [InlineData("advertencia")]
    [InlineData("bloqueo_cuenta")]
    [InlineData("inhabilitacion_vehiculo")]
    [InlineData("suspension_temporal")]
    public async Task UpdateAsync_TodosLosTiposValidos_PasanValidacionDeTipo(string tipo)
    {
        // Arrange — solo verificamos que no falla en la validación de tipo
        // (el repo devolverá null → "no encontrada", pero no habrá error de tipo)
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Penalizacion?)null);

        var dto = new PenalizacionUpdateDTO { Tipo = tipo, Motivo = "X", Estado = "activa" };
        var sut = CrearSut();

        // Act
        var (_, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert — el error es "no encontrada", no "tipo inválido"
        mensaje.Should().Be("Penalización no encontrada.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_MotivoVacio_RetornaFalse(string motivo)
    {
        // Arrange
        var dto = new PenalizacionUpdateDTO { Tipo = "advertencia", Motivo = motivo, Estado = "activa" };
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El motivo es obligatorio.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_EstadoVacio_RetornaFalse(string estado)
    {
        // Arrange
        var dto = new PenalizacionUpdateDTO { Tipo = "advertencia", Motivo = "X", Estado = estado };
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El estado de la penalización es obligatorio.");
    }

    [Fact]
    public async Task UpdateAsync_EstadoInvalido_RetornaFalse()
    {
        // Arrange
        var dto = new PenalizacionUpdateDTO { Tipo = "advertencia", Motivo = "X", Estado = "invalido" };
        var sut = CrearSut();

        // Act
        var (exito, mensaje) = await sut.UpdateAsync(1, dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("Estado inválido");
    }

    [Fact]
    public async Task UpdateAsync_PenalizacionNoExiste_NoLlamaSaveChanges()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Penalizacion?)null);

        var sut = CrearSut();

        // Act
        await sut.UpdateAsync(1, DtoValido());

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
