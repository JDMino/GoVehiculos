import { useEffect, useState } from "react";
import api from "../../api/axiosConfig";
import { Link } from "react-router-dom";
import { 
  UserPlus, 
  Search, 
  Edit, 
  Trash2, 
  Shield, 
  User as UserIcon, 
  CheckCircle, 
  XCircle, 
  Lock,
  Users,
  Filter,
  MoreHorizontal,
  ChevronDown,
  Mail,
  ArrowUpDown
} from "lucide-react";

export default function UsuariosList() {
  const [usuarios, setUsuarios] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterRole, setFilterRole] = useState("all");
  const [filterStatus, setFilterStatus] = useState("all");

  useEffect(() => {
    api.get("/usuarios").then((res) => setUsuarios(res.data));
  }, []);

  const handleDelete = async (id) => {
    if (confirm("¿Deseas dar de baja este usuario? El registro permanecerá pero figurará como inactivo.")) {
      try {
        await api.delete(`/usuarios/${id}`);
        setUsuarios(usuarios.map(u => u.idUsuario === id ? { ...u, activo: false } : u));
      } catch (error) {
        console.error("Detalles del error:", error);
        alert("Error al procesar la baja.");
      }
    }
  };

  const filteredUsuarios = usuarios.filter(u => {
    const matchesSearch = `${u.nombre} ${u.apellido}`.toLowerCase().includes(searchTerm.toLowerCase()) ||
      u.email.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesRole = filterRole === "all" || u.rol === filterRole;
    const matchesStatus = filterStatus === "all" || 
      (filterStatus === "active" && u.activo) || 
      (filterStatus === "inactive" && !u.activo);
    return matchesSearch && matchesRole && matchesStatus;
  });

  const stats = {
    total: usuarios.length,
    active: usuarios.filter(u => u.activo).length,
    admins: usuarios.filter(u => u.rol === 'administrador').length,
    blocked: usuarios.filter(u => u.bloqueado).length
  };

  const getRoleBadgeStyles = (rol) => {
    const styles = {
      administrador: 'bg-violet-50 text-violet-700 ring-violet-600/20',
      empleado: 'bg-blue-50 text-blue-700 ring-blue-600/20',
      socio: 'bg-amber-50 text-amber-700 ring-amber-600/20',
      cliente: 'bg-slate-50 text-slate-700 ring-slate-600/20'
    };
    return styles[rol] || styles.cliente;
  };

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Header */}
      <div className="bg-white border-b border-slate-200 sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-6 py-5">
          <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
            <div className="flex items-center gap-4">
              <div className="h-12 w-12 rounded-2xl bg-gradient-to-br from-slate-800 to-slate-900 flex items-center justify-center shadow-lg shadow-slate-200">
                <Users className="h-6 w-6 text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Directorio de Usuarios</h1>
                <p className="text-slate-500 text-sm">Gestiona el personal, socios y clientes del sistema</p>
              </div>
            </div>
            <Link
              to="/usuarios/nuevo"
              className="inline-flex items-center justify-center px-5 py-2.5 bg-slate-900 hover:bg-slate-800 text-white font-semibold rounded-xl transition-all shadow-lg shadow-slate-200 hover:shadow-xl hover:shadow-slate-200 hover:-translate-y-0.5"
            >
              <UserPlus className="h-5 w-5 mr-2" />
              Nuevo Usuario
            </Link>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-6 py-8">
        {/* Stats Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          <div className="bg-white rounded-2xl p-5 border border-slate-200 hover:border-slate-300 transition-colors">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-500">Total Usuarios</p>
                <p className="text-3xl font-bold text-slate-900 mt-1">{stats.total}</p>
              </div>
              <div className="h-12 w-12 rounded-xl bg-slate-100 flex items-center justify-center">
                <Users className="h-6 w-6 text-slate-600" />
              </div>
            </div>
          </div>
          <div className="bg-white rounded-2xl p-5 border border-slate-200 hover:border-emerald-200 transition-colors">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-500">Activos</p>
                <p className="text-3xl font-bold text-emerald-600 mt-1">{stats.active}</p>
              </div>
              <div className="h-12 w-12 rounded-xl bg-emerald-50 flex items-center justify-center">
                <CheckCircle className="h-6 w-6 text-emerald-600" />
              </div>
            </div>
          </div>
          <div className="bg-white rounded-2xl p-5 border border-slate-200 hover:border-violet-200 transition-colors">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-500">Administradores</p>
                <p className="text-3xl font-bold text-violet-600 mt-1">{stats.admins}</p>
              </div>
              <div className="h-12 w-12 rounded-xl bg-violet-50 flex items-center justify-center">
                <Shield className="h-6 w-6 text-violet-600" />
              </div>
            </div>
          </div>
          <div className="bg-white rounded-2xl p-5 border border-slate-200 hover:border-amber-200 transition-colors">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-500">Bloqueados</p>
                <p className="text-3xl font-bold text-amber-600 mt-1">{stats.blocked}</p>
              </div>
              <div className="h-12 w-12 rounded-xl bg-amber-50 flex items-center justify-center">
                <Lock className="h-6 w-6 text-amber-600" />
              </div>
            </div>
          </div>
        </div>

        {/* Search and Filters */}
        <div className="bg-white rounded-2xl border border-slate-200 p-4 mb-6">
          <div className="flex flex-col lg:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 text-slate-400 h-5 w-5" />
              <input
                type="text"
                placeholder="Buscar por nombre, apellido o email..."
                className="w-full pl-12 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 focus:border-transparent transition-all text-slate-900 placeholder:text-slate-400"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            <div className="flex gap-3">
              <div className="relative">
                <select
                  value={filterRole}
                  onChange={(e) => setFilterRole(e.target.value)}
                  className="appearance-none pl-4 pr-10 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer text-slate-700 font-medium"
                >
                  <option value="all">Todos los roles</option>
                  <option value="administrador">Administrador</option>
                  <option value="empleado">Empleado</option>
                  <option value="socio">Socio</option>
                  <option value="cliente">Cliente</option>
                </select>
                <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400 pointer-events-none" />
              </div>
              <div className="relative">
                <select
                  value={filterStatus}
                  onChange={(e) => setFilterStatus(e.target.value)}
                  className="appearance-none pl-4 pr-10 py-3 bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer text-slate-700 font-medium"
                >
                  <option value="all">Todos los estados</option>
                  <option value="active">Activos</option>
                  <option value="inactive">Inactivos</option>
                </select>
                <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400 pointer-events-none" />
              </div>
            </div>
          </div>
        </div>

        {/* Users Table */}
        <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-slate-100">
                  <th className="px-6 py-4 text-left">
                    <button className="flex items-center gap-2 text-xs font-semibold text-slate-500 uppercase tracking-wider hover:text-slate-700">
                      Usuario
                      <ArrowUpDown className="h-3.5 w-3.5" />
                    </button>
                  </th>
                  <th className="px-6 py-4 text-left">
                    <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Contacto</span>
                  </th>
                  <th className="px-6 py-4 text-center">
                    <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Rol</span>
                  </th>
                  <th className="px-6 py-4 text-center">
                    <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Estado</span>
                  </th>
                  <th className="px-6 py-4 text-right">
                    <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">Acciones</span>
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {filteredUsuarios.map((u) => (
                  <tr key={u.idUsuario} className="hover:bg-slate-50/50 transition-colors group">
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-4">
                        <div className="h-11 w-11 rounded-xl bg-gradient-to-br from-slate-700 to-slate-800 flex items-center justify-center text-white font-bold text-sm shadow-sm">
                          {u.nombre.charAt(0)}{u.apellido.charAt(0)}
                        </div>
                        <div>
                          <div className="font-semibold text-slate-900">{u.nombre} {u.apellido}</div>
                          <div className="text-xs text-slate-400 font-mono">ID #{u.idUsuario}</div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2 text-sm text-slate-600">
                        <Mail className="h-4 w-4 text-slate-400" />
                        {u.email}
                      </div>
                    </td>
                    <td className="px-6 py-4 text-center">
                      <span className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold ring-1 ring-inset ${getRoleBadgeStyles(u.rol)}`}>
                        {u.rol === 'administrador' && <Shield className="h-3.5 w-3.5" />}
                        {u.rol.charAt(0).toUpperCase() + u.rol.slice(1)}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-center gap-2">
                        {u.activo ? (
                          <span className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-emerald-50 text-emerald-700 ring-1 ring-inset ring-emerald-600/20">
                            <span className="h-1.5 w-1.5 rounded-full bg-emerald-500 animate-pulse" />
                            Activo
                          </span>
                        ) : (
                          <span className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-red-50 text-red-700 ring-1 ring-inset ring-red-600/20">
                            <span className="h-1.5 w-1.5 rounded-full bg-red-500" />
                            Inactivo
                          </span>
                        )}
                        {u.bloqueado && (
                          <span className="inline-flex items-center p-1.5 rounded-lg bg-amber-50 text-amber-600 ring-1 ring-inset ring-amber-600/20" title="Cuenta bloqueada">
                            <Lock className="h-3.5 w-3.5" />
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-end gap-1">
                        <Link
                          to={`/usuarios/editar/${u.idUsuario}`}
                          className="p-2.5 text-slate-400 hover:text-slate-900 hover:bg-slate-100 rounded-xl transition-all"
                          title="Editar usuario"
                        >
                          <Edit className="h-4.5 w-4.5" />
                        </Link>
                        <button
                          onClick={() => handleDelete(u.idUsuario)}
                          className="p-2.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-xl transition-all"
                          title="Dar de baja"
                        >
                          <Trash2 className="h-4.5 w-4.5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          
          {filteredUsuarios.length === 0 && (
            <div className="text-center py-16">
              <div className="h-16 w-16 rounded-2xl bg-slate-100 flex items-center justify-center mx-auto mb-4">
                <UserIcon className="h-8 w-8 text-slate-400" />
              </div>
              <p className="text-slate-900 font-semibold mb-1">No se encontraron usuarios</p>
              <p className="text-slate-500 text-sm">Intenta ajustar los filtros de busqueda</p>
            </div>
          )}
          
          {/* Table Footer */}
          {filteredUsuarios.length > 0 && (
            <div className="px-6 py-4 bg-slate-50 border-t border-slate-100 flex items-center justify-between">
              <p className="text-sm text-slate-500">
                Mostrando <span className="font-semibold text-slate-700">{filteredUsuarios.length}</span> de <span className="font-semibold text-slate-700">{usuarios.length}</span> usuarios
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
