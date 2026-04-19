import { useEffect, useState } from "react";
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
} from "lucide-react";

const ESTADO_MECANICO_CONFIG = {
  regular: {
    label:   "Regular",
    classes: "bg-amber-50 text-amber-700 border border-amber-200",
    dot:     "bg-amber-400",
  },
  malo: {
    label:   "Malo",
    classes: "bg-red-50 text-red-700 border border-red-200",
    dot:     "bg-red-500",
  },
};

const ESTADO_VEHICULO_CONFIG = {
  disponible:        { label: "Disponible",        classes: "text-emerald-600" },
  mantenimiento:     { label: "En Mantenimiento",  classes: "text-amber-600"   },
  fuera_de_servicio: { label: "Fuera de servicio", classes: "text-red-600"     },
  reservado:         { label: "Reservado",          classes: "text-blue-600"   },
  en_uso:            { label: "En uso",             classes: "text-purple-600" },
};

const ESTADO_MANT_CONFIG = {
  pendiente:  { label: "Pendiente",   classes: "bg-slate-100  text-slate-600",  icon: Clock         },
  en_proceso: { label: "En proceso",  classes: "bg-blue-50    text-blue-700",   icon: Wrench        },
  completado: { label: "Completado",  classes: "bg-emerald-50 text-emerald-700",icon: CheckCircle2  },
};

const PRIORIDAD_CONFIG = {
  baja:    "text-emerald-600",
  media:   "text-amber-600",
  alta:    "text-orange-600",
  critica: "text-red-600",
};

