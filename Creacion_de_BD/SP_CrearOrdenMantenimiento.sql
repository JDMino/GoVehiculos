-- ============================================================
-- SP_CrearOrdenMantenimiento
-- Crea una nueva orden de mantenimiento y actualiza el estado
-- del vehículo asociado a "mantenimiento" en una sola transacción.
--
-- Reemplaza la lógica que antes estaba distribuida entre
-- MantenimientoService.CreateAsync() y los repositorios:
--   - Verifica que el vehículo exista y esté activo.
--   - Verifica que el estado mecánico sea "regular" o "malo".
--   - Verifica que no tenga una orden activa ya existente.
--   - Verifica que el mantenimiento no esté a cargo del socio.
--   - Inserta la nueva orden en Mantenimiento.
--   - Actualiza Vehiculo.estado = 'mantenimiento'.
-- Todo en una única transacción atómica del lado del servidor.
--
-- Parámetros de entrada:
--   @VehiculoId    INT           — ID del vehículo a mantener
--   @EmpleadoId    INT           — ID del empleado asignado
--   @Tipo          NVARCHAR(30)  — Tipo de mantenimiento
--   @Descripcion   NVARCHAR(500) — Descripción del trabajo
--   @Prioridad     NVARCHAR(20)  — Prioridad de la orden
--   @FechaProgramada DATE        — Fecha programada del trabajo
--
-- Parámetros de salida:
--   @IdMantenimiento INT           — ID de la orden creada (0 si falló)
--   @Exito           BIT           — 1 si fue exitoso, 0 si no
--   @Mensaje         NVARCHAR(300) — Mensaje descriptivo del resultado
-- ============================================================

CREATE OR ALTER PROCEDURE SP_CrearOrdenMantenimiento
    @VehiculoId      INT,
    @EmpleadoId      INT,
    @Tipo            NVARCHAR(30),
    @Descripcion     NVARCHAR(500),
    @Prioridad       NVARCHAR(20),
    @FechaProgramada DATE,
    @IdMantenimiento INT           OUTPUT,
    @Exito           BIT           OUTPUT,
    @Mensaje         NVARCHAR(300) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @IdMantenimiento = 0;

    -- Variables internas
    DECLARE @EstadoMecanico        NVARCHAR(50);
    DECLARE @MantenimientoACargoDe NVARCHAR(50);
    DECLARE @Activo                BIT;
    DECLARE @TieneActivo           INT;

    -- ── Buscar el vehículo ───────────────────────────────────────
    SELECT
        @EstadoMecanico        = estado_mecanico,
        @MantenimientoACargoDe = mantenimiento_a_cargo_de,
        @Activo                = activo
    FROM Vehiculo
    WHERE id_vehiculo = @VehiculoId;

    -- Verificar que el vehículo existe
    IF @EstadoMecanico IS NULL
    BEGIN
        SET @Exito   = 0;
        SET @Mensaje = 'Vehículo no encontrado.';
        RETURN;
    END

    -- Verificar que el vehículo esté activo
    IF @Activo = 0
    BEGIN
        SET @Exito   = 0;
        SET @Mensaje = 'El vehículo no está activo.';
        RETURN;
    END

    -- Verificar estado mecánico: debe ser "regular" o "malo"
    IF @EstadoMecanico NOT IN ('regular', 'malo')
    BEGIN
        SET @Exito   = 0;
        SET @Mensaje = 'El vehículo no requiere mantenimiento según su estado mecánico.';
        RETURN;
    END

    -- Verificar que el mantenimiento no esté a cargo del socio
    IF @MantenimientoACargoDe = 'socio'
    BEGIN
        SET @Exito   = 0;
        SET @Mensaje = 'Este vehículo tiene el mantenimiento a cargo del socio. Usá la opción correspondiente.';
        RETURN;
    END

    -- Verificar que no tenga una orden activa ya existente
    SELECT @TieneActivo = COUNT(*)
    FROM Mantenimiento
    WHERE vehiculo_id = @VehiculoId
      AND estado IN ('pendiente', 'en_proceso', 'iniciado');

    IF @TieneActivo > 0
    BEGIN
        SET @Exito   = 0;
        SET @Mensaje = 'El vehículo ya tiene una orden de mantenimiento activa.';
        RETURN;
    END

    -- ── Ejecutar la operación en una transacción atómica ─────────
    BEGIN TRANSACTION;
    BEGIN TRY

        -- Insertar la nueva orden de mantenimiento
        INSERT INTO Mantenimiento (
            vehiculo_id, empleado_id, tipo, descripcion,
            estado, prioridad, fecha_programada,
            costo, realizado_por, disponibilizado
        )
        VALUES (
            @VehiculoId, @EmpleadoId, @Tipo, @Descripcion,
            'pendiente', @Prioridad, @FechaProgramada,
            0, '', 0
        );

        SET @IdMantenimiento = SCOPE_IDENTITY();

        -- Actualizar el estado del vehículo
        UPDATE Vehiculo
        SET estado = 'mantenimiento'
        WHERE id_vehiculo = @VehiculoId;

        COMMIT TRANSACTION;

        SET @Exito   = 1;
        SET @Mensaje = 'Orden de mantenimiento creada correctamente.';

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @IdMantenimiento = 0;
        SET @Exito           = 0;
        SET @Mensaje         = 'Error interno al crear la orden: ' + ERROR_MESSAGE();
    END CATCH
END;
GO
