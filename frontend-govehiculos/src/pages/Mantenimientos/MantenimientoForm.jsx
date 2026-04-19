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
  XCircle,
} from "lucide-react";

const TIPOS_MANTENIMIENTO = [
  { value: "preventivo", label: "Preventivo" },
  { value: "correctivo", label: "Correctivo" },
  { value: "revision", label: "Revisión técnica" },
  { value: "emergencia", label: "Emergencia" },
];

const PRIORIDADES = [
  {
    value: "baja",
    label: "Baja",
    color: "text-emerald-600 bg-emerald-50 border-emerald-200",
  },
  {
    value: "media",
    label: "Media",
    color: "text-amber-600   bg-amber-50   border-amber-200",
  },
  {
    value: "alta",
    label: "Alta",
    color: "text-orange-600  bg-orange-50  border-orange-200",
  },
  {
    value: "critica",
    label: "Crítica",
    color: "text-red-600     bg-red-50     border-red-200",
  },
];

export default function MantenimientoForm() {
  const { vehiculoId } = useParams();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  const [vehiculo, setVehiculo] = useState(null);
  const [empleados, setEmpleados] = useState([]);

  // Mensaje de error de regla de negocio (ej: 422 del backend)
  const [errorNegocio, setErrorNegocio] = useState(null);

  const [form, setForm] = useState({
    vehiculoId: parseInt(vehiculoId),
    empleadoId: "",
    tipo: "preventivo",
    descripcion: "",
    estado: "pendiente", // siempre pendiente al crear, no editable en este form
    prioridad: "media",
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
        // Rol 3 = Empleado
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
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setErrorNegocio(null);

    const payload = {
      vehiculoId: parseInt(vehiculoId),
      empleadoId: form.empleadoId ? parseInt(form.empleadoId) : null,
      tipo: form.tipo,
      descripcion: form.descripcion,
      estado: "pendiente",
      prioridad: form.prioridad,
      fechaProgramada: form.fechaProgramada || null,
    };

    try {
      await api.post("/mantenimientos", payload);
      navigate("/mantenimientos");
    } catch (err) {
      if (err.response?.status === 422) {
        // Regla de negocio del backend: el socio se hace cargo
        // El backend ya cambió el estado del vehículo a fuera_de_servicio
        setErrorNegocio(
          err.response.data?.mensaje || "No se pudo generar la orden.",
        );
      } else {
        setErrorNegocio(
          "Error inesperado al crear la orden. Intentá de nuevo.",
        );
      }
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
          >
            <ArrowLeft className="h-4 w-4" />
            Volver a mantenimientos
          </Link>
          <div className="flex items-center gap-4">
            <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center">
              <ClipboardList className="h-7 w-7 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">
                Nueva Orden de Mantenimiento
              </h1>
              <p className="text-slate-300 text-sm mt-0.5">
                Al confirmar, el vehículo pasará automáticamente a estado
                "mantenimiento"
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-6 py-8">
        {/* Banner de error de regla de negocio */}
        {errorNegocio && (
          <div className="flex items-start gap-3 bg-red-50 border border-red-200 rounded-2xl px-5 py-4 mb-6">
            <XCircle className="h-5 w-5 text-red-500 shrink-0 mt-0.5" />
            <div>
              <p className="text-sm font-semibold text-red-700">
                No se pudo generar la orden
              </p>
              <p className="text-sm text-red-600 mt-0.5">{errorNegocio}</p>
              <button
                onClick={() => navigate("/mantenimientos")}
                className="text-xs font-semibold text-red-600 underline mt-2"
              >
                Volver a la lista de mantenimientos
              </button>
            </div>
          </div>
        )}

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
                <span
                  className={`text-xs font-bold px-2 py-0.5 rounded-lg ${
                    vehiculo.estadoMecanico === "malo"
                      ? "bg-red-50 text-red-700 border border-red-200"
                      : "bg-amber-50 text-amber-700 border border-amber-200"
                  }`}
                >
                  {vehiculo.estadoMecanico === "malo"
                    ? "Estado malo"
                    : "Estado regular"}
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
                    required
                    className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 text-slate-900 font-medium cursor-pointer"
                  >
                    {TIPOS_MANTENIMIENTO.map((t) => (
                      <option key={t.value} value={t.value}>
                        {t.label}
                      </option>
                    ))}
                  </select>
                  <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
                </div>
              </div>

              {/* Prioridad */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Prioridad
                </label>
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                  {PRIORIDADES.map((p) => (
                    <button
                      key={p.value}
                      type="button"
                      onClick={() =>
                        setForm((prev) => ({ ...prev, prioridad: p.value }))
                      }
                      className={`py-2.5 px-4 rounded-xl border text-sm font-semibold transition-all ${
                        form.prioridad === p.value
                          ? p.color + " ring-2 ring-offset-1 ring-current"
                          : "bg-slate-50 border-slate-200 text-slate-500 hover:bg-slate-100"
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
                  Descripción del trabajo{" "}
                  <span className="text-red-500">*</span>
                </label>
                <textarea
                  name="descripcion"
                  value={form.descripcion}
                  onChange={handleChange}
                  required
                  rows={4}
                  className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 text-slate-900 placeholder:text-slate-400 resize-none transition-all"
                  placeholder="Describí el trabajo de mantenimiento a realizar..."
                />
              </div>

              {/* Estado — solo informativo, no editable */}
              <div className="flex items-center gap-3 px-4 py-3 bg-slate-50 rounded-xl border border-slate-200">
                <CheckCircle className="h-4 w-4 text-slate-400 shrink-0" />
                <p className="text-sm text-slate-600">
                  La orden se creará con estado{" "}
                  <span className="font-semibold text-slate-800">
                    Pendiente
                  </span>{" "}
                  y el vehículo pasará automáticamente a{" "}
                  <span className="font-semibold text-slate-800">
                    En mantenimiento
                  </span>
                  .
                </p>
              </div>
            </div>
          </div>

          {/* Fecha programada */}
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
                  Fecha programada
                </label>
                <div className="relative">
                  <Calendar className="absolute left-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                  <input
                    type="date"
                    name="fechaProgramada"
                    value={form.fechaProgramada}
                    onChange={handleChange}
                    className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 text-slate-900 transition-all"
                  />
                </div>
                <p className="text-xs text-slate-400">
                  El costo y la fecha de realización se registran al completar
                  el trabajo.
                </p>
              </div>
            </div>
          </div>

          {/* Asignación de empleado */}
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
                  Empleado asignado
                </label>
                <div className="relative">
                  <select
                    name="empleadoId"
                    value={form.empleadoId}
                    onChange={handleChange}
                    className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 text-slate-900 cursor-pointer"
                  >
                    <option value="">Sin asignar</option>
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
                    No hay empleados disponibles. Podés asignar uno después.
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Actions */}
          <div className="flex items-center justify-between pt-2">
            <Link
              to="/mantenimientos"
              className="px-6 py-3 text-slate-600 font-semibold hover:text-slate-900 transition-colors"
            >
              Cancelar
            </Link>
            <button
              type="submit"
              disabled={loading}
              className="inline-flex items-center justify-center px-8 py-3 bg-slate-900 hover:bg-slate-800 disabled:bg-slate-400 text-white font-semibold rounded-xl transition-all shadow-lg shadow-slate-200 hover:shadow-xl disabled:shadow-none"
            >
              {loading ? (
                <>
                  <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                  Generando orden...
                </>
              ) : (
                <>
                  <CheckCircle className="h-5 w-5 mr-2" />
                  Crear Orden
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
