import { useState } from "react";
import api from "../../api/axiosConfig";
import { useNavigate } from "react-router-dom";

export default function VehiculoForm() {
  const navigate = useNavigate();
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
    await api.post("/vehiculos", form);
    navigate("/vehiculos");
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">Registrar nuevo vehículo</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        {["tipo", "marca", "modelo", "anio", "patente", "precioPorDia"].map(
          (field) => (
            <div key={field}>
              <label className="block font-semibold capitalize">{field}</label>
              <input
                type={
                  field === "anio" || field === "precioPorDia"
                    ? "number"
                    : "text"
                }
                name={field}
                value={form[field]}
                onChange={handleChange}
                className="border p-2 w-full"
                required
              />
            </div>
          ),
        )}
        <button
          type="submit"
          className="bg-green-600 text-white px-4 py-2 rounded"
        >
          Guardar
        </button>
      </form>
    </div>
  );
}
