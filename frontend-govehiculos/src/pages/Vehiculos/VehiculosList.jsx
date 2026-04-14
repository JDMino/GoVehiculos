import { useEffect, useState } from "react";
import api from "../../api/axiosConfig";
import { Link } from "react-router-dom";
import { PlusCircle, Search, Edit, Trash2, Car, Settings, AlertCircle, CheckCircle } from "lucide-react";

export default function VehiculosList() {
  const [vehiculos, setVehiculos] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    api.get("/vehiculos").then((res) => setVehiculos(res.data));
  }, []);

  const handleDelete = async (id) => {
    if (confirm("¿Deseas dar de baja este vehículo? Pasará a estado inactivo.")) {
      try {
        await api.delete(`/vehiculos/${id}`);
        setVehiculos(vehiculos.filter((v) => v.idVehiculo !== id));
      } catch (error) {
        console.error("Detalles del error:", error);
        alert("Error al intentar eliminar el vehículo.");
      }
    }
  };

  const filteredVehiculos = vehiculos.filter(v => 
    v.marca?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    v.modelo?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    v.patente?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getEstadoBadge = (estado) => {
    switch (estado?.toLowerCase()) {
      case 'disponible': return <span className="flex items-center text-green-600 bg-green-50 px-2 py-1 rounded-md text-xs font-bold"><CheckCircle className="w-3 h-3 mr-1"/> Disponible</span>;
      case 'mantenimiento': return <span className="flex items-center text-orange-600 bg-orange-50 px-2 py-1 rounded-md text-xs font-bold"><Settings className="w-3 h-3 mr-1"/> En Taller</span>;
      case 'alquilado': return <span className="flex items-center text-blue-600 bg-blue-50 px-2 py-1 rounded-md text-xs font-bold"><Car className="w-3 h-3 mr-1"/> Alquilado</span>;
      default: return <span className="flex items-center text-gray-600 bg-gray-100 px-2 py-1 rounded-md text-xs font-bold">{estado}</span>;
    }
  };

  return (
    <div className="p-6 bg-gray-50 min-h-screen">
      <div className="max-w-7xl mx-auto">
        {/* Cabecera */}
        <div className="flex flex-col md:flex-row md:items-center justify-between mb-8 gap-4">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Catálogo de Flota</h2>
            <p className="text-gray-500 text-sm">Administra los vehículos, sus estados y disponibilidad.</p>
          </div>
          <Link
            to="/vehiculos/nuevo"
            className="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-lg transition-colors shadow-sm"
          >
            <PlusCircle className="h-5 w-5 mr-2" />
            Nuevo Vehículo
          </Link>
        </div>

        {/* Buscador */}
        <div className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 mb-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-5 w-5" />
            <input
              type="text"
              placeholder="Buscar por marca, modelo o patente..."
              className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>

        {/* Grilla */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-100">
                  <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase">Vehículo</th>
                  <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase text-center">Patente</th>
                  <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase text-center">Estado</th>
                  <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase text-center">Mecánica</th>
                  <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase text-right">Precio/Día</th>
                  <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {filteredVehiculos.map((v) => (
                  <tr key={v.idVehiculo} className="hover:bg-blue-50/30 transition-colors group">
                    <td className="px-6 py-4">
                      <div className="flex items-center">
                        <div className="h-10 w-10 rounded-lg bg-blue-100 flex items-center justify-center text-blue-600 mr-3">
                          <Car className="h-6 w-6" />
                        </div>
                        <div>
                          <div className="font-bold text-gray-900">{v.marca} {v.modelo}</div>
                          <div className="text-xs text-gray-500">ID: #{v.idVehiculo} • Año: {v.anio || 'N/A'}</div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 text-center">
                      <span className="font-mono text-sm bg-gray-100 px-2 py-1 rounded border border-gray-200">{v.patente}</span>
                    </td>
                    <td className="px-6 py-4 flex justify-center">
                      {getEstadoBadge(v.estado)}
                    </td>
                    <td className="px-6 py-4 text-center">
                      <span className={`text-xs font-bold uppercase ${v.estadoMecanico === 'bueno' ? 'text-green-600' : 'text-orange-500'}`}>
                        {v.estadoMecanico}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right font-bold text-gray-900">
                      ${v.precioPorDia}
                    </td>
                    <td className="px-6 py-4 text-right">
                      <div className="flex justify-end gap-2">
                        <Link
                          to={`/vehiculos/editar/${v.idVehiculo}`}
                          className="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-all"
                          title="Editar"
                        >
                          <Edit className="h-5 w-5" />
                        </Link>
                        <button
                          onClick={() => handleDelete(v.idVehiculo)}
                          className="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-all"
                          title="Eliminar"
                        >
                          <Trash2 className="h-5 w-5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {filteredVehiculos.length === 0 && (
            <div className="text-center py-12">
              <AlertCircle className="mx-auto h-12 w-12 text-gray-300 mb-4" />
              <p className="text-gray-500">No se encontraron vehículos en la flota.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}