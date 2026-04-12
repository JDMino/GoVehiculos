import { useEffect, useState } from "react";
import api from "../../api/axiosConfig";
import { Link } from "react-router-dom";

export default function VehiculosList() {
  const [vehiculos, setVehiculos] = useState([]);

  useEffect(() => {
    api.get("/vehiculos").then((res) => setVehiculos(res.data));
  }, []);

  const handleDelete = async (id) => {
    if (confirm("¿Deseas dar de baja este vehículo?")) {
      await api.delete(`/vehiculos/${id}`);
      setVehiculos(vehiculos.filter((v) => v.idVehiculo !== id));
    }
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">Catálogo de Vehículos</h2>
      <Link
        to="/vehiculos/nuevo"
        className="bg-blue-600 text-white px-4 py-2 rounded mb-4 inline-block"
      >
        + Nuevo Vehículo
      </Link>

      <table className="min-w-full border border-gray-300">
        <thead className="bg-gray-100">
          <tr>
            <th className="p-2 border">ID</th>
            <th className="p-2 border">Marca</th>
            <th className="p-2 border">Modelo</th>
            <th className="p-2 border">Patente</th>
            <th className="p-2 border">Estado</th>
            <th className="p-2 border">Estado Mecánico</th>
            <th className="p-2 border">Precio/Día</th>
            <th className="p-2 border">Acciones</th>
          </tr>
        </thead>
        <tbody>
          {vehiculos.map((v) => (
            <tr key={v.idVehiculo}>
              <td className="p-2 border">{v.idVehiculo}</td>
              <td className="p-2 border">{v.marca}</td>
              <td className="p-2 border">{v.modelo}</td>
              <td className="p-2 border">{v.patente}</td>
              <td className="p-2 border">{v.estado}</td>
              <td className="p-2 border">{v.estadoMecanico}</td>
              <td className="p-2 border">${v.precioPorDia}</td>
              <td className="p-2 border text-center">
                <Link
                  to={`/vehiculos/editar/${v.idVehiculo}`}
                  className="bg-yellow-500 text-white px-2 py-1 rounded mr-2"
                >
                  Editar
                </Link>
                <button
                  onClick={() => handleDelete(v.idVehiculo)}
                  className="bg-red-600 text-white px-2 py-1 rounded"
                >
                  Eliminar
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
