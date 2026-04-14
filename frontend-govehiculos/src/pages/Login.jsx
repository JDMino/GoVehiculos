import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { Mail, Lock, LogIn, Car, Eye, EyeOff } from "lucide-react";
import useAuthStore from "../context/AuthStore";

export default function Login() {
  const login = useAuthStore((state) => state.login);
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [errorMessage, setErrorMessage] = useState(""); 

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setErrorMessage(""); // limpiamos error previo

    const result = await login(email, password);
    setLoading(false);

    if (result.success) {
      const { rolId } = useAuthStore.getState().user;
      if (rolId === 1 || rolId === 2) {
        navigate("/home");
      } else if (rolId === 3 || rolId === 4) {
        navigate("/dashboard");
      }
    } else {
      setErrorMessage("Credenciales inválidas o cuenta inactiva. Contacta al administrador.");
    }
  };

  return (
    <div className="min-h-screen w-full flex">
      {/* Panel izquierdo - Imagen */}
      <div className="hidden lg:flex lg:w-1/2 relative overflow-hidden">
        <div
          className="absolute inset-0 bg-cover bg-center"
          style={{
            backgroundImage:
              "url('https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?q=80&w=1920&auto=format&fit=crop')",
          }}
        />
        <div className="absolute inset-0 bg-gradient-to-br from-slate-900/90 via-slate-900/70 to-slate-800/80" />
        <div className="relative z-10 flex flex-col justify-between p-12 text-white">
          <div className="flex items-center gap-3">
            <div className="bg-white/10 backdrop-blur-sm p-3 rounded-xl border border-white/20">
              <Car className="h-7 w-7 text-white" />
            </div>
            <span className="text-2xl font-bold tracking-tight">GoVehiculos</span>
          </div>
          <div className="space-y-6 max-w-md">
            <h1 className="text-4xl font-bold leading-tight">Tu próximo viaje comienza aquí</h1>
            <p className="text-lg text-slate-300 leading-relaxed">
              Accede a la mejor selección de vehículos con la confianza y seguridad que mereces.
            </p>
          </div>
          <p className="text-sm text-slate-500">2026 GoVehiculos. Todos los derechos reservados.</p>
        </div>
      </div>

      {/* Panel derecho - Formulario */}
      <div className="w-full lg:w-1/2 flex items-center justify-center p-6 sm:p-12 bg-slate-50">
        <div className="w-full max-w-md space-y-8">
          <div className="lg:hidden flex items-center justify-center gap-3 mb-8">
            <div className="bg-slate-900 p-3 rounded-xl">
              <Car className="h-6 w-6 text-white" />
            </div>
            <span className="text-xl font-bold text-slate-900">GoVehiculos</span>
          </div>

          <div className="space-y-2">
            <h2 className="text-3xl font-bold text-slate-900 tracking-tight">Bienvenido de nuevo</h2>
            <p className="text-slate-600">Ingresa tus credenciales para acceder a tu cuenta</p>
          </div>

          {/* Banner de error */}
          {errorMessage && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative">
              <strong className="font-bold">Error: </strong>
              <span className="block sm:inline">{errorMessage}</span>
            </div>
          )}

          <form className="space-y-5" onSubmit={handleSubmit}>
            <div className="space-y-4">
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">Correo electrónico</label>
                <div className="relative">
                  <Mail className="absolute left-3 top-3 h-5 w-5 text-slate-400" />
                  <input
                    type="email"
                    required
                    className="w-full pl-10 pr-3 py-2 border rounded-lg"
                    placeholder="ejemplo@correo.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                  />
                </div>
              </div>

              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <label className="text-sm font-medium text-slate-700">Contraseña</label>
                  <Link
                    to="/forgot-password"
                    className="text-sm font-medium text-slate-600 hover:text-slate-900 transition-colors"
                  >
                    ¿Olvidaste tu contraseña?
                  </Link>
                </div>
                <div className="relative">
                  <Lock className="absolute left-3 top-3 h-5 w-5 text-slate-400" />
                  <input
                    type={showPassword ? "text" : "password"}
                    required
                    className="w-full pl-10 pr-10 py-2 border rounded-lg"
                    placeholder="Ingresa tu contraseña"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-3 top-3 text-slate-400 hover:text-slate-600"
                  >
                    {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                  </button>
                </div>
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full flex items-center justify-center gap-2 py-3 bg-slate-900 text-white rounded-lg hover:bg-slate-800 transition disabled:opacity-70 disabled:cursor-not-allowed"
            >
              {loading ? "Ingresando..." : <><LogIn className="h-5 w-5" /> Iniciar sesión</>}
            </button>
          </form>

          <p className="text-center text-slate-600">
            ¿No tienes una cuenta?{" "}
            <Link to="/register" className="font-semibold text-slate-900 hover:underline">
              Regístrate gratis
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
