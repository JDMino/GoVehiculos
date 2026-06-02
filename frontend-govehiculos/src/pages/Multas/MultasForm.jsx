import { useState, useEffect } from "react";
import { useNavigate, Link } from "react-router-dom";
import {
  Scale,
  ArrowLeft,
  User,
  Car,
  AlertTriangle,
  CheckCircle,
  Loader2,
  ChevronDown,
  ShieldAlert,
  DollarSign,
  FileWarning,
  Info,
  X,
  Calendar,
  Clock,
  ChevronRight,
} from "lucide-react";
import api from "../../api/axiosConfig";

// ── Labels legibles para desplegables ──────────────────────────────────────

const LABEL_TIPO_INCIDENCIA = {
  daño_fisico: "Daño físico",
  accidente: "Accidente",
  infraccion_vial: "Infracción vial",
  comportamiento_indebido: "Comportamiento indebido",
  retraso_en_pago: "Retraso en pago",
};

const LABEL_TIPO_MULTA = {
  economica: "Económica",
  administrativa: "Administrativa",
  mixta: "Mixta",
};

const LABEL_TIPO_PENALIZACION = {
  suspension_temporal: "Suspensión temporal",
  bloqueo_cuenta: "Bloqueo de cuenta",
  inhabilitacion_vehiculo: "Inhabilitación de vehículo",
  advertencia: "Advertencia",
};

const LABEL_GRAVEDAD = {
  baja: "Baja",
  media: "Media",
  alta: "Alta",
};

// ── Estado inicial del formulario ───────────────────────────────────────────

const FORM_INICIAL = {
  usuarioId: "",
  vehiculoId: "",
  incidenciaTipo: "",
  nivelGravedad: "media",
  incidenciaDesc: "",
  multaTipo: "",
  monto: "",
  multaDesc: "",
  penalizacionTipo: "",
  motivo: "",
  fechaFin: "",
};

// ── Sub-componentes de UI ───────────────────────────────────────────────────

function SectionHeader({
  icon: Icon,
  title,
  subtitle,
  color = "bg-slate-100 text-slate-500",
}) {
  return (
    <div className="px-6 py-4 border-b border-slate-100 bg-slate-50 flex items-center gap-3">
      <div
        className={`h-8 w-8 rounded-lg flex items-center justify-center shrink-0 ${color}`}
      >
        <Icon className="h-4 w-4" />
      </div>
      <div>
        <h2 className="font-semibold text-slate-900 text-sm">{title}</h2>
        {subtitle && (
          <p className="text-xs text-slate-500 mt-0.5">{subtitle}</p>
        )}
      </div>
    </div>
  );
}

function FormField({ label, required, children, hint }) {
  return (
    <div className="space-y-1.5">
      <label className="text-sm font-medium text-slate-700">
        {label} {required && <span className="text-red-500">*</span>}
      </label>
      {children}
      {hint && <p className="text-xs text-slate-400">{hint}</p>}
    </div>
  );
}

function SelectField({
  value,
  onChange,
  placeholder,
  options,
  disabled = false,
}) {
  return (
    <div className="relative">
      <select
        value={value}
        onChange={onChange}
        disabled={disabled}
        className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed transition-all"
      >
        <option value="">{placeholder}</option>
        {options.map(({ value: v, label }) => (
          <option key={v} value={v}>
            {label}
          </option>
        ))}
      </select>
      <ChevronDown className="absolute right-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
    </div>
  );
}

