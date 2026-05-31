import { useEffect, useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import {
  Scale,
  Plus,
  Search,
  Filter,
  AlertTriangle,
  CheckCircle2,
  Ban,
  Clock,
  ChevronDown,
  RefreshCw,
  Loader2,
  X,
  FileWarning,
  Car,
  User,
  DollarSign,
  ShieldAlert,
  Info,
} from "lucide-react";
import api from "../../api/axiosConfig";

// ── Config visual ───────────────────────────────────────────────────────────

const ESTADO_MULTA_CONFIG = {
  pendiente: {
    label: "Pendiente",
    classes: "bg-amber-50 text-amber-700 border border-amber-200",
    dot: "bg-amber-400",
    icon: Clock,
  },
  pagada: {
    label: "Pagada",
    classes: "bg-emerald-50 text-emerald-700 border border-emerald-200",
    dot: "bg-emerald-500",
    icon: CheckCircle2,
  },
  cancelada: {
    label: "Cancelada",
    classes: "bg-slate-100 text-slate-500 border border-slate-200",
    dot: "bg-slate-400",
    icon: Ban,
  },
};

const TIPO_INCIDENCIA_CONFIG = {
  daño_fisico: {
    label: "Daño físico",
    classes: "bg-red-50    text-red-700   border border-red-200",
  },
  accidente: {
    label: "Accidente",
    classes: "bg-orange-50 text-orange-700 border border-orange-200",
  },
  infraccion_vial: {
    label: "Infracción vial",
    classes: "bg-amber-50  text-amber-700 border border-amber-200",
  },
  comportamiento_indebido: {
    label: "Comp. indebido",
    classes: "bg-purple-50 text-purple-700 border border-purple-200",
  },
  retraso_en_pago: {
    label: "Retraso en pago",
    classes: "bg-blue-50   text-blue-700  border border-blue-200",
  },
};

const GRAVEDAD_CONFIG = {
  baja: {
    label: "Baja",
    classes: "text-emerald-600 bg-emerald-50 border-emerald-200",
  },
  media: {
    label: "Media",
    classes: "text-amber-600   bg-amber-50   border-amber-200",
  },
  alta: {
    label: "Alta",
    classes: "text-red-600     bg-red-50     border-red-200",
  },
};

const TIPO_PENALIZACION_CONFIG = {
  suspension_temporal: { label: "Suspensión temporal" },
  bloqueo_cuenta: { label: "Bloqueo de cuenta" },
  inhabilitacion_vehiculo: { label: "Inhabilitación veh." },
  advertencia: { label: "Advertencia" },
};

const TIPO_MULTA_CONFIG = {
  economica: { label: "Económica" },
  administrativa: { label: "Administrativa" },
  mixta: { label: "Mixta" },
};

// ── Helpers ─────────────────────────────────────────────────────────────────

function BadgeEstado({ estado }) {
  const estadoNormalizado = estado?.toLowerCase();
  const cfg = ESTADO_MULTA_CONFIG[estadoNormalizado] ?? {
    label: estado,
    classes: "bg-slate-100 text-slate-600 border border-slate-200",
    dot: "bg-slate-400",
    icon: Info,
  };
  const Icon = cfg.icon;
  return (
    <span
      className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-semibold ${cfg.classes}`}
    >
      <span className={`h-1.5 w-1.5 rounded-full ${cfg.dot}`} />
      {cfg.label}
    </span>
  );
}

function BadgeTipoIncidencia({ tipo }) {
  const tipoNormalizado = tipo?.toLowerCase();
  const cfg = TIPO_INCIDENCIA_CONFIG[tipoNormalizado] ?? {
    label: tipo,
    classes: "bg-slate-100 text-slate-600 border border-slate-200",
  };
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium ${cfg.classes}`}
    >
      {cfg.label}
    </span>
  );
}

