import { useEffect, useState } from "react";
import api from "../../api/axiosConfig";
import { useNavigate, useParams, Link } from "react-router-dom";
import {
  ArrowLeft,
  Gauge,
  Wrench,
  ShieldCheck,
  MapPin,
  Car,
  DollarSign,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Loader2,
  FileCheck,
  Settings,
} from "lucide-react";

export default function VehiculoEdit() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [vehiculoInfo, setVehiculoInfo] = useState(null);
  const [form, setForm] = useState({
    estado: "disponible",
    estadoMecanico: "bueno",
    kilometraje: 0,
    precioPorDia: 0,
    ubicacionActual: "",
    seguroVigente: true,
    documentacionVigente: true,
    activo: true,
  });

  useEffect(() => {
    api
      .get(`/vehiculos/${id}`)
      .then((res) => {
        setVehiculoInfo({
          marca: res.data.marca,
          modelo: res.data.modelo,
          anio: res.data.anio,
          patente: res.data.patente,
        });

        setForm({
          estado: res.data.estado,
          estadoMecanico: res.data.estadoMecanico,
          kilometraje: res.data.kilometraje,
          precioPorDia: res.data.precioPorDia,
          ubicacionActual: res.data.ubicacionActual,
          seguroVigente: res.data.seguroVigente,
          documentacionVigente: res.data.documentacionVigente,
          activo: res.data.activo,
        });
        setLoading(false);
      })
      .catch(() => {
        alert("Error al cargar los datos");
        navigate("/vehiculos");
      });
  }, [id, navigate]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm({ ...form, [name]: type === "checkbox" ? checked : value });
  };

  const handleToggle = (field) => {
    setForm({ ...form, [field]: !form[field] });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.put(`/vehiculos/${id}`, form); // ✅ corregido comillas
      navigate("/vehiculos");
    } catch (error) {
      console.error("Detalles del error: ", error);
      alert("Error al actualizar");
    } finally {
      setSaving(false);
    }
  };

  const getEstadoBadgeStyles = (estado) => {
    const styles = {
      disponible: "bg-emerald-50 text-emerald-700 ring-emerald-600/20",
      alquilado: "bg-blue-50 text-blue-700 ring-blue-600/20",
      mantenimiento: "bg-amber-50 text-amber-700 ring-amber-600/20",
    };
    return styles[estado] || "bg-slate-50 text-slate-700 ring-slate-600/20";
  };

  if (loading)
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <div className="text-center">
          <Loader2 className="h-12 w-12 text-slate-400 animate-spin mx-auto mb-4" />
          <p className="text-slate-500 font-medium">
            Cargando datos del vehículo...
          </p>
        </div>
      </div>
    );

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-white">
        <div className="max-w-4xl mx-auto px-6 py-8">
          <Link
            to="/vehiculos"
            className="inline-flex items-center gap-2 text-slate-300 hover:text-white text-sm font-medium mb-4 transition-colors"
          >
            <ArrowLeft className="h-4 w-4" />
            Volver al catálogo
          </Link>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <div className="h-16 w-16 rounded-2xl bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center shadow-lg">
                <Car className="h-8 w-8 text-white" />
              </div>
              <div>
                <div className="flex items-center gap-3 mb-1">
                  <h1 className="text-2xl font-bold tracking-tight">
                    {vehiculoInfo?.marca} {vehiculoInfo?.modelo}
                  </h1>
                  <span
                    className={`inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-semibold ring-1 ring-inset ${getEstadoBadgeStyles(
                      form.estado
                    )}`}
                  >
                    {form.estado?.charAt(0).toUpperCase() +
                      form.estado?.slice(1)}
                  </span>
                </div>
                <div className="flex items-center gap-3 text-slate-300 text-sm">
                  <span>ID #{id}</span>
                  <span>&bull;</span>
                  <span>Año {vehiculoInfo?.anio}</span>
                  <span>&bull;</span>
                  <span className="font-mono bg-white/10 px-2 py-0.5 rounded">
                    {vehiculoInfo?.patente}
                  </span>
                </div>
              </div>
            </div>
            {!form.activo && (
              <div className="flex items-center gap-2 px-4 py-2 bg-red-500/20 border border-red-400/30 rounded-xl">
                <XCircle className="h-5 w-5 text-red-300" />
                <span className="text-red-200 font-medium text-sm">
                  Vehículo Inactivo
                </span>
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-6 py-8">
        {/* Alert if in maintenance */}
        {form.estado === "mantenimiento" && (
          <div className="mb-6 p-4 bg-amber-50 border border-amber-200 rounded-2xl flex items-start gap-3">
            <AlertTriangle className="h-5 w-5 text-amber-600 mt-0.5" />
            <div>
              <p className="font-semibold text-amber-800">
                Vehículo en mantenimiento
              </p>
              <p className="text-amber-700 text-sm">
                Este vehículo no está disponible para alquiler hasta que se
                complete el mantenimiento.
              </p>
            </div>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-8">
          {/* Estado de Operacion */}
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-100 bg-slate-50">
              <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                <Settings className="h-5 w-5 text-slate-400" />
                Estado de Operación
              </h2>
            </div>
            <div className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Estado */}
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Estado de Operación
                  </label>
                  <select
                    name="estado"
                    value={form.estado}
                    onChange={handleChange}
                    className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 text-slate-900 font-medium"
                  >
                    <option value="disponible">Disponible</option>
                    <option value="alquilado">Alquilado</option>
                    <option value="mantenimiento">En Mantenimiento</option>
                  </select>
                </div>

                {/* Estado Mecánico */}
                  <div className="space-y-2">
                    <label className="text-sm font-medium text-slate-700">
                      Condición Mecánica
                    </label>
                    <select
                      name="estadoMecanico"
                      value={form.estadoMecanico}
                      onChange={handleChange}
                      className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 text-slate-900 font-medium"
                    >
                      <option value="bueno">Bueno</option>
                      <option value="regular">Regular</option>
                      <option value="malo">Malo</option>
                    </select>
                  </div>

                  {/* Kilometraje */}
                  <div className="space-y-2">
                    <label className="text-sm font-medium text-slate-700">
                      Kilometraje Actual
                    </label>
                    <div className="relative">
                      <Gauge className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                      <input
                        type="number"
                        name="kilometraje"
                        value={form.kilometraje}
                        onChange={handleChange}
                        className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 text-slate-900"
                      />
                    </div>
                  </div>

                  {/* Ubicación */}
                  <div className="space-y-2">
                    <label className="text-sm font-medium text-slate-700">
                      Ubicación / Sucursal
                    </label>
                    <div className="relative">
                      <MapPin className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                      <input
                        type="text"
                        name="ubicacionActual"
                        value={form.ubicacionActual || ""}
                        onChange={handleChange}
                        className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 text-slate-900 placeholder:text-slate-400"
                        placeholder="Sucursal Centro"
                      />
                    </div>
                  </div>

                  {/* Precio por día */}
                  <div className="space-y-2 md:col-span-2">
                    <label className="text-sm font-medium text-slate-700">
                      Precio por Día ($)
                    </label>
                    <div className="relative max-w-xs">
                      <DollarSign className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                      <input
                        type="number"
                        name="precioPorDia"
                        value={form.precioPorDia}
                        onChange={handleChange}
                        className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 text-slate-900"
                      />
                    </div>
                  </div>
                  </div>
                  </div>
                  </div>

                  {/* Documentación y Auditoría */}
                  <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
                    <div className="px-6 py-4 border-b border-slate-100 bg-slate-50">
                      <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                        <FileCheck className="h-5 w-5 text-slate-400" />
                        Documentación y Auditoría
                      </h2>
                    </div>
                    <div className="p-6">
                      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        {/* Seguro Vigente */}
                        <button
                          type="button"
                          onClick={() => handleToggle("seguroVigente")}
                          className={`p-4 rounded-xl border-2 transition-all text-left ${
                            form.seguroVigente
                              ? "border-emerald-200 bg-emerald-50"
                              : "border-slate-200 bg-slate-50"
                          }`}
                        >
                          <div className="flex items-center justify-between mb-2">
                            <ShieldCheck
                              className={`h-5 w-5 ${
                                form.seguroVigente ? "text-emerald-600" : "text-slate-400"
                              }`}
                            />
                            <div
                              className={`w-10 h-6 rounded-full p-1 transition-colors ${
                                form.seguroVigente ? "bg-emerald-500" : "bg-slate-300"
                              }`}
                            >
                              <div
                                className={`w-4 h-4 rounded-full bg-white shadow transition-transform ${
                                  form.seguroVigente ? "translate-x-4" : "translate-x-0"
                                }`}
                              />
                            </div>
                          </div>
                          <p className="font-semibold text-slate-900 text-sm">Seguro Vigente</p>
                          <p className="text-xs text-slate-500 mt-0.5">Póliza activa</p>
                        </button>

                        {/* Documentación Vigente */}
                        <button
                          type="button"
                          onClick={() => handleToggle("documentacionVigente")}
                          className={`p-4 rounded-xl border-2 transition-all text-left ${
                            form.documentacionVigente
                              ? "border-emerald-200 bg-emerald-50"
                              : "border-slate-200 bg-slate-50"
                          }`}
                        >
                          <div className="flex items-center justify-between mb-2">
                            <FileCheck
                              className={`h-5 w-5 ${
                                form.documentacionVigente ? "text-emerald-600" : "text-slate-400"
                              }`}
                            />
                            <div
                              className={`w-10 h-6 rounded-full p-1 transition-colors ${
                                form.documentacionVigente ? "bg-emerald-500" : "bg-slate-300"
                              }`}
                            >
                              <div
                                className={`w-4 h-4 rounded-full bg-white shadow transition-transform ${
                                  form.documentacionVigente ? "translate-x-4" : "translate-x-0"
                                }`}
                              />
                            </div>
                          </div>
                          <p className="font-semibold text-slate-900 text-sm">Documentación Vigente</p>
                          <p className="text-xs text-slate-500 mt-0.5">Papeles al día</p>
                        </button>

                        {/* Vehículo Activo */}
                        <button
                          type="button"
                          onClick={() => handleToggle("activo")}
                          className={`p-4 rounded-xl border-2 transition-all text-left ${
                            form.activo ? "border-emerald-200 bg-emerald-50" : "border-red-200 bg-red-50"
                          }`}
                        >
                          <div className="flex items-center justify-between mb-2">
                            {form.activo ? (
                              <CheckCircle className="h-5 w-5 text-emerald-600" />
                            ) : (
                              <XCircle className="h-5 w-5 text-red-600" />
                            )}
                            <div
                              className={`w-10 h-6 rounded-full p-1 transition-colors ${
                                form.activo ? "bg-emerald-500" : "bg-red-500"
                              }`}
                            >
                              <div
                                className={`w-4 h-4 rounded-full bg-white shadow transition-transform ${
                                  form.activo ? "translate-x-4" : "translate-x-0"
                                }`}
                              />
                            </div>
                          </div>
                          <p className="font-semibold text-slate-900 text-sm">Vehículo Activo</p>
                          <p className="text-xs text-slate-500 mt-0.5">
                            {form.activo ? "En operación" : "Dado de baja"}
                          </p>
                        </button>
                      </div>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex items-center justify-between pt-4">
                    <Link
                      to="/vehiculos"
                      className="inline-flex items-center gap-2 px-6 py-3 text-slate-600 font-semibold hover:text-slate-900 transition-colors"
                    >
                      <ArrowLeft className="h-4 w-4" />
                      Volver
                    </Link>
                    <button
                      type="submit"
                      disabled={saving}
                      className="inline-flex items-center justify-center px-8 py-3 bg-slate-900 hover:bg-slate-800 disabled:bg-slate-400 text-white font-semibold rounded-xl transition-all shadow-lg disabled:shadow-none"
                    >
                      {saving ? (
                        <>
                          <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                          Guardando...
                        </>
                      ) : (
                        <>
                          <CheckCircle className="h-5 w-5 mr-2" />
                          Guardar Cambios
                        </>
                      )}
                    </button>
                  </div>
                  </form>
                  </div>
                  </div>
                  );
                  }
