-- ============================================================
-- SP_CancelarMulta
-- Cancela una multa y revoca su penalización asociada en una
-- sola transacción atómica.
--
-- Reemplaza la lógica que antes estaba en MultaService.CancelarAsync()
-- usando MultaRepository y PenalizacionRepository por separado:
--   - Verifica que la multa exista.
--   - Verifica que la multa no esté ya cancelada.
--   - Actualiza Multa.estado = 'cancelada'.
--   - Adjunta el motivo de cancelación a Multa.descripcion.
--   - Actualiza Penalizacion.estado = 'revocada' si existe.
-- Todo en una única transacción atómica del lado del servidor.
--
-- Parámetros de entrada:
--   @IdMulta           INT           — ID de la multa a cancelar
--   @MotivoCancelacion NVARCHAR(300) — Motivo opcional de cancelación
--
-- Parámetros de salida:
--   @Exito   BIT           — 1 si fue exitoso, 0 si no
--   @Mensaje NVARCHAR(300) — Mensaje descriptivo del resultado
-- ============================================================

CREATE OR ALTER PROCEDURE SP_CancelarMulta
    @IdMulta           INT,
    @MotivoCancelacion NVARCHAR(300),
    @Exito             BIT           OUTPUT,
    @Mensaje           NVARCHAR(300) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Variables internas
    DECLARE @EstadoActual  NVARCHAR(50);
    DECLARE @DescActual    NVARCHAR(500);
    DECLARE @DescNueva     NVARCHAR(500);
    DECLARE @IdPenalizacion INT;

    -- ── Buscar la multa ──────────────────────────────────────────
    SELECT
        @EstadoActual = estado,
        @DescActual   = descripcion
    FROM Multa
    WHERE id_multa = @IdMulta;

    -- Verificar que la multa existe
    IF @EstadoActual IS NULL
    BEGIN
        SET @Exito   = 0;
        SET @Mensaje = 'Multa no encontrada.';
        RETURN;
    END

    -- Verificar que no esté ya cancelada
    IF @EstadoActual = 'cancelada'
    BEGIN
        SET @Exito   = 0;
        SET @Mensaje = 'La multa ya fue cancelada anteriormente.';
        RETURN;
    END

    -- Construir la descripción con el motivo adjunto
    SET @DescNueva = ISNULL(@DescActual, '');
    IF @MotivoCancelacion IS NOT NULL AND LEN(LTRIM(RTRIM(@MotivoCancelacion))) > 0
        SET @DescNueva = @DescNueva + ' | CANCELADA: ' + LTRIM(RTRIM(@MotivoCancelacion));

    -- Buscar la penalización asociada (puede no existir)
    SELECT @IdPenalizacion = id_penalizacion
    FROM Penalizacion
    WHERE multa_id = @IdMulta;

    -- ── Ejecutar la operación en una transacción atómica ─────────
    BEGIN TRANSACTION;
    BEGIN TRY

        -- Cancelar la multa
        UPDATE Multa
        SET
            estado      = 'cancelada',
            descripcion = @DescNueva
        WHERE id_multa = @IdMulta;

        -- Revocar la penalización asociada si existe
        IF @IdPenalizacion IS NOT NULL
        BEGIN
            UPDATE Penalizacion
            SET estado = 'revocada'
            WHERE id_penalizacion = @IdPenalizacion;
        END

        COMMIT TRANSACTION;

        SET @Exito   = 1;
        SET @Mensaje = 'Multa cancelada y penalización revocada correctamente.';

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @Exito   = 0;
        SET @Mensaje = 'Error interno al cancelar la multa: ' + ERROR_MESSAGE();
    END CATCH
END;
GO
