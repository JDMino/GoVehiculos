import { useEffect, useState } from "react";
import api from "../../api/axiosConfig";
import { useNavigate, useParams } from "react-router-dom";

export default function UsuarioEdit() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    nombre: "",
    apellido: "",
    telefono: "",
    direccion: "",
    bloqueado: false,
    activo: true,
  });

  useEffect(() => {
    api.get(`/usuarios/${id}`).then((res) => setForm(res.data));
  }, [id]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm({ ...form, [name]: type === "checkbox" ? checked : value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    await api.put(`/usuarios/${id}`, form);
    navigate("/usuarios");
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">Editar usuario</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        {["nombre", "apellido", "telefono", "direccion"].map((field) => (
          <div key={field}>
            <label className="block font-semibold capitalize">{field}</label>
            <input
              type="text"
              name={field}
              value={form[field] || ""}
              onChange={handleChange}
              className="border p-2 w-full"
            />
          </div>
        ))}
        <div className="flex items-center space-x-4">
          <label>
            <input
              type="checkbox"
              name="bloqueado"
              checked={form.bloqueado}
              onChange={handleChange}
            />{" "}
            Bloqueado
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
