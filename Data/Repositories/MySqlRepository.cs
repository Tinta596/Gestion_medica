using GestionMedica.DB;
using Gestion.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Data.Repositories
{
    public abstract class MySqlRepository
    {
        private readonly DBUsuario _database;

        protected MySqlRepository()
        {
            _database = new DBUsuario();
        }

        protected Task<MySqlConnection> OpenConnectionAsync()
        {
            return _database.AbrirConexion();
        }

        protected async Task<List<T>> QueryAsync<T>(
            string sql,
            Action<MySqlCommand> addParameters,
            Func<MySqlDataReader, T> map)
        {
            var results = new List<T>();
            using (var connection = await _database.AbrirConexion())
            using (var command = new MySqlCommand(sql, connection))
            {
                addParameters?.Invoke(command);
                using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        results.Add(map(reader));
                }
            }

            return results;
        }

        protected async Task<T> QuerySingleOrDefaultAsync<T>(
            string sql,
            Action<MySqlCommand> addParameters,
            Func<MySqlDataReader, T> map)
        {
            using (var connection = await _database.AbrirConexion())
            using (var command = new MySqlCommand(sql, connection))
            {
                addParameters?.Invoke(command);
                using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    return await reader.ReadAsync() ? map(reader) : default(T);
                }
            }
        }

        protected async Task<object> ScalarAsync(string sql, Action<MySqlCommand> addParameters)
        {
            using (var connection = await _database.AbrirConexion())
            using (var command = new MySqlCommand(sql, connection))
            {
                addParameters?.Invoke(command);
                return await command.ExecuteScalarAsync();
            }
        }

        protected async Task<int> ExecuteAsync(string sql, Action<MySqlCommand> addParameters)
        {
            using (var connection = await _database.AbrirConexion())
            using (var command = new MySqlCommand(sql, connection))
            {
                addParameters?.Invoke(command);
                return await command.ExecuteNonQueryAsync();
            }
        }

        protected static Usuario MapUsuario(MySqlDataReader reader, bool includePassword = false)
        {
            return new Usuario
            {
                Id = Convert.ToInt32(reader["Id"]),
                Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                Apellido = reader["Apellido"]?.ToString() ?? string.Empty,
                Correo = reader["Correo"]?.ToString() ?? string.Empty,
                Direccion = reader["Direccion"] == DBNull.Value ? string.Empty : reader["Direccion"].ToString(),
                Contrasena = includePassword && HasColumn(reader, "Contrasena")
                    ? reader["Contrasena"]?.ToString() ?? string.Empty
                    : string.Empty,
                Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"]),
                IdRol = reader["IdRol"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IdRol"]),
                Telefono = reader["Telefono"] == DBNull.Value ? string.Empty : reader["Telefono"].ToString()
            };
        }

        protected static Citas MapCita(MySqlDataReader reader, bool hasPrice = true)
        {
            return new Citas
            {
                Id = Convert.ToInt32(reader["Id"]),
                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                Estado = (EstadoCita)Convert.ToInt32(reader["Estado"]),
                IdEspecialidad = (TipoEspecialidad)Convert.ToInt32(reader["IdEspecialidad"]),
                Precio = hasPrice && reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0m,
                Paciente = HasColumn(reader, "Paciente") ? reader["Paciente"]?.ToString().Trim() : null,
                Motivo = HasColumn(reader, "Motivo") && reader["Motivo"] != DBNull.Value ? reader["Motivo"].ToString() : string.Empty,
                Observaciones = HasColumn(reader, "Observaciones") && reader["Observaciones"] != DBNull.Value ? reader["Observaciones"].ToString() : string.Empty
            };
        }

        protected static bool HasColumn(MySqlDataReader reader, string columnName)
        {
            for (int ordinal = 0; ordinal < reader.FieldCount; ordinal++)
            {
                if (string.Equals(reader.GetName(ordinal), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
