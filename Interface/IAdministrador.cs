using Gestion.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Interface
{
    public interface IAdministrador
    {
        Task<bool> CrearUsuarios(Usuario usuario);
        Task<bool> ActualizarUsuario(Usuario usuario);
        Task<Usuario> BuscarUsuario(int id);
        Task<List<Usuario>> BuscarUsuariosPorNombre(string filtroNombre);
        Task<bool> VerificarDisponibilidad(DateTime fechaHora, TipoEspecialidad especialidad);
        Task<List<Usuario>> ObtenerDisponiblidadDoctor();
        Task<bool> EnviarConfirmacionPorCorreo(string correo, Citas cita);
        Task<Usuario> VerificarCredenciales(string correo, string contrasena);
        Task<(bool ok, string msg)> DesactivarUsuario(int id);
        Task<List<Citas>> ObtenerCitasMedicas();
        Task<(bool ok, string msg)> CambiarEstadoCita(int citaId, EstadoCita estado);
        Task<(bool ok, string msg)> ConfirmarCita(int citaId);
    }
}
