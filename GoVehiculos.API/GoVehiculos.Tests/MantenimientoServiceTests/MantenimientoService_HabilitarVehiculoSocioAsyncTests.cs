using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using GoVehiculos.Tests.Helpers;
using Moq;

namespace GoVehiculos.Tests.Services;

/// <summary>
/// Tests unitarios para:
///   MantenimientoService.HabilitarVehiculoSocioAsync(HabilitarVehiculoSocioDTO dto)
///     → Task&lt;(bool exito, string mensaje, MantenimientoResponseDTO? dto)&gt;
///
/// Casos cubiertos (alineados con planilla de pruebas):
///   HV01 — Datos válidos, vehículo de socio fuera de servicio
///   HV05 — Tipo vacío
///   HV06 — Descripción vacía
///   HV07 — FechaRealizacion con valor default
///   HV08 — Vehículo no encontrado en la base de datos
///   HV09 — Vehículo no es de socio (mantenimientoACargoDe = "empresa")
///   HV10 — Vehículo de socio en estado "disponible"
///   HV11 — Vehículo de socio en estado "mantenimiento"
///   HV12 — Vehículo de socio en estado "en_uso"
///   HV13 — Vehículo de socio en estado "reservado"
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
    // Helper — configura persistencia y recarga exitosa
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
    // HV01 — Datos válidos, vehículo de socio fuera de servicio
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV01_DatosValidosVehiculoSocioFueraDeServicio_RetornaExitoYMensajeCorrecto()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.VehiculoSocioFueraDeServicio(id: 1);
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(vehiculo.IdVehiculo))
            .ReturnsAsync(vehiculo);
        ConfigurarPersistenciaExitosa();

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(
            vehiculoId:       1,
            tipo:             "preventivo",
            descripcion:      "Revisión general",
            fechaRealizacion: DateOnly.FromDateTime(DateTime.Today));

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeTrue();
        mensaje.Should().Be("Vehículo habilitado correctamente.");
        resultado.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // HV05 — Tipo vacío
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV05_TipoVacio_RetornaFalseConMensajeObligatorio()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(tipo: "");

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El tipo es obligatorio.");
        resultado.Should().BeNull();
        _vehiculoRepoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // HV06 — Descripción vacía
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV06_DescripcionVacia_RetornaFalseConMensajeObligatorio()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(descripcion: "");

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La descripción es obligatoria.");
        resultado.Should().BeNull();
        _vehiculoRepoMock.Verify(r => r.GetByIdSimpleAsync(It.IsAny<int>()), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────
    // HV07 — FechaRealizacion con valor default (DateOnly)
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV07_FechaRealizacionDefault_RetornaFalseConMensajeObligatorio()
    {
        // Arrange
        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(fechaRealizacion: default(DateOnly));

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("La fecha de realización es obligatoria.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // HV08 — Vehículo no encontrado
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV08_VehiculoNoEncontrado_RetornaFalseConMensajeNoEncontrado()
    {
        // Arrange — todos los campos del DTO son válidos, pero el repositorio devuelve null
        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(1))
            .ReturnsAsync((Vehiculo?)null);

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(
            vehiculoId:       1,
            tipo:             "preventivo",
            descripcion:      "Revisión general",
            fechaRealizacion: DateOnly.FromDateTime(DateTime.Today));

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Vehículo no encontrado.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // HV09 — Vehículo no es de socio (mantenimientoACargoDe = "empresa")
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV09_VehiculoEsDeEmpresa_RetornaFalseConMensajeSocio()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(
            id:                    1,
            estado:                "fuera_de_servicio",
            mantenimientoACargoDe: "empresa");

        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(1))
            .ReturnsAsync(vehiculo);

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: 1);

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("Este flujo solo aplica a vehículos con mantenimiento a cargo del socio.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // HV10 — Vehículo de socio en estado "disponible"
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV10_VehiculoSocioEnEstadoDisponible_RetornaFalseConMensajeFueraDeServicio()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(
            id:                    1,
            estado:                "disponible",
            mantenimientoACargoDe: "socio");

        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(1))
            .ReturnsAsync(vehiculo);

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: 1);

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El vehículo debe estar en estado 'fuera de servicio' para poder habilitarlo.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // HV11 — Vehículo de socio en estado "mantenimiento"
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV11_VehiculoSocioEnEstadoMantenimiento_RetornaFalseConMensajeFueraDeServicio()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(
            id:                    1,
            estado:                "mantenimiento",
            mantenimientoACargoDe: "socio");

        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(1))
            .ReturnsAsync(vehiculo);

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: 1);

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El vehículo debe estar en estado 'fuera de servicio' para poder habilitarlo.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // HV12 — Vehículo de socio en estado "en_uso"
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV12_VehiculoSocioEnEstadoEnUso_RetornaFalseConMensajeFueraDeServicio()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(
            id:                    1,
            estado:                "en_uso",
            mantenimientoACargoDe: "socio");

        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(1))
            .ReturnsAsync(vehiculo);

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: 1);

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El vehículo debe estar en estado 'fuera de servicio' para poder habilitarlo.");
        resultado.Should().BeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // HV13 — Vehículo de socio en estado "reservado"
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HV13_VehiculoSocioEnEstadoReservado_RetornaFalseConMensajeFueraDeServicio()
    {
        // Arrange
        var vehiculo = ModelBuilderMantenimiento.Vehiculo(
            id:                    1,
            estado:                "reservado",
            mantenimientoACargoDe: "socio");

        _vehiculoRepoMock
            .Setup(r => r.GetByIdSimpleAsync(1))
            .ReturnsAsync(vehiculo);

        var dto = ModelBuilderMantenimiento.HabilitarSocioDTO(vehiculoId: 1);

        // Act
        var (exito, mensaje, resultado) = await _sut.HabilitarVehiculoSocioAsync(dto);

        // Assert
        exito.Should().BeFalse();
        mensaje.Should().Be("El vehículo debe estar en estado 'fuera de servicio' para poder habilitarlo.");
        resultado.Should().BeNull();
    }
}