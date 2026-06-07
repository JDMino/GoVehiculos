// =============================================================================
// GoVehiculos.Tests/Services/IncidenciaServiceTests/GetAllAsyncTests.cs
// Tests unitarios para IncidenciaService.GetAllAsync()
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.IncidenciaServiceTests;

public class GetAllAsyncTests
{
    // ── Dependencias mockeadas ────────────────────────────────────────────
    private readonly Mock<IIncidenciaRepository> _repoMock        = new();
    private readonly Mock<IVehiculoRepository>   _vehiculoRepoMock = new();
    private readonly Mock<IUsuarioRepository>    _usuarioRepoMock  = new();

    private IncidenciaService CrearSut() =>
        new(_repoMock.Object, _vehiculoRepoMock.Object, _usuarioRepoMock.Object);

    // ── Casos exitosos ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ConIncidencias_RetornaListaMapeadaCorrectamente()
    {
        // Arrange
        var incidencias = new List<Incidencia>
        {
            ModelBuilders.Incidencia(id: 1, usuarioId: 1, vehiculoId: 1, tipo: "accidente",    gravedad: "alta"),
            ModelBuilders.Incidencia(id: 2, usuarioId: 2, vehiculoId: 2, tipo: "daño_fisico",  gravedad: "media")
        };
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(incidencias);

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        var lista = resultado.ToList();
        lista.Should().HaveCount(2);

        lista[0].IdIncidencia.Should().Be(1);
        lista[0].Tipo.Should().Be("accidente");
        lista[0].NivelGravedad.Should().Be("alta");
        lista[0].UsuarioId.Should().Be(1);
        lista[0].VehiculoId.Should().Be(1);
        lista[0].UsuarioNombreCompleto.Should().Be("Juan Perez");
        lista[0].VehiculoPatente.Should().Be("ABC001");
        lista[0].VehiculoMarca.Should().Be("Chevrolet");
        lista[0].VehiculoModelo.Should().Be("Corsa");

        lista[1].IdIncidencia.Should().Be(2);
        lista[1].Tipo.Should().Be("daño_fisico");
    }

    [Fact]
    public async Task GetAllAsync_SinIncidencias_RetornaColeccionVacia()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Incidencia>());

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_LlamaAlRepositorioExactamenteUnaVez()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Incidencia>());

        var sut = CrearSut();

        // Act
        await sut.GetAllAsync();

        // Assert
        _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_IncidenciaSinNavegaciones_MapeaCamposVaciosCorrectamente()
    {
        // Arrange — incidencia sin entidades de navegación cargadas
        var incidenciaSinNav = new Incidencia
        {
            IdIncidencia  = 99,
            UsuarioId     = 5,
            VehiculoId    = 7,
            Tipo          = "infraccion_vial",
            NivelGravedad = "baja",
            Descripcion   = "Sin nav",
            FechaReporte  = DateTime.Now,
            Usuario       = null,
            Vehiculo      = null
        };
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Incidencia> { incidenciaSinNav });

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        var dto = resultado.Single();
        dto.UsuarioNombreCompleto.Should().BeEmpty();
        dto.VehiculoPatente.Should().BeEmpty();
        dto.VehiculoMarca.Should().BeEmpty();
        dto.VehiculoModelo.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_RetornaIEnumerable_NoLista()
    {
        // Arrange — verifica que el contrato de retorno es IEnumerable
        _repoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Incidencia>());

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        resultado.Should().BeAssignableTo<IEnumerable<GoVehiculos.API.DTOs.IncidenciaResponseDTO>>();
    }
}
