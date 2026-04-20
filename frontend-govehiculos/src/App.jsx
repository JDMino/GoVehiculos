import { BrowserRouter, Routes, Route, Navigate, useLocation } from "react-router-dom";
import useAuthStore from "./context/AuthStore";

// Componentes Globales
import Navbar from "./components/Navbar";
import Footer from "./components/Footer";

// Páginas
import Home from "./pages/Home";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import UsuariosList from "./pages/Usuarios/UsuariosList";
import UsuarioForm from "./pages/Usuarios/UsuarioForm";
import UsuarioEdit from "./pages/Usuarios/UsuarioEdit";
import VehiculosList from "./pages/Vehiculos/VehiculosList";
import VehiculoForm from "./pages/Vehiculos/VehiculoForm";
import VehiculoEdit from "./pages/Vehiculos/VehiculoEdit";

// Mantenimientos — vistas separadas por rol
import MantenimientosList    from "./pages/Mantenimientos/MantenimientosList";
import MantenimientoForm     from "./pages/Mantenimientos/MantenimientoForm";
import MantenimientosEmpleado  from "./pages/Mantenimientos/MantenimientosEmpleado";
import MantenimientosHistorial from "./pages/Mantenimientos/MantenimientosHistorial";

const ROLES = {
  CLIENTE:       1,
  SOCIO:         2,
  EMPLEADO:      3,
  ADMINISTRADOR: 4,
};

// ── Protección de rutas por rol ─────────────────────────────────────────────
const ProtectedRoute = ({ allowedRoles, children }) => {
  const { isAuthenticated, user } = useAuthStore();

  if (!isAuthenticated) return <Navigate to="/login" replace />;

  const userRolId = user?.rolId || ROLES.CLIENTE;
  if (allowedRoles && !allowedRoles.includes(userRolId))
    return <Navigate to="/home" replace />;

  return children;
};

// ── Layout con Navbar/Footer condicional ────────────────────────────────────
const Layout = ({ children }) => {
  const location = useLocation();
  const { user, logout } = useAuthStore();
  const isAuthPage =
    location.pathname === "/login" || location.pathname === "/register";

  if (isAuthPage) return <>{children}</>;

  return (
    <div className="flex flex-col min-h-screen bg-gray-50">
      <Navbar user={user} onLogout={logout} />
      <main className="flex-grow">{children}</main>
      <Footer />
    </div>
  );
};

// ── Selector de vista de mantenimiento según rol ────────────────────────────
const MantenimientosRouter = () => {
  const { user } = useAuthStore();
  if (user?.rolId === ROLES.ADMINISTRADOR) return <MantenimientosList />;
  if (user?.rolId === ROLES.EMPLEADO)      return <MantenimientosEmpleado user={user} />;
  return <Navigate to="/home" replace />;
};

// ── App ─────────────────────────────────────────────────────────────────────
export default function App() {
  return (
    <BrowserRouter>
      <Layout>
        <Routes>
          {/* Redirección raíz */}
          <Route path="/" element={<Navigate to="/home" replace />} />

          {/* Rutas públicas */}
          <Route path="/home"     element={<Home />}     />
          <Route path="/login"    element={<Login />}    />
          <Route path="/register" element={<Register />} />

          {/* Dashboard — empleado y administrador */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute allowedRoles={[ROLES.ADMINISTRADOR]}>
                <Dashboard />
              </ProtectedRoute>
            }
          />

          {/* ── VEHÍCULOS ──────────────────────────────────────────────── */}
          <Route
            path="/vehiculos"
            element={
              <ProtectedRoute allowedRoles={[ROLES.CLIENTE, ROLES.SOCIO, ROLES.EMPLEADO, ROLES.ADMINISTRADOR]}>
                <VehiculosList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/vehiculos/nuevo"
            element={
              <ProtectedRoute allowedRoles={[ROLES.SOCIO, ROLES.ADMINISTRADOR]}>
                <VehiculoForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/vehiculos/editar/:id"
            element={
              <ProtectedRoute allowedRoles={[ROLES.SOCIO, ROLES.ADMINISTRADOR]}>
                <VehiculoEdit />
              </ProtectedRoute>
            }
          />

          {/* ── USUARIOS ───────────────────────────────────────────────── */}
          <Route
            path="/usuarios"
            element={
              <ProtectedRoute allowedRoles={[ROLES.ADMINISTRADOR]}>
                <UsuariosList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/usuarios/nuevo"
            element={
              <ProtectedRoute allowedRoles={[ROLES.ADMINISTRADOR]}>
                <UsuarioForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/usuarios/editar/:id"
            element={
              <ProtectedRoute allowedRoles={[ROLES.ADMINISTRADOR]}>
                <UsuarioEdit />
              </ProtectedRoute>
            }
          />

          {/* ── MANTENIMIENTOS ─────────────────────────────────────────── */}

          {/* Ruta principal — selector por rol */}
          <Route
            path="/mantenimientos"
            element={
              <ProtectedRoute allowedRoles={[ROLES.EMPLEADO, ROLES.ADMINISTRADOR]}>
                <MantenimientosRouter />
              </ProtectedRoute>
            }
          />

          {/* Historial — solo administrador */}
          <Route
            path="/mantenimientos/historial"
            element={
              <ProtectedRoute allowedRoles={[ROLES.ADMINISTRADOR]}>
                <MantenimientosHistorial />
              </ProtectedRoute>
            }
          />

          {/* Crear orden — solo administrador */}
          <Route
            path="/mantenimientos/nuevo/:vehiculoId"
            element={
              <ProtectedRoute allowedRoles={[ROLES.ADMINISTRADOR]}>
                <MantenimientoForm />
              </ProtectedRoute>
            }
          />

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/home" replace />} />
        </Routes>
      </Layout>
    </BrowserRouter>
  );
}
