import { useEffect, useState, useCallback } from "react";
import api from "../../api/axiosConfig";
import { useNavigate } from "react-router-dom";
import {
  Wrench,
  Car,
  Search,
  AlertTriangle,
  ChevronRight,
  Gauge,
  MapPin,
  ClipboardList,
  UserX,
  Filter,
  ShieldAlert,
  User,
  Clock,
  CheckCircle2,
  Ban,
  RefreshCw,
  ChevronDown,
  DollarSign,
  Calendar,
  X,
  Loader2,
  Info,
} from "lucide-react";

// ── Config visual ──────────────────────────────────────────────────────────
const ESTADO_MECANICO_CONFIG = {
  regular: { label: "Regular", classes: "bg-amber-50 text-amber-700 border border-amber-200", dot: "bg-amber-400" },
  malo:    { label: "Malo",    classes: "bg-red-50   text-red-700   border border-red-200",   dot: "bg-red-500"   },
};

const ESTADO_VEHICULO_CONFIG = {
  disponible:        { label: "Disponible",        classes: "text-emerald-600" },
  mantenimiento:     { label: "En Mantenimiento",  classes: "text-amber-600"   },
  fuera_de_servicio: { label: "Fuera de servicio", classes: "text-red-600"     },
  reservado:         { label: "Reservado",          classes: "text-blue-600"   },
  en_uso:            { label: "En uso",             classes: "text-purple-600" },
};

const ESTADO_MANT_CONFIG = {
  pendiente:  { label: "Pendiente",  classes: "bg-slate-100  text-slate-600",   icon: Clock        },
  iniciado:   { label: "Iniciado",   classes: "bg-blue-50    text-blue-700",    icon: Wrench       },
  finalizado: { label: "Finalizado", classes: "bg-emerald-50 text-emerald-700", icon: CheckCircle2 },
  cancelado:  { label: "Cancelado",  classes: "bg-red-50     text-red-700",     icon: Ban          },
};

const PRIORIDAD_CONFIG = {
  baja:    { label: "Baja",    color: "text-emerald-600 bg-emerald-50 border-emerald-200" },
  media:   { label: "Media",   color: "text-amber-600   bg-amber-50   border-amber-200"   },
  alta:    { label: "Alta",    color: "text-orange-600  bg-orange-50  border-orange-200"  },
  critica: { label: "Crítica", color: "text-red-600     bg-red-50     border-red-200"     },
};

const TIPOS_MANTENIMIENTO = [
  { value: "preventivo", label: "Preventivo"       },
  { value: "correctivo", label: "Correctivo"       },
  { value: "revision",   label: "Revisión técnica" },
  { value: "emergencia", label: "Emergencia"       },
];

const FORM_SOCIO_INICIAL = {
  tipo: "correctivo", descripcion: "", prioridad: "media", fechaRealizacion: "",
};

