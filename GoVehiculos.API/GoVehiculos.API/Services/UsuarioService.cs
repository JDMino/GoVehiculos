﻿using GoVehiculos.API.Data;
using GoVehiculos.API.DTOs;
using GoVehiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GoVehiculos.API.Services
{
    public class UsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UsuarioResponseDTO>> GetAllAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Select(u => new UsuarioResponseDTO
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Dni = u.Dni,
                    Rol = u.Rol != null ? u.Rol.Nombre : "",
                    RolId = u.RolId,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado,
                    FechaRegistro = u.FechaRegistro,
                    DireccionId = u.DireccionId,
                    Telefono = u.Telefono,
                })
                .ToListAsync();
        }

        public async Task<UsuarioResponseDTO?> GetByIdAsync(int id)
        {
            var u = await _context.Usuarios
                .Include(r => r.Rol)
                .FirstOrDefaultAsync(x => x.IdUsuario == id);

            if (u == null) return null;

            return new UsuarioResponseDTO
            {
                IdUsuario = u.IdUsuario,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                Dni = u.Dni,
                Rol = u.Rol?.Nombre ?? "",
                RolId = u.RolId,
                Activo = u.Activo,
                Bloqueado = u.Bloqueado,
                FechaRegistro = u.FechaRegistro,
                DireccionId = u.DireccionId,
                   Telefono = u.Telefono,
            };
        }

        public async Task<UsuarioResponseDTO> CreateAsync(UsuarioCreateDTO dto)
        {
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Dni = dto.Dni,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RolId = dto.RolId,
                DireccionId = dto.DireccionId,
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(usuario.IdUsuario) ?? new UsuarioResponseDTO();
        }

        public async Task<bool> UpdateAsync(int id, UsuarioUpdateDTO dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Telefono = dto.Telefono;
            usuario.Bloqueado = dto.Bloqueado;
            usuario.Activo = dto.Activo;
            usuario.RolId = dto.RolId;
            usuario.DireccionId = dto.DireccionId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            usuario.Activo = false;
            usuario.FechaBaja = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}