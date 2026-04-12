import { useState } from "react";
import api from "../../api/axiosConfig";
import { useNavigate } from "react-router-dom";

export default function UsuarioForm() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    nombre: "",
    apellido: "",
    email: "",
    dni: "",
    password: "",
    rolId: 1,
  });

  const handleChange = (e) =>
    setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    await api.post("/usuarios", form);
    navigate("/usuarios");
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">Registrar nuevo usuario</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        {["nombre", "apellido", "email", "dni", "password"].map((field) => (
          <div key={field}>
            <label className="block font-semibold capitalize">{field}</label>
            <input
              type={field === "password" ? "password" : "text"}
              name={field}
              value={form[field]}
              onChange={handleChange}
              className="border p-2 w-full"
              required
            />
          </div>
        ))}
        <div>
          <label className="block font-semibold">Rol</label>
          <select
            name="rolId"
            value={form.rolId}
            onChange={handleChange}
            className="border p-2 w-full"
          >
            <option value={1}>Cliente</option>
            <option value={2}>Socio</option>
            <option value={3}>Empleado</option>
            <option value={4}>Administrador</option>
          </select>
        </div>
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
