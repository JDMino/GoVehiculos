using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services.MantenimientoServiceTests;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.HabilitarVehiculoSocioAsync(HabilitarVehiculoSocioDTO dto)
///     → Task&lt;(bool exito, string mensaje, MantenimientoResponseDTO? dto)&gt;
///
/// Flujo real del service:
///   1. ValidarCamposHabilitar (validaciones locales)
///   2. vehiculoRepo.GetByIdSimpleAsync → debe existir
///   3. vehiculo.MantenimientoACargoDe == "socio"
///   4. vehiculo.Estado == "fuera_de_servicio"
///   5. repo.AddAsync + repo.SaveChangesAsync
///   6. GetByIdAsync para construir el DTO resultado
///
/// Ningún test toca Entity Framework ni base de datos real.
/// </summary>
public class MantenimientoService_HabilitarVehiculoSocioAsyncTests
{
    private readonly Mock<IMantenimientoRepository> _repoMock;
    private readonly Mock<IVehiculoRepository>      _vehiculoRepoMock;
    private readonly MantenimientoService           _sut;

    public MantenimientoService_HabilitarVehiculoSocioAsyncTests()
    {
        _repoMock         = new Mock<IMantenimientoRepository>();
        _vehiculoRepoMock = new Mock<IVehiculoRepository>();
        _sut = new MantenimientoService(_repoMock.Object, _vehiculoRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────────
    // Helper — configura el repo para persistencia y recarga exitosa
    // ────────────────────────────────────────────────────────────────

    private void ConfigurarPersistenciaExitosa()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Mantenimiento>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                 .ReturnsAsync(ModelBuilderMantenimiento.MantenimientoConVehiculo(
                     estadoVehiculo: "disponible", mantenimientoCargo: "socio"));
    }

    // ────────────────────────────────────────────────────────────────
    // Camino feliz
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DatosValidos_VehiculoSocioFueraDeServicio_RetornaExitoTrue()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.VehiculoSocioFueraDeServicio();
        _vehiculoRepoMock.Setup(r => r.GetByIdSimpleAsync(vehiculo.IdVehiculo)).ReturnsAsync(vehiculo);
        ConfigurarPersistenciaExitosa();

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: vehiculo.IdVehiculo);

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Contain("habilitado");
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task DatosValidos_MutaVehiculoEstadoADisponible()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.VehiculoSocioFueraDeServicio();
        _vehiculoRepoMock.Setup(r => r.GetByIdSimpleAsync(vehiculo.IdVehiculo)).ReturnsAsync(vehiculo);
        ConfigurarPersistenciaExitosa();

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: vehiculo.IdVehiculo);

        // Act
        await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert — el service muta el estado directamente en memoria
        vehiculo.Estado.Should().Be("disponible");
    }

    [Fact]
    public async Task DatosValidos_MutaVehiculoEstadoMecanicoABueno()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.VehiculoSocioFueraDeServicio();
        _vehiculoRepoMock.Setup(r => r.GetByIdSimpleAsync(vehiculo.IdVehiculo)).ReturnsAsync(vehiculo);
        ConfigurarPersistenciaExitosa();

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: vehiculo.IdVehiculo);

        // Act
        await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        vehiculo.EstadoMecanico.Should().Be("bueno");
    }

    [Fact]
    public async Task DatosValidos_InvocaAddYSaveChangesUnaVezCadaUno()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.VehiculoSocioFueraDeServicio();
        _vehiculoRepoMock.Setup(r => r.GetByIdSimpleAsync(vehiculo.IdVehiculo)).ReturnsAsync(vehiculo);
        ConfigurarPersistenciaExitosa();

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: vehiculo.IdVehiculo);

        // Act
        await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Mantenimiento>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // Validaciones locales
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TipoVacio_RetornaFalseSinConsultarVehiculo()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(tipo: "");

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().NotBeEmpty();
        resultado.Should().BeNull();
        _vehiculoRepoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DescripcionVacia_RetornaFalseSinConsultarVehiculo()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(descripcion: "");

        // Act
        var (exito, _, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        resultado.Should().BeNull();
        _vehiculoRepoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task FechaRealizacionDefault_RetornaFalse()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(fechaRealizacion: default(DateOnly));

        // Act
        var (exito, _, _) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
    }

    // ────────────────────────────────────────────────────────────────
    // Vehículo no encontrado
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VehiculoNoExiste_RetornaFalseConMensajeEncontrado()
    {
        // Arrange
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(It.IsAny<int>()))
            .ReturnsAsync((Vehiculo?)null);

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO();

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("encontrado");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // Vehículo no es de socio
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VehiculoEsDeEmpresa_RetornaFalseConMensajeSocio()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(
            estado:                "fuera_de_servicio",
            mantenimientoACargoDe: "empresa");   // <-- no es socio

        _vehiculoRepoMock.Setup(r => r.GetByIdSimpleAsync(vehiculo.IdVehiculo)).ReturnsAsync(vehiculo);

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: vehiculo.IdVehiculo);

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("socio");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // Vehículo con estado incorrecto
    // ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("disponible")]
    [InlineData("mantenimiento")]
    [InlineData("en_uso")]
    [InlineData("reservado")]
    public async Task VehiculoNoEstaFueraDeServicio_RetornaFalseConMensajeFueraDeServicio(
        string estadoVehiculo)
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(
            estado:                estadoVehiculo,
            mantenimientoACargoDe: "socio");

        _vehiculoRepoMock.Setup(r => r.GetByIdSimpleAsync(vehiculo.IdVehiculo)).ReturnsAsync(vehiculo);

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: vehiculo.IdVehiculo);

        // Act
        var (exito, mensaje, _) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Contain("fuera de servicio");
    }
}
