import { useEffect, useState } from "react";
import api from "../../api/axiosConfig";
import {
  Wrench,
  Car,
  Search,
  AlertTriangle,
  Play,
  CheckCircle2,
  XCircle,
  Eye,
  Clock,
  Ban,
  Calendar,
  DollarSign,
  User,
  Filter,
  ChevronDown,
  ChevronUp,
  Info,
  X,
  Loader2,
} from "lucide-react";

// ── Configuración visual de estados ─────────────────────────────────────────
const ESTADO_CONFIG = {
  pendiente:  { label: "Pendiente",  bg: "bg-slate-100",   text: "text-slate-600",   border: "border-slate-200",  dot: "bg-slate-400"    },
  iniciado:   { label: "Iniciado",   bg: "bg-blue-50",     text: "text-blue-700",    border: "border-blue-200",   dot: "bg-blue-500"     },
  finalizado: { label: "Finalizado", bg: "bg-emerald-50",  text: "text-emerald-700", border: "border-emerald-200",dot: "bg-emerald-500"  },
  cancelado:  { label: "Cancelado",  bg: "bg-red-50",      text: "text-red-700",     border: "border-red-200",    dot: "bg-red-400"      },
};

const PRIORIDAD_CONFIG = {
  baja:    { label: "Baja",    classes: "text-emerald-600 bg-emerald-50 border-emerald-200" },
  media:   { label: "Media",   classes: "text-amber-600   bg-amber-50   border-amber-200"   },
  alta:    { label: "Alta",    classes: "text-orange-600  bg-orange-50  border-orange-200"  },
  critica: { label: "Crítica", classes: "text-red-600     bg-red-50     border-red-200"     },
};

const ESTADOS_TERMINALES = ["finalizado", "cancelado"];

// ── Helper: resuelve el id del usuario independientemente del casing ─────────
// El serializador de ASP.NET puede devolver idUsuario, id, userId, etc.
// según cómo esté configurado y cómo el AuthStore guarde el objeto.
function resolverEmpleadoId(user) {
  if (!user) return null;
  // Intentar las variantes más comunes en orden de probabilidad
  return (
    user.idUsuario   ??   // camelCase ASP.NET por defecto
    user.IdUsuario   ??   // PascalCase si el serializador no convierte
    user.id          ??   // genérico
    user.Id          ??
    user.userId      ??
    user.UserId      ??
    null
  );
}

const hoy = () => new Date().toISOString().split("T")[0];

