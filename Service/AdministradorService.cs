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
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
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
                    Debug.WriteLine($"[Exception] {ex.Message}");
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

                // ── Si el objeto llegó incompleto, recargarlo desde BD usando su Id ──
                if (cita.FechaHora == DateTime.MinValue && cita.Id > 0)
                {
                    cita = await ObtenerCitaCompletaPorId(cita.Id, correo) ?? cita;
                }

                if (cita.FechaHora == DateTime.MinValue)
                {
                    MessageBox.Show("No se pudo obtener la fecha de la cita. Intente de nuevo.");
                    return false;
                }

                string mailFrom    = "juandavidbarriosperneth@gmail.com";
                string mailPassword = "qoozcmuiahwkgtxg";

                string especialidadTexto;
                switch (cita.IdEspecialidad)
                {
                    case TipoEspecialidad.Cardiologia: especialidadTexto = "Cardiología"; break;
                    case TipoEspecialidad.Dermatologia: especialidadTexto = "Dermatología"; break;
                    case TipoEspecialidad.Oftalmologia: especialidadTexto = "Oftalmología"; break;
                    case TipoEspecialidad.Ortopedia: especialidadTexto = "Ortopedia"; break;
                    case TipoEspecialidad.Pediatria: especialidadTexto = "Pediatría"; break;
                    case TipoEspecialidad.Neurologia: especialidadTexto = "Neurología"; break;
                    case TipoEspecialidad.General: especialidadTexto = "Medicina General"; break;
                    default: especialidadTexto = cita.IdEspecialidad.ToString(); break;
                }

                string estadoTexto;
                switch (cita.Estado)
                {
                    case EstadoCita.Pendiente: estadoTexto = "Pendiente"; break;
                    case EstadoCita.Confirmada: estadoTexto = "Confirmada"; break;
                    case EstadoCita.Cancelada: estadoTexto = "Cancelada"; break;
                    case EstadoCita.Completada: estadoTexto = "Completada"; break;
                    default: estadoTexto = cita.Estado.ToString(); break;
                }

                string pacienteNombre = !string.IsNullOrWhiteSpace(cita.Paciente)
                    ? cita.Paciente
                    : "N/A";

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress("Sistema Médico", mailFrom));
                mensaje.To.Add(MailboxAddress.Parse(correo));
                mensaje.Subject = "Confirmación de Cita Médica";

                mensaje.Body = new TextPart("html")
                {
                    Text = $@"<!DOCTYPE html>
                                <html>
                                <body style='font-family:Segoe UI,Arial,sans-serif; color:#1A2633; max-width:520px; margin:auto;'>
                                <div style='background:#1565C0; padding:24px 32px; border-radius:8px 8px 0 0;'>
                                    <h2 style='color:white; margin:0;'> Confirmación de Cita Médica</h2>
                                </div>
                                <div style='background:#F4F8FB; padding:24px 32px; border-radius:0 0 8px 8px; border:1px solid #D0E3F0;'>
                                    <p style='font-size:15px;'>Su cita ha sido agendada correctamente.</p>
                                    <table style='width:100%; border-collapse:collapse; margin-top:12px;'>
                                    <tr><td style='padding:8px 0; color:#555; width:130px;'><b>Paciente:</b></td><td style='padding:8px 0;'>{pacienteNombre}</td></tr>
                                    <tr style='background:#E8F4FD;'><td style='padding:8px 6px; color:#555;'><b>Fecha:</b></td><td style='padding:8px 6px;'>{cita.FechaHora:dd/MM/yyyy}</td></tr>
                                    <tr><td style='padding:8px 0; color:#555;'><b>Hora:</b></td><td style='padding:8px 0;'>{cita.FechaHora:HH:mm}</td></tr>
                                    <tr style='background:#E8F4FD;'><td style='padding:8px 6px; color:#555;'><b>Especialidad:</b></td><td style='padding:8px 6px;'>{especialidadTexto}</td></tr>
                                    <tr><td style='padding:8px 0; color:#555;'><b>Estado:</b></td><td style='padding:8px 0;'><span style='background:#1565C0;color:white;padding:2px 10px;border-radius:10px;font-size:13px;'>{estadoTexto}</span></td></tr>
                                    </table>
                                    <hr style='border:none;border-top:1px solid #D0E3F0; margin:20px 0;'/>
                                    <p style='color:#666; font-size:13px;'>Gracias por usar nuestro sistema de gestión médica.</p>
                                </div>
                                </body>
                                </html>"
                };

                using (var cliente = new MailKit.Net.Smtp.SmtpClient())
                {
                    cliente.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    cliente.Timeout = 10000;
                    await cliente.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                    await cliente.AuthenticateAsync(mailFrom, mailPassword);
                    await cliente.SendAsync(mensaje);
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
                MessageBox.Show(ex.Message, "ERROR SMTP", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private async Task<Citas> ObtenerCitaCompletaPorId(int citaId, string correoDestino)
        {
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
                                            CONCAT(u.Nombre, ' ', COALESCE(u.Apellido,'')) AS Paciente
                                     FROM Citas c
                                     INNER JOIN Usuario u ON c.IdUsuario = u.Id
                                     WHERE c.Id = @Id";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", citaId);

                        using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Citas
                                {
                                    Id             = Convert.ToInt32(reader["Id"]),
                                    IdUsuario      = Convert.ToInt32(reader["IdUsuario"]),
                                    FechaHora      = Convert.ToDateTime(reader["FechaHora"]),
                                    Estado         = (EstadoCita)Convert.ToInt32(reader["Estado"]),
                                    IdEspecialidad = (TipoEspecialidad)Convert.ToInt32(reader["IdEspecialidad"]),
                                    Precio         = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0m,
                                    Paciente       = reader["Paciente"]?.ToString().Trim()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ObtenerCitaCompletaPorId] {ex.Message}");
            }
            return null;
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
