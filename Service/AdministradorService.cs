using BCrypt.Net;
using Gestion.Data;
using Gestion.Data.Repositories;
using Gestion.Interface;
using Gestion.Model;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace GestionMedica.Services
{
    public class AdministradorService : IAdministrador
    {
        private readonly AdministradorRepository _repository;

        public AdministradorService()
        {
            _repository = new AdministradorRepository();
        }

        public async Task<bool> CrearUsuarios(Usuario usuario)
        {
            try
            {
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
                return await _repository.InsertarUsuarioAsync(usuario) > 0;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarUsuario(Usuario usuario)
        {
            try
            {
                if (!string.IsNullOrEmpty(usuario.Contrasena))
                    usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);

                return await _repository.ActualizarUsuarioAsync(usuario) > 0;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                return false;
            }
        }

        public async Task<Usuario> BuscarUsuario(int id)
        {
            try
            {
                return await _repository.ObtenerUsuarioAsync(id);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                return null;
            }
        }

        public async Task<List<Usuario>> BuscarUsuariosPorNombre(string filtroNombre)
        {
            try
            {
                return await _repository.BuscarUsuariosAsync(filtroNombre);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Exception] {ex.Message}");
                return new List<Usuario>();
            }
        }

        public async Task<bool> VerificarDisponibilidad(DateTime fechaHora, TipoEspecialidad especialidad)
        {
            try
            {
                return await _repository.VerificarDisponibilidadAsync(fechaHora, especialidad);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Usuario>> ObtenerDisponiblidadDoctor()
        {
            try
            {
                return await _repository.ObtenerDoctoresActivosAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                return new List<Usuario>();
            }
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

                string mailFrom = ConfigurationManager.AppSettings["Smtp.User"];
                string mailPassword = ConfigurationManager.AppSettings["Smtp.Password"];
                string smtpHost = ConfigurationManager.AppSettings["Smtp.Host"];
                string smtpPortValue = ConfigurationManager.AppSettings["Smtp.Port"];

                if (string.IsNullOrWhiteSpace(mailFrom) ||
                    string.IsNullOrWhiteSpace(mailPassword) ||
                    string.IsNullOrWhiteSpace(smtpHost) ||
                    !int.TryParse(smtpPortValue, out int smtpPort))
                {
                    throw new ConfigurationErrorsException(
                        "Configura el usuario, contraseña, servidor y puerto SMTP en la configuración protegida.");
                }

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
                    cliente.Timeout = 10000;
                    await cliente.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.SslOnConnect);
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
                return await _repository.ObtenerCitaCompletaAsync(citaId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ObtenerCitaCompletaPorId] {ex.Message}");
            }
            return null;
        }
        public async Task<Usuario> VerificarCredenciales(string correo, string contrasena)
        {
            try
            {
                var usuario = await _repository.ObtenerUsuarioActivoPorCorreoAsync(correo);
                return usuario != null && BCrypt.Net.BCrypt.Verify(contrasena, usuario.Contrasena)
                    ? usuario
                    : null;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                return null;
            }
        }

        public async Task<(bool ok, string msg)> DesactivarUsuario(int id)
        {
            try
            {
                int affected = await _repository.DesactivarUsuarioAsync(id);
                return affected > 0
                    ? (true, "Usuario desactivado correctamente")
                    : (false, "Usuario no encontrado o ya desactivado");
            }
            catch (Exception ex)
            {
                return (false, $"Error al desactivar usuario: {ex.Message}");
            }
        }

        public async Task<List<Citas>> ObtenerCitasMedicas()
        {
            try
            {
                return await _repository.ObtenerCitasAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                return new List<Citas>();
            }
        }

        public async Task<(bool ok, string msg)> CambiarEstadoCita(int citaId, EstadoCita estado)
        {
            try
            {
                int affected = await _repository.CambiarEstadoCitaAsync(citaId, estado);
                return affected > 0
                    ? (true, $"Estado de cita actualizado a {estado}")
                    : (false, "La cita no existe");
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
