import { useEffect, useState } from "react";
import api from "../../api/axiosConfig";
import { useNavigate, useParams } from "react-router-dom";

export default function VehiculoEdit() {
  const { id } = useParams();
  const navigate = useNavigate();
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
    api.get(`/vehiculos/${id}`).then((res) => {
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
    });
  }, [id]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm({ ...form, [name]: type === "checkbox" ? checked : value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    await api.put(`/vehiculos/${id}`, form);
    navigate("/vehiculos");
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">Editar vehículo</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block font-semibold">Estado</label>
          <select
            name="estado"
            value={form.estado}
            onChange={handleChange}
            className="border p-2 w-full"
          >
            <option value="disponible">Disponible</option>
            <option value="reservado">Reservado</option>
            <option value="en_uso">En uso</option>
            <option value="mantenimiento">Mantenimiento</option>
            <option value="fuera_de_servicio">Fuera de servicio</option>
          </select>
        </div>

        <div>
          <label className="block font-semibold">Estado Mecánico</label>
          <select
            name="estadoMecanico"
            value={form.estadoMecanico}
            onChange={handleChange}
            className="border p-2 w-full"
          >
            <option value="excelente">Excelente</option>
            <option value="bueno">Bueno</option>
            <option value="regular">Regular</option>
            <option value="malo">Malo</option>
            <option value="critico">Crítico</option>
          </select>
        </div>

        <div>
          <label className="block font-semibold">Kilometraje</label>
          <input
            type="number"
            name="kilometraje"
            value={form.kilometraje}
            onChange={handleChange}
            className="border p-2 w-full"
          />
        </div>

        <div>
          <label className="block font-semibold">Precio por día</label>
          <input
            type="number"
            name="precioPorDia"
            value={form.precioPorDia}
            onChange={handleChange}
            className="border p-2 w-full"
          />
        </div>

        <div>
          <label className="block font-semibold">Ubicación Actual</label>
          <input
            type="text"
            name="ubicacionActual"
            value={form.ubicacionActual || ""}
            onChange={handleChange}
            className="border p-2 w-full"
          />
        </div>

        <div className="flex items-center space-x-4">
          <label>
            <input
              type="checkbox"
              name="seguroVigente"
              checked={form.seguroVigente}
              onChange={handleChange}
            />{" "}
            Seguro vigente
          </label>
          <label>
            <input
              type="checkbox"
              name="documentacionVigente"
              checked={form.documentacionVigente}
              onChange={handleChange}
            />{" "}
            Documentación vigente
          </label>
          <label>
            <input
              type="checkbox"
              name="activo"
              checked={form.activo}
              onChange={handleChange}
            />{" "}
            Activo
          </label>
        </div>

        <button
          type="submit"
          className="bg-blue-600 text-white px-4 py-2 rounded"
        >
          Guardar cambios
        </button>
      </form>
    </div>
  );
}
