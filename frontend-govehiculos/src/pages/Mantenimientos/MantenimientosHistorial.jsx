import { useEffect, useState, useCallback } from "react";
import api from "../../api/axiosConfig";
import { useNavigate } from "react-router-dom";
import {
  ArrowLeft,
  ClipboardList,
  Search,
  Filter,
  CheckCircle2,
  XCircle,
  ChevronDown,
  ChevronUp,
  Car,
  User,
  Calendar,
  DollarSign,
  Wrench,
  Clock,
  AlertTriangle,
  CheckCheck,
  Loader2,
  X,
  Info,
} from "lucide-react";

// ── Config visual ──────────────────────────────────────────────────────────
const ESTADO_MANT_CONFIG = {
  finalizado: {
    label:   "Finalizado",
    bg:      "bg-emerald-50",
    text:    "text-emerald-700",
    border:  "border-emerald-200",
    dot:     "bg-emerald-500",
    icon:    CheckCircle2,
  },
  cancelado: {
    label:   "Cancelado",
    bg:      "bg-red-50",
    text:    "text-red-700",
    border:  "border-red-200",
    dot:     "bg-red-400",
    icon:    XCircle,
  },
};

const PRIORIDAD_CONFIG = {
  baja:    { label: "Baja",    classes: "text-emerald-700 bg-emerald-50 border-emerald-200" },
  media:   { label: "Media",   classes: "text-amber-700   bg-amber-50   border-amber-200"   },
  alta:    { label: "Alta",    classes: "text-orange-700  bg-orange-50  border-orange-200"  },
  critica: { label: "Crítica", classes: "text-red-700     bg-red-50     border-red-200"     },
};

