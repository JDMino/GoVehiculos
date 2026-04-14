import { useState } from "react";
import api from "../../api/axiosConfig";
import { useNavigate, Link } from "react-router-dom";
import { 
  Car, 
  ArrowLeft, 
  DollarSign, 
  Tag, 
  Calendar, 
  MapPin,
  Gauge,
  Wrench,
  CheckCircle,
  FileText,
  Loader2
} from "lucide-react";

export default function VehiculoForm() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState({
    tipo: "",
    marca: "",
    modelo: "",
    anio: 2024,
    patente: "",
    precioPorDia: 0,
    estado: "disponible",
    estadoMecanico: "bueno",
    kilometraje: 0,
    licenciaRequerida: "",
    ubicacionActual: "",
    mantenimientoACargoDe: "empresa",
  });

  const handleChange = (e) =>
    setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await api.post("/vehiculos", form);
      navigate("/vehiculos");
    } catch (error) {
      console.error("Detalles del error:", error);
      alert("Error al registrar el vehiculo. Verifica los datos.");
    } finally {
      setLoading(false);
    }
  };

  const tiposVehiculo = [
    { value: "sedan", label: "Sedan", icon: Car },
    { value: "suv", label: "SUV", icon: Car },
    { value: "pickup", label: "Pickup", icon: Car },
    { value: "van", label: "Van", icon: Car },
  ];

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
            Volver al catalogo
          </Link>
          <div className="flex items-center gap-4">
            <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center">
              <Car className="h-7 w-7 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Registrar Nuevo Vehiculo</h1>
              <p className="text-slate-300 text-sm mt-0.5">Anade una nueva unidad a la flota operativa</p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-6 py-8">
        <form onSubmit={handleSubmit} className="space-y-8">
          {/* Informacion Basica */}
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-100 bg-slate-50">
              <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                <FileText className="h-5 w-5 text-slate-400" />
                Informacion Basica
              </h2>
            </div>
            <div className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Marca</label>
                  <input 
                    type="text" 
                    name="marca" 
                    value={form.marca} 
                    onChange={handleChange} 
                    required
                    className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900 placeholder:text-slate-400"
                    placeholder="Ej: Toyota"
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Modelo</label>
                  <input 
                    type="text" 
                    name="modelo" 
                    value={form.modelo} 
                    onChange={handleChange} 
                    required
                    className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900 placeholder:text-slate-400"
                    placeholder="Ej: Corolla"
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Ano</label>
                  <div className="relative">
                    <Calendar className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                    <input 
                      type="number" 
                      name="anio" 
                      value={form.anio} 
                      onChange={handleChange} 
                      required
                      className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900"
                    />
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Patente</label>
                  <div className="relative">
                    <Tag className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                    <input 
                      type="text" 
                      name="patente" 
                      value={form.patente} 
                      onChange={handleChange} 
                      required
                      className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900 uppercase placeholder:text-slate-400"
                      placeholder="AB 123 CD"
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Tipo de Vehiculo</label>
                  <input 
                    type="text" 
                    name="tipo" 
                    value={form.tipo} 
                    onChange={handleChange} 
                    required
                    className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900 placeholder:text-slate-400"
                    placeholder="Ej: Sedan, SUV"
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Precio por Dia</label>
                  <div className="relative">
                    <DollarSign className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                    <input 
                      type="number" 
                      name="precioPorDia" 
                      value={form.precioPorDia} 
                      onChange={handleChange} 
                      required
                      className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Estado y Operacion */}
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-100 bg-slate-50">
              <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                <Wrench className="h-5 w-5 text-slate-400" />
                Estado y Operacion
              </h2>
            </div>
            <div className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Estado Inicial</label>
                  <div className="relative">
                    <select 
                      name="estado" 
                      value={form.estado} 
                      onChange={handleChange}
                      className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer text-slate-900 font-medium"
                    >
                      <option value="disponible">Disponible</option>
                      <option value="mantenimiento">En Mantenimiento</option>
                    </select>
                  </div>
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Estado Mecanico</label>
                  <div className="relative">
                    <select 
                      name="estadoMecanico" 
                      value={form.estadoMecanico} 
                      onChange={handleChange}
                      className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer text-slate-900 font-medium"
                    >
                      <option value="bueno">Bueno</option>
                      <option value="regular">Regular</option>
                      <option value="malo">Malo</option>
                    </select>
                  </div>
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Kilometraje</label>
                  <div className="relative">
                    <Gauge className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                    <input 
                      type="number" 
                      name="kilometraje" 
                      value={form.kilometraje} 
                      onChange={handleChange}
                      className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900"
                      placeholder="0"
                    />
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Ubicacion Actual</label>
                  <div className="relative">
                    <MapPin className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                    <input 
                      type="text" 
                      name="ubicacionActual" 
                      value={form.ubicacionActual} 
                      onChange={handleChange}
                      className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900 placeholder:text-slate-400"
                      placeholder="Sucursal Centro"
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">Licencia Requerida</label>
                  <input 
                    type="text" 
                    name="licenciaRequerida" 
                    value={form.licenciaRequerida} 
                    onChange={handleChange}
                    className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900 placeholder:text-slate-400"
                    placeholder="Ej: Categoria B"
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Actions */}
          <div className="flex items-center justify-between pt-4">
            <Link 
              to="/vehiculos" 
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
                  Guardando...
                </>
              ) : (
                <>
                  <CheckCircle className="h-5 w-5 mr-2" />
                  Guardar Vehiculo
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
