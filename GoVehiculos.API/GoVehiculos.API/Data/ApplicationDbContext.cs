using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ============================
        // DbSets
        // ============================
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Mantenimiento> Mantenimientos { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }

        // Nuevos DbSets según normalización
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<Direccion> Direcciones { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Modelo> Modelos { get; set; }
        public DbSet<Ubicacion> Ubicaciones { get; set; }
        public DbSet<Incidencia> Incidencias { get; set; }
        public DbSet<Multa> Multas { get; set; }
        public DbSet<Penalizacion> Penalizaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // Mapeo de tablas
            // ============================
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Vehiculo>().ToTable("Vehiculo");
            modelBuilder.Entity<Mantenimiento>().ToTable("Mantenimiento");
            modelBuilder.Entity<Notificacion>().ToTable("Notificacion");

            modelBuilder.Entity<Provincia>().ToTable("Provincia");
            modelBuilder.Entity<Localidad>().ToTable("Localidad");
            modelBuilder.Entity<Direccion>().ToTable("Direccion");
            modelBuilder.Entity<Marca>().ToTable("Marca");
            modelBuilder.Entity<Modelo>().ToTable("Modelo");
            modelBuilder.Entity<Ubicacion>().ToTable("Ubicacion");
            modelBuilder.Entity<Incidencia>().ToTable("Incidencia");
            modelBuilder.Entity<Multa>().ToTable("Multa");
            modelBuilder.Entity<Penalizacion>().ToTable("Penalizacion");

            // ============================
            // Claves primarias
            // ============================
            modelBuilder.Entity<Rol>().HasKey(r => r.IdRol);
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Vehiculo>().HasKey(v => v.IdVehiculo);
            modelBuilder.Entity<Mantenimiento>().HasKey(m => m.IdMantenimiento);
            modelBuilder.Entity<Notificacion>().HasKey(n => n.IdNotificacion);

            modelBuilder.Entity<Provincia>().HasKey(p => p.IdProvincia);
            modelBuilder.Entity<Localidad>().HasKey(l => l.IdLocalidad);
            modelBuilder.Entity<Direccion>().HasKey(d => d.IdDireccion);
            modelBuilder.Entity<Marca>().HasKey(ma => ma.IdMarca);
            modelBuilder.Entity<Modelo>().HasKey(mo => mo.IdModelo);
            modelBuilder.Entity<Ubicacion>().HasKey(u => u.IdUbicacion);
            modelBuilder.Entity<Incidencia>().HasKey(i => i.IdIncidencia);
            modelBuilder.Entity<Multa>().HasKey(mu => mu.IdMulta);
            modelBuilder.Entity<Penalizacion>().HasKey(p => p.IdPenalizacion);

            // ============================
            // Relaciones entre entidades
            // ============================

            // Usuario -> Rol
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.RolId)
                .HasConstraintName("FK_Usuario_Rol");

            // Usuario -> Direccion
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Direccion)
                .WithMany()
                .HasForeignKey(u => u.DireccionId)
                .HasConstraintName("FK_Usuario_Direccion");

            // Localidad -> Provincia
            modelBuilder.Entity<Localidad>()
                .HasOne(l => l.Provincia)
                .WithMany(p => p.Localidades)
                .HasForeignKey(l => l.ProvinciaId)
                .HasConstraintName("FK_Localidad_Provincia");

            // Direccion -> Localidad
            modelBuilder.Entity<Direccion>()
                .HasOne(d => d.Localidad)
                .WithMany(l => l.Direcciones)
                .HasForeignKey(d => d.LocalidadId)
                .HasConstraintName("FK_Direccion_Localidad");

            // Modelo -> Marca
            modelBuilder.Entity<Modelo>()
                .HasOne(m => m.Marca)
                .WithMany(ma => ma.Modelos)
                .HasForeignKey(m => m.MarcaId)
                .HasConstraintName("FK_Modelo_Marca");

            // Vehiculo -> Modelo
            modelBuilder.Entity<Vehiculo>()
                .HasOne(v => v.Modelo)
                .WithMany(m => m.Vehiculos)
                .HasForeignKey(v => v.ModeloId)
                .HasConstraintName("FK_Vehiculo_Modelo");

            // Vehiculo -> Ubicacion
            modelBuilder.Entity<Vehiculo>()
                .HasOne(v => v.UbicacionActual)
                .WithMany(u => u.Vehiculos)
                .HasForeignKey(v => v.UbicacionActualId)
                .HasConstraintName("FK_Vehiculo_Ubicacion");

            // Vehiculo -> Usuario (Socio)
            modelBuilder.Entity<Vehiculo>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(v => v.SocioId)
                .HasConstraintName("FK_Vehiculo_Socio");

            // Mantenimiento -> Vehiculo
            modelBuilder.Entity<Mantenimiento>()
                .HasOne<Vehiculo>()
                .WithMany()
                .HasForeignKey(m => m.VehiculoId)
                .HasConstraintName("FK_Mantenimiento_Vehiculo");

            // Mantenimiento -> Usuario (Empleado)
            modelBuilder.Entity<Mantenimiento>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(m => m.EmpleadoId)
                .HasConstraintName("FK_Mantenimiento_Usuario");

            // Notificacion -> Usuario
            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Usuario)
                .WithMany()
                .HasForeignKey(n => n.UsuarioId)
                .HasConstraintName("FK_Notificacion_Usuario");

            // Incidencia -> Usuario
            modelBuilder.Entity<Incidencia>()
                .HasOne(i => i.Usuario)
                .WithMany()
                .HasForeignKey(i => i.UsuarioId)
                .HasConstraintName("FK_Incidencia_Usuario");

            // Incidencia -> Vehiculo
            modelBuilder.Entity<Incidencia>()
                .HasOne(i => i.Vehiculo)
                .WithMany()
                .HasForeignKey(i => i.VehiculoId)
                .HasConstraintName("FK_Incidencia_Vehiculo");

            // Multa -> Incidencia
            modelBuilder.Entity<Multa>()
                .HasOne(m => m.Incidencia)
                .WithMany()
                .HasForeignKey(m => m.IncidenciaId)
                .HasConstraintName("FK_Multa_Incidencia");

            // Multa -> Usuario
            modelBuilder.Entity<Multa>()
                .HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .HasConstraintName("FK_Multa_Usuario");

            // Multa -> Vehiculo
            modelBuilder.Entity<Multa>()
                .HasOne(m => m.Vehiculo)
                .WithMany()
                .HasForeignKey(m => m.VehiculoId)
                .HasConstraintName("FK_Multa_Vehiculo");

            // Penalizacion -> Usuario
            modelBuilder.Entity<Penalizacion>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .HasConstraintName("FK_Penalizacion_Usuario");

            // Penalizacion -> Multa
            modelBuilder.Entity<Penalizacion>()
                .HasOne(p => p.Multa)
                .WithMany()
                .HasForeignKey(p => p.MultaId)
                .HasConstraintName("FK_Penalizacion_Multa");

            // Penalizacion -> Incidencia
            modelBuilder.Entity<Penalizacion>()
                .HasOne(p => p.Incidencia)
                .WithMany()
                .HasForeignKey(p => p.IncidenciaId)
                .HasConstraintName("FK_Penalizacion_Incidencia");
        }
    }
}
