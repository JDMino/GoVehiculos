import { useEffect, useState } from "react";
import api from "../../api/axiosConfig";
import { useNavigate, useParams, Link } from "react-router-dom";
import {
  Save,
  ArrowLeft,
  User,
  Shield,
  Phone,
  MapPin,
  UserCheck,
  Loader2,
  AlertTriangle,
  CheckCircle,
  XCircle,
  Lock,
  Unlock,
} from "lucide-react";

export default function UsuarioEdit() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({
    nombre: "",
    apellido: "",
    telefono: "",
    direccion: "",
    bloqueado: false,
    activo: true,
    rolId: 1,
  });

  useEffect(() => {
    const fetchUser = async () => {
      try {
        const res = await api.get(`/usuarios/${id}`);
        setForm(res.data);
      } catch (error) {
        console.error("Detalles del error:", error);
        alert("Error al cargar los datos del usuario.");
        navigate("/usuarios");
      } finally {
        setLoading(false);
      }
    };
    fetchUser();
  }, [id, navigate]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm({
      ...form,
      [name]:
        type === "checkbox"
          ? checked
          : name === "rolId"
            ? parseInt(value)
            : value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.put(`/usuarios/${id}`, form);
      navigate("/usuarios");
    } catch (error) {
      console.error("Detalles del error:", error);
      alert("Error al actualizar el usuario.");
    } finally {
      setSaving(false);
    }
  };

  const roles = [
    { id: 1, name: "Cliente", color: "slate" },
    { id: 2, name: "Socio", color: "amber" },
    { id: 3, name: "Empleado", color: "blue" },
    { id: 4, name: "Administrador", color: "violet" },
  ];

  if (loading)
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <div className="flex flex-col items-center gap-4">
          <div className="h-12 w-12 rounded-xl bg-slate-900 flex items-center justify-center animate-pulse">
            <User className="h-6 w-6 text-white" />
          </div>
          <p className="text-slate-500 font-medium">
            Cargando datos del usuario...
          </p>
        </div>
      </div>
    );

  return (
    <div className="min-h-screen bg-slate-50 py-8 px-6">
      <div className="max-w-4xl mx-auto">
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
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
              <div className="flex items-center gap-5">
                <div className="h-16 w-16 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center text-white text-xl font-bold">
                  {form.nombre.charAt(0)}
                  {form.apellido.charAt(0)}
                </div>
                <div>
                  <h1 className="text-2xl font-bold text-white tracking-tight">
                    {form.nombre} {form.apellido}
                  </h1>
                  <p className="text-slate-300 mt-1">
                    Editando perfil de usuario
                  </p>
                </div>
              </div>
              <div className="flex items-center gap-2">
                {form.activo ? (
                  <span className="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-emerald-500/20 text-emerald-300 text-sm font-semibold">
                    <span className="h-2 w-2 rounded-full bg-emerald-400 animate-pulse" />
                    Cuenta Activa
                  </span>
                ) : (
                  <span className="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-red-500/20 text-red-300 text-sm font-semibold">
                    <span className="h-2 w-2 rounded-full bg-red-400" />
                    Cuenta Inactiva
                  </span>
                )}
                {form.bloqueado && (
                  <span className="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-amber-500/20 text-amber-300 text-sm font-semibold">
                    <Lock className="h-4 w-4" />
                    Bloqueado
                  </span>
                )}
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
                    className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 focus:border-transparent outline-none transition-all"
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
                    className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 focus:border-transparent outline-none transition-all"
                    required
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium text-slate-700">
                    Telefono de Contacto
                  </label>
                  <div className="relative">
                    <Phone className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                    <input
                      type="text"
                      name="telefono"
                      value={form.telefono || ""}
                      onChange={handleChange}
                      placeholder="Ej: +54 11 1234-5678"
                      className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 focus:border-transparent outline-none transition-all placeholder:text-slate-400"
                    />
                  </div>
                </div>
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

              <div className="max-w-md">
                <label className="text-sm font-medium text-slate-700 block mb-2">
                  Rol del Usuario
                </label>
                <div className="relative">
                  <select
                    name="rolId"
                    value={form.rolId || ""}
                    onChange={handleChange}
                    className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:ring-2 focus:ring-slate-900 focus:border-transparent outline-none transition-all cursor-pointer pr-12"
                  >
                    {roles.map((role) => (
                      <option key={role.id} value={role.id}>
                        {role.name}
                      </option>
                    ))}
                  </select>
                  <Shield className="absolute right-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400 pointer-events-none" />
                </div>
              </div>
            </div>

            {/* Account Status Section */}
            <div className="mb-8 pt-8 border-t border-slate-100">
              <div className="flex items-center gap-2 mb-6">
                <div className="h-8 w-8 rounded-lg bg-slate-100 flex items-center justify-center">
                  <UserCheck className="h-4 w-4 text-slate-600" />
                </div>
                <h2 className="text-sm font-semibold text-slate-900 uppercase tracking-wider">
                  Estado de la Cuenta
                </h2>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Active Toggle */}
                <label
                  className={`relative flex items-center justify-between p-5 rounded-xl border-2 cursor-pointer transition-all ${
                    form.activo
                      ? "border-emerald-200 bg-emerald-50"
                      : "border-slate-200 bg-slate-50"
                  }`}
                >
                  <div className="flex items-center gap-4">
                    <div
                      className={`h-12 w-12 rounded-xl flex items-center justify-center ${
                        form.activo ? "bg-emerald-100" : "bg-slate-200"
                      }`}
                    >
                      {form.activo ? (
                        <CheckCircle className="h-6 w-6 text-emerald-600" />
                      ) : (
                        <XCircle className="h-6 w-6 text-slate-500" />
                      )}
                    </div>
                    <div>
                      <span className="font-semibold text-slate-900 block">
                        Cuenta Activa
                      </span>
                      <span className="text-sm text-slate-500">
                        Permitir acceso al sistema
                      </span>
                    </div>
                  </div>
                  <div className="relative">
                    <input
                      type="checkbox"
                      name="activo"
                      checked={form.activo}
                      onChange={handleChange}
                      className="sr-only peer"
                    />
                    <div
                      className={`w-14 h-8 rounded-full transition-colors ${
                        form.activo ? "bg-emerald-500" : "bg-slate-300"
                      }`}
                    >
                      <div
                        className={`absolute top-1 w-6 h-6 bg-white rounded-full shadow-sm transition-transform ${
                          form.activo ? "translate-x-7" : "translate-x-1"
                        }`}
                      />
                    </div>
                  </div>
                </label>

                {/* Blocked Toggle */}
                <label
                  className={`relative flex items-center justify-between p-5 rounded-xl border-2 cursor-pointer transition-all ${
                    form.bloqueado
                      ? "border-amber-200 bg-amber-50"
                      : "border-slate-200 bg-slate-50"
                  }`}
                >
                  <div className="flex items-center gap-4">
                    <div
                      className={`h-12 w-12 rounded-xl flex items-center justify-center ${
                        form.bloqueado ? "bg-amber-100" : "bg-slate-200"
                      }`}
                    >
                      {form.bloqueado ? (
                        <Lock className="h-6 w-6 text-amber-600" />
                      ) : (
                        <Unlock className="h-6 w-6 text-slate-500" />
                      )}
                    </div>
                    <div>
                      <span className="font-semibold text-slate-900 block">
                        Bloquear Usuario
                      </span>
                      <span className="text-sm text-slate-500">
                        Restringir operaciones temporalmente
                      </span>
                    </div>
                  </div>
                  <div className="relative">
                    <input
                      type="checkbox"
                      name="bloqueado"
                      checked={form.bloqueado}
                      onChange={handleChange}
                      className="sr-only peer"
                    />
                    <div
                      className={`w-14 h-8 rounded-full transition-colors ${
                        form.bloqueado ? "bg-amber-500" : "bg-slate-300"
                      }`}
                    >
                      <div
                        className={`absolute top-1 w-6 h-6 bg-white rounded-full shadow-sm transition-transform ${
                          form.bloqueado ? "translate-x-7" : "translate-x-1"
                        }`}
                      />
                    </div>
                  </div>
                </label>
              </div>

              {form.bloqueado && (
                <div className="mt-4 p-4 rounded-xl bg-amber-50 border border-amber-200 flex items-start gap-3">
                  <AlertTriangle className="h-5 w-5 text-amber-600 shrink-0 mt-0.5" />
                  <div>
                    <p className="text-sm font-semibold text-amber-800">
                      Usuario bloqueado
                    </p>
                    <p className="text-sm text-amber-700">
                      Este usuario no podra realizar operaciones en el sistema
                      hasta que sea desbloqueado.
                    </p>
                  </div>
                </div>
              )}
            </div>

            {/* Actions */}
            <div className="flex items-center justify-between pt-8 border-t border-slate-100">
              <Link
                to="/usuarios"
                className="px-6 py-3 text-slate-600 font-semibold hover:text-slate-900 transition-colors"
              >
                Descartar cambios
              </Link>
              <button
                type="submit"
                disabled={saving}
                className="inline-flex items-center gap-2 px-8 py-3 bg-slate-900 hover:bg-slate-800 disabled:bg-slate-400 text-white font-semibold rounded-xl transition-all shadow-lg shadow-slate-200 hover:shadow-xl disabled:shadow-none"
              >
                {saving ? (
                  <>
                    <Loader2 className="h-5 w-5 animate-spin" />
                    Guardando...
                  </>
                ) : (
                  <>
                    <Save className="h-5 w-5" />
                    Aplicar Cambios
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