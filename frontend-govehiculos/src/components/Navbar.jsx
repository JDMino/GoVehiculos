import { useState, useEffect, useCallback, useMemo, useRef } from "react";
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
import api from "../api/axiosConfig";

const ROLES = {
  CLIENTE:       1,
  SOCIO:         2,
  EMPLEADO:      3,
  ADMINISTRADOR: 4,
};

const ROLE_NAMES = {
  [ROLES.CLIENTE]:       "Cliente",
  [ROLES.SOCIO]:         "Socio",
  [ROLES.EMPLEADO]:      "Empleado",
  [ROLES.ADMINISTRADOR]: "Administrador",
};

const ROLE_COLORS = {
  [ROLES.CLIENTE]:       "bg-emerald-500/20 text-emerald-300",
  [ROLES.SOCIO]:         "bg-amber-500/20  text-amber-300",
  [ROLES.EMPLEADO]:      "bg-sky-500/20    text-sky-300",
  [ROLES.ADMINISTRADOR]: "bg-violet-500/20 text-violet-300",
};

// ── Badge de contador ────────────────────────────────────────────────────────
function ContadorBadge({ count }) {
  if (!count || count < 1) return null;
  return (
    <span className="ml-1.5 min-w-[18px] h-[18px] px-1 inline-flex items-center justify-center rounded-full bg-blue-400 text-slate-900 text-[10px] font-black leading-none">
      {count > 99 ? "99+" : count}
    </span>
  );
}

