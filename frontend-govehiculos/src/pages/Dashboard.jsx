import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  Car,
  Users,
  Wrench,
  PlusCircle,
  Activity,
  ArrowRight,
  ShieldCheck,
} from "lucide-react";
import api from "../api/axiosConfig";
import useAuthStore from "../context/AuthStore";

const StatCard = ({ title, value, Icon, colorClass, subtitle, isLoading }) => (
  <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 flex items-center space-x-4 hover:shadow-md transition-shadow">
    <div className={`p-4 rounded-full ${colorClass} bg-opacity-10`}>
      <Icon className={`h-8 w-8 ${colorClass.replace("bg-", "text-")}`} />
    </div>
    <div>
      <p className="text-sm font-medium text-gray-500 uppercase tracking-wider">
        {title}
      </p>
      <h3 className="text-3xl font-bold text-gray-900 mt-1">
        {/* Usamos el prop isLoading en lugar de la variable global */}
        {isLoading ? "..." : value}
      </h3>
      {subtitle && <p className="text-xs text-gray-400 mt-1">{subtitle}</p>}
    </div>
  </div>
);

export default function Dashboard() {
  const user = useAuthStore((state) => state.user);
  const [stats, setStats] = useState({
    totalVehiculos: 0,
    totalUsuarios: 0,
    vehiculosDisponibles: 0,
    mantenimientosActivos: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      let vehiculosData = [];
      let usuariosData = [];

      // 1. Intentamos cargar Vehículos
      try {
        const vehiculosRes = await api.get("/vehiculos");
        vehiculosData = vehiculosRes.data;
      } catch (error) {
        console.warn("No se pudieron cargar los vehículos:", error.message);
      }

      // 2. Intentamos cargar Usuarios
      try {
        const usuariosRes = await api.get("/usuarios");
        usuariosData = usuariosRes.data;
      } catch (error) {
        console.error("No se pudieron cargar los usuarios:", error.message);
      }

      // 3. Actualizamos las estadísticas
      setStats({
        totalVehiculos: vehiculosData.length,
        vehiculosDisponibles: vehiculosData.filter(
          (v) => v.estado === "disponible",
        ).length,
        totalUsuarios: usuariosData.length,
        mantenimientosActivos: 0,
      });

      setLoading(false);
    };

    fetchStats();
  }, []);

  return (
    <div className="min-h-screen bg-gray-50 p-4 sm:p-8">
      {/* Encabezado */}
      <div className="max-w-7xl mx-auto mb-8 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-gray-900 flex items-center gap-2">
            <ShieldCheck className="h-8 w-8 text-blue-600" />
            Panel de Administración
          </h1>
          <p className="mt-2 text-gray-600">
            Bienvenido,{" "}
            <span className="font-semibold text-blue-600">
              {user?.email || "Administrador"}
            </span>
            . Aquí tienes un resumen del sistema.
          </p>
        </div>
      </div>

      <div className="max-w-7xl mx-auto space-y-8">
        {/* Sección 1: Tarjetas de Estadísticas */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          <StatCard
            title="Vehículos Totales"
            value={stats.totalVehiculos}
            Icon={Car}
            colorClass="bg-blue-500 text-blue-600"
            subtitle={`${stats.vehiculosDisponibles} disponibles ahora`}
            isLoading={loading}
          />
          <StatCard
            title="Usuarios Registrados"
            value={stats.totalUsuarios}
            Icon={Users}
            colorClass="bg-green-500 text-green-600"
            subtitle="Clientes, Socios y Empleados"
            isLoading={loading}
          />
          <StatCard
            title="Mantenimientos"
            value={stats.mantenimientosActivos}
            Icon={Wrench}
            colorClass="bg-orange-500 text-orange-600"
            subtitle="Órdenes en proceso (Próximamente)"
            isLoading={loading}
          />
          <StatCard
            title="Actividad de red"
            value="Estable"
            Icon={Activity}
            colorClass="bg-purple-500 text-purple-600"
            subtitle="Conexión con BD GoVehiculosDB"
            isLoading={false} // Siempre estable visualmente
          />
        </div>

        {/* Sección 2: Acciones Rápidas */}
        <div>
          <h2 className="text-xl font-bold text-gray-900 mb-4">
            Acciones Rápidas
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {/* Acción 1: Gestión de Vehículos */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 flex flex-col h-full hover:border-blue-300 transition-colors">
              <div className="flex items-center gap-3 mb-4">
                <div className="bg-blue-100 p-2 rounded-lg">
                  <Car className="h-6 w-6 text-blue-600" />
                </div>
                <h3 className="text-lg font-bold text-gray-900">
                  Catálogo de Flota
                </h3>
              </div>
              <p className="text-gray-600 text-sm mb-6 flex-grow">
                Administra los vehículos del sistema. Registra nuevas unidades,
                edita sus datos o dalos de baja lógica.
              </p>
              <div className="flex gap-2 mt-auto">
                <Link
                  to="/vehiculos"
                  className="flex-1 text-center bg-gray-100 hover:bg-gray-200 text-gray-800 font-medium py-2 px-4 rounded-lg transition-colors"
                >
                  Ver Todos
                </Link>
                <Link
                  to="/vehiculos/nuevo"
                  className="flex items-center justify-center bg-blue-600 hover:bg-blue-700 text-white py-2 px-4 rounded-lg transition-colors"
                >
                  <PlusCircle className="h-5 w-5" />
                </Link>
              </div>
            </div>

            {/* Acción 2: Gestión de Usuarios */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 flex flex-col h-full hover:border-green-300 transition-colors">
              <div className="flex items-center gap-3 mb-4">
                <div className="bg-green-100 p-2 rounded-lg">
                  <Users className="h-6 w-6 text-green-600" />
                </div>
                <h3 className="text-lg font-bold text-gray-900">
                  Gestion de Usuarios
                </h3>
              </div>
              <p className="text-gray-600 text-sm mb-6 flex-grow">
                Controla los accesos al sistema. Gestiona roles, actualiza datos
                personales o bloquea cuentas.
              </p>
              <div className="flex gap-2 mt-auto">
                <Link
                  to="/usuarios"
                  className="flex-1 text-center bg-gray-100 hover:bg-gray-200 text-gray-800 font-medium py-2 px-4 rounded-lg transition-colors"
                >
                  Directorio
                </Link>
                <Link
                  to="/usuarios/nuevo"
                  className="flex items-center justify-center bg-green-600 hover:bg-green-700 text-white py-2 px-4 rounded-lg transition-colors"
                >
                  <PlusCircle className="h-5 w-5" />
                </Link>
              </div>
            </div>

            {/* Acción 3: Mantenimiento */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 flex flex-col h-full relative overflow-hidden">
              <div className="absolute top-4 right-[-35px] bg-yellow-400 text-yellow-900 text-xs font-bold px-10 py-1 rotate-45 shadow-sm">
                PRÓXIMAMENTE
              </div>
              <div className="flex items-center gap-3 mb-4 opacity-70">
                <div className="bg-orange-100 p-2 rounded-lg">
                  <Wrench className="h-6 w-6 text-orange-600" />
                </div>
                <h3 className="text-lg font-bold text-gray-900">
                  Módulo Mantenimiento
                </h3>
              </div>
              <p className="text-gray-500 text-sm mb-6 flex-grow opacity-70">
                Genera órdenes de mantenimiento, asigna mecánicos y devuelve
                vehículos a la disponibilidad.
              </p>
              <button
                disabled
                className="mt-auto flex items-center justify-center gap-2 w-full bg-gray-200 text-gray-500 font-medium py-2 px-4 rounded-lg cursor-not-allowed"
              >
                Módulo Bloqueado <ArrowRight className="h-4 w-4" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}