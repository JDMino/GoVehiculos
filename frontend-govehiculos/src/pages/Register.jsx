import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import {
  User,
  Mail,
  Lock,
  CreditCard,
  UserPlus,
  ArrowLeft,
  Car,
  Eye,
  EyeOff,
  ChevronDown,
  Check,
} from "lucide-react";
import useAuthStore from "../context/AuthStore";

export default function Register() {
  const register = useAuthStore((state) => state.register);
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [form, setForm] = useState({
    nombre: "",
    apellido: "",
    email: "",
    dni: "",
    password: "",
    rolId: 1,
  });

  const roles = [
    { id: 1, name: "Cliente" },
    { id: 2, name: "Socio" },
    { id: 3, name: "Empleado" },
    { id: 4, name: "Administrador" },
  ];

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    const success = await register(form);
    setLoading(false);

    if (success) {
      navigate("/login");
    } else {
      alert("Hubo un error al procesar el registro. Intentalo de nuevo.");
    }
  };

  return (
    <div className="min-h-screen w-full flex">
      {/* Panel izquierdo - Imagen */}
      <div className="hidden lg:flex lg:w-2/5 relative overflow-hidden">
        <div
          className="absolute inset-0 bg-cover bg-center"
          style={{
            backgroundImage:
              "url('https://images.unsplash.com/photo-1503376780353-7e6692767b70?q=80&w=1920&auto=format&fit=crop')",
          }}
        />
        <div className="absolute inset-0 bg-gradient-to-br from-slate-900/90 via-slate-900/70 to-slate-800/80" />

        {/* Contenido sobre la imagen */}
        <div className="relative z-10 flex flex-col justify-between p-12 text-white">
          <div className="flex items-center gap-3">
            <div className="bg-white/10 backdrop-blur-sm p-3 rounded-xl border border-white/20">
              <Car className="h-7 w-7 text-white" />
            </div>
            <span className="text-2xl font-bold tracking-tight">
              GoVehiculos
            </span>
          </div>

          <div className="space-y-6 max-w-sm">
            <h1 className="text-4xl font-bold leading-tight">
              Unete a nuestra comunidad
            </h1>
            <p className="text-lg text-slate-300 leading-relaxed">
              Crea tu cuenta y accede a beneficios exclusivos en alquiler y
              compra de vehiculos.
            </p>

            <div className="space-y-4 pt-4">
              {[
                "Reservas rapidas y seguras",
                "Descuentos exclusivos para miembros",
                "Soporte 24/7 personalizado",
              ].map((benefit, i) => (
                <div key={i} className="flex items-center gap-3">
                  <div className="flex-shrink-0 w-6 h-6 rounded-full bg-white/20 flex items-center justify-center">
                    <Check className="h-4 w-4 text-white" />
                  </div>
                  <span className="text-slate-300">{benefit}</span>
                </div>
              ))}
            </div>
          </div>

          <p className="text-sm text-slate-500">
            2024 GoVehiculos. Todos los derechos reservados.
          </p>
        </div>
      </div>

      {/* Panel derecho - Formulario */}
      <div className="w-full lg:w-3/5 flex items-center justify-center p-6 sm:p-12 bg-slate-50">
        <div className="w-full max-w-xl space-y-8">
          {/* Header con navegacion */}
          <div className="flex items-center justify-between">
            <Link
              to="/login"
              className="flex items-center gap-2 text-slate-600 hover:text-slate-900 transition-colors group"
            >
              <ArrowLeft className="h-4 w-4 group-hover:-translate-x-1 transition-transform" />
              <span className="text-sm font-medium">Volver al login</span>
            </Link>

            {/* Logo mobile */}
            <div className="lg:hidden flex items-center gap-2">
              <div className="bg-slate-900 p-2 rounded-lg">
                <Car className="h-5 w-5 text-white" />
              </div>
              <span className="text-lg font-bold text-slate-900">
                GoVehiculos
              </span>
            </div>
          </div>

          <div className="space-y-2">
            <div className="flex items-center gap-3">
              <div className="bg-slate-900 p-2.5 rounded-xl">
                <UserPlus className="h-5 w-5 text-white" />
              </div>
              <h2 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight">
                Crea tu cuenta
              </h2>
            </div>
            <p className="text-slate-600">
              Completa tus datos para unirte a GoVehiculos
            </p>
          </div>

          <form className="space-y-6" onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              {/* Nombre */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Nombre
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                    <User className="h-5 w-5 text-slate-400" />
                  </div>
                  <input
                    name="nombre"
                    required
                    className="w-full px-4 py-3.5 pl-12 bg-white border border-slate-200 rounded-xl text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all duration-200"
                    placeholder="Ej: Juan"
                    value={form.nombre}
                    onChange={handleChange}
                  />
                </div>
              </div>

              {/* Apellido */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Apellido
                </label>
                <input
                  name="apellido"
                  required
                  className="w-full px-4 py-3.5 bg-white border border-slate-200 rounded-xl text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all duration-200"
                  placeholder="Ej: Perez"
                  value={form.apellido}
                  onChange={handleChange}
                />
              </div>

              {/* DNI */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  DNI
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                    <CreditCard className="h-5 w-5 text-slate-400" />
                  </div>
                  <input
                    name="dni"
                    required
                    className="w-full px-4 py-3.5 pl-12 bg-white border border-slate-200 rounded-xl text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all duration-200"
                    placeholder="Numero de documento"
                    value={form.dni}
                    onChange={handleChange}
                  />
                </div>
              </div>

              {/* Email */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Correo electronico
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                    <Mail className="h-5 w-5 text-slate-400" />
                  </div>
                  <input
                    name="email"
                    type="email"
                    required
                    className="w-full px-4 py-3.5 pl-12 bg-white border border-slate-200 rounded-xl text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all duration-200"
                    placeholder="ejemplo@correo.com"
                    value={form.email}
                    onChange={handleChange}
                  />
                </div>
              </div>

              {/* Password */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Contrasena
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                    <Lock className="h-5 w-5 text-slate-400" />
                  </div>
                  <input
                    name="password"
                    type={showPassword ? "text" : "password"}
                    required
                    className="w-full px-4 py-3.5 pl-12 pr-12 bg-white border border-slate-200 rounded-xl text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all duration-200"
                    placeholder="Minimo 6 caracteres"
                    value={form.password}
                    onChange={handleChange}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute inset-y-0 right-0 pr-4 flex items-center text-slate-400 hover:text-slate-600 transition-colors"
                  >
                    {showPassword ? (
                      <EyeOff className="h-5 w-5" />
                    ) : (
                      <Eye className="h-5 w-5" />
                    )}
                  </button>
                </div>
              </div>

              {/* Rol */}
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Tipo de usuario
                </label>
                <div className="relative">
                  <select
                    name="rolId"
                    className="w-full px-4 py-3.5 bg-white border border-slate-200 rounded-xl text-slate-900 focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all duration-200 appearance-none cursor-pointer"
                    value={form.rolId}
                    onChange={handleChange}
                  >
                    {roles.map((role) => (
                      <option key={role.id} value={role.id}>
                        {role.name}
                      </option>
                    ))}
                  </select>
                  <div className="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none">
                    <ChevronDown className="h-5 w-5 text-slate-400" />
                  </div>
                </div>
              </div>
            </div>

            {/* Terminos y condiciones */}
            <div className="flex items-start gap-3">
              <input
                type="checkbox"
                required
                id="terms"
                className="mt-1 h-4 w-4 rounded border-slate-300 text-slate-900 focus:ring-slate-900"
              />
              <label
                htmlFor="terms"
                className="text-sm text-slate-600 leading-relaxed"
              >
                Acepto los{" "}
                <Link
                  to="/terms"
                  className="font-medium text-slate-900 hover:underline"
                >
                  Terminos y Condiciones
                </Link>{" "}
                y la{" "}
                <Link
                  to="/privacy"
                  className="font-medium text-slate-900 hover:underline"
                >
                  Politica de Privacidad
                </Link>
              </label>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full flex items-center justify-center gap-2 py-3.5 px-4 bg-slate-900 hover:bg-slate-800 text-white font-semibold rounded-xl transition-all duration-200 disabled:opacity-70 disabled:cursor-not-allowed shadow-lg shadow-slate-900/20"
            >
              {loading ? (
                <>
                  <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24">
                    <circle
                      className="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      strokeWidth="4"
                      fill="none"
                    />
                    <path
                      className="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    />
                  </svg>
                  <span>Creando cuenta...</span>
                </>
              ) : (
                <>
                  <UserPlus className="h-5 w-5" />
                  <span>Crear cuenta</span>
                </>
              )}
            </button>
          </form>

          <p className="text-center text-slate-600">
            Ya tienes una cuenta?{" "}
            <Link
              to="/login"
              className="font-semibold text-slate-900 hover:underline underline-offset-4"
            >
              Inicia sesion
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
