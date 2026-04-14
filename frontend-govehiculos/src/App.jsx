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

// 1. COMPONENTE DE PROTECCIÓN: Verifica sesión y roles
const ProtectedRoute = ({ allowedRoles, children }) => {
  const { isAuthenticated, user } = useAuthStore();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  const userRolId = user?.rolId || 1;
  if (allowedRoles && !allowedRoles.includes(userRolId)) {
    return <Navigate to="/home" replace />;
  }

  return children;
};

// 2. COMPONENTE LAYOUT: Renderiza Navbar/Footer condicionalmente
const Layout = ({ children }) => {
  const location = useLocation();
  const { user, logout } = useAuthStore(); // 👈 obtenemos user y logout del store
  const isAuthPage = location.pathname === "/login" || location.pathname === "/register";

  if (isAuthPage) return <>{children}</>;

  return (
    <div className="flex flex-col min-h-screen bg-gray-50">
      {/* Pasamos user y logout al Navbar */}
      <Navbar user={user} onLogout={logout} />
      <main className="flex-grow">{children}</main>
      <Footer />
    </div>
  );
};

export default function App() {
  return (
    <BrowserRouter>
      <Layout>
        <Routes>
          {/* Redirección por defecto */}
          <Route path="/" element={<Navigate to="/home" replace />} />

          {/* Rutas Públicas */}
          <Route path="/home" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          {/* Dashboard (solo roles 3 y 4) */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute allowedRoles={[3, 4]}>
                <Dashboard />
              </ProtectedRoute>
            }
          />

          {/* MÓDULO VEHÍCULOS (Accesible por todos los logueados) */}
          <Route
            path="/vehiculos"
            element={
              <ProtectedRoute allowedRoles={[1, 2, 3, 4]}>
                <VehiculosList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/vehiculos/nuevo"
            element={
              <ProtectedRoute allowedRoles={[2, 4]}>
                <VehiculoForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/vehiculos/editar/:id"
            element={
              <ProtectedRoute allowedRoles={[2, 4]}>
                <VehiculoEdit />
              </ProtectedRoute>
            }
          />

          {/* MÓDULO USUARIOS (Solo Administrador rol 4) */}
          <Route
            path="/usuarios"
            element={
              <ProtectedRoute allowedRoles={[4]}>
                <UsuariosList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/usuarios/nuevo"
            element={
              <ProtectedRoute allowedRoles={[4]}>
                <UsuarioForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/usuarios/editar/:id"
            element={
              <ProtectedRoute allowedRoles={[4]}>
                <UsuarioEdit />
              </ProtectedRoute>
            }
          />

          {/* Fallback para cualquier otra ruta inexistente */}
          <Route path="*" element={<Navigate to="/home" replace />} />
        </Routes>
      </Layout>
    </BrowserRouter>
  );
}
