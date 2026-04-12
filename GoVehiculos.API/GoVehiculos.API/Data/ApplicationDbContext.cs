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

            // ============================
            // Claves primarias
            // ============================
            modelBuilder.Entity<Rol>().HasKey(r => r.IdRol);
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Vehiculo>().HasKey(v => v.IdVehiculo);
            modelBuilder.Entity<Mantenimiento>().HasKey(m => m.IdMantenimiento);
            modelBuilder.Entity<Notificacion>().HasKey(n => n.IdNotificacion);

            // ============================
            // Relaciones entre entidades
            // ============================

            // Usuario ? Rol (FK rol_id)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.RolId)
                .HasConstraintName("FK_Usuario_Rol");

            // Vehiculo ? Usuario (FK socio_id)
            modelBuilder.Entity<Vehiculo>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(v => v.SocioId)
                .HasConstraintName("FK_Vehiculo_Socio");

            // Mantenimiento ? Vehiculo (FK vehiculo_id)
            modelBuilder.Entity<Mantenimiento>()
                .HasOne<Vehiculo>()
                .WithMany()
                .HasForeignKey(m => m.VehiculoId)
                .HasConstraintName("FK_Mantenimiento_Vehiculo");

            // Mantenimiento ? Usuario (FK empleado_id)
            modelBuilder.Entity<Mantenimiento>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(m => m.EmpleadoId)
                .HasConstraintName("FK_Mantenimiento_Empleado");

            // Notificacion ? Usuario (FK usuario_id)
            modelBuilder.Entity<Notificacion>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(n => n.UsuarioId)
                .HasConstraintName("FK_Notificacion_Usuario");

            // ============================
            // Mapeo de columnas
            // ============================

            // Rol
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.Property(e => e.IdRol).HasColumnName("id_rol");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
            });

            // Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.Apellido).HasColumnName("apellido");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Dni).HasColumnName("dni");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.Telefono).HasColumnName("telefono");
                entity.Property(e => e.Direccion).HasColumnName("direccion");
                entity.Property(e => e.RolId).HasColumnName("rol_id");
                entity.Property(e => e.Verificado).HasColumnName("verificado");
                entity.Property(e => e.Bloqueado).HasColumnName("bloqueado");
                entity.Property(e => e.Activo).HasColumnName("activo");
                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");
                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");
            });

            // Vehiculo
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
                entity.Property(e => e.SocioId).HasColumnName("socio_id");
                entity.Property(e => e.Tipo).HasColumnName("tipo");
                entity.Property(e => e.Marca).HasColumnName("marca");
                entity.Property(e => e.Modelo).HasColumnName("modelo");
                entity.Property(e => e.Anio).HasColumnName("anio");
                entity.Property(e => e.Patente).HasColumnName("patente");
                entity.Property(e => e.Estado).HasColumnName("estado");
                entity.Property(e => e.EstadoMecanico).HasColumnName("estado_mecanico");
                entity.Property(e => e.Kilometraje).HasColumnName("kilometraje");
                entity.Property(e => e.LicenciaRequerida).HasColumnName("licencia_requerida");
                entity.Property(e => e.PrecioPorDia).HasColumnName("precio_por_dia");
                entity.Property(e => e.UbicacionActual).HasColumnName("ubicacion_actual");
                entity.Property(e => e.SeguroVigente).HasColumnName("seguro_vigente");
                entity.Property(e => e.DocumentacionVigente).HasColumnName("documentacion_vigente");
                entity.Property(e => e.MantenimientoACargoDe).HasColumnName("mantenimiento_a_cargo_de");
                entity.Property(e => e.ImagenUrl).HasColumnName("imagen_url");
                entity.Property(e => e.Activo).HasColumnName("activo");
                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");
            });

            // Mantenimiento
            modelBuilder.Entity<Mantenimiento>(entity =>
            {
                entity.Property(e => e.IdMantenimiento).HasColumnName("id_mantenimiento");
                entity.Property(e => e.VehiculoId).HasColumnName("vehiculo_id");
                entity.Property(e => e.EmpleadoId).HasColumnName("empleado_id");
                entity.Property(e => e.Tipo).HasColumnName("tipo");
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
                entity.Property(e => e.Estado).HasColumnName("estado");
                entity.Property(e => e.Prioridad).HasColumnName("prioridad");
                entity.Property(e => e.FechaProgramada).HasColumnName("fecha_programada");
                entity.Property(e => e.FechaRealizacion).HasColumnName("fecha_realizacion");
                entity.Property(e => e.Costo).HasColumnName("costo");
                entity.Property(e => e.RealizadoPor).HasColumnName("realizado_por");
            });

            // Notificacion
            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
                entity.Property(e => e.Tipo).HasColumnName("tipo");
                entity.Property(e => e.Titulo).HasColumnName("titulo");
                entity.Property(e => e.Mensaje).HasColumnName("mensaje");
                entity.Property(e => e.Canal).HasColumnName("canal");
                entity.Property(e => e.Fecha).HasColumnName("fecha");
                entity.Property(e => e.Leido).HasColumnName("leido");
                entity.Property(e => e.EstadoEnvio).HasColumnName("estado_envio");
                entity.Property(e => e.Prioridad).HasColumnName("prioridad");
            });
        }
    }
}