function ModalConfirmacion({
  open,
  titulo,
  descripcion,
  labelConfirmar,
  colorConfirmar,
  onConfirmar,
  onCancelar,
  loading,
}) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md">
        <div className="p-6">
          <div className="flex items-start gap-4 mb-5">
            <div className="h-10 w-10 rounded-full bg-slate-100 flex items-center justify-center shrink-0">
              <Scale className="h-5 w-5 text-slate-600" />
            </div>
            <div>
              <h3 className="font-bold text-slate-900">{titulo}</h3>
              <p className="text-sm text-slate-500 mt-1">{descripcion}</p>
            </div>
          </div>
          <div className="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button
              onClick={onCancelar}
              disabled={loading}
              className="px-5 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors disabled:opacity-50"
            >
              No, volver
            </button>
            <button
              onClick={onConfirmar}
              disabled={loading}
              className={`inline-flex items-center gap-2 px-5 py-2.5 text-sm font-semibold text-white rounded-xl shadow-md transition-colors disabled:opacity-60 ${colorConfirmar}`}
            >
              {loading && <Loader2 className="h-4 w-4 animate-spin" />}
              {labelConfirmar}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

// ── Componente principal ────────────────────────────────────────────────────

export default function MultasForm() {
  const navigate = useNavigate();

  const [form, setForm] = useState(FORM_INICIAL);
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  const [errorNegocio, setErrorNegocio] = useState(null);

  // Estado del Wizard
  const [pasoActual, setPasoActual] = useState(1);

  // Datos para desplegables
  const [usuarios, setUsuarios] = useState([]);
  const [vehiculos, setVehiculos] = useState([]);
  const [valores, setValores] = useState(null);

  // Modales
  const [modalConfirmar, setModalConfirmar] = useState(false);
  const [modalCancelar, setModalCancelar] = useState(false);

  // ── Carga inicial de datos ────────────────────────────────────────────
  useEffect(() => {
    const fetchData = async () => {
      try {
        const [uRes, vRes, valRes] = await Promise.all([
          api.get("/usuarios"),
          api.get("/vehiculos"),
          api.get("/multas/valores"),
        ]);
        setUsuarios(uRes.data.filter((u) => u.activo && [1, 2].includes(u.rolId)));
        setVehiculos(vRes.data.filter((v) => v.activo));
        setValores(valRes.data);
      } catch {
        setErrorNegocio("Error al cargar los datos. Recargá la página.");
      } finally {
        setLoadingData(false);
      }
    };
    fetchData();
  }, []);

  // ── Handlers ──────────────────────────────────────────────────────────
  const handleChange = (campo, valor) => {
    setErrorNegocio(null);
    setForm((prev) => ({ ...prev, [campo]: valor }));
  };

  const vehiculoSeleccionado = vehiculos.find(
    (v) => String(v.idVehiculo) === String(form.vehiculoId),
  );

  const esDañoFisico = form.incidenciaTipo === "daño_fisico";
  const esAdministrativa = form.multaTipo === "administrativa";

  // ── Navegación sin validación estricta ────────────────────────────────
  const avanzarPaso = () => {
    setErrorNegocio(null);
    setPasoActual((p) => p + 1);
  };

  const retrocederPaso = () => {
    setErrorNegocio(null);
    setPasoActual((p) => p - 1);
  };

  // ── Submit con manejo de errores desde el Backend ─────────────────────
  const handleSubmit = async () => {
    setLoading(true);
    setErrorNegocio(null);
    setModalConfirmar(false);

    const payload = {
      incidencia: {
        usuarioId: parseInt(form.usuarioId) || 0,
        vehiculoId: parseInt(form.vehiculoId) || 0,
        tipo: form.incidenciaTipo,
        nivelGravedad: form.nivelGravedad,
        descripcion: form.incidenciaDesc,
      },
      multa: {
        tipo: form.multaTipo,
        monto: esAdministrativa ? 0 : parseFloat(form.monto) || 0,
        descripcion: form.multaDesc || null,
      },
      penalizacion: {
        tipo: form.penalizacionTipo,
        motivo: form.motivo,
        fechaFin: form.fechaFin || null,
      },
    };

    try {
      await api.post("/multas", payload);
      navigate("/multas");
    } catch (err) {
      const data = err.response?.data;
      
      // 1. Manejar errores automáticos de Data Annotations ([Required], etc.)
      if (data?.errors) {
        // Extraemos el primer error del diccionario enviado por ASP.NET
        const primerCampoConError = Object.keys(data.errors)[0];
        const mensajeError = data.errors[primerCampoConError][0];
        setErrorNegocio(mensajeError);
      } 
      // 2. Manejar mensajes de validación personalizados devueltos por MultaService
      else if (data?.mensaje) {
        setErrorNegocio(data.mensaje);
      } 
      // 3. Fallback genérico
      else {
        setErrorNegocio("Error inesperado al validar o crear la multa. Revisá los datos.");
      }
    } finally {
      setLoading(false);
    }
  };

  // ── Loading inicial ───────────────────────────────────────────────────
  if (loadingData) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <div className="flex flex-col items-center gap-3 text-slate-400">
          <Scale className="h-10 w-10 animate-pulse" />
          <p className="text-sm">Cargando datos del formulario...</p>
        </div>
      </div>
    );
  }

  // ── Render ────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen bg-slate-50">
      {/* ── Header ──────────────────────────────────────────────────────── */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-white pb-8">
        <div className="max-w-4xl mx-auto px-6 py-8">
          <Link
            to="/multas"
            className="inline-flex items-center gap-2 text-slate-300 hover:text-white text-sm font-medium mb-4 transition-colors"
          >
            <ArrowLeft className="h-4 w-4" />
            Volver a Gestión de Multas
          </Link>
          <div className="flex items-center gap-4">
            <div className="h-14 w-14 rounded-2xl bg-white/10 backdrop-blur flex items-center justify-center shrink-0">
              <Scale className="h-7 w-7 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Nueva Multa</h1>
              <p className="text-slate-300 text-sm mt-0.5">
                Paso {pasoActual} de 3
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-6 -mt-6 space-y-6">
        
        {/* ── Stepper Visual ────────────────────────────────────────────── */}
        <div className="bg-white rounded-2xl p-4 shadow-sm border border-slate-200 flex items-center justify-between relative">
          <div className="absolute top-1/2 left-8 right-8 h-0.5 bg-slate-100 -z-10 -translate-y-1/2"></div>
          
          {[
            { id: 1, label: "Incidencia", icon: FileWarning, color: "text-red-500", bg: "bg-red-50" },
            { id: 2, label: "Multa", icon: DollarSign, color: "text-amber-500", bg: "bg-amber-50" },
            { id: 3, label: "Penalización", icon: ShieldAlert, color: "text-purple-500", bg: "bg-purple-50" },
          ].map((step) => {
            const Icon = step.icon;
            const isActive = pasoActual === step.id;
            const isCompleted = pasoActual > step.id;
            
            return (
              <div key={step.id} className="flex flex-col items-center gap-2 bg-white px-4">
                <div className={`h-10 w-10 rounded-full flex items-center justify-center border-2 transition-all ${
                  isActive ? `border-slate-900 bg-slate-900 text-white` : 
                  isCompleted ? `border-slate-300 ${step.bg} ${step.color}` : 
                  `border-slate-200 bg-slate-50 text-slate-300`
                }`}>
                  {isCompleted ? <CheckCircle className="h-5 w-5" /> : <Icon className="h-4 w-4" />}
                </div>
                <span className={`text-xs font-bold ${isActive ? "text-slate-900" : isCompleted ? "text-slate-600" : "text-slate-400"}`}>
                  {step.label}
                </span>
              </div>
            );
          })}
        </div>

        {/* ── Error de negocio global ───────────────────────────────────── */}
        {errorNegocio && (
          <div className="flex items-start gap-3 bg-red-50 border border-red-200 rounded-2xl px-5 py-4">
            <AlertTriangle className="h-5 w-5 text-red-500 shrink-0 mt-0.5" />
            <div className="flex-1">
              <p className="text-sm font-semibold text-red-800">No se pudo confirmar la sanción</p>
              <p className="text-sm font-medium text-red-600 mt-0.5">{errorNegocio}</p>
            </div>
            <button
              onClick={() => setErrorNegocio(null)}
              className="ml-auto text-red-400 hover:text-red-600"
            >
              <X className="h-4 w-4" />
            </button>
          </div>
        )}

        {/* ── SECCIÓN 1: Incidencia ─────────────────────────────────────── */}
        {pasoActual === 1 && (
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden shadow-sm animate-in fade-in slide-in-from-bottom-4 duration-300">
            <SectionHeader
              icon={FileWarning}
              title="1. Incidencia"
              subtitle="Datos del suceso que origina la sanción"
              color="bg-red-50 text-red-500"
            />
            <div className="p-6 space-y-5">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
                <FormField label="Usuario involucrado" required>
                  <SelectField
                    value={form.usuarioId}
                    onChange={(e) => handleChange("usuarioId", e.target.value)}
                    placeholder="— Seleccioná un usuario —"
                    options={usuarios.map((u) => ({
                      value: u.idUsuario,
                      label: `${u.nombre} ${u.apellido} (ID: ${u.idUsuario})`,
                    }))}
                  />
                </FormField>

                <FormField label="Vehículo involucrado" required>
                  <SelectField
                    value={form.vehiculoId}
                    onChange={(e) => handleChange("vehiculoId", e.target.value)}
                    placeholder="— Seleccioná un vehículo —"
                    options={vehiculos.map((v) => ({
                      value: v.idVehiculo,
                      label: `${v.patente} — ${v.marcaNombre} ${v.modeloNombre}`,
                    }))}
                  />
                </FormField>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
                <FormField label="Tipo de incidencia" required>
                  <SelectField
                    value={form.incidenciaTipo}
                    onChange={(e) => handleChange("incidenciaTipo", e.target.value)}
                    placeholder="— Seleccioná el tipo —"
                    options={
                      valores?.tiposIncidencia?.map((t) => ({
                        value: t,
                        label: LABEL_TIPO_INCIDENCIA[t] ?? t,
                      })) ?? []
                    }
                  />
                </FormField>

                <FormField label="Nivel de gravedad" required>
                  <SelectField
                    value={form.nivelGravedad}
                    onChange={(e) => handleChange("nivelGravedad", e.target.value)}
                    placeholder="— Seleccioná la gravedad —"
                    options={
                      valores?.nivelesGravedad?.map((g) => ({
                        value: g,
                        label: LABEL_GRAVEDAD[g] ?? g,
                      })) ?? []
                    }
                  />
                </FormField>
              </div>

              {esDañoFisico && vehiculoSeleccionado && (
                <div className="flex items-start gap-3 bg-red-50 border border-red-200 rounded-xl px-4 py-3">
                  <AlertTriangle className="h-4 w-4 text-red-500 shrink-0 mt-0.5" />
                  <p className="text-sm text-red-700">
                    <span className="font-semibold">Atención:</span> Al confirmar la multa, el estado mecánico del vehículo <span className="font-semibold">{vehiculoSeleccionado.patente}</span> pasará automáticamente a <span className="font-semibold">Malo</span>.
                  </p>
                </div>
              )}

              <FormField label="Descripción del suceso" required>
                <textarea
                  rows={3}
                  value={form.incidenciaDesc}
                  onChange={(e) => handleChange("incidenciaDesc", e.target.value)}
                  placeholder="Describí el suceso con el mayor detalle posible..."
                  className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 resize-none transition-all"
                />
              </FormField>
              
              <div className="flex items-center gap-3 px-4 py-3 bg-slate-50 rounded-xl border border-slate-200">
                <Calendar className="h-4 w-4 text-slate-400 shrink-0" />
                <p className="text-sm text-slate-600">
                  Fecha de reporte: <span className="font-semibold text-slate-800">{new Date().toLocaleDateString("es-AR")}</span>
                </p>
              </div>
            </div>
          </div>
        )}

        {/* ── SECCIÓN 2: Multa ──────────────────────────────────────────── */}
        {pasoActual === 2 && (
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden shadow-sm animate-in fade-in slide-in-from-right-4 duration-300">
            <SectionHeader
              icon={DollarSign}
              title="2. Multa económica"
              subtitle="Sanción formal vinculada a la incidencia"
              color="bg-amber-50 text-amber-500"
            />
            <div className="p-6 space-y-5">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
                <FormField label="Tipo de multa" required>
                  <SelectField
                    value={form.multaTipo}
                    onChange={(e) => handleChange("multaTipo", e.target.value)}
                    placeholder="— Seleccioná el tipo —"
                    options={
                      valores?.tiposMulta?.map((t) => ({
                        value: t,
                        label: LABEL_TIPO_MULTA[t] ?? t,
                      })) ?? []
                    }
                  />
                </FormField>

                <FormField
                  label="Monto"
                  hint={esAdministrativa ? "Las multas administrativas no tienen monto." : undefined}
                >
                  <div className="relative">
                    <DollarSign className="absolute left-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
                    <input
                      type="number"
                      min="0"
                      step="0.01"
                      value={esAdministrativa ? "" : form.monto}
                      onChange={(e) => handleChange("monto", e.target.value)}
                      disabled={esAdministrativa}
                      placeholder={esAdministrativa ? "No aplica" : "0.00"}
                      className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
                    />
                  </div>
                </FormField>
              </div>

              <div className="flex items-center gap-3 px-4 py-3 bg-slate-50 rounded-xl border border-slate-200">
                <Clock className="h-4 w-4 text-slate-400 shrink-0" />
                <p className="text-sm text-slate-600">
                  Estado inicial: <span className="font-semibold text-amber-700 bg-amber-50 border border-amber-200 px-2 py-0.5 rounded-lg text-xs ml-1">Pendiente</span>
                </p>
              </div>

              <FormField label="Descripción (opcional)">
                <textarea
                  rows={2}
                  value={form.multaDesc}
                  onChange={(e) => handleChange("multaDesc", e.target.value)}
                  placeholder="Observaciones adicionales sobre la multa..."
                  className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 resize-none transition-all"
                />
              </FormField>
            </div>
          </div>
        )}

        {/* ── SECCIÓN 3: Penalización ───────────────────────────────────── */}
        {pasoActual === 3 && (
          <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden shadow-sm animate-in fade-in slide-in-from-right-4 duration-300">
            <SectionHeader
              icon={ShieldAlert}
              title="3. Penalización operativa"
              subtitle="Consecuencia operativa vinculada a la multa"
              color="bg-purple-50 text-purple-500"
            />
            <div className="p-6 space-y-5">
              <FormField label="Tipo de penalización" required>
                <SelectField
                  value={form.penalizacionTipo}
                  onChange={(e) => handleChange("penalizacionTipo", e.target.value)}
                  placeholder="— Seleccioná el tipo —"
                  options={
                    valores?.tiposPenalizacion?.map((t) => ({
                      value: t,
                      label: LABEL_TIPO_PENALIZACION[t] ?? t,
                    })) ?? []
                  }
                />
              </FormField>

              {form.penalizacionTipo === "bloqueo_cuenta" && (
                <div className="flex items-start gap-3 bg-orange-50 border border-orange-200 rounded-xl px-4 py-3">
                  <Info className="h-4 w-4 text-orange-500 shrink-0 mt-0.5" />
                  <p className="text-sm text-orange-700">
                    La cuenta del usuario seleccionado quedará <span className="font-semibold">bloqueada</span> de forma inmediata.
                  </p>
                </div>
              )}
              {form.penalizacionTipo === "inhabilitacion_vehiculo" && (
                <div className="flex items-start gap-3 bg-orange-50 border border-orange-200 rounded-xl px-4 py-3">
                  <Info className="h-4 w-4 text-orange-500 shrink-0 mt-0.5" />
                  <p className="text-sm text-orange-700">
                    El vehículo seleccionado pasará a <span className="font-semibold">Fuera de servicio</span> de forma inmediata.
                  </p>
                </div>
              )}

              <FormField label="Motivo de la penalización" required>
                <textarea
                  rows={3}
                  value={form.motivo}
                  onChange={(e) => handleChange("motivo", e.target.value)}
                  placeholder="Explicá el motivo de la penalización aplicada..."
                  className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 resize-none transition-all"
                />
              </FormField>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
                <FormField
                  label="Fecha de fin"
                  hint="Opcional. Dejalo vacío para una penalización indefinida."
                >
                  <div className="relative">
                    <Calendar className="absolute left-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
                    <input
                      type="date"
                      value={form.fechaFin}
                      onChange={(e) => handleChange("fechaFin", e.target.value)}
                      className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-slate-900 transition-all"
                    />
                  </div>
                </FormField>

                <FormField label="Estado de la penalización">
                  <div className="w-full h-[46px] flex items-center gap-3 px-4 bg-slate-50 rounded-xl border border-slate-200">
                    <CheckCircle className="h-4 w-4 text-slate-400 shrink-0" />
                    <p className="text-sm text-slate-600">
                      Valor fijo: <span className="font-semibold text-emerald-700 bg-emerald-50 border border-emerald-200 px-2 py-0.5 rounded-lg text-xs ml-1">Activa</span>
                    </p>
                  </div>
                </FormField>
              </div>
            </div>
          </div>
        )}

        {/* ── Botonera de navegación ────────────────────────────────────── */}
        <div className="flex items-center justify-between pb-8 pt-2">
          {pasoActual > 1 ? (
            <button
              type="button"
              onClick={retrocederPaso}
              className="inline-flex items-center gap-2 px-6 py-3 text-slate-600 font-semibold hover:text-slate-900 transition-colors"
            >
              <ArrowLeft className="h-4 w-4" />
              Atrás
            </button>
          ) : (
            <button
              type="button"
              onClick={() => setModalCancelar(true)}
              className="px-6 py-3 text-slate-500 font-semibold hover:text-slate-900 transition-colors"
            >
              Cancelar
            </button>
          )}

          {pasoActual < 3 ? (
            <button
              type="button"
              onClick={avanzarPaso}
              className="inline-flex items-center gap-2 px-8 py-3 bg-slate-900 hover:bg-slate-800 text-white font-semibold rounded-xl transition-all shadow-lg shadow-slate-200 hover:shadow-xl"
            >
              Siguiente
              <ChevronRight className="h-4 w-4" />
            </button>
          ) : (
            <button
              type="button"
              onClick={() => setModalConfirmar(true)}
              disabled={loading}
              className="inline-flex items-center gap-2 px-8 py-3 bg-slate-900 hover:bg-slate-800 disabled:bg-slate-600 disabled:cursor-not-allowed text-white font-semibold rounded-xl transition-all shadow-lg shadow-slate-200 hover:shadow-xl"
            >
              {loading ? (
                <>
                  <Loader2 className="h-5 w-5 animate-spin" />
                  Guardando...
                </>
              ) : (
                <>
                  <Scale className="h-5 w-5" />
                  Confirmar Multa
                </>
              )}
            </button>
          )}
        </div>
      </div>

      {/* ── Modal Confirmación de Creación ────────────────────────────── */}
      <ModalConfirmacion
        open={modalConfirmar}
        titulo="¿Confirmar la creación de la multa?"
        descripcion="Se registrará la incidencia, la multa y la penalización. Los efectos sobre el vehículo o usuario se aplicarán de forma inmediata."
        labelConfirmar="Sí, crear multa"
        colorConfirmar="bg-slate-900 hover:bg-slate-800"
        onConfirmar={handleSubmit}
        onCancelar={() => setModalConfirmar(false)}
        loading={loading}
      />

      {/* ── Modal Cancelar Formulario ─────────────────────────────────── */}
      <ModalConfirmacion
        open={modalCancelar}
        titulo="¿Descartar los cambios?"
        descripcion="Todos los datos ingresados se perderán y volverás al listado de multas."
        labelConfirmar="Sí, descartar"
        colorConfirmar="bg-red-600 hover:bg-red-700"
        onConfirmar={() => navigate("/multas")}
        onCancelar={() => setModalCancelar(false)}
        loading={false}
      />
    </div>
  );
}