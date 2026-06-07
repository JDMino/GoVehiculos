// =============================================================================
// GoVehiculos.Tests/Services/MultaServiceTests/GetAllAsyncTests.cs
// CORRECCIÓN: eliminados Mock<IncidenciaService> y Mock<PenalizacionService>.
// Se usan instancias REALES construidas con sus repositorios mockeados,
// porque CrearAsync no es virtual y no puede setearse con Moq.
// =============================================================================
using FluentAssertions;
using GoVehiculos.API.Models;
using GoVehiculos.API.Observers;
using GoVehiculos.API.Repositories;
using GoVehiculos.API.Services;
using Moq;
using GoVehiculos.Tests.Helpers;

namespace GoVehiculos.Tests.Services.MultaServiceTests;

public class GetAllAsyncTests
{
    // ── Repositorios mockeados ────────────────────────────────────────────
    private readonly Mock<IMultaRepository>        _multaRepoMock        = new();
    private readonly Mock<IPenalizacionRepository> _penalizacionRepoMock = new();
    private readonly Mock<IVehiculoRepository>     _vehiculoRepoMock     = new();
    private readonly Mock<IUsuarioRepository>      _usuarioRepoMock      = new();
    private readonly Mock<IIncidenciaRepository>   _incidenciaRepoMock   = new();

    // ── Servicios REALES (no mockeados) ──────────────────────────────────
    // IncidenciaService y PenalizacionService se instancian con new porque
    // sus métodos no son virtuales: no se puede hacer Setup() sobre ellos.
    private IncidenciaService CrearIncidenciaService() =>
        new(_incidenciaRepoMock.Object,
            _vehiculoRepoMock.Object,
            _usuarioRepoMock.Object);

    private PenalizacionService CrearPenalizacionService() =>
        new(_penalizacionRepoMock.Object,
            _vehiculoRepoMock.Object,
            _usuarioRepoMock.Object,
            _multaRepoMock.Object);

    private MultaService CrearSut(IEnumerable<IMultaObserver>? observadores = null) =>
        new(_multaRepoMock.Object,
            _penalizacionRepoMock.Object,
            _vehiculoRepoMock.Object,
            _usuarioRepoMock.Object,
            CrearIncidenciaService(),
            CrearPenalizacionService(),
            observadores ?? Enumerable.Empty<IMultaObserver>());

    // ── Tests ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_SinFiltros_RetornaTodasLasMultas()
    {
        // Arrange
        var multas = new List<Multa>
        {
            ModelBuilders.Multa(id: 1, estado: "pendiente"),
            ModelBuilders.Multa(id: 2, estado: "pagada")
        };
        _multaRepoMock
            .Setup(r => r.GetAllAsync(null, null, null))
            .ReturnsAsync(multas);
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        var lista = resultado.ToList();
        lista.Should().HaveCount(2);
        lista[0].IdMulta.Should().Be(1);
        lista[1].IdMulta.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ConFiltros_PasaFiltrosAlRepositorio()
    {
        // Arrange
        _multaRepoMock
            .Setup(r => r.GetAllAsync("pagada", "accidente", "alta"))
            .ReturnsAsync(new List<Multa>());
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        await sut.GetAllAsync("pagada", "accidente", "alta");

        // Assert
        _multaRepoMock.Verify(r => r.GetAllAsync("pagada", "accidente", "alta"), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_MapeoCorrectoDeCampos()
    {
        // Arrange
        var fecha = new DateTime(2024, 5, 1);
        var multa = ModelBuilders.Multa(id: 10, estado: "pendiente", tipo: "economica", monto: 3000m);
        multa.FechaCreacion = fecha;

        _multaRepoMock
            .Setup(r => r.GetAllAsync(null, null, null))
            .ReturnsAsync(new List<Multa> { multa });
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        var dto = resultado.Single();
        dto.IdMulta.Should().Be(10);
        dto.Estado.Should().Be("pendiente");
        dto.Tipo.Should().Be("economica");
        dto.Monto.Should().Be(3000m);
        dto.FechaCreacion.Should().Be(fecha);
        dto.IdPenalizacion.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ConPenalizacion_MapeaSusDatosEnElDTO()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 5);
        var pen   = ModelBuilders.Penalizacion(id: 99, multaId: 5, tipo: "bloqueo_cuenta", estado: "activa");

        _multaRepoMock
            .Setup(r => r.GetAllAsync(null, null, null))
            .ReturnsAsync(new List<Multa> { multa });
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.Is<List<int>>(l => l.Contains(5))))
            .ReturnsAsync(new List<Penalizacion> { pen });

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        var dto = resultado.Single();
        dto.IdPenalizacion.Should().Be(99);
        dto.PenalizacionTipo.Should().Be("bloqueo_cuenta");
        dto.PenalizacionEstado.Should().Be("activa");
    }

    [Fact]
    public async Task GetAllAsync_SinMultas_RetornaColeccionVacia()
    {
        // Arrange
        _multaRepoMock
            .Setup(r => r.GetAllAsync(null, null, null))
            .ReturnsAsync(new List<Multa>());
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_UsaGetByMultaIdsAsync_ParaEvitarN1()
    {
        // Arrange
        var multas = new List<Multa>
        {
            ModelBuilders.Multa(id: 1),
            ModelBuilders.Multa(id: 2),
            ModelBuilders.Multa(id: 3)
        };
        _multaRepoMock
            .Setup(r => r.GetAllAsync(null, null, null))
            .ReturnsAsync(multas);
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        await sut.GetAllAsync();

        // Assert — se usa la query batch, nunca la individual
        _penalizacionRepoMock.Verify(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()), Times.Once);
        _penalizacionRepoMock.Verify(r => r.GetByMultaIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_EstaCancelada_EsTrue_CuandoEstadoEsCancelada()
    {
        // Arrange
        var multa = ModelBuilders.Multa(id: 1, estado: "cancelada");
        _multaRepoMock
            .Setup(r => r.GetAllAsync(null, null, null))
            .ReturnsAsync(new List<Multa> { multa });
        _penalizacionRepoMock
            .Setup(r => r.GetByMultaIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Penalizacion>());

        var sut = CrearSut();

        // Act
        var resultado = await sut.GetAllAsync();

        // Assert
        resultado.Single().EstaCancelada.Should().BeTrue();
    }
}
