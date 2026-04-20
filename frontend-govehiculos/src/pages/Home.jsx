import { Link } from "react-router-dom";
import {
  ShieldCheck,
  TrendingUp,
  Users,
  ChevronRight,
  Key,
  BarChart3,
  Sparkles,
  Zap,
  Clock,
} from "lucide-react";
import useAuthStore from "../context/AuthStore";

// Definimos los roles (igual que en tu Navbar para mantener consistencia)
const ROLES = {
  ADMINISTRADOR: 4,
};

export default function Home() {
  // Extraemos user e isAuthenticated del store
  const { isAuthenticated, user } = useAuthStore();

  // Verificamos si es administrador
  // Usamos el encadenamiento opcional ?. por si user es null
  const isAdmin = user?.rolId === ROLES.ADMINISTRADOR;

  return (
    <div className="min-h-screen bg-zinc-950 flex flex-col font-sans antialiased text-white">
      <main className="flex-grow">
        {/* Hero Section */}
        <section className="relative pt-20 pb-24 lg:pt-32 lg:pb-32 overflow-hidden">
          {/* Background gradients decorativos */}
          <div className="absolute inset-0 overflow-hidden pointer-events-none">
            <div className="absolute top-1/4 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] bg-blue-500/10 rounded-full blur-3xl" />
            <div className="absolute bottom-0 left-0 w-[600px] h-[600px] bg-emerald-500/5 rounded-full blur-3xl" />
          </div>

          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
            <div className="text-center max-w-4xl mx-auto">
              {/* Badge superior */}
              <div className="inline-flex items-center gap-2 bg-zinc-800/50 border border-zinc-700/50 rounded-full px-4 py-2 mb-8">
                <Sparkles className="h-4 w-4 text-blue-400" />
                <span className="text-sm text-zinc-300 font-medium">
                  Plataforma líder en movilidad
                </span>
              </div>

              <h1 className="text-4xl sm:text-5xl md:text-6xl lg:text-7xl font-bold leading-tight mb-6 tracking-tight">
                La red de movilidad{" "}
                <span className="bg-gradient-to-r from-blue-400 via-blue-500 to-emerald-400 bg-clip-text text-transparent">
                  más inteligente
                </span>
              </h1>
              
              <p className="text-lg md:text-xl text-zinc-400 mb-12 leading-relaxed max-w-2xl mx-auto">
                Ya sea que necesites un vehículo para tu próximo viaje o busques
                rentabilizar tu flota, conectamos oportunidades con soluciones
                reales.
              </p>

              {/* Tarjetas de Acción */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-2xl mx-auto">
                
                {/* Opción Cliente (RESTRINGIDA A ADMIN) */}
                <div className={`bg-zinc-900/80 backdrop-blur-sm p-6 lg:p-8 rounded-2xl border border-zinc-800 flex flex-col items-center text-center group transition-all duration-300 
                  ${isAdmin ? "hover:border-blue-500/50 hover:bg-zinc-900" : "opacity-60 grayscale-[0.5]"}`}>
                  
                  <div className="bg-blue-500/10 p-4 rounded-2xl mb-5 group-hover:bg-blue-500/20 transition-colors">
                    <Key className="h-7 w-7 text-blue-400" />
                  </div>
                  
                  <h3 className="text-lg font-semibold mb-2">Quiero Alquilar</h3>
                  
                  <p className="text-sm text-zinc-500 mb-6">
                    {isAdmin 
                      ? "Accede a una flota premium con reserva inmediata." 
                      : "Acceso exclusivo para administradores del sistema."}
                  </p>

                  {isAdmin ? (
                    <Link
                      to="/vehiculos"
                      className="w-full bg-blue-500 hover:bg-blue-600 text-white py-3 rounded-xl font-semibold flex items-center justify-center gap-1 transition-colors"
                    >
                      Ver Vehículos <ChevronRight className="h-4 w-4" />
                    </Link>
                  ) : (
                    <button
                      disabled
                      className="w-full bg-zinc-800 text-zinc-500 py-3 rounded-xl font-semibold flex items-center justify-center gap-1 cursor-not-allowed"
                    >
                      No disponible
                    </button>
                  )}
                </div>

                {/* Opción Socio */}
                <div className="bg-zinc-900/80 backdrop-blur-sm p-6 lg:p-8 rounded-2xl border border-zinc-800 flex flex-col items-center text-center group hover:border-emerald-500/50 hover:bg-zinc-900 transition-all duration-300">
                  <div className="bg-emerald-500/10 p-4 rounded-2xl mb-5 group-hover:bg-emerald-500/20 transition-colors">
                    <BarChart3 className="h-7 w-7 text-emerald-400" />
                  </div>
                  <h3 className="text-lg font-semibold mb-2">Soy Propietario</h3>
                  <p className="text-sm text-zinc-500 mb-6">
                    Suma tus vehículos y gestiona tus ganancias.
                  </p>
                  <Link
                    to={isAuthenticated ? "/vehiculos/nuevo" : "/register"}
                    className="w-full bg-emerald-500 hover:bg-emerald-600 text-white py-3 rounded-xl font-semibold flex items-center justify-center gap-1 transition-colors"
                  >
                    Ser Socio <ChevronRight className="h-4 w-4" />
                  </Link>
                </div>
              </div>

              {/* Estadísticas Rápidas */}
              <div className="flex flex-wrap items-center justify-center gap-8 lg:gap-12 mt-16 pt-16 border-t border-zinc-800/50">
                <div className="text-center">
                  <p className="text-3xl lg:text-4xl font-bold">2.5K+</p>
                  <p className="text-sm text-zinc-500 mt-1">Vehículos activos</p>
                </div>
                <div className="text-center">
                  <p className="text-3xl lg:text-4xl font-bold">15K+</p>
                  <p className="text-sm text-zinc-500 mt-1">Usuarios registrados</p>
                </div>
                <div className="text-center">
                  <p className="text-3xl lg:text-4xl font-bold">98%</p>
                  <p className="text-sm text-zinc-500 mt-1">Satisfacción</p>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Sección de Características */}
        <section className="py-24 bg-zinc-900/50 border-y border-zinc-800/50">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="text-center mb-16">
              <h2 className="text-3xl md:text-4xl font-bold mb-4 tracking-tight">
                Diseñado para cada perfil
              </h2>
              <p className="text-zinc-400 max-w-xl mx-auto">
                Herramientas potentes para clientes y propietarios por igual.
              </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <div className="bg-zinc-900/80 p-6 rounded-2xl border border-zinc-800 hover:border-zinc-700 transition-colors group">
                <div className="bg-zinc-800 p-3 rounded-xl w-fit mb-4 group-hover:bg-zinc-700 transition-colors">
                  <ShieldCheck className="h-5 w-5 text-blue-400" />
                </div>
                <h4 className="font-semibold mb-2">Seguridad Certificada</h4>
                <p className="text-sm text-zinc-500 leading-relaxed">
                  Validamos cada documentación y seguro para que operes con tranquilidad total.
                </p>
              </div>

              <div className="bg-zinc-900/80 p-6 rounded-2xl border border-zinc-800 hover:border-zinc-700 transition-colors group">
                <div className="bg-zinc-800 p-3 rounded-xl w-fit mb-4 group-hover:bg-zinc-700 transition-colors">
                  <TrendingUp className="h-5 w-5 text-emerald-400" />
                </div>
                <h4 className="font-semibold mb-2">Escalabilidad Total</h4>
                <p className="text-sm text-zinc-500 leading-relaxed">
                  Gestiona desde una unidad hasta una flota completa con herramientas de análisis.
                </p>
              </div>

              <div className="bg-zinc-900/80 p-6 rounded-2xl border border-zinc-800 hover:border-zinc-700 transition-colors group">
                <div className="bg-zinc-800 p-3 rounded-xl w-fit mb-4 group-hover:bg-zinc-700 transition-colors">
                  <Zap className="h-5 w-5 text-yellow-400" />
                </div>
                <h4 className="font-semibold mb-2">Reserva Instantánea</h4>
                <p className="text-sm text-zinc-500 leading-relaxed">
                  Proceso de reserva ágil y confirmación inmediata para tu comodidad.
                </p>
              </div>

              <div className="bg-zinc-900/80 p-6 rounded-2xl border border-zinc-800 hover:border-zinc-700 transition-colors group">
                <div className="bg-zinc-800 p-3 rounded-xl w-fit mb-4 group-hover:bg-zinc-700 transition-colors">
                  <Users className="h-5 w-5 text-purple-400" />
                </div>
                <h4 className="font-semibold mb-2">Comunidad Verificada</h4>
                <p className="text-sm text-zinc-500 leading-relaxed">
                  Sistema de perfiles y calificaciones para la mejor experiencia entre usuarios.
                </p>
              </div>
            </div>
          </div>
        </section>

        {/* Sección CTA (Llamada a la acción) */}
        {!isAuthenticated && (
          <section className="py-24">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
              <div className="bg-gradient-to-br from-blue-600 to-blue-700 rounded-3xl p-8 md:p-12 lg:p-16 text-center relative overflow-hidden">
                <div className="relative z-10">
                  <div className="inline-flex items-center gap-2 bg-white/10 rounded-full px-4 py-2 mb-6">
                    <Clock className="h-4 w-4 text-white" />
                    <span className="text-sm text-white/90 font-medium">Comienza en minutos</span>
                  </div>
                  <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold mb-4 tracking-tight">
                    ¿Listo para transformar tu movilidad?
                  </h2>
                  <p className="text-blue-100 mb-8 max-w-xl mx-auto text-lg">
                    Únete a miles de usuarios que ya confían en nuestra plataforma.
                  </p>
                  <div className="flex flex-col sm:flex-row gap-4 justify-center">
                    <Link
                      to="/register"
                      className="bg-white hover:bg-zinc-100 text-blue-600 px-8 py-4 rounded-xl font-semibold transition-colors text-base"
                    >
                      Crear cuenta gratis
                    </Link>
                    <Link
                      to="/login"
                      className="bg-white/10 hover:bg-white/20 text-white border border-white/20 px-8 py-4 rounded-xl font-semibold transition-colors text-base"
                    >
                      Ya tengo cuenta
                    </Link>
                  </div>
                </div>
              </div>
            </div>
          </section>
        )}
      </main>
    </div>
  );
}