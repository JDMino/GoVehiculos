import { useState, useEffect } from "react";
import api from "../../api/axiosConfig";
import { useNavigate, useParams, Link } from "react-router-dom";
import {
  Wrench,
  ArrowLeft,
  Car,
  User,
  ClipboardList,
  Calendar,
  AlertTriangle,
  CheckCircle,
  Loader2,
  ChevronDown,
  Gauge,
  MapPin,
} from "lucide-react";

const TIPOS_MANTENIMIENTO = [
  { value: "preventivo", label: "Preventivo"       },
  { value: "correctivo", label: "Correctivo"       },
  { value: "revision",   label: "Revisión técnica" },
  { value: "emergencia", label: "Emergencia"       },
];

const PRIORIDADES = [
  { value: "baja",    label: "Baja",    color: "text-emerald-600 bg-emerald-50 border-emerald-200" },
  { value: "media",   label: "Media",   color: "text-amber-600   bg-amber-50   border-amber-200"   },
  { value: "alta",    label: "Alta",    color: "text-orange-600  bg-orange-50  border-orange-200"  },
  { value: "critica", label: "Crítica", color: "text-red-600     bg-red-50     border-red-200"     },
];

export default function MantenimientoForm() {
  const { vehiculoId } = useParams();
  const navigate       = useNavigate();

  const [loading, setLoading]           = useState(false);
  const [loadingData, setLoadingData]   = useState(true);
  const [vehiculo, setVehiculo]         = useState(null);
  const [empleados, setEmpleados]       = useState([]);
  
  const [errorNegocio, setErrorNegocio] = useState(null);
  const [mensajeExito, setMensajeExito] = useState(null);

  const [form, setForm] = useState({
    vehiculoId:      parseInt(vehiculoId),
    empleadoId:      "",
    tipo:            "preventivo",
    descripcion:     "",
    estado:          "pendiente",
    prioridad:       "media",
    fechaProgramada: "",
  });

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [vRes, uRes] = await Promise.all([
          api.get(`/vehiculos/${vehiculoId}`),
          api.get("/usuarios"),
        ]);
        setVehiculo(vRes.data);
        // Rol 3 = Empleado, solo activos
        setEmpleados(uRes.data.filter((u) => u.rolId === 3 && u.activo));
      } catch (err) {
        console.error("Error cargando datos:", err);
      } finally {
        setLoadingData(false);
      }
    };
    fetchData();
  }, [vehiculoId]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setErrorNegocio(null);
    setMensajeExito(null);
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setErrorNegocio(null);
    setMensajeExito(null);

    const empleadoIdParsed = parseInt(form.empleadoId);
    
    const payload = {
      vehiculoId:      parseInt(vehiculoId),
      empleadoId:      isNaN(empleadoIdParsed) ? 0 : empleadoIdParsed,
      tipo:            form.tipo,
      descripcion:     form.descripcion,
      estado:          "pendiente",
      prioridad:       form.prioridad,
    };

    if (form.fechaProgramada) {
      payload.fechaProgramada = form.fechaProgramada;
    }

    try {
      const res = await api.post("/mantenimientos", payload);
      
      // Muestra el mensaje de éxito enviado desde el controlador (ej. "Orden creada correctamente")
      setMensajeExito(res.data?.mensaje || "Orden de mantenimiento generada correctamente.");
      
      setTimeout(() => {
        navigate("/mantenimientos");
      }, 2500);

    } catch (err) {
      let mensajeFinal = "Error inesperado al crear la orden. Intentá de nuevo.";

      // 1. Captura de mensajes legibles de las reglas de negocio del Service / SP (status 422 o similares)
      if (err.response?.data?.mensaje) {
        mensajeFinal = err.response.data.mensaje;

        // CORRECCIÓN FECHA: Si el usuario no ingresó nada y el backend responde con "no puede ser anterior a hoy"
        // debido al valor por defecto (0001-01-01) del struct DateOnly, lo convertimos en un mensaje claro.
        if (!form.fechaProgramada && mensajeFinal.includes("anterior a hoy")) {
          mensajeFinal = "La fecha programada es obligatoria.";
        }
      } 
      // 2. Captura de errores automáticos de validación de modelos de ASP.NET Core (status 400 - ModelState)
      else if (err.response?.status === 400 && err.response?.data?.errors) {
        const errors = err.response.data.errors;
        
        // Buscamos la primera propiedad que falló (ej: "Descripcion", "Tipo")
        const propKey = Object.keys(errors).find(k => k.toLowerCase() !== "dto" && k !== "");
        
        if (propKey) {
          const rawError = errors[propKey][0];
          
          // CORRECCIÓN DESCRIPCIÓN: Si el framework rechaza el campo requerido en inglés, lo traducimos al vuelo
          if (rawError.toLowerCase().includes("required")) {
            if (propKey.toLowerCase().includes("descripcion") || rawError.toLowerCase().includes("description")) {
              mensajeFinal = "La descripción es obligatoria.";
            } else if (propKey.toLowerCase().includes("tipo")) {
              mensajeFinal = "El tipo de mantenimiento es obligatorio.";
            } else if (propKey.toLowerCase().includes("fecha")) {
              mensajeFinal = "La fecha programada es obligatoria.";
            } else {
              mensajeFinal = `El campo ${propKey} es obligatorio.`;
            }
          } else {
            mensajeFinal = rawError;
          }
        } else if (errors["dto"]) {
          mensajeFinal = "Verificá que todos los campos requeridos estén completos.";
        }
      } 
      // 3. Captura de excepciones crudas de base de datos o caídas internas del servidor (status 500)
      else if (err.response?.data) {
        const errorContent = typeof err.response.data === 'string' 
          ? err.response.data 
          : JSON.stringify(err.response.data);

        // CORRECCIÓN EMPLEADO: Interceptamos el conflicto de clave foránea de SQL Server cuando EmpleadoId es 0
        if (errorContent.includes("FK_Mantenimiento_Usuario") || errorContent.includes("FOREIGN KEY constraint")) {
          mensajeFinal = "El empleado asignado es obligatorio. Debe seleccionar un empleado válido.";
        } else {
          mensajeFinal = err.response.data?.mensaje || err.response.data?.exceptionMessage || "Error interno en el servidor.";
        }
      }

      setErrorNegocio(mensajeFinal);
    } finally {
      setLoading(false);
    }
  };

  if (loadingData) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <div className="flex flex-col items-center gap-3 text-slate-400">
          <Wrench className="h-10 w-10 animate-pulse" />
          <p className="text-sm">Cargando datos del vehículo...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50">

      {/* Header */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-white">
        <div className="max-w-4xl mx-auto px-6 py-8">
          <Link
            to="/mantenimientos"
            className="inline-flex items-center gap-2 text-slate-300 hover:text-white text-sm font-medium mb-4 transition-colors"
            style={{ pointerEvents: (loading || mensajeExito) ? 'none' : 'auto' }}
          >
            <ArrowLeft className="h-4 w-4" />
            Volver a mantenimientos
          </Link>
          <div className="flex items-center gap-4">
            <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center">
              <ClipboardList className="h-7 w-7 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Nueva Orden de Mantenimiento</h1>
              <p className="text-slate-300 text-sm mt-0.5">
                Al confirmar, el vehículo pasará automáticamente a estado "mantenimiento"
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-6 py-8">

        {/* Info del vehículo */}
        {vehiculo && (
          <div className="bg-white rounded-2xl border border-slate-200 p-5 mb-6 flex items-center gap-4">
            <div className="h-12 w-12 rounded-xl bg-slate-100 flex items-center justify-center text-slate-500 shrink-0">
              <Car className="h-6 w-6" />
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 flex-wrap">
                <h2 className="font-bold text-slate-900">
                  {vehiculo.marcaNombre} {vehiculo.modeloNombre}
                </h2>
                <span className="font-mono text-xs bg-slate-100 text-slate-700 px-2 py-0.5 rounded border border-slate-200">
                  {vehiculo.patente}
                </span>
                <span className={`text-xs font-bold px-2 py-0.5 rounded-lg ${
                  vehiculo.estadoMecanico === "malo"
                    ? "bg-red-50 text-red-700 border border-red-200"
                    : "bg-amber-50 text-amber-700 border border-amber-200"
                }`}>
                  {vehiculo.estadoMecanico === "malo" ? "Estado malo" : "Estado regular"}
                </span>
              </div>
              <div className="flex items-center gap-4 mt-1.5 flex-wrap">
                <span className="flex items-center gap-1 text-xs text-slate-500">
                  <Gauge className="h-3.5 w-3.5" />
                  {vehiculo.kilometraje?.toLocaleString()} km
                </span>
                {vehiculo.ubicacionNombre && (
                  <span className="flex items-center gap-1 text-xs text-slate-500">
                    <MapPin className="h-3.5 w-3.5" />
                    {vehiculo.ubicacionNombre}
                  </span>
                )}
              </div>
            </div>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">

          {/* Detalles de la orden */}
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-100 bg-slate-50">
              <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                <ClipboardList className="h-5 w-5 text-slate-400" />
                Detalles de la Orden
              </h2>
            </div>
            <div className="p-6 space-y-6">

              {/* Tipo */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Tipo de mantenimiento <span className="text-red-500">*</span>
                </label>
                <div className="relative">
                  <select
                    name="tipo"
                    value={form.tipo}
                    onChange={handleChange}
                    disabled={loading || !!mensajeExito}
                    className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 text-slate-900 font-medium cursor-pointer disabled:opacity-60"
                  >
                    {TIPOS_MANTENIMIENTO.map((t) => (
                      <option key={t.value} value={t.value}>{t.label}</option>
                    ))}
                  </select>
                  <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
                </div>
              </div>

              {/* Prioridad */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">Prioridad</label>
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                  {PRIORIDADES.map((p) => (
                    <button
                      key={p.value}
                      type="button"
                      disabled={loading || !!mensajeExito}
                      onClick={() => {
                        setErrorNegocio(null);
                        setMensajeExito(null);
                        setForm((prev) => ({ ...prev, prioridad: p.value }));
                      }}
                      className={`py-2.5 px-4 rounded-xl border text-sm font-semibold transition-all ${
                        form.prioridad === p.value
                          ? p.color + " ring-2 ring-offset-1 ring-current"
                          : "bg-slate-50 border-slate-200 text-slate-500 hover:bg-slate-100 disabled:opacity-50"
                      }`}
                    >
                      {p.label}
                    </button>
                  ))}
                </div>
              </div>

              {/* Descripción */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Descripción del trabajo <span className="text-red-500">*</span>
                </label>
                <textarea
                  name="descripcion"
                  value={form.descripcion}
                  onChange={handleChange}
                  disabled={loading || !!mensajeExito}
                  rows={4}
                  className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 text-slate-900 placeholder:text-slate-400 resize-none transition-all disabled:opacity-60"
                  placeholder="Describí el trabajo de mantenimiento a realizar..."
                />
              </div>

              {/* Aviso de estado automático */}
              <div className="flex items-center gap-3 px-4 py-3 bg-slate-50 rounded-xl border border-slate-200">
                <CheckCircle className="h-4 w-4 text-slate-400 shrink-0" />
                <p className="text-sm text-slate-600">
                  La orden se creará con estado{" "}
                  <span className="font-semibold text-slate-800">Pendiente</span> y el vehículo
                  pasará automáticamente a{" "}
                  <span className="font-semibold text-slate-800">En mantenimiento</span>.
                </p>
              </div>
            </div>
          </div>

          {/* Planificación */}
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-100 bg-slate-50">
              <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                <Calendar className="h-5 w-5 text-slate-400" />
                Planificación
              </h2>
            </div>
            <div className="p-6">
              <div className="space-y-2 max-w-xs">
                <label className="text-sm font-medium text-slate-700">
                  Fecha programada <span className="text-red-500">*</span>
                </label>
                <div className="relative">
                  <Calendar className="absolute left-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                  <input
                    type="date"
                    name="fechaProgramada"
                    value={form.fechaProgramada}
                    onChange={handleChange}
                    disabled={loading || !!mensajeExito}
                    className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 text-slate-900 transition-all disabled:opacity-60"
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Asignación */}
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-100 bg-slate-50">
              <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                <User className="h-5 w-5 text-slate-400" />
                Asignación
              </h2>
            </div>
            <div className="p-6">
              <div className="space-y-2 max-w-sm">
                <label className="text-sm font-medium text-slate-700">
                  Empleado asignado <span className="text-red-500">*</span>
                </label>
                <div className="relative">
                  <select
                    name="empleadoId"
                    value={form.empleadoId}
                    onChange={handleChange}
                    disabled={loading || !!mensajeExito}
                    className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 text-slate-900 cursor-pointer disabled:opacity-60"
                  >
                    <option value="">— Seleccioná un empleado —</option>
                    {empleados.map((e) => (
                      <option key={e.idUsuario} value={e.idUsuario}>
                        {e.nombre} {e.apellido}
                      </option>
                    ))}
                  </select>
                  <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
                </div>
                {empleados.length === 0 && (
                  <p className="text-xs text-amber-600 flex items-center gap-1 mt-1">
                    <AlertTriangle className="h-3.5 w-3.5" />
                    No hay empleados disponibles. No podés generar la orden sin asignar uno.
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Feedback Section */}
          <div className="space-y-3 pt-2">
            {errorNegocio && (
              <div className="flex items-center gap-3 bg-red-50 border border-red-200 rounded-2xl px-5 py-4 transition-all animate-fadeIn">
                <AlertTriangle className="h-5 w-5 text-red-500 shrink-0" />
                <p className="text-sm font-medium text-red-700">{errorNegocio}</p>
              </div>
            )}

            {mensajeExito && (
              <div className="flex items-center gap-3 bg-emerald-50 border border-emerald-200 rounded-2xl px-5 py-4 transition-all animate-fadeIn">
                <CheckCircle className="h-5 w-5 text-emerald-500 shrink-0" />
                <p className="text-sm font-medium text-emerald-700">{mensajeExito}</p>
              </div>
            )}

            {/* Actions */}
            <div className="flex items-center justify-between">
              <Link
                to="/mantenimientos"
                className="px-6 py-3 text-slate-600 font-semibold hover:text-slate-900 transition-colors"
                style={{ pointerEvents: (loading || mensajeExito) ? 'none' : 'auto' }}
              >
                Cancelar
              </Link>
              
              <button
                type="submit"
                disabled={loading || !!mensajeExito}
                className={`inline-flex items-center justify-center px-8 py-3 font-semibold rounded-xl transition-all shadow-lg shadow-slate-200 hover:shadow-xl ${
                  mensajeExito 
                    ? "bg-emerald-600 text-white cursor-default" 
                    : "bg-slate-900 hover:bg-slate-800 text-white disabled:bg-slate-600 disabled:cursor-not-allowed"
                }`}
              >
                {loading ? (
                  <>
                    <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                    Generando orden...
                  </>
                ) : mensajeExito ? (
                  <>
                    <CheckCircle className="h-5 w-5 mr-2" />
                    ¡Orden Creada!
                  </>
                ) : (
                  <>
                    <CheckCircle className="h-5 w-5 mr-2" />
                    Crear Orden
                  </>
                )}
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
}