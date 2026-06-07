// =============================================================================
// GoVehiculos.Tests/Services/IncidenciaServiceTests/CrearAsyncTests.cs
// Tests unitarios para IncidenciaService.CrearAsync()
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;

namespace GoVehiculos.Tests.Services.IncidenciaServiceTests;

public class CrearAsyncTests
{
    private readonly Mock<IIncidenciaRepository> _repoMock        = new();
    private readonly Mock<IVehiculoRepository>   _vehiculoRepoMock = new();
    private readonly Mock<IUsuarioRepository>    _usuarioRepoMock  = new();

    private IncidenciaService CrearSut() =>
        new(_repoMock.Object, _vehiculoRepoMock.Object, _usuarioRepoMock.Object);

    // ── Caso exitoso ──────────────────────────────────────────────────────

    [Fact]
    public async Task CrearAsync_DatosValidos_RetornaEntidadPersistida()
    {
        // Arrange
        var dto = new IncidenciaCreateDTO
        {
            UsuarioId     = 1,
            VehiculoId    = 2,
            Tipo          = "accidente",
            NivelGravedad = "alta",
            Descripcion   = "Choque en esquina"
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Incidencia>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var resultado = await sut.CrearAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.UsuarioId.Should().Be(1);
        resultado.VehiculoId.Should().Be(2);
        resultado.Tipo.Should().Be("accidente");           // lowercase sin Trim
        resultado.NivelGravedad.Should().Be("alta");
        resultado.Descripcion.Should().Be("Choque en esquina");
    }

    [Fact]
    public async Task CrearAsync_NormalizaTipoALowercase()
    {
        // Arrange
        var dto = new IncidenciaCreateDTO
        {
            UsuarioId     = 1,
            VehiculoId    = 1,
            Tipo          = "ACCIDENTE",
            NivelGravedad = "ALTA",
            Descripcion   = "Desc"
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Incidencia>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var resultado = await sut.CrearAsync(dto);

        // Assert
        resultado.Tipo.Should().Be("accidente");
        resultado.NivelGravedad.Should().Be("alta");
    }

    [Fact]
    public async Task CrearAsync_LlamaAddAsyncYSaveChangesAsync()
    {
        // Arrange
        var dto = new IncidenciaCreateDTO
        {
            UsuarioId     = 1,
            VehiculoId    = 1,
            Tipo          = "accidente",
            NivelGravedad = "media",
            Descripcion   = "Desc"
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Incidencia>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        await sut.CrearAsync(dto);

        // Assert
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Incidencia>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_FechaReporteEsEstablecidaAlMomentoDeCreacion()
    {
        // Arrange
        var antes = DateTime.Now.AddSeconds(-1);
        var dto = new IncidenciaCreateDTO
        {
            UsuarioId     = 1,
            VehiculoId    = 1,
            Tipo          = "accidente",
            NivelGravedad = "media",
            Descripcion   = "Test"
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Incidencia>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var resultado = await sut.CrearAsync(dto);
        var despues   = DateTime.Now.AddSeconds(1);

        // Assert — FechaReporte fue seteada en el rango de la prueba
        resultado.FechaReporte.Should().BeAfter(antes).And.BeBefore(despues);
    }

    [Fact]
    public async Task CrearAsync_DescripcionConEspacios_RealizaTrim()
    {
        // Arrange
        var dto = new IncidenciaCreateDTO
        {
            UsuarioId     = 1,
            VehiculoId    = 1,
            Tipo          = "accidente",
            NivelGravedad = "media",
            Descripcion   = "  Descripción con espacios  "
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Incidencia>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = CrearSut();

        // Act
        var resultado = await sut.CrearAsync(dto);

        // Assert
        resultado.Descripcion.Should().Be("Descripción con espacios");
    }
}
