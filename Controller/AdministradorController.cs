using Gestion.Model;
using GestionMedica.Services;
using GestionMedica.Services.Usuarios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Controller
{
    public class AdminController
    {
        private readonly AdministradorService _service;

        public AdminController()
        {
            _service = new AdministradorService();
        }

        public async Task<(bool ok, string msg, Usuario data)> Login(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return (false, "Correo requerido", null);

            if (string.IsNullOrWhiteSpace(contrasena))
                return (false, "Contraseña requerida", null);

            if (!correo.Contains("@"))
                return (false, "Correo inválido", null);

            var usuario = await _service.VerificarCredenciales(correo, contrasena);
            
            return usuario != null 
                ? (true, "Login exitoso", usuario) 
                : (false, "Correo o contraseña incorrectos", null);
        }

        public async Task<(bool ok, string msg)> CrearUsuario(Usuario u)
        {
            if (u == null) return (false, "Usuario nulo");
            if (string.IsNullOrWhiteSpace(u.Nombre)) return (false, "Nombre requerido");
            if (string.IsNullOrWhiteSpace(u.Correo) || !u.Correo.Contains("@")) return (false, "Correo inválido");

            u.IdRol = (int)TipoRol.Paciente;
            var resultado = await _service.CrearUsuarios(u);
            return resultado ? (true, "Usuario creado") : (false, "Error al crear");
        }

        public async Task<(bool ok, string msg)> CrearDoctor(Usuario u)
        {
            if (u == null) return (false, "Doctor nulo");
            if (string.IsNullOrWhiteSpace(u.Nombre)) return (false, "Nombre requerido");
            if (string.IsNullOrWhiteSpace(u.Correo) || !u.Correo.Contains("@")) return (false, "Correo inválido");
            if (string.IsNullOrWhiteSpace(u.Contrasena)) return (false, "Contraseña requerida");

            u.IdRol = (int)TipoRol.Doctor;
            u.Activo = true;
            var resultado = await _service.CrearUsuarios(u);
            return resultado ? (true, "Doctor creado") : (false, "Error al crear doctor");
        }

        public async Task<(bool ok, string msg)> ActualizarUsuario(Usuario u)
        {
            if (u == null || u.Id <= 0) return (false, "Usuario inválido");
            var ok = await _service.ActualizarUsuario(u);
            return ok ? (true, "Actualizado") : (false, "Error al actualizar");
        }

        public async Task<(bool ok, string msg, Usuario data)> BuscarUsuario(int id)
        {
            if (id <= 0) return (false, "Id inválido", null);
            var u = await _service.BuscarUsuario(id);
            return u != null ? (true, "Encontrado", u) : (false, "No existe", null);
        }

        public async Task<(bool ok, string msg, List<Usuario> data)> BuscarUsuariosPorNombre(string filtroNombre)
        {
            var usuarios = await _service.BuscarUsuariosPorNombre(filtroNombre) ?? new List<Usuario>();
            return (true, $"{usuarios.Count} usuario(s)", usuarios);
        }

        public async Task<(bool ok, string msg, bool disponible)> VerificarDisponibilidad(DateTime fecha, TipoEspecialidad esp)
        {
            if (fecha == DateTime.MinValue) return (false, "Fecha inválida", false);
            if (fecha <= DateTime.Now) return (false, "La fecha y hora deben ser futuras", false);

            var disp = await _service.VerificarDisponibilidad(fecha, esp);
            return (true,
                disp
                    ? $"Disponible para {esp} el {fecha:dd/MM/yyyy HH:mm}"
                    : $"Ocupado para {esp} el {fecha:dd/MM/yyyy HH:mm}",
                disp);
        }

        public async Task<(bool ok, string msg, List<Usuario> data)> ObtenerDoctores()
        {
            var lista = await _service.ObtenerDisponiblidadDoctor();
            return (true, $"{lista.Count} doctores", lista);
        }

        public async Task<(bool ok, string msg)> EnviarCorreo(string correo, Citas cita)
        {
            if (string.IsNullOrWhiteSpace(correo) || !correo.Contains("@")) return (false, "Correo inválido");
            if (cita == null) return (false, "Cita nula");
            var ok = await _service.EnviarConfirmacionPorCorreo(correo, cita);
            return ok ? (true, "Correo enviado") : (false, "Error al enviar");
        }

        public async Task<(bool ok, string msg)> DesactivarUsuario(int id)
        {
            return await _service.DesactivarUsuario(id);
        }

        public async Task<(bool ok, string msg, List<Citas> data)> ObtenerCitasMedicas()
        {
            var citas = await _service.ObtenerCitasMedicas();
            return (true, $"{citas.Count} cita(s)", citas);
        }

        public async Task<(bool ok, string msg)> CambiarEstadoCita(int citaId, EstadoCita estado)
        {
            if (citaId <= 0) return (false, "Id de cita inválido");

            if (estado != EstadoCita.Pendiente &&
                estado != EstadoCita.Confirmada &&
                estado != EstadoCita.Cancelada)
            {
                return (false, "Estado de cita inválido");
            }

            return await _service.CambiarEstadoCita(citaId, estado);
        }

        public async Task<(bool ok, string msg)> ConfirmarCita(int citaId)
        {
            if (citaId <= 0) return (false, "Id de cita inválido");
            return await CambiarEstadoCita(citaId, EstadoCita.Confirmada);
        }
    }
}
