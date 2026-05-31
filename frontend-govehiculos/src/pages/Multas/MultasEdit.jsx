import { useState, useEffect, useCallback } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import {
  Scale,
  ArrowLeft,
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
  Ban,
  Pencil,
} from "lucide-react";
import api from "../../api/axiosConfig";

// ── Labels legibles (idénticos a MultasForm) ────────────────────────────────

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

const LABEL_ESTADO_MULTA = {
  pendiente: "Pendiente",
  pagada: "Pagada",
};

const LABEL_ESTADO_PENALIZACION = {
  activa: "Activa",
  cumplida: "Cumplida",
};

// ── Sub-componentes ──────────────────────────────────────────────────────────

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

function FormField({ label, required, children, hint, disabled }) {
  return (
    <div className="space-y-1.5">
      <label
        className={`text-sm font-medium ${disabled ? "text-slate-400" : "text-slate-700"}`}
      >
        {label}{" "}
        {required && !disabled && <span className="text-red-500">*</span>}
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
        className="w-full appearance-none px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-slate-900 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed transition-all"
      >
        {placeholder && <option value="">{placeholder}</option>}
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

function TextArea({
  value,
  onChange,
  placeholder,
  rows = 3,
  disabled = false,
}) {
  return (
    <textarea
      rows={rows}
      value={value}
      onChange={onChange}
      disabled={disabled}
      placeholder={placeholder}
      className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 resize-none disabled:opacity-40 disabled:cursor-not-allowed transition-all"
    />
  );
}

function InfoReadOnly({ label, value }) {
  return (
    <div className="space-y-1.5">
      <p className="text-xs font-semibold text-slate-400 uppercase tracking-wide">
        {label}
      </p>
      <p className="text-sm font-medium text-slate-600 px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl">
        {value || "—"}
      </p>
    </div>
  );
}

// ── Modal genérico de confirmación ──────────────────────────────────────────

function ModalConfirmacion({
  open,
  titulo,
  descripcion,
  labelConfirmar,
  colorConfirmar,
  onConfirmar,
  onCancelar,
  loading,
  children,
}) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md">
        <div className="p-6">
          <div className="flex items-start gap-4 mb-4">
            <div className="h-10 w-10 rounded-full bg-slate-100 flex items-center justify-center shrink-0">
              <Scale className="h-5 w-5 text-slate-600" />
            </div>
            <div className="flex-1">
              <h3 className="font-bold text-slate-900">{titulo}</h3>
              <p className="text-sm text-slate-500 mt-1">{descripcion}</p>
            </div>
          </div>
          {children}
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

export default function MultasEdit() {
  const { id } = useParams();
  const navigate = useNavigate();

  // ── Estado de datos ───────────────────────────────────────────────────
  const [loadingData, setLoadingData] = useState(true);
  const [loading, setLoading] = useState(false);
  const [errorNegocio, setErrorNegocio] = useState(null);
  const [valores, setValores] = useState(null);

  // Datos de solo lectura
  const [infoFija, setInfoFija] = useState({
    usuarioNombre: "",
    vehiculoPatente: "",
    vehiculoMarca: "",
    vehiculoModelo: "",
    idIncidencia: null,
    idPenalizacion: null,
    multaFechaCreacion: "",
  });

  // Formulario editable — Incidencia
  const [formIncidencia, setFormIncidencia] = useState({
    tipo: "",
    nivelGravedad: "",
    descripcion: "",
  });

  // Formulario editable — Multa
  const [formMulta, setFormMulta] = useState({
    tipo: "",
    monto: "",
    descripcion: "",
    estado: "",
  });

  // Formulario editable — Penalización
  const [formPenalizacion, setFormPenalizacion] = useState({
    tipo: "",
    motivo: "",
    fechaFin: "",
    estado: "",
  });

  // Motivo cancelación (modal cancelar multa)
  const [motivoCancelacion, setMotivoCancelacion] = useState("");

  // Guard: la multa está cancelada
  const [estaCancelada, setEstaCancelada] = useState(false);

  // Modales
  const [modalConfirmar, setModalConfirmar] = useState(false);
  const [modalCancelarForm, setModalCancelarForm] = useState(false);
  const [modalCancelarMul, setModalCancelarMul] = useState(false);

  // ── Carga de datos ────────────────────────────────────────────────────
  // El MultaResponseDTO ya incluye todos los datos de incidencia y penalización
  // gracias al fix del backend. No se necesitan llamadas adicionales.
  const cargar = useCallback(async () => {
    setLoadingData(true);
    setErrorNegocio(null);
    try {
      const [multaRes, valRes] = await Promise.all([
        api.get(`/multas/${id}`),
        api.get("/multas/valores"),
      ]);

      const m = multaRes.data;
      setValores(valRes.data);
      setEstaCancelada(m.estaCancelada ?? m.estado === "cancelada");

      // Info fija — todo viene del MultaResponseDTO
      setInfoFija({
        usuarioNombre: m.usuarioNombreCompleto,
        vehiculoPatente: m.vehiculoPatente,
        vehiculoMarca: m.vehiculoMarca,
        vehiculoModelo: m.vehiculoModelo,
        idIncidencia: m.incidenciaId,
        idPenalizacion: m.idPenalizacion ?? null,
        multaFechaCreacion: m.fechaCreacion,
      });

      // Prellenar incidencia
      setFormIncidencia({
        tipo: m.incidenciaTipo ?? "",
        nivelGravedad: m.incidenciaNivelGravedad ?? "",
        descripcion: m.incidenciaDescripcion ?? "",
      });

      // Prellenar multa
      setFormMulta({
        tipo: m.tipo ?? "",
        monto: String(m.monto ?? ""),
        descripcion: m.descripcion ?? "",
        estado: m.estado ?? "",
      });

      // Prellenar penalización (viene en el mismo DTO)
      setFormPenalizacion({
        tipo: m.penalizacionTipo ?? "",
        motivo: m.penalizacionMotivo ?? "",
        fechaFin: m.penalizacionFechaFin
          ? new Date(m.penalizacionFechaFin).toISOString().split("T")[0]
          : "",
        estado: m.penalizacionEstado ?? "",
      });
    } catch {
      setErrorNegocio(
        "No se pudieron cargar los datos de la multa. Intentá de nuevo.",
      );
    } finally {
      setLoadingData(false);
    }
  }, [id]);

  useEffect(() => {
    cargar();
  }, [cargar]);

  // ── Helpers de cambio de campo ────────────────────────────────────────
  const changeInc = (campo, val) => {
    setErrorNegocio(null);
    setFormIncidencia((p) => ({ ...p, [campo]: val }));
  };
  const changeMul = (campo, val) => {
    setErrorNegocio(null);
    setFormMulta((p) => ({ ...p, [campo]: val }));
  };
  const changePen = (campo, val) => {
    setErrorNegocio(null);
    setFormPenalizacion((p) => ({ ...p, [campo]: val }));
  };

  const esAdministrativa = formMulta.tipo === "administrativa";

  // ── Confirmar cambios ─────────────────────────────────────────────────
  const handleUpdate = async () => {
    setLoading(true);
    setErrorNegocio(null);
    setModalConfirmar(false);
    try {
      // Actualizar Incidencia
      await api.put(`/incidencias/${infoFija.idIncidencia}`, {
        tipo: formIncidencia.tipo,
        nivelGravedad: formIncidencia.nivelGravedad,
        descripcion: formIncidencia.descripcion,
      });

      // Actualizar Multa
      await api.put(`/multas/${id}`, {
        tipo: formMulta.tipo,
        monto: esAdministrativa ? 0 : parseFloat(formMulta.monto) || 0,
        descripcion: formMulta.descripcion || null,
        estado: formMulta.estado,
      });

      // Actualizar Penalización (si existe)
      if (infoFija.idPenalizacion) {
        await api.put(`/penalizaciones/${infoFija.idPenalizacion}`, {
          tipo: formPenalizacion.tipo,
          motivo: formPenalizacion.motivo,
          fechaFin: formPenalizacion.fechaFin || null,
          estado: formPenalizacion.estado,
        });
      }

      navigate("/multas");
    } catch (err) {
      const data = err.response?.data;
      
      // 1. Manejar errores automáticos de Data Annotations ([Required], etc.)
      if (data?.errors) {
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
        setErrorNegocio("Error inesperado al validar o guardar la multa. Revisá los datos.");
      }
    } finally {
      setLoading(false);
    }
  };

  // ── Cancelar multa ────────────────────────────────────────────────────
  const handleCancelarMulta = async () => {
    setLoading(true);
    setErrorNegocio(null);
    setModalCancelarMul(false);
    try {
      await api.patch(`/multas/${id}/cancelar`, {
        motivoCancelacion: motivoCancelacion.trim(),
      });
      navigate("/multas");
    } catch (err) {
      const data = err.response?.data;
      setErrorNegocio(
        data?.mensaje || "No se pudo cancelar la multa."
      );
    } finally {
      setLoading(false);
    }
  };

  // ── Loading ───────────────────────────────────────────────────────────
  if (loadingData) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <div className="flex flex-col items-center gap-3 text-slate-400">
          <Scale className="h-10 w-10 animate-pulse" />
          <p className="text-sm">Cargando datos de la multa...</p>
        </div>
      </div>
    );
  }

  const disabled = estaCancelada;

  // ── Render ────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen bg-slate-50">
      {/* ── Header ──────────────────────────────────────────────────────── */}
      <div className="bg-gradient-to-r from-slate-900 to-slate-800 text-white">
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
              <Pencil className="h-7 w-7 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">
                Editar Multa{" "}
                <span className="font-mono text-slate-400 text-xl">#{id}</span>
              </h1>
              <p className="text-slate-300 text-sm mt-0.5">
                {estaCancelada
                  ? "Esta multa fue cancelada y no puede modificarse."
                  : "Modificá los campos necesarios y confirmá los cambios."}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-6 py-8 space-y-6">
        {/* ── Banner: multa cancelada ───────────────────────────────────── */}
        {estaCancelada && (
          <div className="flex items-start gap-3 bg-slate-100 border border-slate-300 rounded-2xl px-5 py-4">
            <Ban className="h-5 w-5 text-slate-500 shrink-0 mt-0.5" />
            <div>
              <p className="text-sm font-semibold text-slate-700">
                Multa cancelada
              </p>
              <p className="text-sm text-slate-500 mt-0.5">
                Esta multa fue cancelada y su penalización fue revocada. Los
                campos son de solo lectura y no pueden modificarse.
              </p>
            </div>
          </div>
        )}

        {/* ── Info fija: usuario y vehículo ─────────────────────────────── */}
        <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden">
          <SectionHeader
            icon={Info}
            title="Datos no editables"
            subtitle="Usuario y vehículo no pueden modificarse tras la creación"
            color="bg-slate-100 text-slate-400"
          />
          <div className="p-6 grid grid-cols-1 sm:grid-cols-2 gap-5">
            <InfoReadOnly
              label="Usuario involucrado"
              value={infoFija.usuarioNombre}
            />
            <InfoReadOnly
              label="Vehículo involucrado"
              value={`${infoFija.vehiculoPatente} — ${infoFija.vehiculoMarca} ${infoFija.vehiculoModelo}`}
            />
            <InfoReadOnly
              label="Fecha de creación"
              value={
                infoFija.multaFechaCreacion
                  ? new Date(infoFija.multaFechaCreacion).toLocaleDateString(
                      "es-AR",
                      {
                        day: "2-digit",
                        month: "2-digit",
                        year: "numeric",
                      },
                    )
                  : "—"
              }
            />
          </div>
        </div>

        {/* ── SECCIÓN 1: Incidencia ─────────────────────────────────────── */}
        <div
          className={`bg-white rounded-2xl border overflow-hidden ${disabled ? "border-slate-100 opacity-60" : "border-slate-200"}`}
        >
          <SectionHeader
            icon={FileWarning}
            title="1. Incidencia"
            subtitle="Tipo, gravedad y descripción del suceso"
            color="bg-red-50 text-red-500"
          />
          <div className="p-6 space-y-5">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              <FormField
                label="Tipo de incidencia"
                required
                disabled={disabled}
              >
                <SelectField
                  value={formIncidencia.tipo}
                  onChange={(e) => changeInc("tipo", e.target.value)}
                  options={
                    valores?.tiposIncidencia?.map((t) => ({
                      value: t,
                      label: LABEL_TIPO_INCIDENCIA[t] ?? t,
                    })) ?? []
                  }
                  disabled={disabled}
                />
              </FormField>

              <FormField label="Nivel de gravedad" required disabled={disabled}>
                <SelectField
                  value={formIncidencia.nivelGravedad}
                  onChange={(e) => changeInc("nivelGravedad", e.target.value)}
                  options={
                    valores?.nivelesGravedad?.map((g) => ({
                      value: g,
                      label: LABEL_GRAVEDAD[g] ?? g,
                    })) ?? []
                  }
                  disabled={disabled}
                />
              </FormField>
            </div>

            <FormField
              label="Descripción del suceso"
              required
              disabled={disabled}
            >
              <TextArea
                value={formIncidencia.descripcion}
                onChange={(e) => changeInc("descripcion", e.target.value)}
                placeholder="Describí el suceso..."
                disabled={disabled}
              />
            </FormField>
          </div>
        </div>

        {/* ── SECCIÓN 2: Multa ──────────────────────────────────────────── */}
        <div
          className={`bg-white rounded-2xl border overflow-hidden ${disabled ? "border-slate-100 opacity-60" : "border-slate-200"}`}
        >
          <SectionHeader
            icon={DollarSign}
            title="2. Multa económica"
            subtitle="Tipo, monto, estado y descripción de la sanción"
            color="bg-amber-50 text-amber-500"
          />
          <div className="p-6 space-y-5">
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-5">
              <FormField label="Tipo de multa" required disabled={disabled}>
                <SelectField
                  value={formMulta.tipo}
                  onChange={(e) => changeMul("tipo", e.target.value)}
                  options={
                    valores?.tiposMulta?.map((t) => ({
                      value: t,
                      label: LABEL_TIPO_MULTA[t] ?? t,
                    })) ?? []
                  }
                  disabled={disabled}
                />
              </FormField>

              <FormField
                label="Monto"
                disabled={disabled || esAdministrativa}
                hint={
                  esAdministrativa
                    ? "No aplica para multas administrativas."
                    : undefined
                }
              >
                <div className="relative">
                  <DollarSign className="absolute left-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    value={esAdministrativa ? "" : formMulta.monto}
                    onChange={(e) => changeMul("monto", e.target.value)}
                    disabled={disabled || esAdministrativa}
                    placeholder={esAdministrativa ? "No aplica" : "0.00"}
                    className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-slate-900 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
                  />
                </div>
              </FormField>

              <FormField
                label="Estado de la multa"
                required
                disabled={disabled}
              >
                <SelectField
                  value={formMulta.estado}
                  onChange={(e) => changeMul("estado", e.target.value)}
                  options={
                    valores?.estadosMultaEditables?.map((s) => ({
                      value: s,
                      label: LABEL_ESTADO_MULTA[s] ?? s,
                    })) ?? []
                  }
                  disabled={disabled}
                />
              </FormField>
            </div>

            <FormField label="Descripción (opcional)" disabled={disabled}>
              <TextArea
                value={formMulta.descripcion}
                onChange={(e) => changeMul("descripcion", e.target.value)}
                placeholder="Observaciones adicionales..."
                rows={2}
                disabled={disabled}
              />
            </FormField>
          </div>
        </div>

        {/* ── SECCIÓN 3: Penalización ───────────────────────────────────── */}
        <div
          className={`bg-white rounded-2xl border overflow-hidden ${disabled ? "border-slate-100 opacity-60" : "border-slate-200"}`}
        >
          <SectionHeader
            icon={ShieldAlert}
            title="3. Penalización operativa"
            subtitle="Tipo, motivo, fechas y estado de la penalización"
            color="bg-purple-50 text-purple-500"
          />
          <div className="p-6 space-y-5">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              <FormField
                label="Tipo de penalización"
                required
                disabled={disabled}
              >
                <SelectField
                  value={formPenalizacion.tipo}
                  onChange={(e) => changePen("tipo", e.target.value)}
                  options={
                    valores?.tiposPenalizacion?.map((t) => ({
                      value: t,
                      label: LABEL_TIPO_PENALIZACION[t] ?? t,
                    })) ?? []
                  }
                  disabled={disabled}
                />
              </FormField>

              <FormField
                label="Estado de la penalización"
                required
                disabled={disabled}
              >
                <SelectField
                  value={formPenalizacion.estado}
                  onChange={(e) => changePen("estado", e.target.value)}
                  options={
                    valores?.estadosPenalizacionEditables?.map((s) => ({
                      value: s,
                      label: LABEL_ESTADO_PENALIZACION[s] ?? s,
                    })) ?? []
                  }
                  disabled={disabled}
                />
              </FormField>
            </div>

            <FormField
              label="Motivo de la penalización"
              required
              disabled={disabled}
            >
              <TextArea
                value={formPenalizacion.motivo}
                onChange={(e) => changePen("motivo", e.target.value)}
                placeholder="Explicá el motivo de la penalización..."
                disabled={disabled}
              />
            </FormField>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
              <FormField
                label="Fecha de fin"
                hint="Opcional. Vacío indica penalización indefinida."
                disabled={disabled}
              >
                <div className="relative">
                  <Calendar className="absolute left-4 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />
                  <input
                    type="date"
                    value={formPenalizacion.fechaFin}
                    onChange={(e) => changePen("fechaFin", e.target.value)}
                    disabled={disabled}
                    className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-slate-900 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
                  />
                </div>
              </FormField>
            </div>

            {/* Aviso: estado "revocada" no editable desde aquí */}
            {disabled && (
              <div className="flex items-start gap-3 bg-slate-50 border border-slate-200 rounded-xl px-4 py-3">
                <Info className="h-4 w-4 text-slate-400 shrink-0 mt-0.5" />
                <p className="text-sm text-slate-500">
                  La penalización fue{" "}
                  <span className="font-semibold">revocada</span>{" "}
                  automáticamente al cancelar la multa.
                </p>
              </div>
            )}
          </div>
        </div>

        {/* ── Error de negocio ──────────────────────────────────────────── */}
        {errorNegocio && (
          <div className="flex items-start gap-3 bg-red-50 border border-red-200 rounded-2xl px-5 py-4">
            <AlertTriangle className="h-5 w-5 text-red-500 shrink-0 mt-0.5" />
            <div className="flex-1">
              <p className="text-sm font-semibold text-red-800">No se pudo confirmar la acción</p>
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

        {/* ── Acciones ──────────────────────────────────────────────────── */}
        <div className="flex items-center justify-between gap-3 pb-4 flex-wrap">
          {/* Izquierda: Cancelar formulario + Cancelar Multa */}
          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={() => setModalCancelarForm(true)}
              className="px-6 py-3 text-slate-600 font-semibold hover:text-slate-900 transition-colors"
            >
              Cancelar
            </button>

            {!estaCancelada && (
              <button
                type="button"
                onClick={() => {
                  setMotivoCancelacion("");
                  setModalCancelarMul(true);
                }}
                disabled={loading}
                className="inline-flex items-center gap-2 px-5 py-3 bg-red-50 hover:bg-red-100 text-red-700 font-semibold text-sm rounded-xl border border-red-200 transition-all disabled:opacity-50"
              >
                <Ban className="h-4 w-4" />
                Cancelar Multa
              </button>
            )}
          </div>

          {/* Derecha: Confirmar cambios */}
          {!estaCancelada && (
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
                  <CheckCircle className="h-5 w-5" />
                  Confirmar cambios
                </>
              )}
            </button>
          )}

          {/* Si está cancelada: solo botón de volver */}
          {estaCancelada && (
            <Link
              to="/multas"
              className="inline-flex items-center gap-2 px-6 py-3 bg-slate-900 hover:bg-slate-800 text-white font-semibold rounded-xl transition-all"
            >
              <ArrowLeft className="h-4 w-4" />
              Volver al listado
            </Link>
          )}
        </div>
      </div>

      {/* ── Modal: confirmar cambios ───────────────────────────────────────── */}
      <ModalConfirmacion
        open={modalConfirmar}
        titulo="¿Confirmar los cambios?"
        descripcion="Se actualizarán los datos de la incidencia, la multa y la penalización."
        labelConfirmar="Sí, guardar cambios"
        colorConfirmar="bg-slate-900 hover:bg-slate-800"
        onConfirmar={handleUpdate}
        onCancelar={() => setModalConfirmar(false)}
        loading={loading}
      />

      {/* ── Modal: confirmar cancelar formulario ───────────────────────────── */}
      <ModalConfirmacion
        open={modalCancelarForm}
        titulo="¿Descartar los cambios?"
        descripcion="Los cambios no guardados se perderán y volverás al listado de multas."
        labelConfirmar="Sí, descartar"
        colorConfirmar="bg-red-600 hover:bg-red-700"
        onConfirmar={() => navigate("/multas")}
        onCancelar={() => setModalCancelarForm(false)}
        loading={false}
      />

      {/* ── Modal: cancelar multa ──────────────────────────────────────────── */}
      <ModalConfirmacion
        open={modalCancelarMul}
        titulo="¿Cancelar esta multa?"
        descripcion="Esta acción no puede deshacerse. La multa quedará cancelada y su penalización será revocada."
        labelConfirmar="Sí, cancelar multa"
        colorConfirmar="bg-red-600 hover:bg-red-700"
        onConfirmar={handleCancelarMulta}
        onCancelar={() => setModalCancelarMul(false)}
        loading={loading}
      >
        <div className="mb-4">
          <label className="text-sm font-medium text-slate-700 block mb-1.5">
            Motivo de cancelación{" "}
            <span className="text-slate-400 font-normal">(opcional)</span>
          </label>
          <textarea
            rows={3}
            value={motivoCancelacion}
            onChange={(e) => setMotivoCancelacion(e.target.value)}
            placeholder="Explicá brevemente el motivo del error o la razón de la cancelación..."
            className="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-xl text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-red-300 resize-none transition-all"
          />
          <p className="text-xs text-slate-400 mt-1.5">
            El motivo quedará registrado en la descripción de la multa para
            trazabilidad.
          </p>
        </div>
      </ModalConfirmacion>
    </div>
  );
}