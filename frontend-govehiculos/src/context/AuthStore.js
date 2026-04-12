import { create } from "zustand";
import api from "../api/axiosConfig";

const useAuthStore = create((set) => ({
  user: null,
  token: null,
  login: async (email, password) => {
    try {
      const response = await api.post("/auth/login", { email, password });
      localStorage.setItem("token", response.data.token);
      set({ user: { rolId: response.data.rolId }, token: response.data.token });
      return true;
    } catch {
      return false;
    }
  },
  register: async (data) => {
    try {
      await api.post("/auth/register", data);
      return true;
    } catch {
      return false;
    }
  },
  logout: () => {
    localStorage.removeItem("token");
    set({ user: null, token: null });
  },
}));

export default useAuthStore;
