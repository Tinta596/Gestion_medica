using Gestion.Model;
using Gestion.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Controller
{
    public class DoctorController
    {
        private readonly DoctorServices _service;

        public DoctorController()
        {
            _service = new DoctorServices();
        }

        public async Task<(bool ok, string msg, List<Usuario> data)> ListarPacientes(string filtroNombre)
        {
            var pacientes = await _service.ObtenerPacientes(filtroNombre) ?? new List<Usuario>();
            return (true, $"{pacientes.Count} paciente(s)", pacientes);
        }

        public async Task<(bool ok, string msg, List<HistorialClinico> data)> HistorialPaciente(int pacienteId)
        {
            if (pacienteId <= 0) return (false, "Id paciente inválido", null);
            var hist = await _service.ObtenerHistorialPaciente(pacienteId) ?? new List<HistorialClinico>();
            return (true, $"{hist.Count} registros", hist);
        }

        public async Task<(bool ok, string msg)> CrearHistorialClinico(HistorialClinico historial)
        {
            if (historial == null) return (false, "Historial nulo.");
            if (historial.IdUsuario <= 0) return (false, "Selecciona un paciente válido.");
            if (string.IsNullOrWhiteSpace(historial.Diagnostico)) return (false, "Ingresa un diagnóstico.");
            if (historial.FechaConsulta > DateTime.Now)
                return (false, "La fecha de consulta no puede ser futura.");

            return await _service.CrearHistorialClinico(historial);
        }

        public async Task<(bool ok, string msg, Usuario data)> DatosPaciente(int pacienteId)
        {
            if (pacienteId <= 0) return (false, "Id inválido", null);
            var p = await _service.ObtenerDatosPaciente(pacienteId);
            return p != null ? (true, "Paciente encontrado", p) : (false, "No existe", null);
        }

        public async Task<(bool ok, string msg, List<Citas> data)> CitasPaciente(int pacienteId)
        {
            if (pacienteId <= 0) return (false, "Paciente inválido", null);
            var citas = await _service.ObtenerCitasPaciente(pacienteId) ?? new List<Citas>();
            return (true, $"{citas.Count} cita(s)", citas);
        }

        public async Task<(bool ok, string msg)> CompletarCita(int citaId)
        {
            if (citaId <= 0) return (false, "Cita inválida");
            var ok = await _service.CompletarCita(citaId);
            return ok ? (true, "Cita marcada como completada") : (false, "No se pudo completar la cita");
        }

        public async Task<(bool ok, string msg)> ProgramarCita(Citas cita)
        {
            if (cita == null) return (false, "Cita nula");
            if (cita.IdUsuario <= 0) return (false, "Selecciona un paciente válido.");
            if (cita.FechaHora <= DateTime.Now) return (false, "La fecha y hora deben ser futuras.");
            if (!EsEspecialidadValida(cita.IdEspecialidad))
                return (false, "Selecciona una especialidad válida.");

            return await _service.ProgramarCita(cita);
        }

        public async Task<(bool ok, string msg)> ActualizarCita(Citas cita)
        {
            if (cita == null || cita.Id <= 0) return (false, "Cita inválida.");

            // Sólo bloqueamos si no se proporcionó ningún cambio.
            bool sinCambios = cita.IdUsuario <= 0
                              && cita.FechaHora <= DateTime.MinValue
                              && cita.Estado == 0
                              && (int)cita.IdEspecialidad <= 0;

            if (sinCambios)
                return (false, "No se proporcionó ningún cambio para actualizar.");

            if ((int)cita.IdEspecialidad > 0 && !EsEspecialidadValida(cita.IdEspecialidad))
                return (false, "Selecciona una especialidad válida.");

            return await _service.ActualizarCita(cita);
        }

        private static bool EsEspecialidadValida(TipoEspecialidad especialidad)
        {
            return (int)especialidad > 0 && Enum.IsDefined(typeof(TipoEspecialidad), especialidad);
        }
    }
}
