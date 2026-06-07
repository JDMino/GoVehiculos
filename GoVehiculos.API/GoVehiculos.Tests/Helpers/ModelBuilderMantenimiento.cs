using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;

namespace GoVehiculos.Tests.Helpers;

/// <summary>
/// Centraliza la construcción de todas las entidades de dominio y DTOs
/// utilizados en los tests de <see cref="GoVehiculos.API.Services.MantenimientoService"/>.
///
/// Principios aplicados:
///   - Ningún test instancia modelos directamente: todo pasa por este builder.
///   - Ningún test toca Entity Framework ni base de datos real.
///   - Los valores por defecto son siempre válidos para el camino feliz;
///     cada test sobreescribe solo el campo que necesita probar.
///   - Los métodos son estáticos para maximizar la legibilidad en los tests
///     ("ModelBuilderMantenimiento.Mantenimiento(estado: "cancelado")").
/// </summary>
public static class ModelBuilderMantenimiento
{
    // ══════════════════════════════════════════════════════════════════════
    // ENTIDADES DE DOMINIO
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Construye una <see cref="Marca"/> lista para ser embebida en un Modelo.
    /// </summary>
    public static Marca Marca(
        int id      = 1,
        string nombre = "Chevrolet") => new()
    {
        IdMarca = id,
        Nombre  = nombre
    };

    /// <summary>
    /// Construye un <see cref="Modelo"/> con su Marca anidada.
    /// </summary>
    public static Modelo Modelo(
        int    id      = 1,
        string nombre  = "Corsa",
        int    marcaId = 1) => new()
    {
        IdModelo = id,
        Nombre   = nombre,
        MarcaId  = marcaId,
        Marca    = Marca()
    };

    /// <summary>
    /// Construye un <see cref="Usuario"/> mínimo, utilizado como empleado
    /// en la navegación de <see cref="Mantenimiento.Empleado"/>.
    /// </summary>
    public static Usuario Empleado(
        int    id       = 2,
        string nombre   = "Juan",
        string apellido = "Pérez") => new()
    {
        IdUsuario = id,
        Nombre    = nombre,
        Apellido  = apellido,
        Email     = "empleado@test.com",
        Dni       = "12345678",
        PasswordHash = "$2a$11$fakeHashForTests",
        RolId     = 3,
        Activo    = true,
        Bloqueado = false
    };

    /// <summary>
    /// Construye un <see cref="Vehiculo"/> con navegaciones completas
    /// (Modelo → Marca) listo para ser referenciado desde Mantenimiento.
    ///
    /// Por defecto representa un vehículo de empresa en estado "disponible"
    /// con estado mecánico "regular" (candidato a mantenimiento).
    /// </summary>
    public static Vehiculo Vehiculo(
        int    id                    = 1,
        string patente               = "AA000BB",
        string estado                = "disponible",
        string estadoMecanico        = "regular",
        string mantenimientoACargoDe = "empresa",
        int    anio                  = 2021,
        decimal kilometraje          = 15_000,
        decimal precioPorDia         = 5_000) => new()
    {
        IdVehiculo            = id,
        Patente               = patente,
        Anio                  = anio,
        Tipo                  = "sedan",
        Estado                = estado,
        EstadoMecanico        = estadoMecanico,
        Kilometraje           = kilometraje,
        PrecioPorDia          = precioPorDia,
        MantenimientoACargoDe = mantenimientoACargoDe,
        SeguroVigente         = true,
        DocumentacionVigente  = true,
        Activo                = true,
        ModeloId              = 1,
        Modelo                = Modelo()
    };

    /// <summary>
    /// Construye un <see cref="Vehiculo"/> de socio en estado "fuera_de_servicio",
    /// que es el estado requerido para ejecutar el flujo de habilitación por socio.
    /// </summary>
    public static Vehiculo VehiculoSocioFueraDeServicio(int id = 1) =>
        Vehiculo(
            id:                    id,
            estado:                "fuera_de_servicio",
            mantenimientoACargoDe: "socio");

