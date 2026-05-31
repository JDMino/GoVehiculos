using GoVehiculos.API.Data;
using GoVehiculos.API.Observers;
using GoVehiculos.API.Services;
using GoVehiculos.API.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Conexión BD
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================
// Servicios — existentes
// ============================
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<VehiculoService>();
builder.Services.AddScoped<MantenimientoService>();

// ============================
// Servicios — módulo de multas
// ============================
builder.Services.AddScoped<IncidenciaService>();
builder.Services.AddScoped<PenalizacionService>();
builder.Services.AddScoped<MultaService>();

// ============================
// Repositorios — existentes
// ============================
builder.Services.AddScoped<IMantenimientoRepository, MantenimientoRepository>();
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

// ============================
// Repositorios — módulo de multas
// ============================
builder.Services.AddScoped<IIncidenciaRepository, IncidenciaRepository>();
builder.Services.AddScoped<IMultaRepository, MultaRepository>();
builder.Services.AddScoped<IPenalizacionRepository, PenalizacionRepository>();

// ============================
// Observadores — módulo de multas
//
// PATRÓN OBSERVADOR — Registro de observadores:
// Cada observador se registra como implementación de IMultaObserver.
// El contenedor de DI los inyecta como IEnumerable<IMultaObserver>
// en el constructor de MultaService, quien los recorre al notificar.
//
// Se registran como Scoped porque dependen de repositorios que también
// son Scoped. Agregar un nuevo efecto secundario implica únicamente
// crear la clase observadora y añadir una línea aquí, sin tocar
// MultaService ni ningún otro archivo existente.
// ============================
builder.Services.AddScoped<IMultaObserver, EstadoMecanicoObserver>();
builder.Services.AddScoped<IMultaObserver, BloqueoUsuarioObserver>();
builder.Services.AddScoped<IMultaObserver, InhabilitacionVehiculoObserver>();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();