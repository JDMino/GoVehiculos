import { useState } from "react";
import useAuthStore from "../context/AuthStore";

export default function Register() {
  const register = useAuthStore((state) => state.register);
  const [form, setForm] = useState({
    nombre: "",
    apellido: "",
    email: "",
    dni: "",
    password: "",
    rolId: 1, // por defecto cliente
  });

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const success = await register(form);
    if (success) {
      alert("Registro exitoso");
      window.location.href = "/login";
    } else {
      alert("Error en el registro");
    }
  };

  return (
    <div>
      <h2>Registro</h2>
      <form onSubmit={handleSubmit}>
        <input
          name="nombre"
          placeholder="Nombre"
          value={form.nombre}
          onChange={handleChange}
          required
        />
        <input
          name="apellido"
          placeholder="Apellido"
          value={form.apellido}
          onChange={handleChange}
          required
        />
        <input
          name="email"
          type="email"
          placeholder="Email"
          value={form.email}
          onChange={handleChange}
          required
        />
        <input
          name="dni"
          placeholder="DNI"
          value={form.dni}
          onChange={handleChange}
          required
        />
        <input
          name="password"
          type="password"
          placeholder="Contraseña"
          value={form.password}
          onChange={handleChange}
          required
        />
        <select name="rolId" value={form.rolId} onChange={handleChange}>
          <option value={1}>Cliente</option>
          <option value={2}>Socio</option>
          <option value={3}>Empleado</option>
          <option value={4}>Administrador</option>
        </select>
        <button type="submit">Registrarse</button>
      </form>
    </div>
  );
}
