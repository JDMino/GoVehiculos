import axios from "axios";

const api = axios.create({
  baseURL: "http://localhost:5000/api", // URL del backend
});

// Interceptor para incluir el token JWT en cada request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