export default function MantenimientosList() {
  const navigate = useNavigate();

  const [vehiculos, setVehiculos]       = useState([]);
  const [loading, setLoading]           = useState(true);
  const [searchTerm, setSearchTerm]     = useState("");
  const [filtroEstado, setFiltroEstado] = useState("todos");
  const [modalSocio, setModalSocio]     = useState({ isOpen: false, vehiculo: null });

  const cargarCandidatos = () => {
    setLoading(true);
    api.get("/mantenimientos/candidatos")
      .then((res) => setVehiculos(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(() => { cargarCandidatos(); }, []);

  const filtrados = vehiculos.filter((v) => {
    const coincide =
      v.marcaNombre?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      v.modeloNombre?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      v.patente?.toLowerCase().includes(searchTerm.toLowerCase());

    if (filtroEstado === "todos")    return coincide;
    if (filtroEstado === "con_orden") return coincide && v.tieneMantenimientoActivo;
    return coincide && v.estadoMecanico === filtroEstado;
  });

  const countRegular  = vehiculos.filter((v) => v.estadoMecanico === "regular").length;
  const countMalo     = vehiculos.filter((v) => v.estadoMecanico === "malo").length;
  const countConOrden = vehiculos.filter((v) => v.tieneMantenimientoActivo).length;

  const handleGenerarOrden = (vehiculo) => {
    if (vehiculo.mantenimientoACargoDe === "socio") {
      setModalSocio({ isOpen: true, vehiculo });
    } else {
      navigate(`/mantenimientos/nuevo/${vehiculo.idVehiculo}`);
    }
  };

  const confirmarSocio = async () => {
    const vehiculo = modalSocio.vehiculo;
    setModalSocio({ isOpen: false, vehiculo: null });
    try {
      await api.post("/mantenimientos", { vehiculoId: vehiculo.idVehiculo });
    } catch {
      // 422 esperado para caso socio — el backend aplica el cambio igual
    } finally {
      cargarCandidatos();
    }
  };

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
              <h1 className="text-2xl font-bold tracking-tight">Gestión de Mantenimientos</h1>
              <p className="text-slate-300 text-sm mt-0.5">
                Vehículos que requieren atención · Seleccioná uno para generar una orden
              </p>
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6">
            {[
              { label: "Total candidatos",  value: vehiculos.length, color: "text-white"       },
              { label: "Estado regular",    value: countRegular,     color: "text-amber-300"   },
              { label: "Estado malo",       value: countMalo,        color: "text-red-300"     },
              { label: "Con orden activa",  value: countConOrden,    color: "text-blue-300"    },
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

        {/* Lista */}
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
              const mantConfig = mant ? (ESTADO_MANT_CONFIG[mant.estado] || ESTADO_MANT_CONFIG.pendiente) : null;
              const MantIcon   = mantConfig?.icon;

              return (
                <div
                  key={v.idVehiculo}
                  className={`bg-white rounded-2xl border overflow-hidden transition-all group ${
                    tieneOrden
                      ? "border-blue-200 hover:border-blue-300 hover:shadow-md"
                      : esSocio
                        ? "border-amber-200 hover:border-amber-300 hover:shadow-md"
                        : "border-slate-200 hover:border-slate-300 hover:shadow-md"
                  }`}
                >
                  {/* Banner superior según situación */}
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

                  {/* Header de la card */}
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

                    {/* Bloque de mantenimiento activo */}
                    {tieneOrden && mant && (
                      <div className="mt-1 pt-3 border-t border-slate-100 space-y-2">
                        {/* Estado + Prioridad */}
                        <div className="flex items-center justify-between">
                          <span className={`inline-flex items-center gap-1.5 text-xs font-semibold px-2 py-1 rounded-lg ${mantConfig.classes}`}>
                            {MantIcon && <MantIcon className="h-3.5 w-3.5" />}
                            {mantConfig.label}
                          </span>
                          <span className={`text-xs font-semibold capitalize ${PRIORIDAD_CONFIG[mant.prioridad] || "text-slate-500"}`}>
                            Prioridad {mant.prioridad}
                          </span>
                        </div>

                        {/* Tipo */}
                        <p className="text-xs text-slate-500 capitalize">
                          Tipo: <span className="font-medium text-slate-700">{mant.tipo}</span>
                        </p>

                        {/* Empleado asignado */}
                        <div className="flex items-center gap-1.5 text-xs text-slate-500">
                          <User className="h-3.5 w-3.5 shrink-0" />
                          {mant.empleadoNombre
                            ? <span className="font-medium text-slate-700">{mant.empleadoNombre}</span>
                            : <span className="italic text-slate-400">Sin empleado asignado</span>
                          }
                        </div>

                        {/* Fecha programada */}
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
                      // Botón deshabilitado con explicación
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

      {/* Modal advertencia socio */}
      {modalSocio.isOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
          <div className="bg-white rounded-2xl p-6 max-w-md w-full shadow-2xl">
            <div className="flex items-start gap-4 mb-5">
              <div className="bg-amber-100 p-3 rounded-full shrink-0">
                <UserX className="h-6 w-6 text-amber-600" />
              </div>
              <div>
                <h3 className="text-lg font-bold text-slate-900">Mantenimiento a cargo del socio</h3>
                <p className="text-sm text-slate-500 mt-1">
                  El vehículo{" "}
                  <span className="font-semibold text-slate-700">
                    {modalSocio.vehiculo?.marcaNombre} {modalSocio.vehiculo?.modeloNombre} · {modalSocio.vehiculo?.patente}
                  </span>{" "}
                  tiene el mantenimiento a cargo del socio propietario.
                </p>
                <p className="text-sm text-slate-500 mt-2">
                  Al confirmar, el vehículo pasará automáticamente a estado{" "}
                  <span className="font-semibold text-red-600">fuera de servicio</span> y no podrá
                  ser reservado hasta nuevo aviso.
                </p>
              </div>
            </div>
            <div className="flex justify-end gap-3 pt-4 border-t border-slate-100">
              <button
                onClick={() => setModalSocio({ isOpen: false, vehiculo: null })}
                className="px-5 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
              >
                Cancelar
              </button>
              <button
                onClick={confirmarSocio}
                className="px-5 py-2.5 text-sm font-semibold text-white bg-amber-500 hover:bg-amber-600 rounded-xl shadow-md transition-colors"
              >
                Confirmar y pasar a fuera de servicio
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}