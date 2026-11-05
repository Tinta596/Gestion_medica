using Gestion.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Data.Repositories
{
    public sealed class DoctorRepository : MySqlRepository
    {
        public async Task<bool> ExisteUsuarioActivoAsync(int idUsuario)
        {
            object result = await ScalarAsync(DoctorQueries.ContarUsuarioActivo,
                command => command.Parameters.AddWithValue("@Id", idUsuario));
            return result != null && Convert.ToInt32(result) > 0;
        }

        public async Task<bool> ExisteEspecialidadAsync(int idEspecialidad)
        {
            object result = await ScalarAsync(DoctorQueries.ContarEspecialidad,
                command => command.Parameters.AddWithValue("@Id", idEspecialidad));
            return result != null && Convert.ToInt32(result) > 0;
        }

        public Task<List<Usuario>> ObtenerPacientesAsync(string filtroNombre)
        {
            string filtro = filtroNombre?.Trim() ?? string.Empty;
            return QueryAsync(DoctorQueries.ObtenerPacientes, command =>
            {
                command.Parameters.AddWithValue("@IdRolPaciente", (int)TipoRol.Paciente);
                command.Parameters.AddWithValue("@Filtro", filtro);
                command.Parameters.AddWithValue("@Busqueda", "%" + filtro + "%");
            }, reader => MapUsuario(reader));
        }

        public Task<List<HistorialClinico>> ObtenerHistorialAsync(int pacienteId)
        {
            return QueryAsync(DoctorQueries.ObtenerHistorialClinico,
                command => command.Parameters.AddWithValue("@Id", pacienteId),
                reader => new HistorialClinico
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                    FechaConsulta = Convert.ToDateTime(reader["FechaConsulta"]),
                    Diagnostico = reader["Diagnostico"]?.ToString() ?? string.Empty,
                    Tratamiento = reader["Tratamiento"]?.ToString() ?? string.Empty,
                    NotasAdicionales = reader["NotasAdicionales"]?.ToString() ?? string.Empty
                });
        }

        public async Task<int> InsertarHistorialAsync(HistorialClinico historial)
        {
            object result = await ScalarAsync(DoctorQueries.InsertarHistorialClinico, command =>
            {
                command.Parameters.AddWithValue("@IdUsuario", historial.IdUsuario);
                command.Parameters.AddWithValue("@FechaConsulta", historial.FechaConsulta);
                command.Parameters.AddWithValue("@Diagnostico", historial.Diagnostico.Trim());
                command.Parameters.AddWithValue("@Tratamiento", historial.Tratamiento?.Trim() ?? string.Empty);
                command.Parameters.AddWithValue("@NotasAdicionales", historial.NotasAdicionales?.Trim() ?? string.Empty);
            });

            return Convert.ToInt32(result);
        }

        public Task<Usuario> ObtenerDatosPacienteAsync(int pacienteId)
        {
            return QuerySingleOrDefaultAsync(DoctorQueries.ObtenerDatosUsuario,
                command => command.Parameters.AddWithValue("@Id", pacienteId),
                reader => MapUsuario(reader));
        }

        public Task<List<Citas>> ObtenerCitasPacienteAsync(int pacienteId)
        {
            return QueryAsync(DoctorQueries.ObtenerCitasPaciente, command =>
            {
                command.Parameters.AddWithValue("@IdUsuario", pacienteId);
                command.Parameters.AddWithValue("@EstadoPendiente", (int)EstadoCita.Pendiente);
                command.Parameters.AddWithValue("@EstadoCompletada", (int)EstadoCita.Completada);
            }, reader => MapCita(reader));
        }

        public Task<Citas> ObtenerCitaAsync(int citaId)
        {
            return QuerySingleOrDefaultAsync(DoctorQueries.ObtenerCitaPorId,
                command => command.Parameters.AddWithValue("@Id", citaId),
                reader => MapCita(reader));
        }

        public Task<int> CompletarCitaAsync(int citaId)
        {
            return ExecuteAsync(DoctorQueries.CompletarCita, command =>
            {
                command.Parameters.AddWithValue("@Id", citaId);
                command.Parameters.AddWithValue("@Estado", (int)EstadoCita.Completada);
            });
        }

        public async Task<int> InsertarCitaAsync(Citas cita)
        {
            try
            {
                int insertedId = await InsertarCitaAsync(cita, DoctorQueries.InsertarCitaConPrecio, true);
                cita.Id = insertedId;
                return insertedId;
            }
            catch (MySqlException exception) when (exception.Number == 1054)
            {
                int insertedId = await InsertarCitaAsync(cita, DoctorQueries.InsertarCitaSinPrecio, false);
                cita.Id = insertedId;
                return insertedId;
            }
        }

        private async Task<int> InsertarCitaAsync(Citas cita, string query, bool includePrice)
        {
            object result = await ScalarAsync(query, command =>
            {
                command.Parameters.AddWithValue("@IdUsuario", cita.IdUsuario);
                command.Parameters.AddWithValue("@FechaHora", cita.FechaHora);
                command.Parameters.AddWithValue("@Estado", (int)cita.Estado);
                command.Parameters.AddWithValue("@IdEspecialidad", (int)cita.IdEspecialidad);
                if (includePrice)
                    command.Parameters.AddWithValue("@Precio", cita.Precio);
            });

            return Convert.ToInt32(result);
        }

        public Task<int> ActualizarCitaAsync(Citas cita)
        {
            return ExecuteAsync(DoctorQueries.ActualizarCita, command =>
            {
                command.Parameters.AddWithValue("@Id", cita.Id);
                command.Parameters.AddWithValue("@IdUsuario", cita.IdUsuario);
                command.Parameters.AddWithValue("@FechaHora", cita.FechaHora);
                command.Parameters.AddWithValue("@Estado", (int)cita.Estado);
                command.Parameters.AddWithValue("@IdEspecialidad", (int)cita.IdEspecialidad);
            });
        }
    }
}