function BadgeGravedad({ nivel }) {
  const nivelNormalizado = nivel?.toLowerCase();
  const cfg = GRAVEDAD_CONFIG[nivelNormalizado] ?? {
    label: nivel,
    classes: "text-slate-600 bg-slate-100 border-slate-200",
  };
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded-md text-xs font-semibold border ${cfg.classes}`}
    >
      {cfg.label}
    </span>
  );
}

function formatFecha(fechaStr) {
  if (!fechaStr) return "—";
  return new Date(fechaStr).toLocaleDateString("es-AR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

// Corregido para manejar strings o números nulos
function formatMonto(monto) {
  const num = Number(monto);
  if (!num || num === 0) return "$0";
  return new Intl.NumberFormat("es-AR", {
    style: "currency",
    currency: "ARS",
    maximumFractionDigits: 0,
  }).format(num);
}

// ── Componente principal ────────────────────────────────────────────────────

export default function MultasList() {
  const navigate = useNavigate();

  const [multas, setMultas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState(null);

  // Filtros
  const [searchTerm, setSearchTerm] = useState("");
  const [filtroEstado, setFiltroEstado] = useState("todos");
  const [filtroTipoInc, setFiltroTipoInc] = useState("todos");
  const [filtroGravedad, setFiltroGravedad] = useState("todos");
  const [filtroTipoMulta, setFiltroTipoMulta] = useState("todos");
  const [mostrarFiltros, setMostrarFiltros] = useState(false);

  // Tooltip cancelada
  const [tooltipId, setTooltipId] = useState(null);

  // ── Carga de datos ─────────────────────────────────────────────────────
  const cargar = useCallback(async (silencioso = false) => {
    if (!silencioso) setLoading(true);
    else setRefreshing(true);
    setError(null);
    try {
      const res = await api.get("/multas");
      setMultas(res.data);
    } catch {
      setError("No se pudo cargar el listado de multas. Intentá de nuevo.");
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    cargar();
  }, [cargar]);

  // ── Filtrado ───────────────────────────────────────────────────────────
  const filtradas = multas.filter((m) => {
    const q = searchTerm.toLowerCase();

    // Mapeado a propiedades reales del DTO (estado, tipo, etc.)
    const coincideTexto =
      !q ||
      m.usuarioNombreCompleto?.toLowerCase().includes(q) ||
      m.vehiculoPatente?.toLowerCase().includes(q) ||
      m.incidenciaTipo?.toLowerCase().includes(q) ||
      m.tipo?.toLowerCase().includes(q) ||
      m.estado?.toLowerCase().includes(q) ||
      String(m.idMulta).includes(q);

    const coincideEstado =
      filtroEstado === "todos" ||
      m.estado?.toLowerCase() === filtroEstado.toLowerCase();
    const coincideTipoInc =
      filtroTipoInc === "todos" ||
      m.incidenciaTipo?.toLowerCase() === filtroTipoInc.toLowerCase();
    const coincideGravedad =
      filtroGravedad === "todos" ||
      m.incidenciaNivelGravedad?.toLowerCase() === filtroGravedad.toLowerCase();
    const coincideTipoMul =
      filtroTipoMulta === "todos" ||
      m.tipo?.toLowerCase() === filtroTipoMulta.toLowerCase();

    return (
      coincideTexto &&
      coincideEstado &&
      coincideTipoInc &&
      coincideGravedad &&
      coincideTipoMul
    );
  });

  // Contadores para chips de resumen (Corregidos con m.estado)
  const countPendiente = multas.filter(
    (m) => m.estado?.toLowerCase() === "pendiente",
  ).length;
  const countPagada = multas.filter(
    (m) => m.estado?.toLowerCase() === "pagada",
  ).length;
  const countCancelada = multas.filter(
    (m) => m.estado?.toLowerCase() === "cancelada",
  ).length;

  const hayFiltrosActivos =
    filtroEstado !== "todos" ||
    filtroTipoInc !== "todos" ||
    filtroGravedad !== "todos" ||
    filtroTipoMulta !== "todos";

  const limpiarFiltros = () => {
    setFiltroEstado("todos");
    setFiltroTipoInc("todos");
    setFiltroGravedad("todos");
    setFiltroTipoMulta("todos");
    setSearchTerm("");
  };

  // ── Render ─────────────────────────────────────────────────────────────

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <div className="flex flex-col items-center gap-3 text-slate-400">
          <Scale className="h-10 w-10 animate-pulse" />
          <p className="text-sm">Cargando multas...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50">
      {/* ── Header ─────────────────────────────────────────────────────────── */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-white">
        <div className="max-w-7xl mx-auto px-6 py-8">
          <div className="flex items-center justify-between gap-4 flex-wrap">
            <div className="flex items-center gap-4">
              <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center shrink-0">
                <Scale className="h-7 w-7 text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold tracking-tight">
                  Gestión de Multas
                </h1>
                <p className="text-slate-300 text-sm mt-0.5">
                  {multas.length}{" "}
                  {multas.length === 1
                    ? "multa registrada"
                    : "multas registradas"}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <button
                onClick={() => cargar(true)}
                disabled={refreshing}
                className="inline-flex items-center gap-2 px-4 py-2.5 text-sm font-medium text-slate-300 hover:text-white hover:bg-white/10 rounded-xl transition-colors disabled:opacity-50"
                title="Actualizar listado"
              >
                <RefreshCw
                  className={`h-4 w-4 ${refreshing ? "animate-spin" : ""}`}
                />
                {refreshing ? "Actualizando..." : "Actualizar"}
              </button>
              <button
                onClick={() => navigate("/multas/nueva")}
                className="inline-flex items-center gap-2 px-5 py-2.5 bg-white text-slate-900 hover:bg-slate-100 font-semibold text-sm rounded-xl shadow-lg transition-all"
              >
                <Plus className="h-4 w-4" />
                Nueva Multa
              </button>
            </div>
          </div>

          {/* Chips de resumen */}
          <div className="flex items-center gap-3 mt-6 flex-wrap">
            {[
              {
                label: "Pendientes",
                count: countPendiente,
                color: "bg-amber-400/20 text-amber-300 border-amber-400/30",
                value: "pendiente",
              },
              {
                label: "Pagadas",
                count: countPagada,
                color:
                  "bg-emerald-400/20 text-emerald-300 border-emerald-400/30",
                value: "pagada",
              },
              {
                label: "Canceladas",
                count: countCancelada,
                color: "bg-slate-400/20 text-slate-300 border-slate-400/30",
                value: "cancelada",
              },
            ].map(({ label, count, color, value }) => (
              <button
                key={value}
                onClick={() =>
                  setFiltroEstado(filtroEstado === value ? "todos" : value)
                }
                className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-lg border text-xs font-semibold transition-all ${
                  filtroEstado === value
                    ? color + " ring-2 ring-white/30"
                    : "bg-white/5 text-slate-400 border-white/10 hover:bg-white/10 hover:text-white"
                }`}
              >
                <span>{label}</span>
                <span
                  className={`h-4 min-w-[16px] px-1 rounded-full flex items-center justify-center text-[10px] font-black ${
                    filtroEstado === value ? "bg-white/20" : "bg-white/10"
                  }`}
                >
                  {count}
                </span>
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-6 py-6 space-y-4">
        {/* ── Error global ─────────────────────────────────────────────────── */}
        {error && (
          <div className="flex items-center gap-3 bg-red-50 border border-red-200 rounded-2xl px-5 py-4">
            <AlertTriangle className="h-5 w-5 text-red-500 shrink-0" />
            <p className="text-sm font-medium text-red-700">{error}</p>
            <button
              onClick={() => cargar()}
              className="ml-auto text-xs font-semibold text-red-600 hover:text-red-800 underline"
            >
              Reintentar
            </button>
          </div>
        )}

        {/* ── Barra de búsqueda y filtros ───────────────────────────────────── */}
        <div className="bg-white rounded-2xl border border-slate-200 p-4">
          <div className="flex items-center gap-3 flex-wrap">
            {/* Buscador */}
            <div className="relative flex-1 min-w-[220px]">
              <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Buscar por usuario, patente, tipo..."
                className="w-full pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
              />
              {searchTerm && (
                <button
                  onClick={() => setSearchTerm("")}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                >
                  <X className="h-3.5 w-3.5" />
                </button>
              )}
            </div>

            {/* Toggle filtros */}
            <button
              onClick={() => setMostrarFiltros((f) => !f)}
              className={`inline-flex items-center gap-2 px-4 py-2.5 rounded-xl border text-sm font-medium transition-all ${
                mostrarFiltros || hayFiltrosActivos
                  ? "bg-slate-900 text-white border-slate-900"
                  : "bg-slate-50 text-slate-600 border-slate-200 hover:bg-slate-100"
              }`}
            >
              <Filter className="h-4 w-4" />
              Filtros
              {hayFiltrosActivos && (
                <span className="h-4 w-4 rounded-full bg-blue-400 text-slate-900 text-[9px] font-black flex items-center justify-center">
                  !
                </span>
              )}
            </button>

            {/* Limpiar */}
            {(hayFiltrosActivos || searchTerm) && (
              <button
                onClick={limpiarFiltros}
                className="inline-flex items-center gap-1.5 px-3 py-2.5 rounded-xl text-xs font-semibold text-red-600 hover:bg-red-50 border border-red-200 transition-colors"
              >
                <X className="h-3.5 w-3.5" />
                Limpiar
              </button>
            )}

            {/* Conteo de resultados */}
            <span className="ml-auto text-xs text-slate-500 hidden sm:block">
              {filtradas.length} de {multas.length} resultado
              {filtradas.length !== 1 ? "s" : ""}
            </span>
          </div>

          {/* Panel de filtros expandible */}
          {mostrarFiltros && (
            <div className="mt-4 pt-4 border-t border-slate-100 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
              {/* Estado multa */}
              <div className="space-y-1.5">
                <label className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
                  Estado multa
                </label>
                <div className="relative">
                  <select
                    value={filtroEstado}
                    onChange={(e) => setFiltroEstado(e.target.value)}
                    className="w-full appearance-none pl-3 pr-8 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer"
                  >
                    <option value="todos">Todos</option>
                    <option value="pendiente">Pendiente</option>
                    <option value="pagada">Pagada</option>
                    <option value="cancelada">Cancelada</option>
                  </select>
                  <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-slate-400 pointer-events-none" />
                </div>
              </div>

              {/* Tipo incidencia */}
              <div className="space-y-1.5">
                <label className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
                  Tipo incidencia
                </label>
                <div className="relative">
                  <select
                    value={filtroTipoInc}
                    onChange={(e) => setFiltroTipoInc(e.target.value)}
                    className="w-full appearance-none pl-3 pr-8 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer"
                  >
                    <option value="todos">Todos</option>
                    {Object.entries(TIPO_INCIDENCIA_CONFIG).map(
                      ([val, cfg]) => (
                        <option key={val} value={val}>
                          {cfg.label}
                        </option>
                      ),
                    )}
                  </select>
                  <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-slate-400 pointer-events-none" />
                </div>
              </div>

              {/* Nivel gravedad */}
              <div className="space-y-1.5">
                <label className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
                  Gravedad
                </label>
                <div className="relative">
                  <select
                    value={filtroGravedad}
                    onChange={(e) => setFiltroGravedad(e.target.value)}
                    className="w-full appearance-none pl-3 pr-8 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer"
                  >
                    <option value="todos">Todos</option>
                    {Object.entries(GRAVEDAD_CONFIG).map(([val, cfg]) => (
                      <option key={val} value={val}>
                        {cfg.label}
                      </option>
                    ))}
                  </select>
                  <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-slate-400 pointer-events-none" />
                </div>
              </div>

              {/* Tipo multa */}
              <div className="space-y-1.5">
                <label className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
                  Tipo multa
                </label>
                <div className="relative">
                  <select
                    value={filtroTipoMulta}
                    onChange={(e) => setFiltroTipoMulta(e.target.value)}
                    className="w-full appearance-none pl-3 pr-8 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer"
                  >
                    <option value="todos">Todos</option>
                    {Object.entries(TIPO_MULTA_CONFIG).map(([val, cfg]) => (
                      <option key={val} value={val}>
                        {cfg.label}
                      </option>
                    ))}
                  </select>
                  <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-slate-400 pointer-events-none" />
                </div>
              </div>
            </div>
          )}
        </div>

        {/* ── Tabla ────────────────────────────────────────────────────────── */}
        <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
          {filtradas.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-20 text-slate-400">
              <FileWarning className="h-12 w-12 mb-3 text-slate-300" />
              <p className="font-semibold text-slate-500">
                No hay multas que coincidan
              </p>
              <p className="text-sm mt-1">
                {hayFiltrosActivos || searchTerm
                  ? "Probá con otros filtros o términos de búsqueda."
                  : "Todavía no hay multas registradas."}
              </p>
              {(hayFiltrosActivos || searchTerm) && (
                <button
                  onClick={limpiarFiltros}
                  className="mt-4 text-xs font-semibold text-blue-600 hover:underline"
                >
                  Limpiar filtros
                </button>
              )}
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-slate-100 bg-slate-50">
                    <th className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      ID
                    </th>
                    <th className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      <span className="flex items-center gap-1.5">
                        <User className="h-3.5 w-3.5" />
                        Usuario
                      </span>
                    </th>
                    <th className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      <span className="flex items-center gap-1.5">
                        <Car className="h-3.5 w-3.5" />
                        Vehículo
                      </span>
                    </th>
                    <th className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      Incidencia
                    </th>
                    <th className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      Gravedad
                    </th>
                    <th className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      <span className="flex items-center gap-1.5">
                        <DollarSign className="h-3.5 w-3.5" />
                        Multa
                      </span>
                    </th>
                    <th className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      Estado
                    </th>
                    <th className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      Fecha
                    </th>
                    <th className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      <span className="flex items-center gap-1.5">
                        <ShieldAlert className="h-3.5 w-3.5" />
                        Penalización
                      </span>
                    </th>
                    <th className="px-5 py-3.5 text-right text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      Acción
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {filtradas.map((m) => (
                    <tr
                      key={m.idMulta}
                      className={`transition-colors ${m.estaCancelada ? "bg-slate-50/60" : "hover:bg-slate-50"}`}
                    >
                      {/* ID */}
                      <td className="px-5 py-4">
                        <span className="font-mono text-xs font-bold text-slate-400">
                          #{m.idMulta}
                        </span>
                      </td>

                      {/* Usuario */}
                      <td className="px-5 py-4">
                        <div className="flex items-center gap-2 min-w-0">
                          <div className="h-7 w-7 rounded-lg bg-slate-100 flex items-center justify-center shrink-0">
                            <User className="h-3.5 w-3.5 text-slate-500" />
                          </div>
                          <span className="text-sm font-medium text-slate-800 truncate max-w-[140px]">
                            {m.usuarioNombreCompleto}
                          </span>
                        </div>
                      </td>

                      {/* Vehículo */}
                      <td className="px-5 py-4">
                        <div className="flex items-center gap-2">
                          <Car className="h-3.5 w-3.5 text-slate-400 shrink-0" />
                          <span className="font-mono text-xs font-bold text-slate-700 bg-slate-100 px-2 py-0.5 rounded border border-slate-200">
                            {m.vehiculoPatente}
                          </span>
                        </div>
                      </td>

                      {/* Incidencia */}
                      <td className="px-5 py-4">
                        <BadgeTipoIncidencia tipo={m.incidenciaTipo} />
                      </td>

                      {/* Gravedad */}
                      <td className="px-5 py-4">
                        <BadgeGravedad nivel={m.incidenciaNivelGravedad} />
                      </td>

                      {/* Multa tipo + monto (Corregido: m.tipo y m.monto) */}
                      <td className="px-5 py-4">
                        <div className="space-y-0.5">
                          <p className="text-xs font-semibold text-slate-600">
                            {TIPO_MULTA_CONFIG[m.tipo?.toLowerCase()]?.label ??
                              m.tipo}
                          </p>
                          <p className="text-sm font-bold text-slate-900">
                            {formatMonto(m.monto)}
                          </p>
                        </div>
                      </td>

                      {/* Estado (Corregido: m.estado) */}
                      <td className="px-5 py-4">
                        <BadgeEstado estado={m.estado} />
                      </td>

                      {/* Fecha creación (Corregido: m.fechaCreacion) */}
                      <td className="px-5 py-4">
                        <span className="text-xs text-slate-500">
                          {formatFecha(m.fechaCreacion)}
                        </span>
                      </td>

                      {/* Penalización */}
                      <td className="px-5 py-4">
                        {m.penalizacionTipo ? (
                          <span className="text-xs text-slate-600 font-medium">
                            {TIPO_PENALIZACION_CONFIG[m.penalizacionTipo]
                              ?.label ?? m.penalizacionTipo}
                          </span>
                        ) : (
                          <span className="text-xs text-slate-400">—</span>
                        )}
                      </td>

                      {/* Acción */}
                      <td className="px-5 py-4 text-right">
                        {m.estaCancelada ? (
                          <div
                            className="relative inline-block"
                            onMouseEnter={() => setTooltipId(m.idMulta)}
                            onMouseLeave={() => setTooltipId(null)}
                          >
                            <button
                              disabled
                              className="px-3 py-1.5 text-xs font-semibold text-slate-400 bg-slate-100 border border-slate-200 rounded-lg cursor-not-allowed"
                            >
                              Editar
                            </button>
                            {tooltipId === m.idMulta && (
                              <div className="absolute right-0 bottom-full mb-2 z-20 w-56 bg-slate-900 text-white text-xs rounded-xl px-3 py-2 shadow-xl leading-relaxed pointer-events-none">
                                <div className="flex items-start gap-1.5">
                                  <Ban className="h-3.5 w-3.5 shrink-0 mt-0.5 text-slate-400" />
                                  <span>
                                    Esta multa fue cancelada y no puede
                                    modificarse.
                                  </span>
                                </div>
                                <div className="absolute right-4 top-full w-0 h-0 border-l-4 border-r-4 border-t-4 border-transparent border-t-slate-900" />
                              </div>
                            )}
                          </div>
                        ) : (
                          <button
                            onClick={() =>
                              navigate(`/multas/${m.idMulta}/editar`)
                            }
                            className="px-3 py-1.5 text-xs font-semibold text-slate-700 bg-white border border-slate-200 rounded-lg hover:bg-slate-900 hover:text-white hover:border-slate-900 transition-all shadow-sm"
                          >
                            Editar
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* ── Footer info ───────────────────────────────────────────────────── */}
        {filtradas.length > 0 && (
          <p className="text-center text-xs text-slate-400 pb-2">
            Mostrando {filtradas.length} de {multas.length} multas
          </p>
        )}
      </div>
    </div>
  );
}
