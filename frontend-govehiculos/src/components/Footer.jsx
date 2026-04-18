import { Car, Mail, Phone, MapPin, ExternalLink } from "lucide-react";
import { Link } from "react-router-dom";

const QUICK_LINKS = [
  { label: "Inicio", href: "/" },
  { label: "Vehiculos", href: "/vehiculos" },
  { label: "Reservas", href: "/reservas" },
  { label: "Mantenimiento", href: "/mantenimiento" },
];

const RESOURCES = [
  { label: "Documentacion", href: "/docs" },
  { label: "Soporte", href: "/soporte" },
  { label: "FAQ", href: "/faq" },
  { label: "Terminos de uso", href: "/terminos" },
];

const TEAM_MEMBERS = [
  {
    name: "Juan Daniel Mino Gomez",
    github: "https://github.com",
    linkedin: "https://linkedin.com",
  },
  {
    name: "Luana Belen Morales Lopez",
    github: "https://github.com",
    linkedin: "https://linkedin.com",
  },
];

export default function Footer() {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="bg-gradient-to-b from-slate-900 to-slate-950 border-t border-slate-800 mt-auto">
      {/* Main Footer Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 lg:gap-12">
          {/* Brand Section */}
          <div className="lg:col-span-1">
            <div className="flex items-center gap-3 mb-4">
              <div className="relative">
                <div className="absolute inset-0 bg-blue-500 blur-lg opacity-40" />
                <div className="relative bg-gradient-to-br from-blue-500 to-blue-600 p-2.5 rounded-xl shadow-lg">
                  <Car className="h-6 w-6 text-white" />
                </div>
              </div>
              <span className="text-white font-bold text-xl tracking-tight">
                GoVehiculos
              </span>
            </div>
            <p className="text-slate-400 text-sm leading-relaxed mb-6">
              Sistema integral de gestion de vehiculos desarrollado para optimizar 
              el control y seguimiento de flotas empresariales.
            </p>
            
            {/* Contact Info */}
            <div className="space-y-3">
              <a 
                href="mailto:contacto@govehiculos.com" 
                className="flex items-center gap-2 text-slate-400 hover:text-blue-400 transition-colors text-sm group"
              >
                <Mail className="h-4 w-4 group-hover:scale-110 transition-transform" />
                <span>contacto@govehiculos.com</span>
              </a>
              <a 
                href="tel:+543794000000" 
                className="flex items-center gap-2 text-slate-400 hover:text-blue-400 transition-colors text-sm group"
              >
                <Phone className="h-4 w-4 group-hover:scale-110 transition-transform" />
                <span>+54 379 400-0000</span>
              </a>
              <div className="flex items-center gap-2 text-slate-400 text-sm">
                <MapPin className="h-4 w-4" />
                <span>Corrientes, Argentina</span>
              </div>
            </div>
          </div>

          {/* Quick Links */}
          <div>
            <h3 className="text-white font-semibold text-sm uppercase tracking-wider mb-4">
              Enlaces Rapidos
            </h3>
            <ul className="space-y-3">
              {QUICK_LINKS.map((link) => (
                <li key={link.href}>
                  <Link
                    to={link.href}
                    className="text-slate-400 hover:text-white transition-colors text-sm flex items-center gap-1 group"
                  >
                    <span className="w-0 group-hover:w-2 h-px bg-blue-500 transition-all duration-300" />
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Resources */}
          <div>
            <h3 className="text-white font-semibold text-sm uppercase tracking-wider mb-4">
              Recursos
            </h3>
            <ul className="space-y-3">
              {RESOURCES.map((link) => (
                <li key={link.href}>
                  <Link
                    to={link.href}
                    className="text-slate-400 hover:text-white transition-colors text-sm flex items-center gap-1 group"
                  >
                    <span className="w-0 group-hover:w-2 h-px bg-blue-500 transition-all duration-300" />
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Team & University */}
          <div>
            <h3 className="text-white font-semibold text-sm uppercase tracking-wider mb-4">
              Equipo de Desarrollo
            </h3>
            <div className="space-y-4">
              {TEAM_MEMBERS.map((member) => (
                <div key={member.name} className="group">
                  <p className="text-slate-300 text-sm font-medium mb-1">
                    {member.name}
                  </p>
                  <div className="flex items-center gap-3">
                    <a
                      href={member.github}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-slate-500 hover:text-white transition-colors"
                      aria-label={`GitHub de ${member.name}`}
                    >
                      {/* Ícono de GitHub nativo en SVG */}
                      <svg className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 0 0-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0 0 20 4.77 5.07 5.07 0 0 0 19.91 1S18.73.65 16 2.48a13.38 13.38 0 0 0-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 0 0 5 4.77a5.44 5.44 0 0 0-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 0 0 9 18.13V22"></path>
                      </svg>
                    </a>
                    <a
                      href={member.linkedin}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-slate-500 hover:text-blue-400 transition-colors"
                      aria-label={`LinkedIn de ${member.name}`}
                    >
                      {/* Ícono de LinkedIn nativo en SVG */}
                      <svg className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z"></path><rect x="2" y="9" width="4" height="12"></rect><circle cx="4" cy="4" r="2"></circle>
                      </svg>
                    </a>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Bottom Bar */}
      <div className="border-t border-slate-800">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex flex-col sm:flex-row justify-between items-center gap-3">
            <p className="text-slate-500 text-xs">
              &copy; {currentYear} GoVehiculos. Todos los derechos reservados.
            </p>
            <div className="flex items-center gap-4">
              <Link 
                to="/privacidad" 
                className="text-slate-500 hover:text-slate-300 text-xs transition-colors"
              >
                Politica de Privacidad
              </Link>
              <span className="text-slate-700">|</span>
              <Link 
                to="/terminos" 
                className="text-slate-500 hover:text-slate-300 text-xs transition-colors"
              >
                Terminos y Condiciones
              </Link>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}