    /// <summary>
    /// Construye un <see cref="Mantenimiento"/> sin navegación a Vehiculo.
    /// Usar cuando la estrategia no necesita el vehículo
    /// (<see cref="GoVehiculos.API.Strategies.IniciarStrategy"/> y
    ///  <see cref="GoVehiculos.API.Strategies.CancelarStrategy"/>).
    /// </summary>
    public static Mantenimiento Mantenimiento(
        int      id               = 1,
        int      vehiculoId       = 1,
        int?     empleadoId       = 2,
        string   estado           = "pendiente",
        string   tipo             = "preventivo",
        string   descripcion      = "Revisión general",
        string   prioridad        = "media",
        decimal  costo            = 0,
        string   realizadoPor     = "",
        bool     disponibilizado  = false,
        DateOnly? fechaProgramada = null,
        DateOnly? fechaRealizacion = null) => new()
    {
        IdMantenimiento  = id,
        VehiculoId       = vehiculoId,
        EmpleadoId       = empleadoId,
        Tipo             = tipo,
        Descripcion      = descripcion,
        Estado           = estado,
        Prioridad        = prioridad,
        Costo            = costo,
        RealizadoPor     = realizadoPor,
        Disponibilizado  = disponibilizado,
        FechaProgramada  = fechaProgramada ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        FechaRealizacion = fechaRealizacion,
        Vehiculo         = null,
        Empleado         = null
    };

    /// <summary>
    /// Construye un <see cref="Mantenimiento"/> con la navegación a
    /// <see cref="API.Models.Vehiculo"/> ya cargada.
    /// Usar con <see cref="GoVehiculos.API.Strategies.FinalizarStrategy"/>
    /// y con <see cref="GoVehiculos.API.Services.MantenimientoService.DisponibilizarVehiculoAsync"/>.
    /// </summary>
    public static Mantenimiento MantenimientoConVehiculo(
        int     id                  = 1,
        int?    empleadoId          = 2,
        string  estadoMantenimiento = "finalizado",
        string  estadoVehiculo      = "mantenimiento",
        string  estadoMecanicoVeh   = "regular",
        string  mantenimientoCargo  = "empresa",
        bool    disponibilizado     = false,
        DateOnly? fechaProgramada   = null)
    {
        var vehiculo = Vehiculo(
            estado:                estadoVehiculo,
            estadoMecanico:        estadoMecanicoVeh,
            mantenimientoACargoDe: mantenimientoCargo);

        var mant = Mantenimiento(
            id:              id,
            empleadoId:      empleadoId,
            estado:          estadoMantenimiento,
            disponibilizado: disponibilizado,
            fechaProgramada: fechaProgramada);

        mant.Vehiculo = vehiculo;
        return mant;
    }

    /// <summary>
    /// Construye un <see cref="Mantenimiento"/> con empleado y vehículo
    /// en estado "iniciado", listo para ser finalizado o cancelado.
    /// </summary>
    public static Mantenimiento MantenimientoIniciado(
        int      id            = 1,
        int?     empleadoId    = 2,
        bool     conVehiculo   = false,
        DateOnly? fechaProg    = null)
    {
        var mant = Mantenimiento(
            id:              id,
            empleadoId:      empleadoId,
            estado:          "iniciado",
            fechaProgramada: fechaProg ?? DateOnly.MinValue);   // MinValue = sin restricción

        if (conVehiculo)
            mant.Vehiculo = Vehiculo(estadoMecanico: "malo");

        return mant;
    }

