using Gestion.Model;
using GestionMedica.DB;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Service
{
    public class DoctorServices
    {
        private readonly DBUsuario dBUsuario;

        public DoctorServices()
        {
            dBUsuario = new DBUsuario();
        }

        private Usuario MapUsuario(MySqlDataReader reader)
        {
            return new Usuario
            {
                Id = Convert.ToInt32(reader["Id"]),
                Nombre = reader["Nombre"]?.ToString() ?? "",
                Apellido = reader["Apellido"]?.ToString() ?? "",
                Correo = reader["Correo"]?.ToString() ?? "",
                Direccion = reader["Direccion"] != DBNull.Value ? reader["Direccion"].ToString() : "",
                Contrasena = reader["Contrasena"] != DBNull.Value ? reader["Contrasena"].ToString() : "",
                Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"]),
                IdRol = reader["IdRol"] != DBNull.Value ? Convert.ToInt32(reader["IdRol"]) : 0,
                Telefono = reader["Telefono"] != DBNull.Value ? reader["Telefono"].ToString() : ""
            };
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // VALIDACIÃ“N DE LLAVES FORÃNEAS
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private async Task<bool> ExisteUsuarioActivo(int idUsuario, MySqlConnection con)
        {
            using (var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM Usuario WHERE Id = @Id AND Activo = 1", con))
            {
                cmd.Parameters.AddWithValue("@Id", idUsuario);
                var r = await cmd.ExecuteScalarAsync();
                return r != null && Convert.ToInt32(r) > 0;
            }
        }

        private async Task<bool> ExisteEspecialidad(int idEspecialidad, MySqlConnection con)
        {
            using (var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM especialidadcatalogo WHERE Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", idEspecialidad);
                var r = await cmd.ExecuteScalarAsync();
                return r != null && Convert.ToInt32(r) > 0;
            }
        }

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
            var lista = new List<Usuario>();

            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    string query = @"SELECT Id, Nombre, Apellido, Correo, Direccion, Contrasena, Activo, IdRol, Telefono
                                     FROM Usuario
                                     WHERE IdRol = @IdRolPaciente
                                       AND Activo = 1
                                       AND (
                                           @Filtro = ''
                                           OR Nombre LIKE @Busqueda
                                           OR Apellido LIKE @Busqueda
                                           OR CONCAT(Nombre, ' ', Apellido) LIKE @Busqueda
                                           OR Correo LIKE @Busqueda
                                       )
                                     ORDER BY Nombre, Apellido";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        string filtro = filtroNombre?.Trim() ?? "";
                        cmd.Parameters.AddWithValue("@IdRolPaciente", (int)TipoRol.Paciente);
                        cmd.Parameters.AddWithValue("@Filtro", filtro);
                        cmd.Parameters.AddWithValue("@Busqueda", "%" + filtro + "%");

                        using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                lista.Add(MapUsuario(reader));
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return lista;
        }

        public async Task<List<HistorialClinico>> ObtenerHistorialPaciente(int id)
        {
            var lista = new List<HistorialClinico>();

            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    using (var cmd = new MySqlCommand("SELECT * FROM historial_clinico WHERE IdUsuario = @Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                lista.Add(new HistorialClinico
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    FechaConsulta = Convert.ToDateTime(reader["FechaConsulta"]),
                                    Diagnostico = reader["Diagnostico"]?.ToString() ?? "",
                                    Tratamiento = reader["Tratamiento"]?.ToString() ?? "",
                                    NotasAdicionales = reader["NotasAdicionales"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return lista;
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

            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    if (!await ExisteUsuarioActivo(historial.IdUsuario, con))
                        return (false, $"El paciente con ID {historial.IdUsuario} no existe o estÃ¡ inactivo.");

                    string query = @"INSERT INTO historial_clinico
                                        (IdUsuario, FechaConsulta, Diagnostico, Tratamiento, NotasAdicionales)
                                     VALUES
                                        (@IdUsuario, @FechaConsulta, @Diagnostico, @Tratamiento, @NotasAdicionales);
                                     SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", historial.IdUsuario);
                        cmd.Parameters.AddWithValue("@FechaConsulta", historial.FechaConsulta);
                        cmd.Parameters.AddWithValue("@Diagnostico", historial.Diagnostico.Trim());
                        cmd.Parameters.AddWithValue("@Tratamiento", historial.Tratamiento?.Trim() ?? string.Empty);
                        cmd.Parameters.AddWithValue("@NotasAdicionales", historial.NotasAdicionales?.Trim() ?? string.Empty);

                        int id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        historial.Id = id;
                        return id > 0
                            ? (true, "Historial clÃ­nico registrado correctamente.")
                            : (false, "No se pudo registrar el historial clÃ­nico.");
                    }
                }
                catch (MySqlException mex)
                {

                    return (false, TraducirErrorMySql(mex));
                }
                catch (Exception ex)
                {

                    return (false, "Error inesperado al registrar el historial clÃ­nico.");
                }
            }
        }

        public async Task<Usuario> ObtenerDatosPaciente(int id)
        {
            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    using (var cmd = new MySqlCommand("SELECT * FROM Usuario WHERE Id = @Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                                return MapUsuario(reader);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return null;
        }

        public async Task<List<Citas>> ObtenerCitasPaciente(int idPaciente)
        {
            var lista = new List<Citas>();

            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    // El panel del doctor sÃ³lo debe mostrar citas activas para Ã©l:
                    // - Pendientes (aÃºn por atender)
                    // - Completadas (registro histÃ³rico de la consulta)
                    // Las Confirmadas (gestionadas por el admin) y Canceladas (por el paciente)
                    // se ocultan automÃ¡ticamente del panel.
                    string query = @"SELECT Id, IdUsuario, FechaHora, Estado, IdEspecialidad, Precio
                                     FROM Citas
                                     WHERE IdUsuario = @IdUsuario
                                       AND Estado IN (@EstadoPendiente, @EstadoCompletada)
                                     ORDER BY FechaHora DESC";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", idPaciente);
                        cmd.Parameters.AddWithValue("@EstadoPendiente", (int)EstadoCita.Pendiente);
                        cmd.Parameters.AddWithValue("@EstadoCompletada", (int)EstadoCita.Completada);

                        using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                lista.Add(new Citas
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                                    Estado = (EstadoCita)Convert.ToInt32(reader["Estado"]),
                                    IdEspecialidad = (TipoEspecialidad)Convert.ToInt32(reader["IdEspecialidad"]),
                                    Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0m
                                });
                            }
                        }
                    }
                }
                catch (MySqlException mex)
                {

                }
                catch (Exception ex)
                {

                }
            }

            return lista;
        }

        private async Task<Citas> ObtenerCitaPorId(int citaId, MySqlConnection con)
        {
            string query = @"SELECT Id, IdUsuario, FechaHora, Estado, IdEspecialidad, Precio
                             FROM Citas
                             WHERE Id = @Id";
            using (var cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Id", citaId);
                using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Citas
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                            FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                            Estado = (EstadoCita)Convert.ToInt32(reader["Estado"]),
                            IdEspecialidad = (TipoEspecialidad)Convert.ToInt32(reader["IdEspecialidad"]),
                            Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0m
                        };
                    }
                }
            }
            return null;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // OPERACIONES SOBRE CITAS
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<bool> CompletarCita(int citaId)
        {
            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    string query = @"UPDATE Citas
                                     SET Estado = @Estado
                                     WHERE Id = @Id";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", citaId);
                        cmd.Parameters.AddWithValue("@Estado", (int)EstadoCita.Completada);

                        return await cmd.ExecuteNonQueryAsync() > 0;
                    }
                }
                catch (MySqlException mex)
                {

                    return false;
                }
                catch (Exception ex)
                {

                    return false;
                }
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

            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    if (!await ExisteUsuarioActivo(cita.IdUsuario, con))
                        return (false, $"El paciente con ID {cita.IdUsuario} no existe o estÃ¡ inactivo.");

                    if (!await ExisteEspecialidad((int)cita.IdEspecialidad, con))
                        return (false, "La especialidad seleccionada no existe en el catÃ¡logo.");

                    if (cita.Estado == 0) cita.Estado = EstadoCita.Pendiente;

                    return await InsertarCita(con, cita);
                }
                catch (MySqlException mex)
                {

                    return (false, TraducirErrorMySql(mex));
                }
                catch (Exception ex)
                {

                    return (false, "Error inesperado al programar la cita.");
                }
            }
        }

        private async Task<(bool ok, string msg)> InsertarCita(MySqlConnection con, Citas cita)
        {
            string query = @"INSERT INTO Citas (IdUsuario, FechaHora, Estado, IdEspecialidad, Precio)
                             VALUES (@IdUsuario, @FechaHora, @Estado, @IdEspecialidad, @Precio);
                             SELECT LAST_INSERT_ID();";
            try
            {
                using (var cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", cita.IdUsuario);
                    cmd.Parameters.AddWithValue("@FechaHora", cita.FechaHora);
                    cmd.Parameters.AddWithValue("@Estado", (int)cita.Estado);
                    cmd.Parameters.AddWithValue("@IdEspecialidad", (int)cita.IdEspecialidad);
                    cmd.Parameters.AddWithValue("@Precio", cita.Precio);

                    int citaId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    cita.Id = citaId;
                    return citaId > 0
                        ? (true, "Cita programada correctamente.")
                        : (false, "No se pudo registrar la cita.");
                }
            }
            catch (MySqlException mex) when (mex.Number == 1054)
            {
                return await InsertarCitaSinPrecio(con, cita);
            }
        }

        private async Task<(bool ok, string msg)> InsertarCitaSinPrecio(MySqlConnection con, Citas cita)
        {
            string query = @"INSERT INTO Citas (IdUsuario, FechaHora, Estado, IdEspecialidad)
                             VALUES (@IdUsuario, @FechaHora, @Estado, @IdEspecialidad);
                             SELECT LAST_INSERT_ID();";

            using (var cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@IdUsuario", cita.IdUsuario);
                cmd.Parameters.AddWithValue("@FechaHora", cita.FechaHora);
                cmd.Parameters.AddWithValue("@Estado", (int)cita.Estado);
                cmd.Parameters.AddWithValue("@IdEspecialidad", (int)cita.IdEspecialidad);

                int citaId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                cita.Id = citaId;
                return citaId > 0
                    ? (true, "Cita programada correctamente.")
                    : (false, "No se pudo registrar la cita.");
            }
        }

        public async Task<(bool ok, string msg)> ActualizarCita(Citas cita)
        {
            if (cita == null || cita.Id <= 0)
                return (false, "Cita invÃ¡lida.");

            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    // Cargamos la cita existente para hacer un UPDATE parcial
                    // y evitar sobrescribir las llaves forÃ¡neas con valores 0.
                    var existente = await ObtenerCitaPorId(cita.Id, con);
                    if (existente == null)
                        return (false, $"No existe la cita con ID {cita.Id}.");

                    // Mezclamos: el caller sÃ³lo cambia los campos que envÃ­a con valor.
                    int idUsuario = cita.IdUsuario > 0 ? cita.IdUsuario : existente.IdUsuario;
                    DateTime fecha = cita.FechaHora > DateTime.MinValue ? cita.FechaHora : existente.FechaHora;
                    EstadoCita estado = cita.Estado != 0 ? cita.Estado : existente.Estado;
                    TipoEspecialidad esp = (int)cita.IdEspecialidad > 0 ? cita.IdEspecialidad : existente.IdEspecialidad;

                    if (!await ExisteUsuarioActivo(idUsuario, con))
                        return (false, $"El paciente con ID {idUsuario} no existe o estÃ¡ inactivo.");

                    if (!await ExisteEspecialidad((int)esp, con))
                        return (false, "La especialidad asignada a la cita no existe en el catÃ¡logo.");

                    string query = @"UPDATE Citas
                                     SET IdUsuario = @IdUsuario,
                                         FechaHora = @FechaHora,
                                         Estado = @Estado,
                                         IdEspecialidad = @IdEspecialidad
                                     WHERE Id = @Id";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", cita.Id);
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        cmd.Parameters.AddWithValue("@FechaHora", fecha);
                        cmd.Parameters.AddWithValue("@Estado", (int)estado);
                        cmd.Parameters.AddWithValue("@IdEspecialidad", (int)esp);

                        int filas = await cmd.ExecuteNonQueryAsync();
                        return filas > 0
                            ? (true, "Cita actualizada correctamente.")
                            : (false, "No se pudo actualizar la cita.");
                    }
                }
                catch (MySqlException mex)
                {

                    return (false, TraducirErrorMySql(mex));
                }
                catch (Exception ex)
                {

                    return (false, "Error inesperado al actualizar la cita.");
                }
            }
        }
    }
}