// ── Componente principal ───────────────────────────────────────────────────
export default function MantenimientosList() {
  const navigate = useNavigate();

  const [vehiculos, setVehiculos]         = useState([]);
  const [loading, setLoading]             = useState(true);
  const [refreshing, setRefreshing]       = useState(false);
  const [searchTerm, setSearchTerm]       = useState("");
  const [filtroEstado, setFiltroEstado]   = useState("todos");

  // Modal socio — dual según estado del vehículo
  const [modalSocio, setModalSocio]       = useState({ open: false, vehiculo: null });
  const [formSocio, setFormSocio]         = useState(FORM_SOCIO_INICIAL);
  const [socioLoading, setSocioLoading]   = useState(false);
  const [socioError, setSocioError]       = useState(null);

  // ── Carga ──────────────────────────────────────────────────────────────
  const cargar = useCallback(async (silencioso = false) => {
    if (!silencioso) setLoading(true);
    else setRefreshing(true);
    try {
      const res = await api.get("/mantenimientos/candidatos");
      setVehiculos(res.data);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    cargar();
    // Polling cada 30s para que el admin vea actualizaciones de estado del empleado
    const intervalo = setInterval(() => cargar(true), 30_000);
    return () => clearInterval(intervalo);
  }, [cargar]);

  // ── Filtrado ────────────────────────────────────────────────────────────
  const filtrados = vehiculos.filter((v) => {
    const q = searchTerm.toLowerCase();
    const coincide =
      v.marcaNombre?.toLowerCase().includes(q) ||
      v.modeloNombre?.toLowerCase().includes(q) ||
      v.patente?.toLowerCase().includes(q);
    if (filtroEstado === "todos")      return coincide;
    if (filtroEstado === "con_orden")  return coincide && v.tieneMantenimientoActivo;
    return coincide && v.estadoMecanico === filtroEstado;
  });

  const countRegular  = vehiculos.filter(v => v.estadoMecanico === "regular").length;
  const countMalo     = vehiculos.filter(v => v.estadoMecanico === "malo").length;
  const countConOrden = vehiculos.filter(v => v.tieneMantenimientoActivo).length;

  // ── Lógica del modal socio ─────────────────────────────────────────────
  const abrirModalSocio = (vehiculo) => {
    setSocioError(null);
    setFormSocio(FORM_SOCIO_INICIAL);
    setModalSocio({ open: true, vehiculo });
  };

  const handleGenerarOrden = (vehiculo) => {
    if (vehiculo.mantenimientoACargoDe === "socio") {
      abrirModalSocio(vehiculo);
    } else {
      navigate(`/mantenimientos/nuevo/${vehiculo.idVehiculo}`);
    }
  };

  // Confirmar "pasar a fuera de servicio" (vehículo AÚN NO está en fuera_de_servicio)
  const confirmarFueraDeServicio = async () => {
  const vehiculo = modalSocio.vehiculo;
  setSocioLoading(true);
  setSocioError(null);
  try {
      // 👉 ahora llamás al endpoint de Vehiculos
        await api.post(`/vehiculos/${vehiculo.idVehiculo}/fuera-de-servicio`);
      } catch {
        setSocioError("No se pudo pasar a fuera de servicio.");
      } finally {
        setSocioLoading(false);
        setModalSocio({ open: false, vehiculo: null });
        cargar();
      }
  };


  // Confirmar "Habilitar Vehículo" — envía el form del mantenimiento del socio
  const confirmarHabilitar = async () => {
    const vehiculo = modalSocio.vehiculo;

    if (!formSocio.tipo)            return setSocioError("El tipo es obligatorio.");
    if (!formSocio.descripcion.trim()) return setSocioError("La descripción es obligatoria.");
    if (!formSocio.prioridad)       return setSocioError("La prioridad es obligatoria.");
    if (!formSocio.fechaRealizacion) return setSocioError("La fecha de realización es obligatoria.");

    setSocioLoading(true);
    setSocioError(null);
    try {
      await api.post("/mantenimientos/habilitar-socio", {
        vehiculoId:       vehiculo.idVehiculo,
        tipo:             formSocio.tipo,
        descripcion:      formSocio.descripcion,
        prioridad:        formSocio.prioridad,
        fechaRealizacion: formSocio.fechaRealizacion,
      });
      setModalSocio({ open: false, vehiculo: null });
      cargar();
    } catch (e) {
      setSocioError(e.response?.data?.mensaje || "Error al habilitar el vehículo.");
    } finally {
      setSocioLoading(false);
    }
  };

  const esFueraDeServicio = modalSocio.vehiculo?.estado === "fuera_de_servicio";

  // ── Render ─────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen bg-slate-50">

      {/* Header */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-white">
        <div className="max-w-7xl mx-auto px-6 py-8">
          <div className="flex items-center justify-between gap-4 flex-wrap">
            <div className="flex items-center gap-4">
              <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center">
                <Wrench className="h-7 w-7 text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold tracking-tight">Gestión de Mantenimientos</h1>
                <p className="text-slate-300 text-sm mt-0.5">
                  Vehículos que requieren atención · Seleccioná uno para generar una orden
                </p>
              </div>
            </div>
            {/* Botón refresh manual */}
            <button
              onClick={() => cargar(true)}
              disabled={refreshing}
              className="flex items-center gap-2 px-4 py-2 bg-white/10 hover:bg-white/20 border border-white/20 rounded-xl text-sm font-medium text-white transition-all disabled:opacity-50"
            >
              <RefreshCw className={`h-4 w-4 ${refreshing ? "animate-spin" : ""}`} />
              {refreshing ? "Actualizando..." : "Actualizar"}
            </button>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6">
            {[
              { label: "Total candidatos", value: vehiculos.length, color: "text-white"      },
              { label: "Estado regular",   value: countRegular,     color: "text-amber-300"  },
              { label: "Estado malo",      value: countMalo,        color: "text-red-300"    },
              { label: "Con orden activa", value: countConOrden,    color: "text-blue-300"   },
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

        {/* Buscador + Filtros */}
        <div className="bg-white rounded-2xl border border-slate-200 p-4 mb-6 flex flex-col md:flex-row gap-4 items-start md:items-center">
          <div className="relative flex-1 w-full">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              type="text"
              placeholder="Buscar por marca, modelo o patente..."
              className="w-full pl-9 pr-4 py-2.5 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex items-center gap-2 flex-wrap">
            <Filter className="h-4 w-4 text-slate-400 shrink-0" />
            {[
              { key: "todos",     label: "Todos",           count: vehiculos.length },
              { key: "regular",   label: "Estado regular",  count: countRegular     },
              { key: "malo",      label: "Estado malo",     count: countMalo        },
              { key: "con_orden", label: "Con orden activa",count: countConOrden    },
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
                  {f.count}
                </span>
              </button>
            ))}
          </div>
        </div>

        {/* Grid de cards */}
        {loading ? (
          <div className="flex items-center justify-center py-24">
            <div className="flex flex-col items-center gap-3 text-slate-400">
              <Wrench className="h-10 w-10 animate-pulse" />
              <p className="text-sm">Cargando vehículos...</p>
            </div>
          </div>
        ) : filtrados.length === 0 ? (
          <div className="bg-white rounded-2xl border border-slate-200 py-20 text-center">
            <AlertTriangle className="mx-auto h-12 w-12 text-slate-300 mb-3" />
            <p className="text-slate-500 font-medium">No hay vehículos que requieran mantenimiento</p>
            <p className="text-slate-400 text-sm mt-1">
              {searchTerm ? "Probá cambiando los términos de búsqueda" : "Todos los vehículos están en buen estado"}
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {filtrados.map((v) => {
              const mecConfig  = ESTADO_MECANICO_CONFIG[v.estadoMecanico];
              const vehConfig  = ESTADO_VEHICULO_CONFIG[v.estado] || ESTADO_VEHICULO_CONFIG.disponible;
              const esSocio    = v.mantenimientoACargoDe === "socio";
              const mant       = v.mantenimientoActivo;
              const tieneOrden = v.tieneMantenimientoActivo;
              const mantCfg    = mant ? (ESTADO_MANT_CONFIG[mant.estado] || ESTADO_MANT_CONFIG.pendiente) : null;
              const MantIcon   = mantCfg?.icon;
              const prioridadCfg = mant ? (PRIORIDAD_CONFIG[mant.prioridad] || PRIORIDAD_CONFIG.media) : null;

              return (
                <div
                  key={v.idVehiculo}
                  className={`bg-white rounded-2xl border overflow-hidden transition-all group ${
                    tieneOrden ? "border-blue-200 hover:border-blue-300 hover:shadow-md"
                    : esSocio  ? "border-amber-200 hover:border-amber-300 hover:shadow-md"
                               : "border-slate-200 hover:border-slate-300 hover:shadow-md"
                  }`}
                >
                  {/* Banner */}
                  {tieneOrden && (
                    <div className="flex items-center gap-2 px-4 py-2 bg-blue-50 border-b border-blue-100">
                      <ClipboardList className="h-3.5 w-3.5 text-blue-600 shrink-0" />
                      <p className="text-xs text-blue-700 font-medium">Orden de mantenimiento activa</p>
                    </div>
                  )}
                  {!tieneOrden && esSocio && (
                    <div className="flex items-center gap-2 px-4 py-2 bg-amber-50 border-b border-amber-200">
                      <UserX className="h-3.5 w-3.5 text-amber-600 shrink-0" />
                      <p className="text-xs text-amber-700 font-medium">Mantenimiento a cargo del socio</p>
                    </div>
                  )}

                  {/* Header */}
                  <div className="px-5 pt-5 pb-4 border-b border-slate-100">
                    <div className="flex items-start justify-between gap-3">
                      <div className="flex items-center gap-3">
                        <div className="h-11 w-11 rounded-xl bg-slate-100 flex items-center justify-center text-slate-500 shrink-0">
                          <Car className="h-6 w-6" />
                        </div>
                        <div>
                          <h3 className="font-bold text-slate-900 leading-tight">
                            {v.marcaNombre} {v.modeloNombre}
                          </h3>
                          <p className="text-xs text-slate-400 mt-0.5">{v.tipo} · {v.anio}</p>
                        </div>
                      </div>
                      {mecConfig && (
                        <span className={`flex items-center gap-1.5 text-[11px] font-bold px-2 py-1 rounded-lg shrink-0 ${mecConfig.classes}`}>
                          <span className={`h-1.5 w-1.5 rounded-full ${mecConfig.dot}`} />
                          {mecConfig.label}
                        </span>
                      )}
                    </div>
                  </div>

                  {/* Body */}
                  <div className="px-5 py-4 space-y-2.5">
                    <div className="flex items-center justify-between">
                      <span className="font-mono text-xs bg-slate-100 text-slate-700 px-2 py-0.5 rounded border border-slate-200">
                        {v.patente}
                      </span>
                      <span className={`text-xs font-semibold ${vehConfig.classes}`}>
                        {vehConfig.label}
                      </span>
                    </div>
                    <div className="flex items-center gap-1.5 text-xs text-slate-500">
                      <Gauge className="h-3.5 w-3.5" />
                      <span>{v.kilometraje?.toLocaleString()} km</span>
                    </div>
                    {v.ubicacionNombre && (
                      <div className="flex items-center gap-1.5 text-xs text-slate-500">
                        <MapPin className="h-3.5 w-3.5" />
                        <span>{v.ubicacionNombre}</span>
                      </div>
                    )}

                    {/* Datos del mantenimiento activo — se actualiza con el estado real */}
                    {tieneOrden && mant && (
                      <div className="mt-1 pt-3 border-t border-slate-100 space-y-2">
                        <div className="flex items-center justify-between">
                          <span className={`inline-flex items-center gap-1.5 text-xs font-semibold px-2 py-1 rounded-lg ${mantCfg.classes}`}>
                            {MantIcon && <MantIcon className="h-3.5 w-3.5" />}
                            {mantCfg.label}
                          </span>
                          {prioridadCfg && (
                            <span className={`text-xs font-semibold capitalize ${prioridadCfg.color.split(" ")[0]}`}>
                              Prioridad {mant.prioridad}
                            </span>
                          )}
                        </div>
                        <p className="text-xs text-slate-500 capitalize">
                          Tipo: <span className="font-medium text-slate-700">{mant.tipo}</span>
                        </p>
                        <div className="flex items-center gap-1.5 text-xs text-slate-500">
                          <User className="h-3.5 w-3.5 shrink-0" />
                          {mant.empleadoNombre
                            ? <span className="font-medium text-slate-700">{mant.empleadoNombre}</span>
                            : <span className="italic text-slate-400">Sin empleado asignado</span>}
                        </div>
                        {mant.fechaProgramada && (
                          <div className="flex items-center gap-1.5 text-xs text-slate-500">
                            <Clock className="h-3.5 w-3.5 shrink-0" />
                            <span>Programado: <span className="font-medium text-slate-700">{mant.fechaProgramada}</span></span>
                          </div>
                        )}
                      </div>
                    )}
                  </div>

                  {/* CTA */}
                  <div className="px-5 pb-5">
                    {tieneOrden ? (
                      <div className="flex items-center justify-center gap-2 w-full py-2.5 bg-slate-100 text-slate-400 text-sm font-semibold rounded-xl cursor-not-allowed select-none">
                        <Ban className="h-4 w-4" />
                        Orden ya generada
                      </div>
                    ) : esSocio ? (
                      <button
                        onClick={() => handleGenerarOrden(v)}
                        className="flex items-center justify-center gap-2 w-full py-2.5 bg-amber-500 hover:bg-amber-600 text-white text-sm font-semibold rounded-xl transition-all group-hover:shadow-md"
                      >
                        <ShieldAlert className="h-4 w-4" />
                        Ver situación del socio
                      </button>
                    ) : (
                      <button
                        onClick={() => handleGenerarOrden(v)}
                        className="flex items-center justify-center gap-2 w-full py-2.5 bg-slate-900 hover:bg-slate-700 text-white text-sm font-semibold rounded-xl transition-all group-hover:shadow-md"
                      >
                        <ClipboardList className="h-4 w-4" />
                        Generar Orden
                        <ChevronRight className="h-4 w-4 ml-auto opacity-60" />
                      </button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* ── MODAL: Situación del socio (dual) ─────────────────────────────── */}
      {modalSocio.open && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <div className="p-6">

              {/* Cabecera del modal */}
              <div className="flex items-start justify-between mb-5">
                <div className="flex items-start gap-4">
                  <div className={`p-3 rounded-full shrink-0 ${esFueraDeServicio ? "bg-emerald-100" : "bg-amber-100"}`}>
                    <UserX className={`h-6 w-6 ${esFueraDeServicio ? "text-emerald-600" : "text-amber-600"}`} />
                  </div>
                  <div>
                    <h3 className="text-lg font-bold text-slate-900">
                      {esFueraDeServicio ? "Habilitar vehículo del socio" : "Mantenimiento a cargo del socio"}
                    </h3>
                    <p className="text-sm text-slate-500 mt-1">
                      <span className="font-semibold text-slate-700">
                        {modalSocio.vehiculo?.marcaNombre} {modalSocio.vehiculo?.modeloNombre} · {modalSocio.vehiculo?.patente}
                      </span>
                    </p>
                  </div>
                </div>
                <button
                  onClick={() => setModalSocio({ open: false, vehiculo: null })}
                  className="text-slate-400 hover:text-slate-600 transition-colors shrink-0"
                >
                  <X className="h-5 w-5" />
                </button>
              </div>

              {esFueraDeServicio ? (
                // ── Caso: ya está fuera de servicio → form para habilitar ──
                <>
                  <div className="flex items-start gap-2 bg-blue-50 border border-blue-200 rounded-xl px-4 py-3 mb-5">
                    <Info className="h-4 w-4 text-blue-500 shrink-0 mt-0.5" />
                    <p className="text-sm text-blue-700">
                      El vehículo está <span className="font-semibold">fuera de servicio</span> porque el socio se hace cargo del mantenimiento.
                      Completá los datos del trabajo realizado para habilitarlo nuevamente.
                    </p>
                  </div>

                  <div className="space-y-4">
                    {/* Tipo */}
                    <div className="space-y-1.5">
                      <label className="text-sm font-medium text-slate-700">
                        Tipo <span className="text-red-500">*</span>
                      </label>
                      <div className="relative">
                        <select
                          value={formSocio.tipo}
                          onChange={(e) => setFormSocio(p => ({ ...p, tipo: e.target.value }))}
                          className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer"
                        >
                          {TIPOS_MANTENIMIENTO.map(t => (
                            <option key={t.value} value={t.value}>{t.label}</option>
                          ))}
                        </select>
                        <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
                      </div>
                    </div>

                    {/* Descripción */}
                    <div className="space-y-1.5">
                      <label className="text-sm font-medium text-slate-700">
                        Descripción <span className="text-red-500">*</span>
                      </label>
                      <textarea
                        rows={3}
                        value={formSocio.descripcion}
                        onChange={(e) => setFormSocio(p => ({ ...p, descripcion: e.target.value }))}
                        className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 resize-none transition-all"
                        placeholder="Describí el trabajo realizado por el socio..."
                      />
                    </div>

                    {/* Prioridad */}
                    <div className="space-y-1.5">
                      <label className="text-sm font-medium text-slate-700">
                        Prioridad <span className="text-red-500">*</span>
                      </label>
                      <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
                        {Object.entries(PRIORIDAD_CONFIG).map(([val, cfg]) => (
                          <button
                            key={val}
                            type="button"
                            onClick={() => setFormSocio(p => ({ ...p, prioridad: val }))}
                            className={`py-2 px-3 rounded-xl border text-xs font-semibold transition-all ${
                              formSocio.prioridad === val
                                ? cfg.color + " ring-2 ring-offset-1 ring-current"
                                : "bg-slate-50 border-slate-200 text-slate-500 hover:bg-slate-100"
                            }`}
                          >
                            {cfg.label}
                          </button>
                        ))}
                      </div>
                    </div>

                    {/* Fecha realización */}
                    <div className="space-y-1.5">
                      <label className="text-sm font-medium text-slate-700">
                        Fecha de realización <span className="text-red-500">*</span>
                      </label>
                      <div className="relative">
                        <Calendar className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                        <input
                          type="date"
                          value={formSocio.fechaRealizacion}
                          onChange={(e) => setFormSocio(p => ({ ...p, fechaRealizacion: e.target.value }))}
                          className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
                        />
                      </div>
                    </div>

                    {/* Realizado por — solo informativo */}
                    <div className="flex items-center gap-3 px-4 py-3 bg-slate-50 rounded-xl border border-slate-200">
                      <DollarSign className="h-4 w-4 text-slate-400 shrink-0" />
                      <p className="text-sm text-slate-600">
                        Realizado por: <span className="font-semibold text-slate-800">A cargo del Socio</span>
                        <span className="text-xs text-slate-400 ml-2">(valor fijo)</span>
                      </p>
                    </div>
                  </div>

                  {socioError && (
                    <div className="flex items-start gap-2 bg-red-50 border border-red-200 rounded-xl px-4 py-3 mt-4">
                      <AlertTriangle className="h-4 w-4 text-red-500 shrink-0 mt-0.5" />
                      <p className="text-sm text-red-700">{socioError}</p>
                    </div>
                  )}

                  <div className="flex justify-end gap-3 mt-6 pt-4 border-t border-slate-100">
                    <button
                      onClick={() => setModalSocio({ open: false, vehiculo: null })}
                      disabled={socioLoading}
                      className="px-5 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors disabled:opacity-50"
                    >
                      Cancelar
                    </button>
                    <button
                      onClick={confirmarHabilitar}
                      disabled={socioLoading}
                      className="inline-flex items-center gap-2 px-5 py-2.5 text-sm font-semibold text-white bg-emerald-600 hover:bg-emerald-700 rounded-xl shadow-md transition-colors disabled:opacity-60"
                    >
                      {socioLoading && <Loader2 className="h-4 w-4 animate-spin" />}
                      Habilitar Vehículo
                    </button>
                  </div>
                </>
              ) : (
                // ── Caso: aún no está fuera de servicio → confirmación ──
                <>
                  <p className="text-sm text-slate-500 mb-2">
                    Este vehículo tiene el mantenimiento a cargo del socio propietario.
                  </p>
                  <p className="text-sm text-slate-500">
                    Al confirmar, el vehículo pasará automáticamente a estado{" "}
                    <span className="font-semibold text-red-600">fuera de servicio</span> y
                    no podrá ser reservado hasta que el socio realice el mantenimiento y vos lo habilites.
                  </p>

                  {socioError && (
                    <div className="flex items-start gap-2 bg-red-50 border border-red-200 rounded-xl px-4 py-3 mt-4">
                      <AlertTriangle className="h-4 w-4 text-red-500 shrink-0 mt-0.5" />
                      <p className="text-sm text-red-700">{socioError}</p>
                    </div>
                  )}

                  <div className="flex justify-end gap-3 mt-6 pt-4 border-t border-slate-100">
                    <button
                      onClick={() => setModalSocio({ open: false, vehiculo: null })}
                      disabled={socioLoading}
                      className="px-5 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors disabled:opacity-50"
                    >
                      Cancelar
                    </button>
                    <button
                      onClick={confirmarFueraDeServicio}
                      disabled={socioLoading}
                      className="inline-flex items-center gap-2 px-5 py-2.5 text-sm font-semibold text-white bg-amber-500 hover:bg-amber-600 rounded-xl shadow-md transition-colors disabled:opacity-60"
                    >
                      {socioLoading && <Loader2 className="h-4 w-4 animate-spin" />}
                      Confirmar y pasar a fuera de servicio
                    </button>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
