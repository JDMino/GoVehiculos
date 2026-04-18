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

        // Módulo geográfico
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<Direccion> Direcciones { get; set; }

        // Módulo de usuarios
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        //public DbSet<Licencia> Licencias { get; set; }

        // Módulo de vehículos
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Modelo> Modelos { get; set; }
        public DbSet<Ubicacion> Ubicaciones { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Mantenimiento> Mantenimientos { get; set; }

        // Módulo operativo
        //public DbSet<Reserva> Reservas { get; set; }
        //public DbSet<Pago> Pagos { get; set; }

        // Módulo de interacción y sanciones
        //public DbSet<Valoracion> Valoraciones { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        //public DbSet<TicketSoporte> TicketsSoporte { get; set; }
        public DbSet<Incidencia> Incidencias { get; set; }
        public DbSet<Multa> Multas { get; set; }
        public DbSet<Penalizacion> Penalizaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // NOTA: Marca, Modelo y Ubicacion ya tienen [Table] y [Column]
            // annotations en sus modelos. No se repite HasColumnName() aquí
            // para evitar conflictos de mapeo duplicado.
            // El resto de entidades se mapea completamente vía Fluent API.
            // ============================

            // ============================
            // MÓDULO GEOGRÁFICO
            // ============================

            modelBuilder.Entity<Provincia>(entity =>
            {
                entity.ToTable("Provincia");
                entity.HasKey(p => p.IdProvincia);
                entity.Property(p => p.IdProvincia).HasColumnName("id_provincia");
                entity.Property(p => p.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(100);
                entity.HasIndex(p => p.Nombre).IsUnique();
            });

            modelBuilder.Entity<Localidad>(entity =>
            {
                entity.ToTable("Localidad");
                entity.HasKey(l => l.IdLocalidad);
                entity.Property(l => l.IdLocalidad).HasColumnName("id_localidad");
                entity.Property(l => l.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(100);
                entity.Property(l => l.ProvinciaId).HasColumnName("provincia_id");

                entity.HasOne(l => l.Provincia)
                    .WithMany(p => p.Localidades)
                    .HasForeignKey(l => l.ProvinciaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Localidad_Provincia");
            });

            modelBuilder.Entity<Direccion>(entity =>
            {
                entity.ToTable("Direccion");
                entity.HasKey(d => d.IdDireccion);
                entity.Property(d => d.IdDireccion).HasColumnName("id_direccion");
                entity.Property(d => d.Calle).HasColumnName("calle").IsRequired().HasMaxLength(150);
                entity.Property(d => d.Numero).HasColumnName("numero");
                entity.Property(d => d.PisoDepto).HasColumnName("piso_depto").HasMaxLength(50);
                entity.Property(d => d.CodigoPostal).HasColumnName("codigo_postal").HasMaxLength(20);
                entity.Property(d => d.LocalidadId).HasColumnName("localidad_id");

                entity.HasOne(d => d.Localidad)
                    .WithMany(l => l.Direcciones)
                    .HasForeignKey(d => d.LocalidadId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Direccion_Localidad");
            });

            // ============================
            // MÓDULO DE USUARIOS
            // ============================

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol");
                entity.HasKey(r => r.IdRol);
                entity.Property(r => r.IdRol).HasColumnName("id_rol");
                entity.Property(r => r.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(50);
                entity.HasIndex(r => r.Nombre).IsUnique();
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(u => u.IdUsuario);
                entity.Property(u => u.IdUsuario).HasColumnName("id_usuario");
                entity.Property(u => u.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(100);
                entity.Property(u => u.Apellido).HasColumnName("apellido").IsRequired().HasMaxLength(100);
                entity.Property(u => u.Dni).HasColumnName("dni").IsRequired().HasMaxLength(20);
                entity.Property(u => u.Email).HasColumnName("email").IsRequired().HasMaxLength(150);
                entity.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
                entity.Property(u => u.Telefono).HasColumnName("telefono").HasMaxLength(20);
                entity.Property(u => u.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("GETDATE()");
                entity.Property(u => u.Activo).HasColumnName("activo").HasDefaultValue(true);
                entity.Property(u => u.Bloqueado).HasColumnName("bloqueado").HasDefaultValue(false);
                entity.Property(u => u.RolId).HasColumnName("rol_id");
                entity.Property(u => u.DireccionId).HasColumnName("direccion_id");
                // Columnas agregadas con ALTER TABLE en el SQL
                entity.Property(u => u.Verificado).HasColumnName("verificado").HasDefaultValue(true);
                entity.Property(u => u.FechaBaja).HasColumnName("fecha_baja");

                entity.HasIndex(u => u.Dni).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasOne(u => u.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(u => u.RolId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Usuario_Rol");

                entity.HasOne(u => u.Direccion)
                    .WithMany()
                    .HasForeignKey(u => u.DireccionId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Usuario_Direccion");
            });

            // modelBuilder.Entity<Licencia>(entity =>
            // {
            //     entity.ToTable("Licencia");
            //     entity.HasKey(l => l.IdLicencia);
            //     entity.Property(l => l.IdLicencia).HasColumnName("id_licencia");
            //     entity.Property(l => l.UsuarioId).HasColumnName("usuario_id");
            //     entity.Property(l => l.Numero).HasColumnName("numero").IsRequired().HasMaxLength(50);
            //     entity.Property(l => l.Categoria).HasColumnName("categoria").IsRequired().HasMaxLength(10);
            //     entity.Property(l => l.FechaVencimiento).HasColumnName("fecha_vencimiento");
            //     entity.Property(l => l.FotoUrl).HasColumnName("foto_url").HasMaxLength(255);

            //     entity.HasIndex(l => l.UsuarioId).IsUnique();
            //     entity.HasIndex(l => l.Numero).IsUnique();

            //     entity.HasOne(l => l.Usuario)
            //         .WithMany()
            //         .HasForeignKey(l => l.UsuarioId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Licencia_Usuario");
            // });

            // ============================
            // MÓDULO DE VEHÍCULOS
            // Marca, Modelo y Ubicacion usan [Column] annotations —
            // solo se configuran relaciones e índices aquí.
            // ============================

            modelBuilder.Entity<Marca>(entity =>
            {
                entity.HasIndex(m => m.Nombre).IsUnique();
            });

            modelBuilder.Entity<Modelo>(entity =>
            {
                entity.HasOne(m => m.Marca)
                    .WithMany(ma => ma.Modelos)
                    .HasForeignKey(m => m.MarcaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Modelo_Marca");
            });

            modelBuilder.Entity<Ubicacion>(entity =>
            {
                entity.HasOne(u => u.Direccion)
                    .WithMany()
                    .HasForeignKey(u => u.DireccionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Ubicacion_Direccion");
            });

            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.ToTable("Vehiculo");
                entity.HasKey(v => v.IdVehiculo);
                entity.Property(v => v.IdVehiculo).HasColumnName("id_vehiculo");
                entity.Property(v => v.SocioId).HasColumnName("socio_id");
                entity.Property(v => v.Patente).HasColumnName("patente").IsRequired().HasMaxLength(20);
                entity.Property(v => v.Anio).HasColumnName("anio");
                entity.Property(v => v.Tipo).HasColumnName("tipo").IsRequired().HasMaxLength(50);
                entity.Property(v => v.Estado).HasColumnName("estado").IsRequired().HasMaxLength(50).HasDefaultValue("disponible");
                entity.Property(v => v.EstadoMecanico).HasColumnName("estado_mecanico").IsRequired().HasMaxLength(50).HasDefaultValue("bueno");
                entity.Property(v => v.Kilometraje).HasColumnName("kilometraje").HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(v => v.PrecioPorDia).HasColumnName("precio_por_dia").HasColumnType("decimal(10,2)");
                entity.Property(v => v.MantenimientoACargoDe).HasColumnName("mantenimiento_a_cargo_de").IsRequired().HasMaxLength(50);
                entity.Property(v => v.SeguroVigente).HasColumnName("seguro_vigente").HasDefaultValue(true);
                entity.Property(v => v.DocumentacionVigente).HasColumnName("documentacion_vigente").HasDefaultValue(true);
                entity.Property(v => v.Activo).HasColumnName("activo").HasDefaultValue(true);
                entity.Property(v => v.ImagenUrl).HasColumnName("imagen_url").HasMaxLength(255);
                entity.Property(v => v.ModeloId).HasColumnName("modelo_id");
                entity.Property(v => v.UbicacionActualId).HasColumnName("ubicacion_actual_id");

                entity.HasIndex(v => v.Patente).IsUnique();

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CHK_Vehiculo_Estado",
                        "[estado] IN ('disponible', 'reservado', 'en_uso', 'mantenimiento', 'fuera_de_servicio')");
                    t.HasCheckConstraint("CHK_Vehiculo_Mantenimiento",
                        "[mantenimiento_a_cargo_de] IN ('empresa', 'socio')");
                });

                entity.HasOne(v => v.Modelo)
                    .WithMany(m => m.Vehiculos)
                    .HasForeignKey(v => v.ModeloId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Vehiculo_Modelo");

                entity.HasOne(v => v.UbicacionActual)
                    .WithMany(u => u.Vehiculos)
                    .HasForeignKey(v => v.UbicacionActualId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Vehiculo_Ubicacion");

                entity.HasOne(v => v.Socio)
                    .WithMany()
                    .HasForeignKey(v => v.SocioId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Vehiculo_Socio");
            });

            modelBuilder.Entity<Mantenimiento>(entity =>
            {
                entity.ToTable("Mantenimiento");
                entity.HasKey(m => m.IdMantenimiento);
                entity.Property(m => m.IdMantenimiento).HasColumnName("id_mantenimiento");
                entity.Property(m => m.VehiculoId).HasColumnName("vehiculo_id");
                entity.Property(m => m.EmpleadoId).HasColumnName("empleado_id");
                entity.Property(m => m.Tipo).HasColumnName("tipo").IsRequired().HasMaxLength(30);
                entity.Property(m => m.Descripcion).HasColumnName("descripcion").IsRequired().HasMaxLength(500);
                entity.Property(m => m.Estado).HasColumnName("estado").IsRequired().HasMaxLength(20).HasDefaultValue("pendiente");
                entity.Property(m => m.Prioridad).HasColumnName("prioridad").IsRequired().HasMaxLength(20).HasDefaultValue("media");
                entity.Property(m => m.FechaProgramada).HasColumnName("fecha_programada");
                entity.Property(m => m.FechaRealizacion).HasColumnName("fecha_realizacion");
                entity.Property(m => m.Costo).HasColumnName("costo").HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(m => m.RealizadoPor).HasColumnName("realizado_por").IsRequired().HasMaxLength(20);

                entity.HasOne(m => m.Vehiculo)
                    .WithMany()
                    .HasForeignKey(m => m.VehiculoId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Mantenimiento_Vehiculo");

                entity.HasOne(m => m.Empleado)
                    .WithMany()
                    .HasForeignKey(m => m.EmpleadoId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Mantenimiento_Usuario");
            });

            // ============================
            // MÓDULO OPERATIVO
            // ============================

            // modelBuilder.Entity<Reserva>(entity =>
            // {
            //     entity.ToTable("Reserva");
            //     entity.HasKey(r => r.IdReserva);
            //     entity.Property(r => r.IdReserva).HasColumnName("id_reserva");
            //     entity.Property(r => r.UsuarioId).HasColumnName("usuario_id");
            //     entity.Property(r => r.VehiculoId).HasColumnName("vehiculo_id");
            //     entity.Property(r => r.UbicacionRetiroId).HasColumnName("ubicacion_retiro_id");
            //     entity.Property(r => r.UbicacionDevolucionId).HasColumnName("ubicacion_devolucion_id");
            //     entity.Property(r => r.EntregaADomicilio).HasColumnName("entrega_a_domicilio").HasDefaultValue(false);
            //     entity.Property(r => r.DireccionEntregaId).HasColumnName("direccion_entrega_id");
            //     entity.Property(r => r.FechaReserva).HasColumnName("fecha_reserva").HasDefaultValueSql("GETDATE()");
            //     entity.Property(r => r.FechaInicio).HasColumnName("fecha_inicio");
            //     entity.Property(r => r.FechaFin).HasColumnName("fecha_fin");
            //     entity.Property(r => r.Estado).HasColumnName("estado").IsRequired().HasMaxLength(50).HasDefaultValue("pendiente");
            //     entity.Property(r => r.PrecioTotal).HasColumnName("precio_total").HasColumnType("decimal(10,2)");

            //     entity.HasOne(r => r.Usuario)
            //         .WithMany()
            //         .HasForeignKey(r => r.UsuarioId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Reserva_Usuario");

            //     entity.HasOne(r => r.Vehiculo)
            //         .WithMany()
            //         .HasForeignKey(r => r.VehiculoId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Reserva_Vehiculo");

            //     entity.HasOne(r => r.UbicacionRetiro)
            //         .WithMany()
            //         .HasForeignKey(r => r.UbicacionRetiroId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Reserva_UbicacionRetiro");

            //     entity.HasOne(r => r.UbicacionDevolucion)
            //         .WithMany()
            //         .HasForeignKey(r => r.UbicacionDevolucionId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Reserva_UbicacionDevolucion");

            //     entity.HasOne(r => r.DireccionEntrega)
            //         .WithMany()
            //         .HasForeignKey(r => r.DireccionEntregaId)
            //         .OnDelete(DeleteBehavior.SetNull)
            //         .HasConstraintName("FK_Reserva_DireccionEntrega");
            // });

            // modelBuilder.Entity<Pago>(entity =>
            // {
            //     entity.ToTable("Pago");
            //     entity.HasKey(p => p.IdPago);
            //     entity.Property(p => p.IdPago).HasColumnName("id_pago");
            //     entity.Property(p => p.ReservaId).HasColumnName("reserva_id");
            //     entity.Property(p => p.Monto).HasColumnName("monto").HasColumnType("decimal(10,2)");
            //     entity.Property(p => p.MetodoPago).HasColumnName("metodo_pago").IsRequired().HasMaxLength(50);
            //     entity.Property(p => p.Estado).HasColumnName("estado").IsRequired().HasMaxLength(50).HasDefaultValue("completado");
            //     entity.Property(p => p.FechaPago).HasColumnName("fecha_pago").HasDefaultValueSql("GETDATE()");

            //     entity.HasIndex(p => p.ReservaId).IsUnique();

            //     entity.HasOne(p => p.Reserva)
            //         .WithMany()
            //         .HasForeignKey(p => p.ReservaId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Pago_Reserva");
            // });

            // ============================
            // MÓDULO DE INTERACCIÓN Y SANCIONES
            // ============================

            // modelBuilder.Entity<Valoracion>(entity =>
            // {
            //     entity.ToTable("Valoracion");
            //     entity.HasKey(v => v.IdValoracion);
            //     entity.Property(v => v.IdValoracion).HasColumnName("id_valoracion");
            //     entity.Property(v => v.ReservaId).HasColumnName("reserva_id");
            //     entity.Property(v => v.UsuarioId).HasColumnName("usuario_id");
            //     entity.Property(v => v.VehiculoId).HasColumnName("vehiculo_id");
            //     entity.Property(v => v.Puntuacion).HasColumnName("puntuacion").IsRequired();
            //     entity.Property(v => v.Comentario).HasColumnName("comentario").HasMaxLength(500);
            //     entity.Property(v => v.Fecha).HasColumnName("fecha").HasDefaultValueSql("GETDATE()");

            //     entity.HasIndex(v => v.ReservaId).IsUnique();

            //     entity.ToTable(t =>
            //     {
            //         t.HasCheckConstraint("CHK_Valoracion_Puntuacion", "[puntuacion] BETWEEN 1 AND 5");
            //     });

            //     entity.HasOne(v => v.Reserva)
            //         .WithMany()
            //         .HasForeignKey(v => v.ReservaId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Valoracion_Reserva");

            //     entity.HasOne(v => v.Usuario)
            //         .WithMany()
            //         .HasForeignKey(v => v.UsuarioId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Valoracion_Usuario");

            //     entity.HasOne(v => v.Vehiculo)
            //         .WithMany()
            //         .HasForeignKey(v => v.VehiculoId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Valoracion_Vehiculo");
            // });

            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.ToTable("Notificacion");
                entity.HasKey(n => n.IdNotificacion);
                entity.Property(n => n.IdNotificacion).HasColumnName("id_notificacion");
                entity.Property(n => n.UsuarioId).HasColumnName("usuario_id");
                entity.Property(n => n.Titulo).HasColumnName("titulo").IsRequired().HasMaxLength(100);
                entity.Property(n => n.Mensaje).HasColumnName("mensaje").IsRequired().HasMaxLength(500);
                entity.Property(n => n.Leido).HasColumnName("leido").HasDefaultValue(false);
                entity.Property(n => n.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");

                entity.HasOne(n => n.Usuario)
                    .WithMany()
                    .HasForeignKey(n => n.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Notificacion_Usuario");
            });

            // modelBuilder.Entity<TicketSoporte>(entity =>
            // {
            //     entity.ToTable("TicketSoporte");
            //     entity.HasKey(t => t.IdTicket);
            //     entity.Property(t => t.IdTicket).HasColumnName("id_ticket");
            //     entity.Property(t => t.UsuarioId).HasColumnName("usuario_id");
            //     entity.Property(t => t.Asunto).HasColumnName("asunto").IsRequired().HasMaxLength(150);
            //     entity.Property(t => t.Descripcion).HasColumnName("descripcion").IsRequired();
            //     entity.Property(t => t.Estado).HasColumnName("estado").IsRequired().HasMaxLength(50).HasDefaultValue("abierto");
            //     entity.Property(t => t.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            //     entity.Property(t => t.FechaCierre).HasColumnName("fecha_cierre");

            //     entity.HasOne(t => t.Usuario)
            //         .WithMany()
            //         .HasForeignKey(t => t.UsuarioId)
            //         .OnDelete(DeleteBehavior.Restrict)
            //         .HasConstraintName("FK_Ticket_Usuario");
            // });

            modelBuilder.Entity<Incidencia>(entity =>
            {
                entity.ToTable("Incidencia");
                entity.HasKey(i => i.IdIncidencia);
                entity.Property(i => i.IdIncidencia).HasColumnName("id_incidencia");
                entity.Property(i => i.UsuarioId).HasColumnName("usuario_id");
                entity.Property(i => i.VehiculoId).HasColumnName("vehiculo_id");
                entity.Property(i => i.Tipo).HasColumnName("tipo").IsRequired().HasMaxLength(50);
                entity.Property(i => i.Descripcion).HasColumnName("descripcion").IsRequired();
                entity.Property(i => i.NivelGravedad).HasColumnName("nivel_gravedad").IsRequired().HasMaxLength(20).HasDefaultValue("media");
                entity.Property(i => i.Estado).HasColumnName("estado").IsRequired().HasMaxLength(50).HasDefaultValue("registrada");
                entity.Property(i => i.FechaReporte).HasColumnName("fecha_reporte").HasDefaultValueSql("GETDATE()");

                entity.HasOne(i => i.Usuario)
                    .WithMany()
                    .HasForeignKey(i => i.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Incidencia_Usuario");

                entity.HasOne(i => i.Vehiculo)
                    .WithMany()
                    .HasForeignKey(i => i.VehiculoId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Incidencia_Vehiculo");
            });

            modelBuilder.Entity<Multa>(entity =>
            {
                entity.ToTable("Multa");
                entity.HasKey(m => m.IdMulta);
                entity.Property(m => m.IdMulta).HasColumnName("id_multa");
                entity.Property(m => m.IncidenciaId).HasColumnName("incidencia_id");
                entity.Property(m => m.UsuarioId).HasColumnName("usuario_id");
                entity.Property(m => m.VehiculoId).HasColumnName("vehiculo_id");
                entity.Property(m => m.Tipo).HasColumnName("tipo").IsRequired().HasMaxLength(50);
                entity.Property(m => m.Monto).HasColumnName("monto").HasColumnType("decimal(10,2)");
                entity.Property(m => m.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
                entity.Property(m => m.Estado).HasColumnName("estado").IsRequired().HasMaxLength(50).HasDefaultValue("pendiente");
                entity.Property(m => m.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");

                entity.HasIndex(m => m.IncidenciaId).IsUnique();

                entity.HasOne(m => m.Incidencia)
                    .WithMany()
                    .HasForeignKey(m => m.IncidenciaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Multa_Incidencia");

                entity.HasOne(m => m.Usuario)
                    .WithMany()
                    .HasForeignKey(m => m.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Multa_Usuario");

                entity.HasOne(m => m.Vehiculo)
                    .WithMany()
                    .HasForeignKey(m => m.VehiculoId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Multa_Vehiculo");
            });

            modelBuilder.Entity<Penalizacion>(entity =>
            {
                entity.ToTable("Penalizacion");
                entity.HasKey(p => p.IdPenalizacion);
                entity.Property(p => p.IdPenalizacion).HasColumnName("id_penalizacion");
                entity.Property(p => p.UsuarioId).HasColumnName("usuario_id");
                entity.Property(p => p.MultaId).HasColumnName("multa_id");
                entity.Property(p => p.IncidenciaId).HasColumnName("incidencia_id");
                entity.Property(p => p.Tipo).HasColumnName("tipo").IsRequired().HasMaxLength(50);
                entity.Property(p => p.Motivo).HasColumnName("motivo").IsRequired().HasMaxLength(255);
                entity.Property(p => p.FechaInicio).HasColumnName("fecha_inicio").HasDefaultValueSql("GETDATE()");
                entity.Property(p => p.FechaFin).HasColumnName("fecha_fin");
                entity.Property(p => p.Estado).HasColumnName("estado").IsRequired().HasMaxLength(50).HasDefaultValue("activa");

                entity.HasOne(p => p.Usuario)
                    .WithMany()
                    .HasForeignKey(p => p.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Penalizacion_Usuario");

                entity.HasOne(p => p.Multa)
                    .WithMany()
                    .HasForeignKey(p => p.MultaId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Penalizacion_Multa");

                entity.HasOne(p => p.Incidencia)
                    .WithMany()
                    .HasForeignKey(p => p.IncidenciaId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Penalizacion_Incidencia");
            });
        }
    }
}