// ── Componente principal ─────────────────────────────────────────────────────
export default function MantenimientosEmpleado({ user }) {
  const empleadoId = resolverEmpleadoId(user);

  const [mantenimientos, setMantenimientos] = useState([]);
  const [loading, setLoading]               = useState(true);
  const [errorCarga, setErrorCarga]         = useState(null);
  const [searchTerm, setSearchTerm]         = useState("");
  const [filtroEstado, setFiltroEstado]     = useState("todos");
  const [expandido, setExpandido]           = useState(null);

  const [modalIniciar,   setModalIniciar]   = useState({ open: false, id: null });
  const [modalFinalizar, setModalFinalizar] = useState({ open: false, mant: null });
  const [modalCancelar,  setModalCancelar]  = useState({ open: false, mant: null });
  const [modalDetalle,   setModalDetalle]   = useState({ open: false, mant: null });

  const [accionLoading, setAccionLoading] = useState(false);
  const [errorAccion,   setErrorAccion]   = useState(null);

  const [formFinalizar, setFormFinalizar] = useState({
    descripcion: "", fechaRealizacion: hoy(), costo: "", realizadoPor: "",
  });
  const [formCancelar, setFormCancelar] = useState({ descripcion: "" });

  // ── Carga de datos ──────────────────────────────────────────────────────
  const cargar = async () => {
    if (!empleadoId) {
      setErrorCarga(
        `No se pudo obtener el ID del empleado. Propiedades disponibles en user: ${
          user ? Object.keys(user).join(", ") : "user es null/undefined"
        }`
      );
      setLoading(false);
      return;
    }

    setLoading(true);
    setErrorCarga(null);
    try {
      const res = await api.get(`/mantenimientos/empleado/${empleadoId}`);
      setMantenimientos(res.data);
    } catch (e) {
      console.error("Error cargando mantenimientos:", e);
      setErrorCarga(
        e.response?.status === 401
          ? "No autorizado. Verificá que tu sesión esté activa."
          : `Error ${e.response?.status ?? "de red"}: ${e.response?.data?.mensaje ?? e.message}`
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { cargar(); }, [empleadoId]);

  // ── Filtrado ────────────────────────────────────────────────────────────
  const filtrados = mantenimientos.filter((m) => {
    const q = searchTerm.toLowerCase();
    const coincide =
      m.vehiculoPatente?.toLowerCase().includes(q) ||
      m.vehiculoMarca?.toLowerCase().includes(q)   ||
      m.vehiculoModelo?.toLowerCase().includes(q);
    if (filtroEstado === "todos") return coincide;
    return coincide && m.estado === filtroEstado;
  });

  const counts = {
    todos:      mantenimientos.length,
    pendiente:  mantenimientos.filter(m => m.estado === "pendiente").length,
    iniciado:   mantenimientos.filter(m => m.estado === "iniciado").length,
    finalizado: mantenimientos.filter(m => m.estado === "finalizado").length,
    cancelado:  mantenimientos.filter(m => m.estado === "cancelado").length,
  };

  // ── Acciones ────────────────────────────────────────────────────────────
  const confirmarIniciar = async () => {
    setAccionLoading(true);
    setErrorAccion(null);
    try {
      await api.patch(
        `/mantenimientos/${modalIniciar.id}/iniciar?empleadoId=${empleadoId}`,
        {}
      );
      setModalIniciar({ open: false, id: null });
      await cargar();
    } catch (e) {
      setErrorAccion(e.response?.data?.mensaje || "Error al iniciar el mantenimiento.");
    } finally {
      setAccionLoading(false);
    }
  };

  const abrirFinalizar = (mant) => {
    setFormFinalizar({ descripcion: mant.descripcion, fechaRealizacion: hoy(), costo: "", realizadoPor: "" });
    setErrorAccion(null);
    setModalFinalizar({ open: true, mant });
  };

  const confirmarFinalizar = async () => {
    if (!formFinalizar.descripcion.trim())   return setErrorAccion("La descripción es obligatoria.");
    if (!formFinalizar.realizadoPor.trim())  return setErrorAccion("El campo 'Realizado por' es obligatorio.");
    if (formFinalizar.costo === "" || Number(formFinalizar.costo) < 0)
      return setErrorAccion("El costo debe ser un número igual o mayor a 0.");

    setAccionLoading(true);
    setErrorAccion(null);
    try {
      await api.patch(
        `/mantenimientos/${modalFinalizar.mant.idMantenimiento}/finalizar?empleadoId=${empleadoId}`,
        {
          descripcion:      formFinalizar.descripcion,
          fechaRealizacion: formFinalizar.fechaRealizacion,
          costo:            parseFloat(formFinalizar.costo),
          realizadoPor:     formFinalizar.realizadoPor,
        }
      );
      setModalFinalizar({ open: false, mant: null });
      await cargar();
    } catch (e) {
      setErrorAccion(e.response?.data?.mensaje || "Error al finalizar el mantenimiento.");
    } finally {
      setAccionLoading(false);
    }
  };

  const abrirCancelar = (mant) => {
    setFormCancelar({ descripcion: mant.descripcion });
    setErrorAccion(null);
    setModalCancelar({ open: true, mant });
  };

  const confirmarCancelar = async () => {
    if (!formCancelar.descripcion.trim()) return setErrorAccion("La descripción / motivo es obligatoria.");

    setAccionLoading(true);
    setErrorAccion(null);
    try {
      await api.patch(
        `/mantenimientos/${modalCancelar.mant.idMantenimiento}/cancelar?empleadoId=${empleadoId}`,
        { descripcion: formCancelar.descripcion }
      );
      setModalCancelar({ open: false, mant: null });
      await cargar();
    } catch (e) {
      setErrorAccion(e.response?.data?.mensaje || "Error al cancelar el mantenimiento.");
    } finally {
      setAccionLoading(false);
    }
  };

  // ── Render ───────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen bg-slate-50">

      {/* Header */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-white">
        <div className="max-w-7xl mx-auto px-6 py-8">
          <div className="flex items-center gap-4">
            <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center">
              <Wrench className="h-7 w-7 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Mis Mantenimientos</h1>
              <p className="text-slate-300 text-sm mt-0.5">
                Órdenes asignadas a vos · Gestioná el estado de cada una
              </p>
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6">
            {[
              { label: "Total",       value: counts.todos,      color: "text-white"       },
              { label: "Pendientes",  value: counts.pendiente,  color: "text-slate-300"   },
              { label: "Iniciados",   value: counts.iniciado,   color: "text-blue-300"    },
              { label: "Finalizados", value: counts.finalizado, color: "text-emerald-300" },
            ].map((s) => (
              <div key={s.label} className="bg-white/10 rounded-xl px-4 py-3 border border-white/10">
                <p className={`text-2xl font-bold ${s.color}`}>{s.value}</p>
                <p className="text-slate-400 text-xs mt-0.5">{s.label}</p>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-6 py-8">

        {/* Error de carga — incluye diagnóstico del objeto user */}
        {errorCarga && (
          <div className="bg-red-50 border border-red-200 rounded-2xl px-5 py-4 mb-6 flex items-start gap-3">
            <AlertTriangle className="h-5 w-5 text-red-500 shrink-0 mt-0.5" />
            <div>
              <p className="text-sm font-semibold text-red-700">Error al cargar los mantenimientos</p>
              <p className="text-sm text-red-600 mt-0.5 font-mono">{errorCarga}</p>
            </div>
          </div>
        )}

        {/* Buscador + Filtros */}
        <div className="bg-white rounded-2xl border border-slate-200 p-4 mb-6 flex flex-col md:flex-row gap-4 items-start md:items-center">
          <div className="relative flex-1 w-full">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Buscar por patente, marca o modelo..."
              className="w-full pl-9 pr-4 py-2.5 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex items-center gap-2 flex-wrap">
            <Filter className="h-4 w-4 text-slate-400 shrink-0" />
            {["todos", "pendiente", "iniciado", "finalizado", "cancelado"].map((f) => {
              const cfg = f === "todos" ? null : ESTADO_CONFIG[f];
              return (
                <button
                  key={f}
                  onClick={() => setFiltroEstado(f)}
                  className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-all capitalize ${
                    filtroEstado === f
                      ? "bg-slate-900 text-white"
                      : "bg-slate-100 text-slate-600 hover:bg-slate-200"
                  }`}
                >
                  {cfg?.label ?? "Todos"}
                  <span className={`ml-1.5 px-1.5 py-0.5 rounded-full text-[10px] font-bold ${
                    filtroEstado === f ? "bg-white/20 text-white" : "bg-slate-200 text-slate-500"
                  }`}>
                    {counts[f]}
                  </span>
                </button>
              );
            })}
          </div>
        </div>

        {/* Cards */}
        {loading ? (
          <div className="flex items-center justify-center py-24">
            <div className="flex flex-col items-center gap-3 text-slate-400">
              <Wrench className="h-10 w-10 animate-pulse" />
              <p className="text-sm">Cargando mantenimientos...</p>
            </div>
          </div>
        ) : filtrados.length === 0 && !errorCarga ? (
          <div className="bg-white rounded-2xl border border-slate-200 py-20 text-center">
            <AlertTriangle className="mx-auto h-12 w-12 text-slate-300 mb-3" />
            <p className="text-slate-500 font-medium">No hay mantenimientos asignados</p>
            <p className="text-slate-400 text-sm mt-1">
              {searchTerm
                ? "Probá cambiando los términos de búsqueda"
                : "El administrador aún no te asignó órdenes"}
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {filtrados.map((m) => {
              const estadoCfg    = ESTADO_CONFIG[m.estado]        || ESTADO_CONFIG.pendiente;
              const prioridadCfg = PRIORIDAD_CONFIG[m.prioridad]  || PRIORIDAD_CONFIG.media;
              const terminal     = ESTADOS_TERMINALES.includes(m.estado);
              const isExpandido  = expandido === m.idMantenimiento;

              return (
                <div
                  key={m.idMantenimiento}
                  className={`bg-white rounded-2xl border overflow-hidden transition-all ${
                    m.estado === "iniciado"
                      ? "border-blue-200 shadow-sm shadow-blue-100"
                      : m.estado === "finalizado"
                        ? "border-emerald-200"
                        : m.estado === "cancelado"
                          ? "border-red-100"
                          : "border-slate-200"
                  }`}
                >
                  {/* Banner de estado */}
                  <div className={`flex items-center justify-between px-4 py-2 border-b ${estadoCfg.bg} ${estadoCfg.border}`}>
                    <span className={`flex items-center gap-1.5 text-xs font-bold ${estadoCfg.text}`}>
                      <span className={`h-1.5 w-1.5 rounded-full ${estadoCfg.dot}`} />
                      {estadoCfg.label}
                    </span>
                    <span className={`text-[10px] font-bold px-2 py-0.5 rounded-lg border ${prioridadCfg.classes}`}>
                      {prioridadCfg.label}
                    </span>
                  </div>

                  {/* Body */}
                  <div className="px-5 pt-4 pb-3">
                    <div className="flex items-center gap-3 mb-3">
                      <div className="h-10 w-10 rounded-xl bg-slate-100 flex items-center justify-center shrink-0">
                        <Car className="h-5 w-5 text-slate-500" />
                      </div>
                      <div>
                        <p className="font-bold text-slate-900 text-sm leading-tight">
                          {m.vehiculoMarca} {m.vehiculoModelo}
                        </p>
                        <span className="font-mono text-xs bg-slate-100 text-slate-600 px-1.5 py-0.5 rounded border border-slate-200">
                          {m.vehiculoPatente}
                        </span>
                      </div>
                    </div>

                    <div className="space-y-1.5 mb-3">
                      <div className="flex items-center gap-1.5 text-xs text-slate-500">
                        <Wrench className="h-3.5 w-3.5 shrink-0" />
                        <span>Tipo: <span className="font-medium text-slate-700 capitalize">{m.tipo}</span></span>
                      </div>
                      {m.fechaProgramada && (
                        <div className="flex items-center gap-1.5 text-xs text-slate-500">
                          <Calendar className="h-3.5 w-3.5 shrink-0" />
                          <span>Programado: <span className="font-medium text-slate-700">{m.fechaProgramada}</span></span>
                        </div>
                      )}
                      {m.fechaRealizacion && (
                        <div className="flex items-center gap-1.5 text-xs text-emerald-600">
                          <CheckCircle2 className="h-3.5 w-3.5 shrink-0" />
                          <span>Realizado: <span className="font-medium">{m.fechaRealizacion}</span></span>
                        </div>
                      )}
                    </div>

                    {/* Toggle descripción */}
                    <button
                      onClick={() => setExpandido(isExpandido ? null : m.idMantenimiento)}
                      className="flex items-center gap-1 text-xs text-slate-400 hover:text-slate-600 transition-colors mb-1"
                    >
                      {isExpandido ? <ChevronUp className="h-3.5 w-3.5" /> : <ChevronDown className="h-3.5 w-3.5" />}
                      {isExpandido ? "Ocultar descripción" : "Ver descripción"}
                    </button>

                    {isExpandido && (
                      <p className="text-xs text-slate-600 bg-slate-50 rounded-xl px-3 py-2.5 border border-slate-100 leading-relaxed mb-2">
                        {m.descripcion || <span className="italic text-slate-400">Sin descripción.</span>}
                      </p>
                    )}
                  </div>

                  {/* Acciones */}
                  <div className="px-5 pb-5 space-y-2">
                    {terminal ? (
                      <button
                        onClick={() => setModalDetalle({ open: true, mant: m })}
                        className="flex items-center justify-center gap-2 w-full py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-600 text-sm font-semibold rounded-xl transition-all"
                      >
                        <Eye className="h-4 w-4" />
                        Ver Detalle
                      </button>
                    ) : m.estado === "pendiente" ? (
                      <button
                        onClick={() => { setErrorAccion(null); setModalIniciar({ open: true, id: m.idMantenimiento }); }}
                        className="flex items-center justify-center gap-2 w-full py-2.5 bg-slate-900 hover:bg-slate-700 text-white text-sm font-semibold rounded-xl transition-all"
                      >
                        <Play className="h-4 w-4" />
                        Iniciar Mantenimiento
                      </button>
                    ) : m.estado === "iniciado" ? (
                      <>
                        <button
                          onClick={() => abrirFinalizar(m)}
                          className="flex items-center justify-center gap-2 w-full py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold rounded-xl transition-all"
                        >
                          <CheckCircle2 className="h-4 w-4" />
                          Finalizar Mantenimiento
                        </button>
                        <button
                          onClick={() => abrirCancelar(m)}
                          className="flex items-center justify-center gap-2 w-full py-2.5 bg-white hover:bg-red-50 text-red-500 border border-red-200 hover:border-red-300 text-sm font-semibold rounded-xl transition-all"
                        >
                          <XCircle className="h-4 w-4" />
                          Cancelar Mantenimiento
                        </button>
                      </>
                    ) : (
                      <div className="flex items-center justify-center gap-2 w-full py-2.5 bg-slate-100 text-slate-400 text-sm font-semibold rounded-xl cursor-not-allowed">
                        <Ban className="h-4 w-4" />
                        Sin acciones disponibles
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* ── MODAL: Confirmar Inicio ─────────────────────────────────────────── */}
      {modalIniciar.open && (
        <Modal onClose={() => setModalIniciar({ open: false, id: null })}>
          <div className="flex items-start gap-4 mb-5">
            <div className="bg-slate-100 p-3 rounded-full shrink-0">
              <Play className="h-6 w-6 text-slate-700" />
            </div>
            <div>
              <h3 className="text-lg font-bold text-slate-900">¿Iniciar mantenimiento?</h3>
              <p className="text-sm text-slate-500 mt-1">
                Al confirmar, el mantenimiento pasará a estado{" "}
                <span className="font-semibold text-blue-600">Iniciado</span>.
                Esta acción no se puede deshacer.
              </p>
            </div>
          </div>
          {errorAccion && <ErrorBanner mensaje={errorAccion} />}
          <ModalFooter
            onCancel={() => setModalIniciar({ open: false, id: null })}
            onConfirm={confirmarIniciar}
            loading={accionLoading}
            confirmLabel="Confirmar inicio"
            confirmClass="bg-slate-900 hover:bg-slate-700 text-white"
          />
        </Modal>
      )}

      {/* ── MODAL: Finalizar ───────────────────────────────────────────────── */}
      {modalFinalizar.open && (
        <Modal onClose={() => setModalFinalizar({ open: false, mant: null })} wide>
          <div className="flex items-start gap-4 mb-5">
            <div className="bg-emerald-100 p-3 rounded-full shrink-0">
              <CheckCircle2 className="h-6 w-6 text-emerald-600" />
            </div>
            <div>
              <h3 className="text-lg font-bold text-slate-900">Finalizar mantenimiento</h3>
              <p className="text-sm text-slate-500 mt-0.5">Completá los datos del trabajo realizado.</p>
            </div>
          </div>

          <div className="space-y-4">
            <div className="space-y-1.5">
              <label className="text-sm font-medium text-slate-700">
                Descripción <span className="text-red-500">*</span>
              </label>
              <textarea
                rows={3}
                value={formFinalizar.descripcion}
                onChange={(e) => setFormFinalizar(p => ({ ...p, descripcion: e.target.value }))}
                className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 resize-none transition-all"
                placeholder="Describí el trabajo realizado..."
              />
            </div>

            <div className="space-y-1.5">
              <label className="text-sm font-medium text-slate-700">
                Fecha de realización <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                <input
                  type="date"
                  value={formFinalizar.fechaRealizacion}
                  min={modalFinalizar.mant?.fechaProgramada || undefined}
                  onChange={(e) => setFormFinalizar(p => ({ ...p, fechaRealizacion: e.target.value }))}
                  className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
                />
              </div>
              {modalFinalizar.mant?.fechaProgramada && (
                <p className="text-xs text-slate-400 flex items-center gap-1">
                  <Info className="h-3.5 w-3.5" />
                  No puede ser anterior a la fecha programada ({modalFinalizar.mant.fechaProgramada})
                </p>
              )}
            </div>

            <div className="space-y-1.5">
              <label className="text-sm font-medium text-slate-700">
                Costo <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <DollarSign className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={formFinalizar.costo}
                  onChange={(e) => setFormFinalizar(p => ({ ...p, costo: e.target.value }))}
                  className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
                  placeholder="0.00"
                />
              </div>
            </div>

            <div className="space-y-1.5">
              <label className="text-sm font-medium text-slate-700">
                Realizado por <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <User className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                <input
                  type="text"
                  value={formFinalizar.realizadoPor}
                  onChange={(e) => setFormFinalizar(p => ({ ...p, realizadoPor: e.target.value }))}
                  maxLength={20}
                  className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
                  placeholder="Nombre del técnico o taller..."
                />
              </div>
              <p className="text-xs text-slate-400">Máximo 20 caracteres.</p>
            </div>
          </div>

          {errorAccion && <ErrorBanner mensaje={errorAccion} className="mt-4" />}
          <ModalFooter
            onCancel={() => setModalFinalizar({ open: false, mant: null })}
            onConfirm={confirmarFinalizar}
            loading={accionLoading}
            confirmLabel="Finalizar"
            confirmClass="bg-emerald-600 hover:bg-emerald-700 text-white"
          />
        </Modal>
      )}

      {/* ── MODAL: Cancelar ────────────────────────────────────────────────── */}
      {modalCancelar.open && (
        <Modal onClose={() => setModalCancelar({ open: false, mant: null })} wide>
          <div className="flex items-start gap-4 mb-5">
            <div className="bg-red-100 p-3 rounded-full shrink-0">
              <XCircle className="h-6 w-6 text-red-600" />
            </div>
            <div>
              <h3 className="text-lg font-bold text-slate-900">Cancelar mantenimiento</h3>
              <p className="text-sm text-slate-500 mt-0.5">
                Podés editar la descripción para agregar el motivo de cancelación.
              </p>
            </div>
          </div>

          <div className="space-y-1.5">
            <label className="text-sm font-medium text-slate-700">
              Descripción / Motivo <span className="text-red-500">*</span>
            </label>
            <textarea
              rows={4}
              value={formCancelar.descripcion}
              onChange={(e) => setFormCancelar({ descripcion: e.target.value })}
              className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 resize-none transition-all"
              placeholder="Añadí el motivo de cancelación al final del texto..."
            />
            <p className="text-xs text-slate-400 flex items-center gap-1">
              <Info className="h-3.5 w-3.5" />
              El texto original del administrador está cargado. Podés editarlo o agregar el motivo al final.
            </p>
          </div>

          {errorAccion && <ErrorBanner mensaje={errorAccion} className="mt-4" />}
          <ModalFooter
            onCancel={() => setModalCancelar({ open: false, mant: null })}
            onConfirm={confirmarCancelar}
            loading={accionLoading}
            confirmLabel="Confirmar cancelación"
            confirmClass="bg-red-600 hover:bg-red-700 text-white"
          />
        </Modal>
      )}

      {/* ── MODAL: Ver Detalle ──────────────────────────────────────────────── */}
      {modalDetalle.open && modalDetalle.mant && (
        <Modal onClose={() => setModalDetalle({ open: false, mant: null })} wide>
          {(() => {
            const m    = modalDetalle.mant;
            const eCfg = ESTADO_CONFIG[m.estado]       || ESTADO_CONFIG.pendiente;
            const pCfg = PRIORIDAD_CONFIG[m.prioridad] || PRIORIDAD_CONFIG.media;
            return (
              <>
                <div className="flex items-start justify-between mb-4">
                  <div className="flex items-center gap-3">
                    <div className="h-11 w-11 rounded-xl bg-slate-100 flex items-center justify-center shrink-0">
                      <Car className="h-6 w-6 text-slate-500" />
                    </div>
                    <div>
                      <h3 className="font-bold text-slate-900">{m.vehiculoMarca} {m.vehiculoModelo}</h3>
                      <span className="font-mono text-xs bg-slate-100 text-slate-600 px-1.5 py-0.5 rounded border border-slate-200">
                        {m.vehiculoPatente}
                      </span>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 flex-wrap justify-end">
                    <span className={`text-xs font-bold px-2 py-1 rounded-lg border ${pCfg.classes}`}>
                      {pCfg.label}
                    </span>
                    <span className={`flex items-center gap-1 text-xs font-bold px-2 py-1 rounded-lg ${eCfg.bg} ${eCfg.text} border ${eCfg.border}`}>
                      <span className={`h-1.5 w-1.5 rounded-full ${eCfg.dot}`} />
                      {eCfg.label}
                    </span>
                  </div>
                </div>

                <div className="space-y-3 text-sm">
                  <DetalleRow icon={Wrench}      label="Tipo"          value={m.tipo}              capitalize />
                  <DetalleRow icon={User}         label="Realizado por" value={m.realizadoPor || "—"} />
                  <DetalleRow icon={Calendar}     label="Programado"    value={m.fechaProgramada  || "—"} />
                  <DetalleRow icon={CheckCircle2} label="Realizado"     value={m.fechaRealizacion || "—"} />
                  <DetalleRow icon={DollarSign}   label="Costo"         value={
                    m.costo != null
                      ? `$${Number(m.costo).toLocaleString("es-AR", { minimumFractionDigits: 2 })}`
                      : "—"
                  } />
                  <div className="pt-2 border-t border-slate-100">
                    <p className="text-xs font-medium text-slate-500 mb-1.5">Descripción</p>
                    <p className="text-sm text-slate-700 bg-slate-50 rounded-xl px-3 py-2.5 border border-slate-100 leading-relaxed">
                      {m.descripcion || <span className="italic text-slate-400">Sin descripción.</span>}
                    </p>
                  </div>
                </div>

                <div className="flex justify-end mt-6 pt-4 border-t border-slate-100">
                  <button
                    onClick={() => setModalDetalle({ open: false, mant: null })}
                    className="px-5 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
                  >
                    Cerrar
                  </button>
                </div>
              </>
            );
          })()}
        </Modal>
      )}
    </div>
  );
}

// ── Sub-componentes ──────────────────────────────────────────────────────────

function Modal({ children, onClose, wide = false }) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
      <div className={`bg-white rounded-2xl shadow-2xl w-full ${wide ? "max-w-lg" : "max-w-md"} max-h-[90vh] overflow-y-auto`}>
        <div className="p-6">
          <div className="flex justify-end mb-1 -mt-1">
            <button onClick={onClose} className="text-slate-400 hover:text-slate-600 transition-colors">
              <X className="h-5 w-5" />
            </button>
          </div>
          {children}
        </div>
      </div>
    </div>
  );
}

function ModalFooter({ onCancel, onConfirm, loading, confirmLabel, confirmClass }) {
  return (
    <div className="flex justify-end gap-3 mt-6 pt-4 border-t border-slate-100">
      <button
        onClick={onCancel}
        disabled={loading}
        className="px-5 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors disabled:opacity-50"
      >
        Cancelar
      </button>
      <button
        onClick={onConfirm}
        disabled={loading}
        className={`inline-flex items-center gap-2 px-5 py-2.5 text-sm font-semibold rounded-xl shadow-md transition-all disabled:opacity-60 ${confirmClass}`}
      >
        {loading && <Loader2 className="h-4 w-4 animate-spin" />}
        {confirmLabel}
      </button>
    </div>
  );
}

function ErrorBanner({ mensaje, className = "" }) {
  return (
    <div className={`flex items-start gap-2 bg-red-50 border border-red-200 rounded-xl px-4 py-3 ${className}`}>
      <AlertTriangle className="h-4 w-4 text-red-500 shrink-0 mt-0.5" />
      <p className="text-sm text-red-700">{mensaje}</p>
    </div>
  );
}

function DetalleRow({ icon: Icon, label, value, capitalize = false }) {
  return (
    <div className="flex items-center gap-3">
      <div className="h-7 w-7 rounded-lg bg-slate-100 flex items-center justify-center shrink-0">
        <Icon className="h-3.5 w-3.5 text-slate-500" />
      </div>
      <span className="text-slate-500 w-28 shrink-0">{label}</span>
      <span className={`font-medium text-slate-800 ${capitalize ? "capitalize" : ""}`}>{value}</span>
    </div>
  );
}