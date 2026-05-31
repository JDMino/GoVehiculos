import { create } from "zustand";
import api from "../api/axiosConfig";

const useAuthStore = create((set) => ({
  user: null,
  isAuthenticated: false,

  login: async (email, password) => {
    try {
      const res = await api.post("/Auth/login", { email, password });
      const {
        token,
        idUsuario,
        rolId,
        nombre,
        apellido,
        email: userEmail,
        errorMessage,
      } = res.data;

      // El backend devuelve errorMessage cuando la cuenta está inactiva
      // o bloqueada (responde 400). Se propaga al componente Login para
      // que lo muestre directamente sin usar un mensaje hardcodeado.
      if (errorMessage) {
        return { success: false, errorMessage };
      }

      localStorage.setItem("token", token);
      set({
        user: { idUsuario, email: userEmail, rolId, nombre, apellido },
        isAuthenticated: true,
      });

      return { success: true, rolId };
    } catch (err) {
      console.error("Error en login:", err);

      // Capturás el mensaje que viene en la respuesta del backend
      const backendMessage = err.response?.data?.errorMessage;

      return { success: false, errorMessage: backendMessage || null };
    }
  },

  register: async (form) => {
    try {
      await api.post("/Auth/register", form);
      return true;
    } catch (err) {
      console.error("Error en registro:", err);
      return false;
    }
  },

  logout: () => {
    localStorage.removeItem("token");
    set({ user: null, isAuthenticated: false });
  },
}));

export default useAuthStore;