export default function Navbar({ user, onLogout, demoRole }) {
  const [isOpen,    setIsOpen]    = useState(false);
  const [isScrolled, setIsScrolled] = useState(false);
  const [contador,  setContador]  = useState(0);
  const location = useLocation();
  const navigate = useNavigate();

  const userRolId        = user?.rolId || demoRole || ROLES.ADMINISTRADOR;
  const isHomePage       = location.pathname === "/home";
  const isAuthenticated  = !!user;
  const showFullNavbar   = [ROLES.ADMINISTRADOR, ROLES.EMPLEADO].includes(userRolId);
  const canSeeMantenimiento = useMemo(
    () => [ROLES.EMPLEADO, ROLES.ADMINISTRADOR].includes(userRolId),
    [userRolId],
  );

  // ── Resolver empleadoId igual que en MantenimientosEmpleado ─────────────
  const empleadoId = useMemo(() => {
    if (!user) return null;
    return (
      user.idUsuario ?? user.IdUsuario ?? user.id ??
      user.Id        ?? user.userId    ?? user.UserId ?? null
    );
  }, [user]);

  // ── Fetch del contador según rol ─────────────────────────────────────────
  const fetchContador = useCallback(async () => {
    if (!isAuthenticated) return;
    try {
      if (userRolId === ROLES.ADMINISTRADOR) {
        const res = await api.get("/mantenimientos/contador-admin");
        setContador(res.data.count ?? 0);
      } else if (userRolId === ROLES.EMPLEADO && empleadoId) {
        const res = await api.get(`/mantenimientos/contador-empleado/${empleadoId}`);
        setContador(res.data.count ?? 0);
      }
    } catch {
      // silencioso — el contador es informativo, no bloquea la UI
    }
  }, [isAuthenticated, userRolId, empleadoId]);

  useEffect(() => {
    fetchContador();
    // Polling cada 5 segundos para mantener el contador fresco
    const intervalo = setInterval(fetchContador, 5_000);
    return () => clearInterval(intervalo);
  }, [fetchContador]);

  // ── Scroll listener ───────────────────────────────────────────────────────
  useEffect(() => {
    const handleScroll = () => setIsScrolled(window.scrollY > 10);
    window.addEventListener("scroll", handleScroll, { passive: true });
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  useEffect(() => {
    document.body.style.overflow = isOpen ? "hidden" : "";
    return () => { document.body.style.overflow = ""; };
  }, [isOpen]);

  // ── Nav links ─────────────────────────────────────────────────────────────
  const navLinks = useMemo(() => [
    {
      name:         "Dashboard",
      path:         "/dashboard",
      icon:         LayoutDashboard,
      allowedRoles: [ROLES.ADMINISTRADOR],
    },
    {
      name:         "Vehiculos",
      path:         "/vehiculos",
      icon:         Car,
      allowedRoles: [ROLES.SOCIO, ROLES.ADMINISTRADOR],
    },
    {
      name:         "Usuarios",
      path:         "/usuarios",
      icon:         Users,
      allowedRoles: [ROLES.ADMINISTRADOR],
    },
  ], []);

  const filteredLinks = useMemo(
    () => navLinks.filter(link => link.allowedRoles.includes(userRolId)),
    [navLinks, userRolId],
  );

  const handleLogout = useCallback(() => {
    if (onLogout) onLogout();
    navigate("/login");
  }, [onLogout, navigate]);

  const isActive = useCallback(
    (path) => location.pathname.startsWith(path),
    [location.pathname],
  );

  const getRolName  = (id) => ROLE_NAMES[id]  || "Usuario";
  const getRolColor = (id) => ROLE_COLORS[id] || "bg-gray-500/20 text-gray-300";

  const userName     = user?.nombre ? `${user.nombre} ${user.apellido}` : "Usuario Demo";
  const userInitials = user?.nombre
    ? `${user.nombre[0]}${user.apellido?.[0] || ""}`.toUpperCase()
    : "UD";

  // ── Render del link de Mantenimiento (desktop) ────────────────────────────
  const MantenimientoLink = ({ mobile = false }) => {
    const active = isActive("/mantenimientos");
    const base   = mobile
      ? `flex items-center px-4 py-3 rounded-xl text-base font-medium transition-all duration-200 ${
          active
            ? "bg-gradient-to-r from-blue-600 to-blue-500 text-white shadow-lg shadow-blue-500/20"
            : "text-slate-300 hover:bg-white/5 hover:text-white"
        }`
      : `relative flex items-center px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 ${
          active ? "text-white" : "text-slate-400 hover:text-white hover:bg-white/5"
        }`;

    return (
      <Link
        to="/mantenimientos"
        onClick={mobile ? () => setIsOpen(false) : undefined}
        className={base}
        aria-current={active ? "page" : undefined}
      >
        {!mobile && active && (
          <span className="absolute inset-0 bg-gradient-to-r from-blue-600/80 to-blue-500/80 rounded-lg" />
        )}
        <span className={`${mobile ? "" : "relative"} flex items-center`}>
          <Wrench className={`${mobile ? "h-5 w-5 mr-3" : "h-4 w-4 mr-2"}`} />
          Mantenimiento
          <ContadorBadge count={contador} />
        </span>
        {mobile && active && <ChevronRight className="h-4 w-4 ml-auto" />}
      </Link>
    );
  };

  return (
    <>
      <nav
        className={`bg-gradient-to-r from-slate-900 via-slate-800 to-slate-900 sticky top-0 z-50 transition-all duration-300 ${
          isScrolled ? "shadow-lg shadow-black/20 backdrop-blur-sm" : "shadow-md"
        }`}
        role="navigation"
        aria-label="Navegacion principal"
      >
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">

            {/* Logo */}
            <Link to="/" className="flex items-center gap-3 group" aria-label="Ir al inicio">
              <div className="relative bg-gradient-to-br from-blue-500 to-blue-600 p-2 rounded-xl">
                <Car className="h-5 w-5 text-white" />
              </div>
              <span className="text-white font-bold text-lg tracking-tight hidden sm:block">
                Go<span className="text-blue-400">Vehiculos</span>
              </span>
            </Link>

            {/* Home sin auth → login */}
            {isHomePage && !isAuthenticated && (
              <div className="flex items-center">
                <Link
                  to="/login"
                  className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium transition"
                >
                  Iniciar sesión
                </Link>
              </div>
            )}

            {/* Autenticado */}
            {isAuthenticated && (
              <>
                {(showFullNavbar || !isHomePage) && (
                  <div className="hidden md:ml-8 md:flex md:items-center md:gap-1">
                    {filteredLinks.map((link) => {
                      const Icon   = link.icon;
                      const active = isActive(link.path);
                      return (
                        <Link
                          key={link.name}
                          to={link.path}
                          className={`relative flex items-center px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 ${
                            active ? "text-white" : "text-slate-400 hover:text-white hover:bg-white/5"
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
                    {canSeeMantenimiento && <MantenimientoLink />}
                  </div>
                )}

                {/* Info + logout */}
                <div className="hidden md:flex items-center gap-3">
                  <div className="flex items-center gap-3 px-3 py-1.5 rounded-xl bg-white/5 border border-white/10">
                    <div className="h-8 w-8 rounded-lg bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white text-xs font-bold">
                      {userInitials}
                    </div>
                    <div className="flex flex-col items-start">
                      <span className="text-white text-sm font-medium max-w-[140px] truncate">
                        {userName}
                      </span>
                      <span className={`text-[10px] font-semibold uppercase tracking-wider px-1.5 py-0.5 rounded-md ${getRolColor(userRolId)}`}>
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

                {/* Mobile toggle */}
                <div className="flex items-center md:hidden">
                  <button
                    onClick={() => setIsOpen(!isOpen)}
                    className={`relative p-2 rounded-lg transition-all duration-200 ${
                      isOpen ? "bg-white/10 text-white" : "text-slate-400 hover:text-white hover:bg-white/5"
                    }`}
                    aria-expanded={isOpen}
                    aria-controls="mobile-menu"
                    aria-label={isOpen ? "Cerrar menu" : "Abrir menu"}
                  >
                    <span className="sr-only">{isOpen ? "Cerrar menu" : "Abrir menu"}</span>
                    {isOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      </nav>

      {/* Mobile menu */}
      {isAuthenticated && (
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
              {/* Info usuario */}
              <div className="flex items-center gap-3 px-3 py-3 mb-3 rounded-xl bg-white/5 border border-white/10">
                <div className="h-10 w-10 rounded-xl bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white text-sm font-bold">
                  {userInitials}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-white font-medium truncate">{userName}</p>
                  <span className={`inline-block text-[10px] font-semibold uppercase tracking-wider px-2 py-0.5 rounded-md mt-1 ${getRolColor(userRolId)}`}>
                    {getRolName(userRolId)}
                  </span>
                </div>
              </div>

              {(showFullNavbar || !isHomePage) && filteredLinks.map((link) => {
                const Icon   = link.icon;
                const active = isActive(link.path);
                return (
                  <Link
                    key={link.name}
                    to={link.path}
                    onClick={() => setIsOpen(false)}
                    className={`flex items-center px-4 py-3 rounded-xl text-base font-medium transition-all duration-200 ${
                      active
                        ? "bg-gradient-to-r from-blue-600 to-blue-500 text-white shadow-lg shadow-blue-500/20"
                        : "text-slate-300 hover:bg-white/5 hover:text-white"
                    }`}
                    aria-current={active ? "page" : undefined}
                  >
                    <Icon className="h-5 w-5 mr-3" />
                    {link.name}
                    {active && <ChevronRight className="h-4 w-4 ml-auto" />}
                  </Link>
                );
              })}

              {canSeeMantenimiento && (showFullNavbar || !isHomePage) && (
                <MantenimientoLink mobile />
              )}

              <div className="my-4 border-t border-slate-800" />

              <button
                onClick={() => { handleLogout(); setIsOpen(false); }}
                className="flex items-center w-full px-4 py-3 rounded-xl text-base font-medium text-red-400 hover:bg-red-500/10 hover:text-red-300 transition-all duration-200"
              >
                <LogOut className="h-5 w-5 mr-3" />
                Cerrar Sesion
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}