    // ══════════════════════════════════════════════════════════════════════
    // DTOs DE ENTRADA (Create / Update / Acciones)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO válido para crear una orden de mantenimiento.
    /// Todos los campos tienen valores que superan las validaciones locales del service.
    /// </summary>
    public static MantenimientoCreateDTO CreateDTO(
        int       vehiculoId      = 1,
        int       empleadoId      = 2,
        string    tipo            = "preventivo",
        string    descripcion     = "Revisión general",
        string    prioridad       = "media",
        DateOnly? fechaProgramada = null) => new()
    {
        VehiculoId      = vehiculoId,
        EmpleadoId      = empleadoId,
        Tipo            = tipo,
        Descripcion     = descripcion,
        Prioridad       = prioridad,
        FechaProgramada = fechaProgramada ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1))
    };

    /// <summary>
    /// DTO válido para actualizar una orden existente.
    /// </summary>
    public static MantenimientoUpdateDTO UpdateDTO(
        string   estado          = "iniciado",
        string   prioridad       = "alta",
        string   descripcion     = "Descripción actualizada",
        int?     empleadoId      = 2,
        decimal  costo           = 1_500,
        string   realizadoPor    = "Taller Norte",
        DateOnly? fechaProgramada = null,
        DateOnly? fechaRealizacion = null) => new()
    {
        Estado           = estado,
        Prioridad        = prioridad,
        Descripcion      = descripcion,
        EmpleadoId       = empleadoId,
        Costo            = costo,
        RealizadoPor     = realizadoPor,
        FechaProgramada  = fechaProgramada  ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        FechaRealizacion = fechaRealizacion
    };

    /// <summary>
    /// DTO válido para habilitar un vehículo de socio.
    /// </summary>
    public static HabilitarVehiculoSocioDTO HabilitarSocioDTO(
        int       vehiculoId       = 1,
        string    tipo             = "correctivo",
        string    descripcion      = "Revisión realizada por el socio",
        string    prioridad        = "alta",
        DateOnly? fechaRealizacion = null) => new()
    {
        VehiculoId       = vehiculoId,
        Tipo             = tipo,
        Descripcion      = descripcion,
        Prioridad        = prioridad,
        FechaRealizacion = fechaRealizacion ?? DateOnly.FromDateTime(DateTime.Today)
    };

    /// <summary>
    /// DTO válido para finalizar un mantenimiento.
    /// Por defecto la fecha de realización es hoy, compatible con
    /// una FechaProgramada de ayer o anterior.
    /// </summary>
    public static MantenimientoFinalizarDTO FinalizarDTO(
        string    descripcion      = "Trabajo completado",
        string    realizadoPor     = "Taller Sur",
        decimal   costo            = 2_000,
        DateOnly? fechaRealizacion = null) => new()
    {
        Descripcion      = descripcion,
        RealizadoPor     = realizadoPor,
        Costo            = costo,
        FechaRealizacion = fechaRealizacion ?? DateOnly.FromDateTime(DateTime.Today)
    };

    /// <summary>
    /// DTO válido para cancelar un mantenimiento.
    /// </summary>
    public static MantenimientoCancelarDTO CancelarDTO(
        string descripcion = "Cancelado: falta de repuestos") => new()
    {
        Descripcion = descripcion
    };

    // ══════════════════════════════════════════════════════════════════════
    // DTOs DE RESPUESTA (Response)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Construye un <see cref="MantenimientoResponseDTO"/> representativo,
    /// usado por los mocks de <c>GetByIdAsync</c> cuando el service
    /// necesita devolver el resultado tras una creación.
    /// </summary>
    public static MantenimientoResponseDTO ResponseDTO(
        int     id        = 1,
        int     vehiculoId = 1,
        string  estado    = "pendiente",
        string  prioridad = "media") => new()
    {
        IdMantenimiento  = id,
        VehiculoId       = vehiculoId,
        VehiculoPatente  = "AA000BB",
        VehiculoMarca    = "Chevrolet",
        VehiculoModelo   = "Corsa",
        VehiculoEstado   = "mantenimiento",
        EmpleadoId       = 2,
        EmpleadoNombre   = "Juan Pérez",
        Tipo             = "preventivo",
        Descripcion      = "Revisión general",
        Estado           = estado,
        Prioridad        = prioridad,
        FechaProgramada  = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        FechaRealizacion = null,
        Costo            = 0,
        RealizadoPor     = string.Empty,
        Disponibilizado  = false
    };
}
