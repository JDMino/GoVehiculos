import { BrowserRouter, Routes, Route } from "react-router-dom";
import UsuariosList from "./pages/Usuarios/UsuariosList";
import UsuarioForm from "./pages/Usuarios/UsuarioForm";
import UsuarioEdit from "./pages/Usuarios/UsuarioEdit";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/usuarios" element={<UsuariosList />} />
        <Route path="/usuarios/nuevo" element={<UsuarioForm />} />
        <Route path="/usuarios/editar/:id" element={<UsuarioEdit />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
