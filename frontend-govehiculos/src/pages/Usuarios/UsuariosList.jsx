import { useEffect, useState } from "react";
import api from "../../api/axiosConfig";
import { Link } from "react-router-dom";

export default function UsuariosList() {
  const [usuarios, setUsuarios] = useState([]);

  useEffect(() => {
    api.get("/usuarios").then((res) => setUsuarios(res.data));
  }, []);

  const handleDelete = async (id) => {
    if (confirm("¿Deseas dar de baja este usuario?")) {
      await api.delete(`/usuarios/${id}`);
      setUsuarios(usuarios.filter((u) => u.idUsuario !== id));
    }
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">Usuarios registrados</h2>
      <Link
        to="/usuarios/nuevo"
        className="bg-blue-600 text-white px-4 py-2 rounded mb-4 inline-block"
      >
        + Nuevo Usuario
      </Link>

      <table className="min-w-full border border-gray-300">
        <thead className="bg-gray-100">
          <tr>
            <th className="p-2 border">ID</th>
            <th className="p-2 border">Nombre</th>
            <th className="p-2 border">Email</th>
            <th className="p-2 border">Rol</th>
            <th className="p-2 border">Activo</th>
            <th className="p-2 border">Acciones</th>
          </tr>
        </thead>
        <tbody>
          {usuarios.map((u) => (
            <tr key={u.idUsuario}>
              <td className="p-2 border">{u.idUsuario}</td>
              <td className="p-2 border">
                {u.nombre} {u.apellido}
              </td>
              <td className="p-2 border">{u.email}</td>
              <td className="p-2 border">{u.rol}</td>
              <td className="p-2 border">{u.activo ? "Sí" : "No"}</td>
              <td className="p-2 border text-center">
                <Link
                  to={`/usuarios/editar/${u.idUsuario}`}
                  className="bg-yellow-500 text-white px-2 py-1 rounded mr-2"
                >
                  Editar
                </Link>
                <button
                  onClick={() => handleDelete(u.idUsuario)}
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
