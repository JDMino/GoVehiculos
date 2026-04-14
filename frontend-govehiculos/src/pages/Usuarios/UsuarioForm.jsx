import { useState } from "react";
import api from "../../api/axiosConfig";
import { useNavigate, Link } from "react-router-dom";
import {
  UserPlus,
  ArrowLeft,
  Mail,
  Lock,
  CreditCard,
  User,
  Shield,
  Eye,
  EyeOff,
  CheckCircle,
  AlertCircle,
  Loader2,
} from "lucide-react";

export default function UsuarioForm() {
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

  const handleChange = (e) =>
    setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await api.post("/usuarios", form);
      navigate("/usuarios");
    } catch (error) {
      console.error("Detalles del error:", error);
      alert("Error al registrar el usuario. Verifique los datos.");
    } finally {
      setLoading(false);
    }
  };

  const roles = [
    {
      id: 1,
      name: "Cliente",
      description: "Acceso basico para alquiler de vehiculos",
      color: "slate",
    },
    {
      id: 2,
      name: "Socio",
      description: "Puede registrar sus propios vehiculos",
      color: "amber",
    },
    {
      id: 3,
      name: "Empleado",
      description: "Gestiona mantenimiento",
      color: "blue",
    },
    {
      id: 4,
      name: "Administrador",
      description: "Control total del sistema",
      color: "violet",
    },
  ];

  return (
    <div className="min-h-screen bg-slate-50 py-8 px-6">
      <div className="max-w-3xl mx-auto">
        {/* Back Button */}
        <Link
          to="/usuarios"
          className="inline-flex items-center gap-2 text-slate-500 hover:text-slate-900 font-medium mb-6 transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          Volver al directorio
        </Link>

        {/* Main Card */}
        <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden shadow-sm">
          {/* Header */}
          <div className="bg-gradient-to-r from-slate-800 to-slate-900 p-8">
            <div className="flex items-center gap-5">
              <div className="h-16 w-16 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center">
                <UserPlus className="h-8 w-8 text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-white tracking-tight">
                  Nuevo Usuario
                </h1>
                <p className="text-slate-300 mt-1">
                  Registra una nueva cuenta en el sistema
                </p>
              </div>
            </div>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="p-8">
            {/* Personal Info Section */}
            <div className="mb-8">
              <div className="flex items-center gap-2 mb-6">
                <div className="h-8 w-8 rounded-lg bg-slate-100 flex items-center justify-center">
                  <User className="h-4 w-4 text-slate-600" />
                </div>
                <h2 className="text-sm font-semibold text-slate-900 uppercase tracking-wider">
                  Informacion Personal
                </h2>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Nombre
                  </label>
                  <input
                    type="text"
                    name="nombre"
                    value={form.nombre}
                    onChange={handleChange}
                    placeholder="Ingresa el nombre"
                    className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 focus:border-transparent outline-none transition-all placeholder:text-slate-400"
                    required
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Apellido
                  </label>
                  <input
                    type="text"
                    name="apellido"
                    value={form.apellido}
                    onChange={handleChange}
                    placeholder="Ingresa el apellido"
                    className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 focus:border-transparent outline-none transition-all placeholder:text-slate-400"
                    required
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Email
                  </label>
                  <div className="relative">
                    <Mail className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                    <input
                      type="email"
                      name="email"
                      value={form.email}
                      onChange={handleChange}
                      placeholder="correo@ejemplo.com"
                      className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 focus:border-transparent outline-none transition-all placeholder:text-slate-400"
                      required
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    DNI / Documento
                  </label>
                  <div className="relative">
                    <CreditCard className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                    <input
                      type="text"
                      name="dni"
                      value={form.dni}
                      onChange={handleChange}
                      placeholder="12345678"
                      className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 focus:border-transparent outline-none transition-all placeholder:text-slate-400"
                      required
                    />
                  </div>
                </div>
              </div>
            </div>

            {/* Security Section */}
            <div className="mb-8 pt-8 border-t border-slate-100">
              <div className="flex items-center gap-2 mb-6">
                <div className="h-8 w-8 rounded-lg bg-slate-100 flex items-center justify-center">
                  <Lock className="h-4 w-4 text-slate-600" />
                </div>
                <h2 className="text-sm font-semibold text-slate-900 uppercase tracking-wider">
                  Seguridad
                </h2>
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-700">
                  Contrasena Provisional
                </label>
                <div className="relative max-w-md">
                  <Lock className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                  <input
                    type={showPassword ? "text" : "password"}
                    name="password"
                    value={form.password}
                    onChange={handleChange}
                    placeholder="Minimo 4 caracteres"
                    className="w-full pl-12 pr-12 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 focus:border-transparent outline-none transition-all placeholder:text-slate-400"
                    required
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 transition-colors"
                  >
                    {showPassword ? (
                      <EyeOff className="h-5 w-5" />
                    ) : (
                      <Eye className="h-5 w-5" />
                    )}
                  </button>
                </div>
                <p className="text-xs text-slate-500 mt-2">
                  El usuario debera cambiar su contrasena en el primer inicio de
                  sesion
                </p>
              </div>
            </div>

            {/* Role Section */}
            <div className="mb-8 pt-8 border-t border-slate-100">
              <div className="flex items-center gap-2 mb-6">
                <div className="h-8 w-8 rounded-lg bg-slate-100 flex items-center justify-center">
                  <Shield className="h-4 w-4 text-slate-600" />
                </div>
                <h2 className="text-sm font-semibold text-slate-900 uppercase tracking-wider">
                  Nivel de Acceso
                </h2>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                {roles.map((role) => (
                  <label
                    key={role.id}
                    className={`relative flex items-start gap-4 p-4 rounded-xl border-2 cursor-pointer transition-all ${
                      parseInt(form.rolId) === role.id
                        ? "border-slate-900 bg-slate-50"
                        : "border-slate-200 hover:border-slate-300 bg-white"
                    }`}
                  >
                    <input
                      type="radio"
                      name="rolId"
                      value={role.id}
                      checked={parseInt(form.rolId) === role.id}
                      onChange={handleChange}
                      className="sr-only"
                    />
                    <div
                      className={`h-10 w-10 rounded-lg flex items-center justify-center shrink-0 ${
                        role.color === "violet"
                          ? "bg-violet-100"
                          : role.color === "blue"
                            ? "bg-blue-100"
                            : role.color === "amber"
                              ? "bg-amber-100"
                              : "bg-slate-100"
                      }`}
                    >
                      <Shield
                        className={`h-5 w-5 ${
                          role.color === "violet"
                            ? "text-violet-600"
                            : role.color === "blue"
                              ? "text-blue-600"
                              : role.color === "amber"
                                ? "text-amber-600"
                                : "text-slate-600"
                        }`}
                      />
                    </div>
                    <div className="flex-1">
                      <span className="font-semibold text-slate-900 block">
                        {role.name}
                      </span>
                      <span className="text-sm text-slate-500">
                        {role.description}
                      </span>
                    </div>
                    {parseInt(form.rolId) === role.id && (
                      <CheckCircle className="h-5 w-5 text-slate-900 shrink-0" />
                    )}
                  </label>
                ))}
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center justify-between pt-8 border-t border-slate-100">
              <Link
                to="/usuarios"
                className="px-6 py-3 text-slate-600 font-semibold hover:text-slate-900 transition-colors"
              >
                Cancelar
              </Link>
              <button
                type="submit"
                disabled={loading}
                className="inline-flex items-center gap-2 px-8 py-3 bg-slate-900 hover:bg-slate-800 disabled:bg-slate-400 text-white font-semibold rounded-xl transition-all shadow-lg shadow-slate-200 hover:shadow-xl disabled:shadow-none"
              >
                {loading ? (
                  <>
                    <Loader2 className="h-5 w-5 animate-spin" />
                    Registrando...
                  </>
                ) : (
                  <>
                    <UserPlus className="h-5 w-5" />
                    Guardar Usuario
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
