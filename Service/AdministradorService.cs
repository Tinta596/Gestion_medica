using BCrypt.Net;
using DotNetEnv;
using Gestion.Interface;
using Gestion.Model;
using GestionMedica.DB;
using MailKit.Security;
using MimeKit;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace GestionMedica.Services
{
    public class AdministradorService : IAdministrador
    {
        private readonly DBUsuario dBUsuario;

        public AdministradorService()
        {
            dBUsuario = new DBUsuario();
        }

        public async Task<bool> CrearUsuarios(Usuario usuario)
        {
            bool resultado = false;
            MySqlConnection con = null;
            try
            {
                
                con = await dBUsuario.AbrirConexion();
                

                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
                
                string query = @"INSERT INTO Usuario (Nombre, Apellido, Correo, Direccion, Contrasena, Activo, IdRol)
                                 VALUES (@Nombre, @Apellido, @Correo, @Direccion, @Contrasena, @Activo, @IdRol)";

                

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                    cmd.Parameters.AddWithValue("@Direccion", usuario.Direccion ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);
                    cmd.Parameters.AddWithValue("@Activo", true);   // nuevo usuario siempre activo
                    cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                    
                    resultado = filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
            }
            return resultado;
        }

        public async Task<bool> ActualizarUsuario(Usuario usuario)
        {
            bool resultado = false;

            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    if (!string.IsNullOrEmpty(usuario.Contrasena))
                        usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);

                    string query = @"UPDATE Usuario
                                     SET Nombre=@Nombre,
                                         Apellido=@Apellido,
                                         Correo=@Correo,
                                         Direccion=@Direccion,
                                         Contrasena=@Contrasena,
                                         Activo=@Activo,
                                         IdRol=@IdRol
                                     WHERE Id=@Id";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", usuario.Id);
                        cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", usuario.Apellido ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                        cmd.Parameters.AddWithValue("@Direccion", usuario.Direccion ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);
                        cmd.Parameters.AddWithValue("@Activo", usuario.Activo);
                        cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);

                        int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                        resultado = filasAfectadas > 0;
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }

            return resultado;
        }

        public async Task<Usuario> BuscarUsuario(int id)
        {
            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    string query = "SELECT * FROM Usuario WHERE Id = @Id";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Usuario
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Nombre = reader["Nombre"].ToString(),
                                    Apellido = reader["Apellido"].ToString(),
                                    Correo = reader["Correo"].ToString(),
                                    Direccion = reader["Direccion"].ToString(),
                                    Contrasena = reader["Contrasena"].ToString(),
                                    Activo = Convert.ToBoolean(reader["Activo"]),
                                    IdRol = Convert.ToInt32(reader["IdRol"]),
                                    Telefono = reader["Telefono"] != DBNull.Value ? reader["Telefono"].ToString() : ""
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                }
            }

            return null;
        }

        public async Task<List<Usuario>> BuscarUsuariosPorNombre(string filtroNombre)
        {
            var usuarios = new List<Usuario>();

            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    string query = @"SELECT Id, Nombre, Apellido, Correo, Direccion, Contrasena, Activo, IdRol, Telefono
                                     FROM Usuario
                                     WHERE IdRol IN (@IdRolDoctor, @IdRolPaciente)
                                       AND (
                                           @Filtro = ''
                                           OR Nombre LIKE @Busqueda
                                           OR Apellido LIKE @Busqueda
                                           OR CONCAT(Nombre, ' ', Apellido) LIKE @Busqueda
                                           OR Correo LIKE @Busqueda
                                       )
                                     ORDER BY IdRol, Nombre, Apellido";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        string filtro = filtroNombre?.Trim() ?? "";
                        cmd.Parameters.AddWithValue("@IdRolDoctor", (int)TipoRol.Doctor);
                        cmd.Parameters.AddWithValue("@IdRolPaciente", (int)TipoRol.Paciente);
                        cmd.Parameters.AddWithValue("@Filtro", filtro);
                        cmd.Parameters.AddWithValue("@Busqueda", "%" + filtro + "%");

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                usuarios.Add(new Usuario
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Nombre = reader["Nombre"].ToString(),
                                    Apellido = reader["Apellido"].ToString(),
                                    Correo = reader["Correo"].ToString(),
                                    Direccion = reader["Direccion"].ToString(),
                                    Contrasena = reader["Contrasena"].ToString(),
                                    Activo = Convert.ToBoolean(reader["Activo"]),
                                    IdRol = Convert.ToInt32(reader["IdRol"]),
                                    Telefono = reader["Telefono"] != DBNull.Value ? reader["Telefono"].ToString() : ""
                                });
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("");
                }
            }

            return usuarios;
        }

        public async Task<bool> VerificarDisponibilidad(DateTime fechaHora, TipoEspecialidad especialidad)
        {
            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    string query = @"SELECT COUNT(*)
                                     FROM Citas
                                     WHERE FechaHora = @FechaHora
                                       AND IdEspecialidad = @IdEspecialidad
                                       AND Estado != 3";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@FechaHora", fechaHora);
                        cmd.Parameters.AddWithValue("@IdEspecialidad", especialidad);

                        var resultado = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(resultado) == 0;
                    }
                }
                catch (Exception ex)
                {
                    
                    return false;
                }
            }
        }

        public async Task<List<Usuario>> ObtenerDisponiblidadDoctor()
        {
            var lista = new List<Usuario>();

            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    string query = @"SELECT u.Id, u.Nombre, u.Apellido, u.Correo, u.Activo, u.IdRol
                                     FROM Usuario u
                                     INNER JOIN Rol r ON u.IdRol = r.Id
                                     WHERE r.Id = 2 AND u.Activo = 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Usuario
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Nombre = reader["Nombre"].ToString(),
                                Apellido = reader["Apellido"].ToString(),
                                Correo = reader["Correo"].ToString(),
                                Activo = Convert.ToBoolean(reader["Activo"]),
                                IdRol = Convert.ToInt32(reader["IdRol"])
                            });
                        }
                    }
                }
                catch(Exception ex)
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                }
            }

            return lista;
        }

        public async Task<bool> EnviarConfirmacionPorCorreo(string correo, Citas cita)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(correo))
                {
                    MessageBox.Show("Correo inválido");
                    return false;
                }

                if (cita == null)
                {
                    MessageBox.Show("La cita es nula");
                    return false;
                }

                // TLS moderno
                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12;

                string mailFrom = "juandavidbarriosperneth@gmail.com";
                string mailPassword = "qoozcmuiahwkgtxg";

                var mensaje = new MimeMessage();

                mensaje.From.Add(
                    new MailboxAddress(
                        "Sistema Médico",
                        mailFrom));

                mensaje.To.Add(
                    MailboxAddress.Parse(correo));

                mensaje.Subject = "Confirmación de Cita Médica";

                mensaje.Body = new TextPart("html")
                {
                    Text = $@"
            <h2>Confirmación de Cita</h2>

            <p>Su cita ha sido agendada correctamente.</p>

            <hr>

            <p><b>Paciente:</b> {cita.Paciente}</p>

            <p><b>Fecha:</b> {cita.FechaHora:dd/MM/yyyy}</p>

            <p><b>Hora:</b> {cita.FechaHora:hh:mm tt}</p>

            <p><b>Especialidad:</b> {cita.IdEspecialidad}</p>

            <p><b>Estado:</b> {cita.Estado}</p>

            <hr>

            <p>Gracias por usar nuestro sistema.</p>
            "
                };

                using (var cliente = new MailKit.Net.Smtp.SmtpClient())
                {
                    // Ignorar SSL local (solo desarrollo)
                    cliente.ServerCertificateValidationCallback =
                        (s, c, h, e) => true;

                    // Timeout
                    cliente.Timeout = 10000;

                    // Conexión SSL directa
                    await cliente.ConnectAsync(
                        "smtp.gmail.com",
                        465,
                        SecureSocketOptions.SslOnConnect);

                    // Login Gmail
                    await cliente.AuthenticateAsync(
                        mailFrom,
                        mailPassword);

                    // Enviar
                    await cliente.SendAsync(mensaje);

                    // Desconectar
                    await cliente.DisconnectAsync(true);
                }

                MessageBox.Show(
                    "Correo enviado correctamente",
                    "Sistema",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "ERROR SMTP",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Debug.WriteLine(ex.ToString());

                return false;
            }
        }

        public async Task<Usuario> VerificarCredenciales(string correo, string contrasena)
        {
            using (var con = await dBUsuario.AbrirConexion())
            {
                try
                {
                    string query = @"SELECT Id, Nombre, Apellido, Correo, Contrasena, Activo, IdRol
                                     FROM Usuario
                                     WHERE Correo = @Correo AND Activo = 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Correo", correo);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string hash = reader["Contrasena"].ToString();

                                if (BCrypt.Net.BCrypt.Verify(contrasena, hash))
                                {
                                    return new Usuario
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        Nombre = reader["Nombre"].ToString(),
                                        Apellido = reader["Apellido"].ToString(),
                                        Correo = reader["Correo"].ToString(),
                                        Activo = Convert.ToBoolean(reader["Activo"]),
                                        IdRol = Convert.ToInt32(reader["IdRol"])
                                    };
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                }
            }

            return null;
        }

        public async Task<(bool ok, string msg)> DesactivarUsuario(int id)
        {
            try
            {
                using (var con = await dBUsuario.AbrirConexion())
                {
                    string query = @"UPDATE Usuario
                                     SET Activo = 0
                                     WHERE Id = @Id AND Activo = 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        int filas = await cmd.ExecuteNonQueryAsync();

                        return filas > 0
                            ? (true, "Usuario desactivado correctamente")
                            : (false, "Usuario no encontrado o ya desactivado");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al desactivar usuario: {ex.Message}");
            }
        }

        public async Task<List<Citas>> ObtenerCitasMedicas()
        {
            var citas = new List<Citas>();

            try
            {
                using (var con = await dBUsuario.AbrirConexion())
                {
                    string query = @"SELECT c.Id,
                                            c.IdUsuario,
                                            c.FechaHora,
                                            c.Estado,
                                            c.IdEspecialidad,
                                            c.Precio,
                                            CONCAT(u.Nombre, ' ', COALESCE(u.Apellido, '')) AS Paciente
                                     FROM Citas c
                                     INNER JOIN Usuario u ON c.IdUsuario = u.Id
                                     ORDER BY c.FechaHora DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            citas.Add(new Citas
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                                Estado = (EstadoCita)Convert.ToInt32(reader["Estado"]),
                                IdEspecialidad = (TipoEspecialidad)Convert.ToInt32(reader["IdEspecialidad"]),
                                Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0m,
                                Paciente = reader["Paciente"]?.ToString().Trim()
                            });
                        }
                    }
                }
            }
            catch (MySqlException mex) when (mex.Number == 1054)
            {
                return await ObtenerCitasMedicasSinPrecio();
            }
            return citas;
        }

        private async Task<List<Citas>> ObtenerCitasMedicasSinPrecio()
        {
            var citas = new List<Citas>();

            try
            {
                using (var con = await dBUsuario.AbrirConexion())
                {
                    string query = @"SELECT c.Id,
                                            c.IdUsuario,
                                            c.FechaHora,
                                            c.Estado,
                                            c.IdEspecialidad,
                                            CONCAT(u.Nombre, ' ', COALESCE(u.Apellido, '')) AS Paciente
                                     FROM Citas c
                                     INNER JOIN Usuario u ON c.IdUsuario = u.Id
                                     ORDER BY c.FechaHora DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            citas.Add(new Citas
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                                Estado = (EstadoCita)Convert.ToInt32(reader["Estado"]),
                                IdEspecialidad = (TipoEspecialidad)Convert.ToInt32(reader["IdEspecialidad"]),
                                Precio = 0m,
                                Paciente = reader["Paciente"]?.ToString().Trim()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
            }
            return citas;
        }

        public async Task<(bool ok, string msg)> CambiarEstadoCita(int citaId, EstadoCita estado)
        {
            try
            {
                using (var con = await dBUsuario.AbrirConexion())
                {
                    string query = @"UPDATE Citas
                                     SET Estado = @Estado
                                     WHERE Id = @Id";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", citaId);
                        cmd.Parameters.AddWithValue("@Estado", (int)estado);

                        int filas = await cmd.ExecuteNonQueryAsync();
                        return filas > 0
                            ? (true, $"Estado de cita actualizado a {estado}")
                            : (false, "La cita no existe");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al cambiar estado de cita: {ex.Message}");
            }
        }

        public async Task<(bool ok, string msg)> ConfirmarCita(int citaId)
        {
            try
            {
                return await CambiarEstadoCita(citaId, EstadoCita.Confirmada);
            }
            catch (Exception ex)
            {
                return (false, $"Error al confirmar cita: {ex.Message}");
            }
        }
    }
}
