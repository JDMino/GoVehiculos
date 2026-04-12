import { BrowserRouter, Routes, Route } from "react-router-dom";
import Login from "./pages/Login";
import Register from "./pages/Register";

// CRUD Usuarios
import UsuariosList from "./pages/Usuarios/UsuariosList";
import UsuarioForm from "./pages/Usuarios/UsuarioForm";
import UsuarioEdit from "./pages/Usuarios/UsuarioEdit";

// CRUD Vehículos
import VehiculosList from "./pages/Vehiculos/VehiculosList";
import VehiculoForm from "./pages/Vehiculos/VehiculoForm";
import VehiculoEdit from "./pages/Vehiculos/VehiculoEdit";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Rutas públicas */}
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* Rutas protegidas (CRUD de usuarios) */}
        <Route path="/usuarios" element={<UsuariosList />} />
        <Route path="/usuarios/nuevo" element={<UsuarioForm />} />
        <Route path="/usuarios/editar/:id" element={<UsuarioEdit />} />

        {/* Rutas protegidas (CRUD de vehículos) */}
        <Route path="/vehiculos" element={<VehiculosList />} />
        <Route path="/vehiculos/nuevo" element={<VehiculoForm />} />
        <Route path="/vehiculos/editar/:id" element={<VehiculoEdit />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
