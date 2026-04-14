import { Link } from "react-router-dom";

export default function Home() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gray-100">
      <h1 className="text-4xl font-bold mb-6">Bienvenido a GoVehiculos</h1>
      <p className="text-lg text-gray-600 mb-8">
        Explora nuestra flota y reserva tu próximo viaje.
      </p>
      <div className="flex gap-4">
        <Link
          to="/vehiculos"
          className="bg-blue-600 text-white px-4 py-2 rounded"
        >
          Ver Vehículos
        </Link>
        <Link
          to="/usuarios"
          className="bg-green-600 text-white px-4 py-2 rounded"
        >
          Gestion de Usuarios
        </Link>
      </div>
    </div>
  );
}