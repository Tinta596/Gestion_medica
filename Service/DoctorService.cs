using Gestion.Model;
using Gestion.Data;
using Gestion.Data.Repositories;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Service
{
    public class DoctorServices
    {
        private readonly DoctorRepository _repository;

        public DoctorServices()
        {
            _repository = new DoctorRepository();
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // VALIDACIÃ“N DE LLAVES FORÃNEAS
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private string TraducirErrorMySql(MySqlException mex)
        {
            switch (mex.Number)
            {
                case 1452: return "Referencias invÃ¡lidas: el paciente o la especialidad no existen en la base de datos.";
                case 1451: return "No se puede modificar el registro porque estÃ¡ siendo usado por otra entidad.";
                case 1062: return "Ya existe un registro con los mismos datos.";
                case 1048: return "Falta un campo obligatorio.";
                case 1054: return "La base de datos no contiene una columna esperada. Contacta al administrador.";
                case 1146: return "Falta una tabla requerida en la base de datos.";
                default: return $"Error de base de datos (#{mex.Number}).";
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // CONSULTAS
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<List<Usuario>> ObtenerPacientes(string filtroNombre)
        {
            try
            {
                return await _repository.ObtenerPacientesAsync(filtroNombre);
            }
            catch
            {
                return new List<Usuario>();
            }
        }

        public async Task<List<HistorialClinico>> ObtenerHistorialPaciente(int id)
        {
            try
            {
                return await _repository.ObtenerHistorialAsync(id);
            }
            catch
            {
                return new List<HistorialClinico>();
            }
        }

        public async Task<(bool ok, string msg)> CrearHistorialClinico(HistorialClinico historial)
        {
            if (historial == null)
                return (false, "Datos del historial no proporcionados.");
            if (historial.IdUsuario <= 0)
                return (false, "Selecciona un paciente vÃ¡lido.");
            if (string.IsNullOrWhiteSpace(historial.Diagnostico))
                return (false, "El diagnÃ³stico es obligatorio.");
            if (historial.FechaConsulta == DateTime.MinValue)
                historial.FechaConsulta = DateTime.Now;

            try
            {
                if (!await _repository.ExisteUsuarioActivoAsync(historial.IdUsuario))
                    return (false, $"El paciente con ID {historial.IdUsuario} no existe o estÃ¡ inactivo.");

                int id = await _repository.InsertarHistorialAsync(historial);
                historial.Id = id;
                return id > 0
                    ? (true, "Historial cli­nico registrado correctamente.")
                    : (false, "No se pudo registrar el historial cli­nico.");
            }
            catch (MySqlException mex)
            {
                return (false, TraducirErrorMySql(mex));
            }
            catch (Exception)
            {
                return (false, "Error inesperado al registrar el historial clÃ­nico.");
            }
        }

        public async Task<Usuario> ObtenerDatosPaciente(int id)
        {
            try
            {
                return await _repository.ObtenerDatosPacienteAsync(id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Citas>> ObtenerCitasPaciente(int idPaciente)
        {
            try
            {
                return await _repository.ObtenerCitasPacienteAsync(idPaciente);
            }
            catch
            {
                return new List<Citas>();
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // OPERACIONES SOBRE CITAS
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<bool> CompletarCita(int citaId)
        {
            try
            {
                return await _repository.CompletarCitaAsync(citaId) > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool ok, string msg)> ProgramarCita(Citas cita)
        {
            if (cita == null)
                return (false, "Datos de cita no proporcionados.");
            if (cita.IdUsuario <= 0)
                return (false, "Selecciona un paciente vÃ¡lido.");
            if (cita.FechaHora <= DateTime.Now)
                return (false, "La fecha y hora deben ser futuras.");
            if ((int)cita.IdEspecialidad <= 0)
                return (false, "Selecciona una especialidad vÃ¡lida.");

            try
            {
                if (!await _repository.ExisteUsuarioActivoAsync(cita.IdUsuario))
                    return (false, $"El paciente con ID {cita.IdUsuario} no existe o estÃ¡ inactivo.");

                if (!await _repository.ExisteEspecialidadAsync((int)cita.IdEspecialidad))
                    return (false, "La especialidad seleccionada no existe en el catÃ¡logo.");

                if (cita.Estado == 0) cita.Estado = EstadoCita.Pendiente;

                int citaId = await _repository.InsertarCitaAsync(cita);
                return citaId > 0
                    ? (true, "Cita programada correctamente.")
                    : (false, "No se pudo registrar la cita.");
            }
            catch (MySqlException mex)
            {
                return (false, TraducirErrorMySql(mex));
            }
            catch (Exception)
            {
                return (false, "Error inesperado al programar la cita.");
            }
        }

        public async Task<(bool ok, string msg)> ActualizarCita(Citas cita)
        {
            if (cita == null || cita.Id <= 0)
                return (false, "Cita invÃ¡lida.");

            try
            {
                // Merge partial updates so missing values do not overwrite existing foreign keys.
                var existing = await _repository.ObtenerCitaAsync(cita.Id);
                if (existing == null)
                    return (false, $"No existe la cita con ID {cita.Id}.");

                cita.IdUsuario = cita.IdUsuario > 0 ? cita.IdUsuario : existing.IdUsuario;
                cita.FechaHora = cita.FechaHora > DateTime.MinValue ? cita.FechaHora : existing.FechaHora;
                cita.Estado = cita.Estado != 0 ? cita.Estado : existing.Estado;
                cita.IdEspecialidad = (int)cita.IdEspecialidad > 0 ? cita.IdEspecialidad : existing.IdEspecialidad;

                if (!await _repository.ExisteUsuarioActivoAsync(cita.IdUsuario))
                    return (false, $"El paciente con ID {cita.IdUsuario} no existe o estÃ¡ inactivo.");

                if (!await _repository.ExisteEspecialidadAsync((int)cita.IdEspecialidad))
                    return (false, "La especialidad asignada a la cita no existe en el catÃ¡logo.");

                int affected = await _repository.ActualizarCitaAsync(cita);
                return affected > 0
                    ? (true, "Cita actualizada correctamente.")
                    : (false, "No se pudo actualizar la cita.");
            }
            catch (MySqlException mex)
            {
                return (false, TraducirErrorMySql(mex));
            }
            catch (Exception)
            {
                return (false, "Error inesperado al actualizar la cita.");
            }
        }
    }
}