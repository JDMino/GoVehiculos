import { useEffect, useState, useCallback, useMemo } from "react";
import api from "../../api/axiosConfig";
import useAuthStore from "../../context/AuthStore";
import {
  Scale,
  Search,
  AlertTriangle,
  X,
  RefreshCw,
  Filter,
  ChevronDown,
  Eye,
  Car,
  Calendar,
  DollarSign,
  ShieldAlert,
  CheckCircle2,
  Clock,
  Ban,
  FileText,
  User,
  Gavel,
  Info,
  ExternalLink,
  CircleAlert,
} from "lucide-react";

// ── Config visual ──────────────────────────────────────────────────────────
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

const ESTADO_PENALIZACION_CONFIG = {
  activa: {
    label: "Activa",
    classes: "bg-red-50 text-red-700 border border-red-200",
  },
  cumplida: {
    label: "Cumplida",
    classes: "bg-emerald-50 text-emerald-700 border border-emerald-200",
  },
  revocada: {
    label: "Revocada",
    classes: "bg-slate-100 text-slate-500 border border-slate-200",
  },
};

const TIPO_INCIDENCIA_LABELS = {
  daño_fisico: "Daño físico",
  accidente: "Accidente",
  infraccion_vial: "Infracción vial",
  comportamiento_indebido: "Comportamiento indebido",
  retraso_en_pago: "Retraso en pago",
};

const TIPO_PENALIZACION_LABELS = {
  suspension_temporal: "Suspensión temporal",
  bloqueo_cuenta: "Bloqueo de cuenta",
  inhabilitacion_vehiculo: "Inhabilitación del vehículo",
  advertencia: "Advertencia",
};

const NIVEL_GRAVEDAD_CONFIG = {
  baja: {
    label: "Baja",
    classes: "bg-emerald-50 text-emerald-700 border border-emerald-200",
  },
  media: {
    label: "Media",
    classes: "bg-amber-50 text-amber-700 border border-amber-200",
  },
  alta: {
    label: "Alta",
    classes: "bg-red-50 text-red-700 border border-red-200",
  },
};

const formatFecha = (fecha) => {
  if (!fecha) return "—";
  return new Date(fecha).toLocaleDateString("es-AR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
};

const formatMonto = (monto) =>
  new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS" }).format(
    monto ?? 0,
  );

