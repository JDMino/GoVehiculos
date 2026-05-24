using GoVehiculos.API.Models;

namespace GoVehiculos.API.Strategies
{
    /// <summary>
    /// Interfaz central del patrón Strategy aplicado a las acciones del empleado
    /// sobre una orden de mantenimiento.
    ///
    /// PATRÓN STRATEGY:
    /// Define un contrato común para una familia de algoritmos (acciones sobre
    /// una orden: iniciar, finalizar, cancelar). Cada algoritmo se encapsula en
    /// su propia clase concreta, permitiendo que el contexto (MantenimientoService)
    /// los intercambie sin conocer sus detalles internos.
    ///
    /// Justificación de uso:
    /// Sin Strategy, MantenimientoService tenía tres métodos (IniciarAsync,
    /// FinalizarAsync, CancelarAsync) que compartían la misma estructura:
    ///   1. Validar campos del DTO
    ///   2. Buscar la orden y verificar permiso del empleado
    ///   3. Validar el estado actual
    ///   4. Aplicar cambios
    /// Al encapsular cada variante en una estrategia, el service se convierte en
    /// un contexto que delega la ejecución sin ramificarse. Agregar una nueva acción
    /// (ej: "pausar") no requiere modificar el service, solo crear una nueva estrategia.
    /// Esto respeta el principio Open/Closed.
    /// </summary>
    public interface IAccionMantenimientoStrategy
    {
        /// <summary>
        /// Ejecuta la acción sobre la orden indicada, verificando que el empleado
        /// tenga permiso para operarla.
        /// </summary>
        /// <param name="mantenimiento">Entidad ya cargada desde el repositorio.</param>
        /// <param name="empleadoId">Id del empleado que solicita la acción.</param>
        /// <param name="contexto">Datos adicionales del DTO (puede ser null para Iniciar).</param>
        /// <returns>Tupla con resultado y mensaje descriptivo.</returns>
        Task<(bool exito, string mensaje)> EjecutarAsync(
            Mantenimiento mantenimiento,
            int empleadoId,
            object? contexto = null);
    }
}
