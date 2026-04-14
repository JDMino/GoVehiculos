import { create } from "zustand";
import api from "../api/axiosConfig";

const useAuthStore = create((set) => ({
  user: null,
  isAuthenticated: false,

  login: async (email, password) => {
    try {
      const res = await api.post("/Auth/login", { email, password });
      const { token, rolId } = res.data;

      localStorage.setItem("token", token);
      set({ user: { email, rolId }, isAuthenticated: true });
      return true;
    } catch (err) {
      console.error("Error en login:", err);
      return false;
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
