import { useState, useEffect } from "react";
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
  Loader2,
} from "lucide-react";

export default function VehiculoForm() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  // Estado del formulario alineado con el backend
  const [form, setForm] = useState({
    socioId: null,
    tipo: "",
    marcaId: null,
    modeloId: 0,
    anio: 2024,
    patente: "",
    estado: "disponible",
    estadoMecanico: "bueno",
    kilometraje: 0,
    licenciaRequerida: "",
    precioPorDia: 0,
    ubicacionActualId: null,
    seguroVigente: true,
    documentacionVigente: true,
    mantenimientoACargoDe: "empresa",
    imagenUrl: "",
  });

  // Estados para cargar datos dinámicos
  const [marcas, setMarcas] = useState([]);
  const [modelos, setModelos] = useState([]);
  const [ubicaciones, setUbicaciones] = useState([]);

  // Cargar marcas y ubicaciones al inicio
  useEffect(() => {
    const fetchData = async () => {
      try {
        const marcasRes = await api.get("/marcas");
        setMarcas(marcasRes.data);

        const ubicacionesRes = await api.get("/ubicaciones");
        setUbicaciones(ubicacionesRes.data);
      } catch (error) {
        console.error("Error cargando datos:", error);
      }
    };
    fetchData();
  }, []);

  // Cuando se selecciona una marca, cargar modelos de esa marca
  const handleMarcaChange = async (e) => {
    const marcaId = e.target.value;
    setForm({ ...form, marcaId, modeloId: 0 }); // reset modelo
    try {
      const modelosRes = await api.get(`/modelos?marcaId=${marcaId}`);
      setModelos(modelosRes.data);
    } catch (error) {
      console.error("Error cargando modelos:", error);
    }
  };

  const handleChange = (e) =>
    setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await api.post("/vehiculos", form);
      navigate("/vehiculos");
    } catch (error) {
      console.error("Detalles del error: ", error);
      alert("Error al registrar el vehículo. Verifica los datos.");
    } finally {
      setLoading(false);
    }
  };

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
          <div className="flex items-center gap-4">
            <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center">
              <Car className="h-7 w-7 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">
                Registrar Nuevo Vehículo
              </h1>
              <p className="text-slate-300 text-sm mt-0.5">
                Añade una nueva unidad a la flota operativa
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Formulario */}
      <div className="max-w-4xl mx-auto px-6 py-8">
        <form onSubmit={handleSubmit} className="space-y-8">
          {/* Información Básica */}
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-100 bg-slate-50">
              <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                <FileText className="h-5 w-5 text-slate-400" />
                Información Básica
              </h2>
            </div>
            <div className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Marca
                  </label>
                  <select
                    name="marcaId"
                    value={form.marcaId || ""}
                    onChange={handleMarcaChange}
                    required
                    className="w-full px-4 py-3 border rounded-xl"
                  >
                    <option value="">Seleccione una marca</option>
                    {marcas.map((m) => (
                      <option key={m.idMarca} value={m.idMarca}>
                        {m.nombre}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Modelo
                  </label>
                  <select
                    name="modeloId"
                    value={form.modeloId || ""}
                    onChange={handleChange}
                    required
                    className="w-full px-4 py-3 border rounded-xl"
                  >
                    <option value="">Seleccione un modelo</option>
                    {modelos.map((m) => (
                      <option key={m.idModelo} value={m.idModelo}>
                        {m.nombre}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Año
                  </label>
                  <input
                    type="number"
                    name="anio"
                    value={form.anio}
                    onChange={handleChange}
                    required
                    className="w-full px-4 py-3 border rounded-xl"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Patente
                  </label>
                  <input
                    type="text"
                    name="patente"
                    value={form.patente}
                    onChange={handleChange}
                    required
                    className="w-full px-4 py-3 border rounded-xl uppercase"
                    placeholder="AB 123 CD"
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Tipo de Vehículo
                  </label>
                  <input
                    type="text"
                    name="tipo"
                    value={form.tipo}
                    onChange={handleChange}
                    required
                    className="w-full px-4 py-3 border rounded-xl"
                    placeholder="Ej: Sedan, SUV"
                  />
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Precio por Día
                  </label>
                  <input
                    type="number"
                    name="precioPorDia"
                    value={form.precioPorDia}
                    onChange={handleChange}
                    required
                    className="w-full px-4 py-3 border rounded-xl"
                  />
                </div>
              </div>
            </div>
          </div>

                    {/* Estado y Operación */}
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-100 bg-slate-50">
              <h2 className="font-semibold text-slate-900 flex items-center gap-2">
                <Wrench className="h-5 w-5 text-slate-400" />
                Estado y Operación
              </h2>
            </div>
            <div className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Estado Inicial
                  </label>
                  <select
                    name="estado"
                    value={form.estado}
                    onChange={handleChange}
                    className="w-full px-4 py-3 border rounded-xl"
                  >
                    <option value="disponible">Disponible</option>
                    <option value="mantenimiento">En Mantenimiento</option>
                  </select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Estado Mecánico
                  </label>
                  <select
                    name="estadoMecanico"
                    value={form.estadoMecanico}
                    onChange={handleChange}
                    className="w-full px-4 py-3 border rounded-xl"
                  >
                    <option value="bueno">Bueno</option>
                    <option value="regular">Regular</option>
                    <option value="malo">Malo</option>
                  </select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Kilometraje
                  </label>
                  <input
                    type="number"
                    name="kilometraje"
                    value={form.kilometraje}
                    onChange={handleChange}
                    className="w-full px-4 py-3 border rounded-xl"
                    placeholder="0"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Ubicación Actual
                  </label>
                  <select
                    name="ubicacionActualId"
                    value={form.ubicacionActualId || ""}
                    onChange={handleChange}
                    className="w-full px-4 py-3 border rounded-xl"
                  >
                    <option value="">Seleccione una sucursal</option>
                    {ubicaciones.map((u) => (
                      <option key={u.idUbicacion} value={u.idUbicacion}>
                        {u.nombre}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Licencia Requerida
                  </label>
                  <input
                    type="text"
                    name="licenciaRequerida"
                    value={form.licenciaRequerida}
                    onChange={handleChange}
                    className="w-full px-4 py-3 border rounded-xl"
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
              className="inline-flex items-center justify-center px-8 py-3
                bg-slate-900 hover:bg-slate-800 disabled:bg-slate-400 text-white
                font-semibold rounded-xl transition-all shadow-lg shadow-slate-200
                hover:shadow-xl disabled:shadow-none"
            >
              {loading ? (
                <>
                  <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                  Guardando ...
                </>
              ) : (
                <>
                  <CheckCircle className="h-5 w-5 mr-2" />
                  Guardar Vehículo
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
