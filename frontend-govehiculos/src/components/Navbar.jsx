import { useState, useEffect, useCallback, useMemo } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import {
  Car,
  LayoutDashboard,
  Users,
  Wrench,
  LogOut,
  Menu,
  X,
  ChevronRight,
} from "lucide-react";

const ROLES = {
  CLIENTE: 1,
  SOCIO: 2,
  EMPLEADO: 3,
  ADMINISTRADOR: 4,
};

const ROLE_NAMES = {
  [ROLES.CLIENTE]: "Cliente",
  [ROLES.SOCIO]: "Socio",
  [ROLES.EMPLEADO]: "Empleado",
  [ROLES.ADMINISTRADOR]: "Administrador",
};

const ROLE_COLORS = {
  [ROLES.CLIENTE]: "bg-emerald-500/20 text-emerald-300",
  [ROLES.SOCIO]: "bg-amber-500/20 text-amber-300",
  [ROLES.EMPLEADO]: "bg-sky-500/20 text-sky-300",
  [ROLES.ADMINISTRADOR]: "bg-violet-500/20 text-violet-300",
};

export default function Navbar({ user, onLogout, demoRole }) {
  const [isOpen, setIsOpen] = useState(false);
  const [isScrolled, setIsScrolled] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();

  const userRolId = user?.rolId || demoRole || ROLES.ADMINISTRADOR;

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 10);
    };
    window.addEventListener("scroll", handleScroll, { passive: true });
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  // Bloquear scroll del body cuando el menu esta abierto
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
    return () => {
      document.body.style.overflow = "";
    };
  }, [isOpen]);

  const navLinks = useMemo(
    () => [
      {
        name: "Dashboard",
        path: "/dashboard",
        icon: LayoutDashboard,
        allowedRoles: [ROLES.CLIENTE, ROLES.SOCIO, ROLES.EMPLEADO, ROLES.ADMINISTRADOR],
      },
      {
        name: "Vehiculos",
        path: "/vehiculos",
        icon: Car,
        allowedRoles: [ROLES.SOCIO, ROLES.ADMINISTRADOR],
      },
      {
        name: "Usuarios",
        path: "/usuarios",
        icon: Users,
        allowedRoles: [ROLES.ADMINISTRADOR],
      },
    ],
    []
  );

  const filteredLinks = useMemo(
    () => navLinks.filter((link) => link.allowedRoles.includes(userRolId)),
    [navLinks, userRolId]
  );

  const canSeeMantenimiento = useMemo(
    () => [ROLES.EMPLEADO, ROLES.ADMINISTRADOR].includes(userRolId),
    [userRolId]
  );

  const handleLogout = useCallback(() => {
    if (onLogout) {
      onLogout();
    }
    navigate("/login");
  }, [onLogout, navigate]);

  const isActive = useCallback(
    (path) => location.pathname.startsWith(path),
    [location.pathname]
  );

  const getRolName = (id) => ROLE_NAMES[id] || "Usuario";
  const getRolColor = (id) => ROLE_COLORS[id] || "bg-gray-500/20 text-gray-300";

  const userName = user?.nombre
    ? `${user.nombre} ${user.apellido}`
    : "Usuario Demo";

  const userInitials = user?.nombre
    ? `${user.nombre[0]}${user.apellido?.[0] || ""}`.toUpperCase()
    : "UD";

  return (
    <>
      <nav
        className={`bg-gradient-to-r from-slate-900 via-slate-800 to-slate-900 sticky top-0 z-50 transition-all duration-300 ${
          isScrolled
            ? "shadow-lg shadow-black/20 backdrop-blur-sm"
            : "shadow-md"
        }`}
        role="navigation"
        aria-label="Navegacion principal"
      >
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Link
                to="/"
                className="flex items-center gap-3 group"
                aria-label="Ir al inicio"
              >
                <div className="relative">
                  <div className="absolute inset-0 bg-blue-500 rounded-xl blur-md opacity-50 group-hover:opacity-75 transition-opacity" />
                  <div className="relative bg-gradient-to-br from-blue-500 to-blue-600 p-2 rounded-xl group-hover:scale-105 transition-transform duration-200">
                    <Car className="h-5 w-5 text-white" />
                  </div>
                </div>
                <span className="text-white font-bold text-lg tracking-tight hidden sm:block">
                  Go
                  <span className="text-blue-400">Vehiculos</span>
                </span>
              </Link>

              <div className="hidden md:ml-8 md:flex md:items-center md:gap-1">
                {filteredLinks.map((link) => {
                  const Icon = link.icon;
                  const active = isActive(link.path);
                  return (
                    <Link
                      key={link.name}
                      to={link.path}
                      className={`relative flex items-center px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 ${
                        active
                          ? "text-white"
                          : "text-slate-400 hover:text-white hover:bg-white/5"
                      }`}
                      aria-current={active ? "page" : undefined}
                    >
                      {active && (
                        <span className="absolute inset-0 bg-gradient-to-r from-blue-600/80 to-blue-500/80 rounded-lg" />
                      )}
                      <span className="relative flex items-center">
                        <Icon className="h-4 w-4 mr-2" />
                        {link.name}
                      </span>
                    </Link>
                  );
                })}

                {canSeeMantenimiento && (
                  <span
                    className="flex items-center px-4 py-2 rounded-lg text-sm font-medium text-slate-500 cursor-not-allowed group relative"
                    title="Disponible en la Etapa 4"
                  >
                    <Wrench className="h-4 w-4 mr-2" />
                    Mantenimiento
                    <span className="ml-2 text-[10px] bg-slate-700 text-slate-400 px-1.5 py-0.5 rounded-full uppercase tracking-wider">
                      Pronto
                    </span>
                  </span>
                )}
              </div>
            </div>

            <div className="hidden md:flex items-center gap-3">
              <div className="flex items-center gap-3 px-3 py-1.5 rounded-xl bg-white/5 border border-white/10">
                <div className="h-8 w-8 rounded-lg bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white text-xs font-bold shadow-inner">
                  {userInitials}
                </div>
                <div className="flex flex-col items-start">
                  <span className="text-white text-sm font-medium max-w-[140px] truncate">
                    {userName}
                  </span>
                  <span
                    className={`text-[10px] font-semibold uppercase tracking-wider px-1.5 py-0.5 rounded-md ${getRolColor(userRolId)}`}
                  >
                    {getRolName(userRolId)}
                  </span>
                </div>
              </div>

              <div className="h-8 w-px bg-slate-700" />

              <button
                onClick={handleLogout}
                className="flex items-center gap-2 px-3 py-2 text-slate-400 hover:text-red-400 hover:bg-red-500/10 rounded-lg transition-all duration-200 group"
                title="Cerrar Sesion"
                aria-label="Cerrar sesion"
              >
                <LogOut className="h-4 w-4 group-hover:translate-x-0.5 transition-transform" />
                <span className="text-sm font-medium">Salir</span>
              </button>
            </div>

            <div className="flex items-center md:hidden">
              <button
                onClick={() => setIsOpen(!isOpen)}
                className={`relative p-2 rounded-lg transition-all duration-200 ${
                  isOpen
                    ? "bg-white/10 text-white"
                    : "text-slate-400 hover:text-white hover:bg-white/5"
                }`}
                aria-expanded={isOpen}
                aria-controls="mobile-menu"
                aria-label={isOpen ? "Cerrar menu" : "Abrir menu"}
              >
                <span className="sr-only">
                  {isOpen ? "Cerrar menu" : "Abrir menu"}
                </span>
                {isOpen ? (
                  <X className="h-6 w-6" />
                ) : (
                  <Menu className="h-6 w-6" />
                )}
              </button>
            </div>
          </div>
        </div>
      </nav>

      <div
        className={`fixed inset-0 z-40 md:hidden transition-opacity duration-300 ${
          isOpen ? "opacity-100" : "opacity-0 pointer-events-none"
        }`}
      >
        <div
          className="absolute inset-0 bg-black/60 backdrop-blur-sm"
          onClick={() => setIsOpen(false)}
          aria-hidden="true"
        />

        <div
          id="mobile-menu"
          className={`absolute top-16 left-0 right-0 bg-slate-900 border-t border-slate-800 shadow-2xl transition-all duration-300 transform ${
            isOpen
              ? "translate-y-0 opacity-100"
              : "-translate-y-4 opacity-0 pointer-events-none"
          }`}
        >
          <div className="px-4 py-4 space-y-1 max-h-[calc(100vh-4rem)] overflow-y-auto">
            <div className="flex items-center gap-3 px-3 py-3 mb-3 rounded-xl bg-white/5 border border-white/10">
              <div className="h-10 w-10 rounded-xl bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white text-sm font-bold">
                {userInitials}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-white font-medium truncate">{userName}</p>
                <span
                  className={`inline-block text-[10px] font-semibold uppercase tracking-wider px-2 py-0.5 rounded-md mt-1 ${getRolColor(userRolId)}`}
                >
                  {getRolName(userRolId)}
                </span>
              </div>
            </div>

            {filteredLinks.map((link) => {
              const Icon = link.icon;
              const active = isActive(link.path);
              return (
                <Link
                  key={link.name}
                  to={link.path}
                  onClick={() => setIsOpen(false)} // CERRAMOS EL MENU AL CLICK
                  className={`flex items-center px-4 py-3 rounded-xl text-base font-medium transition-all duration-200 ${
                    active
                      ? "bg-gradient-to-r from-blue-600 to-blue-500 text-white shadow-lg shadow-blue-500/20"
                      : "text-slate-300 hover:bg-white/5 hover:text-white"
                  }`}
                  aria-current={active ? "page" : undefined}
                >
                  <Icon className="h-5 w-5 mr-3" />
                  {link.name}
                  {active && (
                    <ChevronRight className="h-4 w-4 ml-auto" />
                  )}
                </Link>
              );
            })}

            {canSeeMantenimiento && (
              <span className="flex items-center px-4 py-3 rounded-xl text-base font-medium text-slate-500">
                <Wrench className="h-5 w-5 mr-3" />
                Mantenimiento
                <span className="ml-auto text-[10px] bg-slate-800 text-slate-500 px-2 py-1 rounded-full uppercase tracking-wider">
                  Pronto
                </span>
              </span>
            )}

            <div className="my-4 border-t border-slate-800" />

            <button
              onClick={() => {
                handleLogout();
                setIsOpen(false); // CERRAMOS EL MENU AL CERRAR SESIÓN
              }}
              className="flex items-center w-full px-4 py-3 rounded-xl text-base font-medium text-red-400 hover:bg-red-500/10 hover:text-red-300 transition-all duration-200"
            >
              <LogOut className="h-5 w-5 mr-3" />
              Cerrar Sesion
            </button>
          </div>
        </div>
      </div>
    </>
  );
}