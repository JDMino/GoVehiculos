// =============================================================================
// GoVehiculos.Tests/Helpers/ModelBuilders.cs
// Fábricas de entidades de dominio reutilizables en todos los tests.
// No dependen de EF ni de la base de datos.
// =============================================================================
using GoVehiculos.API.Models;

namespace GoVehiculos.Tests.Helpers;

/// <summary>
/// Centraliza la construcción de entidades de dominio para tests.
/// Valores por defecto válidos que cada test puede sobreescribir
/// con la expresión "with" de C# 9 o asignación directa de propiedades.
/// </summary>
public static class ModelBuilders
{
    // ── Vehiculo ─────────────────────────────────────────────────────────
    public static Vehiculo Vehiculo(
        int    id              = 1,
        string estado          = "disponible",
        string estadoMecanico  = "bueno",
        string mantACargoDe    = "empresa",
        int?   socioId         = null) => new()
    {
        IdVehiculo            = id,
        Patente               = $"ABC{id:D3}",
        Anio                  = 2022,
        Tipo                  = "auto",
        Estado                = estado,
        EstadoMecanico        = estadoMecanico,
        Kilometraje           = 10_000,
        PrecioPorDia          = 5_000,
        MantenimientoACargoDe = mantACargoDe,
        SeguroVigente         = true,
        DocumentacionVigente  = true,
        Activo                = true,
        ModeloId              = 1,
        SocioId               = socioId,
        Modelo = new Modelo
        {
            IdModelo = 1,
            Nombre   = "Corsa",
            Marca    = new Marca { IdMarca = 1, Nombre = "Chevrolet" }
        }
    };

    // ── Usuario ──────────────────────────────────────────────────────────
    public static Usuario Usuario(
        int    id        = 1,
        bool   activo    = true,
        bool   bloqueado = false,
        int    rolId     = 3) => new()
    {
        IdUsuario     = id,
        Nombre        = "Juan",
        Apellido      = "Perez",
        Email         = $"juan{id}@test.com",
        Dni           = $"3000000{id}",
        PasswordHash  = "hash",
        Activo        = activo,
        Bloqueado     = bloqueado,
        RolId         = rolId,
        FechaRegistro = DateTime.Now
    };

    // ── Incidencia ───────────────────────────────────────────────────────
    public static Incidencia Incidencia(
        int    id          = 1,
        int    usuarioId   = 1,
        int    vehiculoId  = 1,
        string tipo        = "accidente",
        string gravedad    = "media") => new()
    {
        IdIncidencia = id,
        UsuarioId    = usuarioId,
        VehiculoId   = vehiculoId,
        Tipo         = tipo,
        NivelGravedad = gravedad,
        Descripcion  = "Descripción de prueba",
        FechaReporte = DateTime.Now,
        Usuario      = Usuario(usuarioId),
        Vehiculo     = Vehiculo(vehiculoId)
    };

    // ── Multa ────────────────────────────────────────────────────────────
    public static Multa Multa(
        int    id          = 1,
        int    incidenciaId = 1,
        string estado      = "pendiente",
        string tipo        = "economica",
        decimal monto      = 5000m) => new()
    {
        IdMulta      = id,
        IncidenciaId = incidenciaId,
        Tipo         = tipo,
        Monto        = monto,
        Descripcion  = "Descripción multa",
        Estado       = estado,
        FechaCreacion = DateTime.Now,
        Incidencia   = Incidencia(incidenciaId)
    };

    // ── Penalizacion ─────────────────────────────────────────────────────
    public static Penalizacion Penalizacion(
        int    id      = 1,
        int?   multaId = 1,
        string tipo    = "advertencia",
        string estado  = "activa") => new()
    {
        IdPenalizacion = id,
        MultaId        = multaId,
        Tipo           = tipo,
        Motivo         = "Motivo de prueba",
        FechaInicio    = DateTime.Now,
        FechaFin       = null,
        Estado         = estado
    };
}