// ── Modal de detalle ───────────────────────────────────────────────────────
function ModalDetalle({ multa, onClose }) {
  if (!multa) return null;

  const estadoCfg =
    ESTADO_MULTA_CONFIG[multa.estado] || ESTADO_MULTA_CONFIG.pendiente;
  const EstadoIcon = estadoCfg.icon;
  const gravedadCfg =
    NIVEL_GRAVEDAD_CONFIG[multa.incidenciaNivelGravedad] ||
    NIVEL_GRAVEDAD_CONFIG.media;
  const penEstadoCfg = multa.penalizacionEstado
    ? ESTADO_PENALIZACION_CONFIG[multa.penalizacionEstado]
    : null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        {/* Header modal */}
        <div className="flex items-start justify-between p-6 border-b border-slate-100">
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-xl bg-slate-100 flex items-center justify-center">
              <Gavel className="h-5 w-5 text-slate-600" />
            </div>
            <div>
              <h2 className="text-lg font-bold text-slate-900">
                Detalle de Multa #{multa.idMulta}
              </h2>
              <p className="text-xs text-slate-400 mt-0.5">
                Registrada el {formatFecha(multa.fechaCreacion)}
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-xl transition-all"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="p-6 space-y-5">
          {/* Estado general */}
          <div className="flex items-center justify-between p-4 bg-slate-50 rounded-xl border border-slate-200">
            <div className="flex items-center gap-2">
              <EstadoIcon className="h-4 w-4 text-slate-500" />
              <span className="text-sm font-medium text-slate-600">
                Estado de la multa
              </span>
            </div>
            <span
              className={`inline-flex items-center gap-1.5 text-xs font-bold px-3 py-1 rounded-full ${estadoCfg.classes}`}
            >
              <span className={`h-1.5 w-1.5 rounded-full ${estadoCfg.dot}`} />
              {estadoCfg.label}
            </span>
          </div>

          {/* Sección: Incidencia */}
          <div>
            <div className="flex items-center gap-2 mb-3">
              <CircleAlert className="h-4 w-4 text-slate-400" />
              <h3 className="text-sm font-bold text-slate-700 uppercase tracking-wider">
                Incidencia
              </h3>
            </div>
            <div className="bg-slate-50 rounded-xl border border-slate-200 divide-y divide-slate-100">
              <div className="flex items-center justify-between px-4 py-3">
                <span className="text-xs text-slate-500">Tipo</span>
                <span className="text-xs font-semibold text-slate-800 capitalize">
                  {TIPO_INCIDENCIA_LABELS[multa.incidenciaTipo] ||
                    multa.incidenciaTipo}
                </span>
              </div>
              <div className="flex items-center justify-between px-4 py-3">
                <span className="text-xs text-slate-500">
                  Nivel de gravedad
                </span>
                <span
                  className={`text-xs font-bold px-2 py-0.5 rounded-full ${gravedadCfg.classes}`}
                >
                  {gravedadCfg.label}
                </span>
              </div>
              <div className="flex items-center justify-between px-4 py-3">
                <span className="text-xs text-slate-500">Fecha de reporte</span>
                <span className="text-xs font-semibold text-slate-800">
                  {formatFecha(multa.incidenciaFechaReporte)}
                </span>
              </div>
              <div className="px-4 py-3">
                <span className="text-xs text-slate-500 block mb-1">
                  Descripción
                </span>
                <p className="text-xs text-slate-700 leading-relaxed">
                  {multa.incidenciaDescripcion || "Sin descripción"}
                </p>
              </div>
            </div>
          </div>

          {/* Sección: Vehículo involucrado */}
          <div>
            <div className="flex items-center gap-2 mb-3">
              <Car className="h-4 w-4 text-slate-400" />
              <h3 className="text-sm font-bold text-slate-700 uppercase tracking-wider">
                Vehículo involucrado
              </h3>
            </div>
            <div className="bg-slate-50 rounded-xl border border-slate-200 divide-y divide-slate-100">
              <div className="flex items-center justify-between px-4 py-3">
                <span className="text-xs text-slate-500">Vehículo</span>
                <span className="text-xs font-semibold text-slate-800">
                  {multa.vehiculoMarca} {multa.vehiculoModelo}
                </span>
              </div>
              <div className="flex items-center justify-between px-4 py-3">
                <span className="text-xs text-slate-500">Patente</span>
                <span className="font-mono text-xs bg-slate-200 text-slate-700 px-2 py-0.5 rounded border border-slate-300">
                  {multa.vehiculoPatente}
                </span>
              </div>
            </div>
          </div>

          {/* Sección: Multa */}
          <div>
            <div className="flex items-center gap-2 mb-3">
              <DollarSign className="h-4 w-4 text-slate-400" />
              <h3 className="text-sm font-bold text-slate-700 uppercase tracking-wider">
                Multa
              </h3>
            </div>
            <div className="bg-slate-50 rounded-xl border border-slate-200 divide-y divide-slate-100">
              <div className="flex items-center justify-between px-4 py-3">
                <span className="text-xs text-slate-500">Tipo de multa</span>
                <span className="text-xs font-semibold text-slate-800 capitalize">
                  {multa.tipo}
                </span>
              </div>
              <div className="flex items-center justify-between px-4 py-3">
                <span className="text-xs text-slate-500">Monto</span>
                <span className="text-sm font-bold text-slate-900">
                  {formatMonto(multa.monto)}
                </span>
              </div>
              {multa.descripcion && (
                <div className="px-4 py-3">
                  <span className="text-xs text-slate-500 block mb-1">
                    Observaciones
                  </span>
                  <p className="text-xs text-slate-700 leading-relaxed">
                    {multa.descripcion}
                  </p>
                </div>
              )}
            </div>
          </div>

          {/* Sección: Penalización */}
          {multa.idPenalizacion && (
            <div>
              <div className="flex items-center gap-2 mb-3">
                <ShieldAlert className="h-4 w-4 text-slate-400" />
                <h3 className="text-sm font-bold text-slate-700 uppercase tracking-wider">
                  Penalización
                </h3>
              </div>
              <div className="bg-slate-50 rounded-xl border border-slate-200 divide-y divide-slate-100">
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-xs text-slate-500">Tipo</span>
                  <span className="text-xs font-semibold text-slate-800">
                    {TIPO_PENALIZACION_LABELS[multa.penalizacionTipo] ||
                      multa.penalizacionTipo}
                  </span>
                </div>
                {penEstadoCfg && (
                  <div className="flex items-center justify-between px-4 py-3">
                    <span className="text-xs text-slate-500">Estado</span>
                    <span
                      className={`text-xs font-bold px-2 py-0.5 rounded-full ${penEstadoCfg.classes}`}
                    >
                      {penEstadoCfg.label}
                    </span>
                  </div>
                )}
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-xs text-slate-500">Inicio</span>
                  <span className="text-xs font-semibold text-slate-800">
                    {formatFecha(multa.penalizacionFechaInicio)}
                  </span>
                </div>
                {multa.penalizacionFechaFin && (
                  <div className="flex items-center justify-between px-4 py-3">
                    <span className="text-xs text-slate-500">Vencimiento</span>
                    <span className="text-xs font-semibold text-slate-800">
                      {formatFecha(multa.penalizacionFechaFin)}
                    </span>
                  </div>
                )}
                <div className="px-4 py-3">
                  <span className="text-xs text-slate-500 block mb-1">
                    Motivo
                  </span>
                  <p className="text-xs text-slate-700 leading-relaxed">
                    {multa.penalizacionMotivo || "—"}
                  </p>
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="px-6 pb-6">
          <button
            onClick={onClose}
            className="w-full py-2.5 bg-slate-900 hover:bg-slate-700 text-white text-sm font-semibold rounded-xl transition-all"
          >
            Cerrar
          </button>
        </div>
      </div>
    </div>
  );
}

// ── Componente principal ───────────────────────────────────────────────────
export default function MultasRecibidas() {
  const { user } = useAuthStore();

  const usuarioId = useMemo(
    () => user?.idUsuario ?? user?.IdUsuario ?? user?.id ?? user?.Id ?? null,
    [user],
  );

  const [multas, setMultas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [filtroEstado, setFiltroEstado] = useState("todos");
  const [multaDetalle, setMultaDetalle] = useState(null);

  const cargar = useCallback(
    async (silencioso = false) => {
      if (!usuarioId) return;
      if (!silencioso) setLoading(true);
      else setRefreshing(true);
      try {
        const res = await api.get(`/multas/usuario/${usuarioId}`);
        setMultas(res.data);
      } catch (e) {
        console.error(e);
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [usuarioId],
  );

  useEffect(() => {
    cargar();
  }, [cargar]);

  // ── Filtrado ────────────────────────────────────────────────────────────
  const filtradas = useMemo(() => {
    const q = searchTerm.toLowerCase();
    return multas.filter((m) => {
      const coincide =
        m.vehiculoPatente?.toLowerCase().includes(q) ||
        m.vehiculoMarca?.toLowerCase().includes(q) ||
        m.vehiculoModelo?.toLowerCase().includes(q) ||
        m.incidenciaTipo?.toLowerCase().includes(q) ||
        m.tipo?.toLowerCase().includes(q);
      if (filtroEstado === "todos") return coincide;
      return coincide && m.estado === filtroEstado;
    });
  }, [multas, searchTerm, filtroEstado]);

  const countPendiente = multas.filter((m) => m.estado === "pendiente").length;
  const countPagada = multas.filter((m) => m.estado === "pagada").length;
  const countCancelada = multas.filter((m) => m.estado === "cancelada").length;

  const totalPendiente = multas
    .filter((m) => m.estado === "pendiente")
    .reduce((acc, m) => acc + (m.monto ?? 0), 0);

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-white">
        <div className="max-w-5xl mx-auto px-6 py-8">
          <div className="flex items-center gap-4 mb-6">
            <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center shrink-0">
              <Scale className="h-7 w-7 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Mis Multas</h1>
              <p className="text-slate-300 text-sm mt-0.5">
                Historial de infracciones y sanciones asociadas a tu cuenta
              </p>
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            {[
              { label: "Total", value: multas.length, color: "text-white" },
              {
                label: "Pendientes",
                value: countPendiente,
                color: "text-amber-300",
              },
              {
                label: "Pagadas",
                value: countPagada,
                color: "text-emerald-300",
              },
              {
                label: "Deuda pendiente",
                value: formatMonto(totalPendiente),
                color: "text-red-300",
                isText: true,
              },
            ].map((s) => (
              <div
                key={s.label}
                className="bg-white/10 rounded-xl px-4 py-3 border border-white/10"
              >
                <p
                  className={`${s.isText ? "text-lg" : "text-2xl"} font-bold ${s.color}`}
                >
                  {s.value}
                </p>
                <p className="text-slate-400 text-xs mt-0.5">{s.label}</p>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="max-w-5xl mx-auto px-6 py-6 space-y-5">
        {/* Banner informativo */}
        <div className="flex items-start gap-3 bg-blue-50 border border-blue-200 rounded-2xl px-5 py-4">
          <Info className="h-5 w-5 text-blue-500 shrink-0 mt-0.5" />
          <div className="text-sm text-blue-800 leading-relaxed">
            <span className="font-bold">¿Cómo pagar tu multa?</span> Podés
            abonar tus multas pendientes con tu{" "}
            <span className="font-semibold">DNI</span> a través de{" "}
            <span className="font-semibold">PagoMisMultas.com.ar</span> o en
            cualquier sucursal del{" "}
            <span className="font-semibold">Banco Nación</span>. Para consultas
            comunicáte al{" "}
            <span className="font-semibold">0800-333-GOVE (4683)</span> de lunes
            a viernes de 9 a 18 hs.
          </div>
        </div>

        {/* Buscador + filtros */}
        <div className="bg-white rounded-2xl border border-slate-200 p-4 flex flex-col md:flex-row gap-4 items-start md:items-center">
          <div className="relative flex-1 w-full">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Buscar por patente, marca, modelo o tipo..."
              className="w-full pl-9 pr-4 py-2.5 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex items-center gap-2 flex-wrap shrink-0">
            <Filter className="h-4 w-4 text-slate-400 shrink-0" />
            {[
              { key: "todos", label: "Todas", count: multas.length },
              { key: "pendiente", label: "Pendientes", count: countPendiente },
              { key: "pagada", label: "Pagadas", count: countPagada },
              { key: "cancelada", label: "Canceladas", count: countCancelada },
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
                <span
                  className={`ml-1.5 px-1.5 py-0.5 rounded-full text-[10px] font-bold ${
                    filtroEstado === f.key
                      ? "bg-white/20 text-white"
                      : "bg-slate-200 text-slate-500"
                  }`}
                >
                  {f.count}
                </span>
              </button>
            ))}
            <button
              onClick={() => cargar(true)}
              disabled={refreshing}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-slate-100 text-slate-600 hover:bg-slate-200 transition-all disabled:opacity-50"
            >
              <RefreshCw
                className={`h-3.5 w-3.5 ${refreshing ? "animate-spin" : ""}`}
              />
              {refreshing ? "..." : "Actualizar"}
            </button>
          </div>
        </div>

        {/* Tabla / lista */}
        {loading ? (
          <div className="flex items-center justify-center py-24">
            <div className="flex flex-col items-center gap-3 text-slate-400">
              <Scale className="h-10 w-10 animate-pulse" />
              <p className="text-sm">Cargando multas...</p>
            </div>
          </div>
        ) : filtradas.length === 0 ? (
          <div className="bg-white rounded-2xl border border-slate-200 py-20 text-center">
            <Scale className="mx-auto h-12 w-12 text-slate-200 mb-3" />
            <p className="text-slate-500 font-medium">
              {multas.length === 0
                ? "No tenés multas registradas"
                : "No hay multas que coincidan con la búsqueda"}
            </p>
            <p className="text-slate-400 text-sm mt-1">
              {multas.length === 0
                ? "¡Todo en orden! No registramos sanciones en tu cuenta."
                : "Probá cambiando los términos de búsqueda o el filtro"}
            </p>
          </div>
        ) : (
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
            {/* Header tabla — solo desktop */}
            <div className="hidden md:grid grid-cols-[1fr_1fr_1fr_120px_100px] gap-4 px-5 py-3 bg-slate-50 border-b border-slate-200 text-xs font-bold text-slate-500 uppercase tracking-wider">
              <span>Vehículo</span>
              <span>Incidencia</span>
              <span>Multa</span>
              <span>Estado</span>
              <span className="text-center">Detalle</span>
            </div>

            <div className="divide-y divide-slate-100">
              {filtradas.map((m) => {
                const estadoCfg =
                  ESTADO_MULTA_CONFIG[m.estado] ||
                  ESTADO_MULTA_CONFIG.pendiente;
                const EstadoIcon = estadoCfg.icon;
                const gravedadCfg =
                  NIVEL_GRAVEDAD_CONFIG[m.incidenciaNivelGravedad] ||
                  NIVEL_GRAVEDAD_CONFIG.media;

                return (
                  <div
                    key={m.idMulta}
                    className="grid grid-cols-1 md:grid-cols-[1fr_1fr_1fr_120px_100px] gap-3 md:gap-4 px-5 py-4 hover:bg-slate-50 transition-colors"
                  >
                    {/* Vehículo */}
                    <div className="flex items-center gap-2.5">
                      <div className="h-9 w-9 rounded-xl bg-slate-100 flex items-center justify-center shrink-0">
                        <Car className="h-4.5 w-4.5 text-slate-500" />
                      </div>
                      <div>
                        <p className="text-sm font-semibold text-slate-800 leading-tight">
                          {m.vehiculoMarca} {m.vehiculoModelo}
                        </p>
                        <span className="font-mono text-[11px] text-slate-400">
                          {m.vehiculoPatente}
                        </span>
                      </div>
                    </div>

                    {/* Incidencia */}
                    <div className="flex flex-col justify-center gap-1">
                      <p className="text-xs font-semibold text-slate-700 capitalize">
                        {TIPO_INCIDENCIA_LABELS[m.incidenciaTipo] ||
                          m.incidenciaTipo}
                      </p>
                      <div className="flex items-center gap-1.5">
                        <span
                          className={`text-[10px] font-bold px-1.5 py-0.5 rounded-full ${gravedadCfg.classes}`}
                        >
                          {gravedadCfg.label}
                        </span>
                        <span className="text-[11px] text-slate-400">
                          {formatFecha(m.incidenciaFechaReporte)}
                        </span>
                      </div>
                    </div>

                    {/* Multa */}
                    <div className="flex flex-col justify-center gap-1">
                      <p className="text-sm font-bold text-slate-900">
                        {formatMonto(m.monto)}
                      </p>
                      <p className="text-[11px] text-slate-400 capitalize">
                        {m.tipo}
                      </p>
                    </div>

                    {/* Estado */}
                    <div className="flex items-center">
                      <span
                        className={`inline-flex items-center gap-1.5 text-[11px] font-bold px-2.5 py-1 rounded-full ${estadoCfg.classes}`}
                      >
                        <span
                          className={`h-1.5 w-1.5 rounded-full ${estadoCfg.dot}`}
                        />
                        {estadoCfg.label}
                      </span>
                    </div>

                    {/* Botón detalle */}
                    <div className="flex items-center justify-start md:justify-center">
                      <button
                        onClick={() => setMultaDetalle(m)}
                        className="flex items-center gap-1.5 px-3 py-2 bg-slate-900 hover:bg-slate-700 text-white text-xs font-semibold rounded-xl transition-all"
                      >
                        <Eye className="h-3.5 w-3.5" />
                        Ver detalle
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>

      {/* Modal detalle */}
      {multaDetalle && (
        <ModalDetalle
          multa={multaDetalle}
          onClose={() => setMultaDetalle(null)}
        />
      )}
    </div>
  );
}
