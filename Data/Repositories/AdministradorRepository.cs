using Gestion.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Data.Repositories
{
    public sealed class AdministradorRepository : MySqlRepository
    {
        public Task<int> InsertarUsuarioAsync(Usuario usuario)
        {
            return ExecuteAsync(AdministradorQueries.InsertarUsuario, command =>
            {
                command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@Apellido", usuario.Apellido ?? string.Empty);
                command.Parameters.AddWithValue("@Correo", usuario.Correo);
                command.Parameters.AddWithValue("@Direccion", usuario.Direccion ?? string.Empty);
                command.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);
                command.Parameters.AddWithValue("@Activo", true);
                command.Parameters.AddWithValue("@IdRol", usuario.IdRol);
            });
        }

        public Task<int> ActualizarUsuarioAsync(Usuario usuario)
        {
            return ExecuteAsync(AdministradorQueries.ActualizarUsuario, command =>
            {
                command.Parameters.AddWithValue("@Id", usuario.Id);
                command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@Apellido", usuario.Apellido ?? string.Empty);
                command.Parameters.AddWithValue("@Correo", usuario.Correo);
                command.Parameters.AddWithValue("@Direccion", usuario.Direccion ?? string.Empty);
                command.Parameters.AddWithValue("@Contrasena", usuario.Contrasena ?? string.Empty);
                command.Parameters.AddWithValue("@Activo", usuario.Activo);
                command.Parameters.AddWithValue("@IdRol", usuario.IdRol);
            });
        }

        public Task<Usuario> ObtenerUsuarioAsync(int id)
        {
            return QuerySingleOrDefaultAsync(
                AdministradorQueries.ObtenerUsuarioPorId,
                command => command.Parameters.AddWithValue("@Id", id),
                reader => MapUsuario(reader));
        }

        public Task<List<Usuario>> BuscarUsuariosAsync(string filtroNombre)
        {
            string filtro = filtroNombre?.Trim() ?? string.Empty;
            return QueryAsync(
                AdministradorQueries.BuscarUsuariosPorNombre,
                command =>
                {
                    command.Parameters.AddWithValue("@IdRolDoctor", (int)TipoRol.Doctor);
                    command.Parameters.AddWithValue("@IdRolPaciente", (int)TipoRol.Paciente);
                    command.Parameters.AddWithValue("@Filtro", filtro);
                    command.Parameters.AddWithValue("@Busqueda", "%" + filtro + "%");
                },
                reader => MapUsuario(reader));
        }

        public async Task<bool> VerificarDisponibilidadAsync(DateTime fechaHora, TipoEspecialidad especialidad)
        {
            object result = await ScalarAsync(AdministradorQueries.ContarCitasEnHorario, command =>
            {
                command.Parameters.AddWithValue("@FechaHora", fechaHora);
                command.Parameters.AddWithValue("@IdEspecialidad", (int)especialidad);
            });

            return Convert.ToInt32(result) == 0;
        }

        public Task<List<Usuario>> ObtenerDoctoresActivosAsync()
        {
            return QueryAsync(
                AdministradorQueries.ObtenerDoctoresActivos,
                null,
                reader => new Usuario
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                    Apellido = reader["Apellido"]?.ToString() ?? string.Empty,
                    Correo = reader["Correo"]?.ToString() ?? string.Empty,
                    Activo = Convert.ToBoolean(reader["Activo"]),
                    IdRol = Convert.ToInt32(reader["IdRol"])
                });
        }

        public Task<Citas> ObtenerCitaCompletaAsync(int citaId)
        {
            return QuerySingleOrDefaultAsync(
                AdministradorQueries.ObtenerCitaCompletaPorId,
                command => command.Parameters.AddWithValue("@Id", citaId),
                reader => MapCita(reader));
        }

        public Task<Usuario> ObtenerUsuarioActivoPorCorreoAsync(string correo)
        {
            return QuerySingleOrDefaultAsync(
                AdministradorQueries.ObtenerCredencialesActivas,
                command => command.Parameters.AddWithValue("@Correo", correo),
                reader => MapUsuario(reader, includePassword: true));
        }

        public Task<int> DesactivarUsuarioAsync(int id)
        {
            return ExecuteAsync(AdministradorQueries.DesactivarUsuario,
                command => command.Parameters.AddWithValue("@Id", id));
        }

        public async Task<List<Citas>> ObtenerCitasAsync()
        {
            try
            {
                return await QueryAsync(AdministradorQueries.ObtenerCitasConPrecio, null,
                    reader => MapCita(reader));
            }
            catch (MySqlException exception) when (exception.Number == 1054)
            {
                return await QueryAsync(AdministradorQueries.ObtenerCitasSinPrecio, null,
                    reader => MapCita(reader, hasPrice: false));
            }
        }

        public Task<int> CambiarEstadoCitaAsync(int citaId, EstadoCita estado)
        {
            return ExecuteAsync(AdministradorQueries.ActualizarEstadoCita, command =>
            {
                command.Parameters.AddWithValue("@Id", citaId);
                command.Parameters.AddWithValue("@Estado", (int)estado);
            });
        }
    }
}
