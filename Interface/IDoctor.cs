using Gestion.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Interface
{
    public interface IDoctor
    {
        Task<List<HistorialClinico>> ObtenerHistorialPaciente(int id);
        Task<Usuario> ObtenerDatosPaciente(int id);
        Task<bool> ProgramarCita(Citas cita, Usuario usuario);
        Task<bool> ActualizarHistorial(int usuarioId, HistorialClinico historial);
        Task<(bool ok, string msg)> CrearHistorialClinico(HistorialClinico historial);
        Task<List<Usuario>> ObtenerPacientes(string filtroNombre);
    }
}