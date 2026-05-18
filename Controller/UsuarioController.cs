using Gestion.Model;
using GestionMedica.Services.Usuarios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Controller
{
    public class UsuarioController
    {
        private readonly UsuarioServices _service;

        public UsuarioController()
        {
            _service = new UsuarioServices();
        }

        public async Task<(bool ok, string msg, List<Citas> data)> MisCitas(int usuarioId)
        {
            if (usuarioId <= 0) return (false, "Id inválido", null);
            var citas = await _service.ObtenerCita(new Citas { IdUsuario = usuarioId });
            return (true, $"{citas.Count} cita(s)", citas);
        }

        
        public async Task<(bool ok, string msg, EstadoCita estado)> ConsultarEstadoCita(int citaId)
        {
            if (citaId <= 0) return (false, "Id cita inválido", EstadoCita.Pendiente);
            var e = await _service.ObtenerEstadoCita(citaId);
            return (true, $"Estado: {e}", e);
        }

        public async Task<(bool ok, string msg, int promedio)> PromedioCitas()
        {
            var prom = await _service.PromedioCitasPorPaciente();
            return (true, $"Promedio: {prom}", prom);
        }

        public async Task<(bool ok, string msg, Dictionary<EstadoCita, int> resumen)> ResumenEstados(int usuarioId)
        {
            if (usuarioId <= 0) return (false, "Id inválido", null);
            var resumen = await _service.ObtenerResumenEstadosUsuario(usuarioId);
            return (true, "Resumen cargado", resumen);
        }

        public async Task<(bool ok, string msg)> CancelarCita(int citaId)
        {
            if (citaId <= 0) return (false, "Id inválido");
            var ok = await _service.CancelarCita(citaId);
            return ok ? (true, "Cita cancelada") : (false, "Error al cancelar");
        }
    }
}