// ── Componente principal ───────────────────────────────────────────────────
export default function MantenimientosHistorial() {
  const navigate = useNavigate();

  const [ordenes, setOrdenes]           = useState([]);
  const [loading, setLoading]           = useState(true);
  const [searchTerm, setSearchTerm]     = useState("");
  const [filtroEstado, setFiltroEstado] = useState("todos");

  // Expandido — id del mantenimiento con detalle visible
  const [expandido, setExpandido] = useState(null);

  // Disponibilizar
  const [disponibilizando, setDisponibilizando] = useState(null); // id del mant en proceso
  const [modalConfirm, setModalConfirm]         = useState({ open: false, mant: null });
  const [errorDisp, setErrorDisp]               = useState(null);

  // ── Carga ──────────────────────────────────────────────────────────────
  const cargar = useCallback(async () => {
    setLoading(true);
    try {
      // Trae finalizados y cancelados en una sola llamada sin filtro
      // El filtro por estado lo hacemos en cliente para poder cambiar rápido entre tabs
      const res = await api.get("/mantenimientos?estado=finalizado");
      const res2 = await api.get("/mantenimientos?estado=cancelado");
      setOrdenes([...res.data, ...res2.data]);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  // ── Filtrado ────────────────────────────────────────────────────────────
  const filtradas = ordenes.filter((m) => {
    const q = searchTerm.toLowerCase();
    const coincide =
      m.vehiculoPatente?.toLowerCase().includes(q) ||
      m.vehiculoMarca?.toLowerCase().includes(q)   ||
      m.vehiculoModelo?.toLowerCase().includes(q)  ||
      m.empleadoNombre?.toLowerCase().includes(q);

    if (filtroEstado === "todos")      return coincide;
    return coincide && m.estado === filtroEstado;
  });

  const counts = {
    todos:      ordenes.length,
    finalizado: ordenes.filter(m => m.estado === "finalizado").length,
    cancelado:  ordenes.filter(m => m.estado === "cancelado").length,
  };

  // ── Disponibilizar ──────────────────────────────────────────────────────
  const confirmarDisponibilizar = async () => {
    const mant = modalConfirm.mant;
    setModalConfirm({ open: false, mant: null });
    setDisponibilizando(mant.idMantenimiento);
    setErrorDisp(null);
    try {
      await api.patch(`/mantenimientos/${mant.idMantenimiento}/disponibilizar`);
      // Actualizar localmente el vehiculoEstado para deshabilitar el botón sin recargar todo
      setOrdenes(prev =>
        prev.map(o =>
          o.idMantenimiento === mant.idMantenimiento
            ? { ...o, vehiculoEstado: "disponible" }
            : o
        )
      );
    } catch (e) {
      setErrorDisp(e.response?.data?.mensaje || "Error al disponibilizar el vehículo.");
    } finally {
      setDisponibilizando(null);
    }
  };

  // ── Render ─────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen bg-slate-50">

      {/* Header */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-white">
        <div className="max-w-7xl mx-auto px-6 py-8">
          {/* Botón volver */}
          <button
            onClick={() => navigate("/mantenimientos")}
            className="inline-flex items-center gap-2 text-slate-300 hover:text-white text-sm font-medium mb-5 transition-colors group"
          >
            <ArrowLeft className="h-4 w-4 group-hover:-translate-x-0.5 transition-transform" />
            Volver a mantenimientos
          </button>

          <div className="flex items-center gap-4">
            <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center">
              <ClipboardList className="h-7 w-7 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Historial de Órdenes</h1>
              <p className="text-slate-300 text-sm mt-0.5">
                Órdenes de mantenimiento finalizadas y canceladas
              </p>
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-3 gap-4 mt-6">
            {[
              { label: "Total",       value: counts.todos,      color: "text-white"       },
              { label: "Finalizadas", value: counts.finalizado, color: "text-emerald-300" },
              { label: "Canceladas",  value: counts.cancelado,  color: "text-red-300"     },
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

        {/* Error global de disponibilizar */}
        {errorDisp && (
          <div className="flex items-start gap-3 bg-red-50 border border-red-200 rounded-2xl px-5 py-4 mb-6">
            <AlertTriangle className="h-5 w-5 text-red-500 shrink-0 mt-0.5" />
            <div className="flex-1">
              <p className="text-sm font-semibold text-red-700">Error al disponibilizar</p>
              <p className="text-sm text-red-600 mt-0.5">{errorDisp}</p>
            </div>
            <button onClick={() => setErrorDisp(null)} className="text-red-400 hover:text-red-600">
              <X className="h-4 w-4" />
            </button>
          </div>
        )}

        {/* Buscador + Filtros */}
        <div className="bg-white rounded-2xl border border-slate-200 p-4 mb-6 flex flex-col md:flex-row gap-4 items-start md:items-center">
          <div className="relative flex-1 w-full">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Buscar por patente, marca, modelo o empleado..."
              className="w-full pl-9 pr-4 py-2.5 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex items-center gap-2 flex-wrap">
            <Filter className="h-4 w-4 text-slate-400 shrink-0" />
            {[
              { key: "todos",      label: "Todos"       },
              { key: "finalizado", label: "Finalizados" },
              { key: "cancelado",  label: "Cancelados"  },
            ].map((f) => (
              <button
                key={f.key}
                onClick={() => setFiltroEstado(f.key)}
                className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-all ${
                  filtroEstado === f.key
                    ? "bg-slate-900 text-white"
                    : "bg-slate-100 text-slate-600 hover:bg-slate-200"
                }`}
              >
                {f.label}
                <span className={`ml-1.5 px-1.5 py-0.5 rounded-full text-[10px] font-bold ${
                  filtroEstado === f.key ? "bg-white/20 text-white" : "bg-slate-200 text-slate-500"
                }`}>
                  {counts[f.key]}
                </span>
              </button>
            ))}
          </div>
        </div>

        {/* Lista de órdenes */}
        {loading ? (
          <div className="flex items-center justify-center py-24">
            <div className="flex flex-col items-center gap-3 text-slate-400">
              <ClipboardList className="h-10 w-10 animate-pulse" />
              <p className="text-sm">Cargando historial...</p>
            </div>
          </div>
        ) : filtradas.length === 0 ? (
          <div className="bg-white rounded-2xl border border-slate-200 py-20 text-center">
            <AlertTriangle className="mx-auto h-12 w-12 text-slate-300 mb-3" />
            <p className="text-slate-500 font-medium">No hay órdenes registradas</p>
            <p className="text-slate-400 text-sm mt-1">
              {searchTerm ? "Probá cambiando los términos de búsqueda" : "Todavía no hay órdenes finalizadas o canceladas"}
            </p>
          </div>
        ) : (
          <div className="space-y-3">
            {filtradas.map((m) => {
              const estadoCfg    = ESTADO_MANT_CONFIG[m.estado];
              const prioridadCfg = PRIORIDAD_CONFIG[m.prioridad] || PRIORIDAD_CONFIG.media;
              const EstadoIcon   = estadoCfg?.icon;
              const isExpandido  = expandido === m.idMantenimiento;
              const yaDisponible = m.vehiculoEstado === "disponible";
              const esFinalizado = m.estado === "finalizado";
              const enProceso    = disponibilizando === m.idMantenimiento;

              return (
                <div
                  key={m.idMantenimiento}
                  className={`bg-white rounded-2xl border overflow-hidden transition-all ${
                    m.estado === "finalizado"
                      ? "border-emerald-100 hover:border-emerald-200"
                      : "border-red-100 hover:border-red-200"
                  }`}
                >
                  {/* Fila principal */}
                  <div className="px-5 py-4 flex items-center gap-4 flex-wrap">

                    {/* Badge estado */}
                    <span className={`inline-flex items-center gap-1.5 text-xs font-bold px-2.5 py-1 rounded-lg border shrink-0 ${estadoCfg.bg} ${estadoCfg.text} ${estadoCfg.border}`}>
                      <span className={`h-1.5 w-1.5 rounded-full ${estadoCfg.dot}`} />
                      {estadoCfg.label}
                    </span>

                    {/* Vehículo */}
                    <div className="flex items-center gap-2 min-w-0 flex-1">
                      <div className="h-9 w-9 rounded-xl bg-slate-100 flex items-center justify-center shrink-0">
                        <Car className="h-4 w-4 text-slate-500" />
                      </div>
                      <div className="min-w-0">
                        <p className="font-bold text-slate-900 text-sm truncate">
                          {m.vehiculoMarca} {m.vehiculoModelo}
                        </p>
                        <span className="font-mono text-xs bg-slate-100 text-slate-600 px-1.5 py-0.5 rounded border border-slate-200">
                          {m.vehiculoPatente}
                        </span>
                      </div>
                    </div>

                    {/* Tipo + prioridad */}
                    <div className="hidden sm:flex items-center gap-2">
                      <span className="text-xs text-slate-500 capitalize">{m.tipo}</span>
                      <span className={`text-[10px] font-bold px-2 py-0.5 rounded-lg border ${prioridadCfg.classes}`}>
                        {prioridadCfg.label}
                      </span>
                    </div>

                    {/* Empleado */}
                    <div className="hidden md:flex items-center gap-1.5 text-xs text-slate-500 shrink-0">
                      <User className="h-3.5 w-3.5" />
                      <span>{m.empleadoNombre || "—"}</span>
                    </div>

                    {/* Fecha realización */}
                    {m.fechaRealizacion && (
                      <div className="hidden lg:flex items-center gap-1.5 text-xs text-slate-500 shrink-0">
                        <Calendar className="h-3.5 w-3.5" />
                        <span>{m.fechaRealizacion}</span>
                      </div>
                    )}

                    {/* Acciones */}
                    <div className="flex items-center gap-2 ml-auto shrink-0">
                      {/* Botón disponibilizar — solo en finalizados */}
                      {esFinalizado && (
                        <button
                          onClick={() => !yaDisponible && !enProceso && setModalConfirm({ open: true, mant: m })}
                          disabled={yaDisponible || enProceso}
                          title={yaDisponible ? "El vehículo ya está disponible" : "Marcar vehículo como disponible"}
                          className={`inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold rounded-xl border transition-all ${
                            yaDisponible
                              ? "bg-emerald-50 text-emerald-600 border-emerald-200 cursor-not-allowed opacity-70"
                              : enProceso
                                ? "bg-slate-100 text-slate-400 border-slate-200 cursor-not-allowed"
                                : "bg-slate-900 text-white border-slate-900 hover:bg-slate-700 shadow-sm"
                          }`}
                        >
                          {enProceso ? (
                            <Loader2 className="h-3.5 w-3.5 animate-spin" />
                          ) : yaDisponible ? (
                            <CheckCheck className="h-3.5 w-3.5" />
                          ) : (
                            <CheckCircle2 className="h-3.5 w-3.5" />
                          )}
                          {yaDisponible ? "Disponible" : enProceso ? "Procesando..." : "Marcar disponible"}
                        </button>
                      )}

                      {/* Botón expandir detalles */}
                      <button
                        onClick={() => setExpandido(isExpandido ? null : m.idMantenimiento)}
                        className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 bg-slate-100 hover:bg-slate-200 rounded-xl transition-all"
                      >
                        {isExpandido ? <ChevronUp className="h-3.5 w-3.5" /> : <ChevronDown className="h-3.5 w-3.5" />}
                        {isExpandido ? "Ocultar" : "Más detalles"}
                      </button>
                    </div>
                  </div>

                  {/* Panel de detalles expandido */}
                  {isExpandido && (
                    <div className="border-t border-slate-100 bg-slate-50/50 px-5 py-5">
                      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">

                        <DetalleItem icon={Wrench}      label="Tipo"             value={m.tipo} capitalize />
                        <DetalleItem icon={Clock}        label="Estado orden"     value={estadoCfg.label} />
                        <DetalleItem icon={Car}          label="Estado vehículo"  value={m.vehiculoEstado || "—"} />
                        <DetalleItem icon={User}         label="Empleado"         value={m.empleadoNombre || "Sin asignar"} />
                        <DetalleItem icon={User}         label="Realizado por"    value={m.realizadoPor || "—"} />
                        <DetalleItem icon={DollarSign}   label="Costo"            value={
                          m.costo != null
                            ? `$${Number(m.costo).toLocaleString("es-AR", { minimumFractionDigits: 2 })}`
                            : "—"
                        } />
                        <DetalleItem icon={Calendar}     label="Fecha programada"  value={m.fechaProgramada  || "—"} />
                        <DetalleItem icon={Calendar}     label="Fecha realización" value={m.fechaRealizacion || "—"} />
                        <DetalleItem icon={Info}         label="Prioridad"         value={prioridadCfg.label} />

                        {/* Descripción ocupa todo el ancho */}
                        <div className="sm:col-span-2 lg:col-span-3">
                          <p className="text-xs font-medium text-slate-500 mb-1.5">Descripción</p>
                          <p className="text-sm text-slate-700 bg-white rounded-xl px-4 py-3 border border-slate-200 leading-relaxed">
                            {m.descripcion || <span className="italic text-slate-400">Sin descripción.</span>}
                          </p>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* ── MODAL: Confirmar disponibilizar ──────────────────────────────── */}
      {modalConfirm.open && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
          <div className="bg-white rounded-2xl p-6 max-w-md w-full shadow-2xl">
            <div className="flex items-start gap-4 mb-5">
              <div className="bg-emerald-100 p-3 rounded-full shrink-0">
                <CheckCircle2 className="h-6 w-6 text-emerald-600" />
              </div>
              <div>
                <h3 className="text-lg font-bold text-slate-900">¿Marcar vehículo como disponible?</h3>
                <p className="text-sm text-slate-500 mt-1">
                  El vehículo{" "}
                  <span className="font-semibold text-slate-700">
                    {modalConfirm.mant?.vehiculoMarca} {modalConfirm.mant?.vehiculoModelo} · {modalConfirm.mant?.vehiculoPatente}
                  </span>{" "}
                  pasará a estado <span className="font-semibold text-emerald-600">disponible</span> y
                  podrá ser reservado por los clientes.
                </p>
                <p className="text-xs text-slate-400 mt-2 flex items-center gap-1">
                  <Info className="h-3.5 w-3.5 shrink-0" />
                  Esta acción no se puede deshacer desde esta pantalla.
                </p>
              </div>
            </div>
            <div className="flex justify-end gap-3 pt-4 border-t border-slate-100">
              <button
                onClick={() => setModalConfirm({ open: false, mant: null })}
                className="px-5 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
              >
                Cancelar
              </button>
              <button
                onClick={confirmarDisponibilizar}
                className="px-5 py-2.5 text-sm font-semibold text-white bg-emerald-600 hover:bg-emerald-700 rounded-xl shadow-md transition-colors"
              >
                Confirmar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ── Sub-componente ─────────────────────────────────────────────────────────
function DetalleItem({ icon: Icon, label, value, capitalize = false }) {
  return (
    <div className="flex items-start gap-3">
      <div className="h-7 w-7 rounded-lg bg-white border border-slate-200 flex items-center justify-center shrink-0 mt-0.5">
        <Icon className="h-3.5 w-3.5 text-slate-400" />
      </div>
      <div>
        <p className="text-xs text-slate-400 font-medium">{label}</p>
        <p className={`text-sm font-semibold text-slate-800 mt-0.5 ${capitalize ? "capitalize" : ""}`}>
          {value}
        </p>
      </div>
    </div>
  );
}
