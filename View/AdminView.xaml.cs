using Gestion.Controller;
using Gestion.Model;
using GestionMedica.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Gestion.Views
{
    public partial class AdminView : Window
    {
        private readonly AdminController _controller;
        private readonly Usuario _usuarioActual;

        // Paneles de navegación
        private readonly List<StackPanel> _paneles = new List<StackPanel>();

        public AdminView(Usuario usuario)
        {
            InitializeComponent();
            _controller = new AdminController();
            _usuarioActual = usuario;

            TxtAdminName.Text = usuario?.Nombre ?? "Administrador";

            // Registrar paneles para navegación
            _paneles.Add(PanelUsuarios);
            _paneles.Add(PanelDoctores);
            _paneles.Add(PanelDisponibilidad);
            _paneles.Add(PanelCitas);
            _paneles.Add(PanelCorreos);

            MostrarPanel(PanelUsuarios, "Gestión de Usuarios");
        }

        // ─────────────────────────────────────────
        // NAVEGACIÓN LATERAL
        // ─────────────────────────────────────────

        private void MostrarPanel(StackPanel panel, string titulo)
        {
            foreach (var p in _paneles)
                p.Visibility = Visibility.Collapsed;

            panel.Visibility = Visibility.Visible;
            TxtPageTitle.Text = titulo;
        }

        private void NavUsuarios_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelUsuarios, "Gestión de Usuarios");

        private async void NavDoctores_Click(object sender, RoutedEventArgs e)
        {
            MostrarPanel(PanelDoctores, "Informe de Doctores");
            await CargarInformeDoctoresAsync();
        }

        private void NavDisponibilidad_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelDisponibilidad, "Verificar Disponibilidad");

        private async void NavCitas_Click(object sender, RoutedEventArgs e)
        {
            MostrarPanel(PanelCitas, "Gestión de Citas");
            await CargarCitasAdminAsync();
        }

        private void NavCorreos_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelCorreos, "Envío de Correos");

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginView();
            login.Show();
            this.Close();
        }

        // ─────────────────────────────────────────
        // BUSCAR USUARIO
        // ─────────────────────────────────────────

        private async void BtnBuscarUsuario_Click(object sender, RoutedEventArgs e)
        {
            await BuscarUsuariosAdminAsync();
        }

        private async System.Threading.Tasks.Task BuscarUsuariosAdminAsync()
        {
            var dgUsuariosAdmin = FindName("DgUsuariosAdmin") as DataGrid;
            var txtSinUsuariosAdmin = FindName("TxtSinUsuariosAdmin") as TextBlock;

            PanelResultadoUsuario.Visibility = Visibility.Collapsed;
            if (dgUsuariosAdmin == null || txtSinUsuariosAdmin == null)
                return;

            dgUsuariosAdmin.ItemsSource = null;
            txtSinUsuariosAdmin.Visibility = Visibility.Collapsed;

            var (ok, msg, usuarios) = await _controller.BuscarUsuariosPorNombre(TxtBuscarId.Text);
            if (!ok || usuarios == null || usuarios.Count == 0)
            {
                txtSinUsuariosAdmin.Text = ok ? "No se encontraron doctores o pacientes con ese nombre." : msg;
                txtSinUsuariosAdmin.Visibility = Visibility.Visible;
                return;
            }

            dgUsuariosAdmin.ItemsSource = usuarios;
        }

        private void BtnVerUsuarioAdmin_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var usuario = boton?.DataContext as Usuario;
            if (usuario == null)
                return;

            TxtResultadoUsuario.Text =
                $"ID: {usuario.Id}\n" +
                $"Nombre: {usuario.Nombre} {usuario.Apellido}\n" +
                $"Correo: {usuario.Correo}\n" +
                $"Teléfono: {usuario.Telefono}\n" +
                $"Rol: {usuario.RolTexto}\n" +
                $"Activo: {(usuario.Activo ? "Sí" : "No")}";
            TxtResultadoUsuario.Foreground =
                new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F0F7"));
            PanelResultadoUsuario.Visibility = Visibility.Visible;
        }

        // ─────────────────────────────────────────
        // CREAR USUARIO
        // ─────────────────────────────────────────

        private async void BtnCrearUsuario_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgCrear.Visibility = Visibility.Collapsed;

            string nombre = TxtNuevoNombre.Text.Trim();
            string correo = TxtNuevoCorreo.Text.Trim();
            string password = TxtNuevoPassword.Password;
            string telefono = TxtNuevoTelefono.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(password))
            {
                MostrarMensajeFeedback(TxtMsgCrear, "Nombre, correo y contraseña son obligatorios.", esError: true);
                return;
            }

            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Contrasena = password,
                Telefono = telefono
            };

            var (ok, msg) = await _controller.CrearUsuario(nuevoUsuario);

            MostrarMensajeFeedback(TxtMsgCrear, msg, esError: !ok);

            if (ok)
            {
                TxtNuevoNombre.Clear();
                TxtNuevoCorreo.Clear();
                TxtNuevoPassword.Clear();
                TxtNuevoTelefono.Clear();
            }
        }

        // ─────────────────────────────────────────
        // DESACTIVAR USUARIO
        // ─────────────────────────────────────────

        private async void BtnDesactivarUsuario_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgDesactivar.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtDesactivarId.Text.Trim(), out int id) || id <= 0)
            {
                MostrarMensajeFeedback(TxtMsgDesactivar, "Ingresa un ID válido.", esError: true);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Seguro que deseas desactivar al usuario con ID {id}?",
                "Confirmar desactivación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            var (ok, msg) = await _controller.DesactivarUsuario(id);
            MostrarMensajeFeedback(TxtMsgDesactivar, msg, esError: !ok);

            if (ok) TxtDesactivarId.Clear();
        }

        // ─────────────────────────────────────────
        // DOCTORES
        // ─────────────────────────────────────────

        private async void BtnActualizarDoctores_Click(object sender, RoutedEventArgs e)
            => await CargarInformeDoctoresAsync();

        private async System.Threading.Tasks.Task CargarInformeDoctoresAsync()
        {
            DgDoctores.ItemsSource = null;
            TxtSinDoctores.Visibility = Visibility.Collapsed;
            TxtTotalDoctores.Text = "0";

            var (ok, msg, doctores) = await _controller.ObtenerDoctores();
            if (!ok || doctores == null || doctores.Count == 0)
            {
                TxtSinDoctores.Text = ok ? "No hay doctores activos registrados." : msg;
                TxtSinDoctores.Visibility = Visibility.Visible;
                return;
            }

            DgDoctores.ItemsSource = doctores;
            TxtTotalDoctores.Text = doctores.Count.ToString();
        }

        private async void BtnCrearDoctor_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgDoctor.Visibility = Visibility.Collapsed;

            string nombre = TxtDoctorNombre.Text.Trim();
            string apellido = TxtDoctorApellido.Text.Trim();
            string correo = TxtDoctorCorreo.Text.Trim();
            string password = TxtDoctorPassword.Password;
            string telefono = TxtDoctorTelefono.Text.Trim();
            string direccion = TxtDoctorDireccion.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(password))
            {
                MostrarMensajeFeedback(TxtMsgDoctor, "Nombre, correo y contraseña son obligatorios.", esError: true);
                return;
            }

            var doctor = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Correo = correo,
                Contrasena = password,
                Telefono = telefono,
                Direccion = direccion,
                Activo = true,
                IdRol = (int)TipoRol.Doctor
            };

            var (ok, msg) = await _controller.CrearDoctor(doctor);
            MostrarMensajeFeedback(TxtMsgDoctor, msg, esError: !ok);

            if (!ok) return;

            TxtDoctorNombre.Clear();
            TxtDoctorApellido.Clear();
            TxtDoctorCorreo.Clear();
            TxtDoctorPassword.Clear();
            TxtDoctorTelefono.Clear();
            TxtDoctorDireccion.Clear();
            await CargarInformeDoctoresAsync();
        }

        // ─────────────────────────────────────────
        // VERIFICAR DISPONIBILIDAD
        // ─────────────────────────────────────────

        private async void BtnVerificarDisp_Click(object sender, RoutedEventArgs e)
        {
            PanelResultadoDisp.Visibility = Visibility.Collapsed;

            if (DpFecha.SelectedDate == null)
            {
                TxtResultadoDisp.Text = "Selecciona una fecha válida.";
                TxtResultadoDisp.Foreground =
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF5350"));
                PanelResultadoDisp.Visibility = Visibility.Visible;
                return;
            }

            ComboBoxItem itemSeleccionado = CbEspecialidad.SelectedItem as ComboBoxItem;
            if (itemSeleccionado == null)
            {
                MessageBox.Show("Selecciona una especialidad.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cbHoraDisp = FindName("CbHoraDisp") as ComboBox;
            var cbMinutosDisp = FindName("CbMinutosDisp") as ComboBox;
            ComboBoxItem horaSeleccionada = cbHoraDisp?.SelectedItem as ComboBoxItem;
            ComboBoxItem minutoSeleccionado = cbMinutosDisp?.SelectedItem as ComboBoxItem;
            if (horaSeleccionada == null || minutoSeleccionado == null)
            {
                TxtResultadoDisp.Text = "Selecciona la hora completa.";
                TxtResultadoDisp.Foreground =
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF5350"));
                PanelResultadoDisp.Visibility = Visibility.Visible;
                return;
            }

            TipoEspecialidad especialidad;
            var especialidadTexto = itemSeleccionado.Content.ToString();
            if (especialidadTexto == "Cardiología")
                especialidad = TipoEspecialidad.Cardiologia;
            else if (especialidadTexto == "Dermatología")
                especialidad = TipoEspecialidad.Dermatologia;
            else if (especialidadTexto == "Pediatría")
                especialidad = TipoEspecialidad.Pediatria;
            else if (especialidadTexto == "Neurología")
                especialidad = TipoEspecialidad.Neurologia;
            else if (especialidadTexto == "Ortopedia")
                especialidad = TipoEspecialidad.Ortopedia;
            else if (especialidadTexto == "Oftalmología")
                especialidad = TipoEspecialidad.Oftalmologia;
            else
                especialidad = TipoEspecialidad.General;

            int hora = int.Parse(horaSeleccionada.Content.ToString());
            int minutos = int.Parse(minutoSeleccionado.Content.ToString());
            DateTime fecha = DpFecha.SelectedDate.Value.Date.AddHours(hora).AddMinutes(minutos);
            var (ok, msg, disponible) = await _controller.VerificarDisponibilidad(fecha, especialidad);

            TxtResultadoDisp.Text = msg;
            TxtResultadoDisp.Foreground = new System.Windows.Media.SolidColorBrush(
                disponible
                    ? (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#66BB6A")
                    : (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF5350"));

            PanelResultadoDisp.Visibility = Visibility.Visible;
        }

        private async void BtnRefrescarCitas_Click(object sender, RoutedEventArgs e)
            => await CargarCitasAdminAsync();

        private async System.Threading.Tasks.Task CargarCitasAdminAsync()
        {
            var dgCitas = FindName("DgCitas") as DataGrid;
            var txtSinCitasAdmin = FindName("TxtSinCitasAdmin") as TextBlock;

            if (dgCitas == null || txtSinCitasAdmin == null)
                return;

            dgCitas.ItemsSource = null;
            txtSinCitasAdmin.Visibility = Visibility.Collapsed;
            TxtMsgConfirmarCita.Visibility = Visibility.Collapsed;

            var (ok, msg, citas) = await _controller.ObtenerCitasMedicas();
            if (!ok || citas == null || citas.Count == 0)
            {
                txtSinCitasAdmin.Text = ok ? "No hay citas médicas pedidas." : msg;
                txtSinCitasAdmin.Visibility = Visibility.Visible;
                return;
            }

            dgCitas.ItemsSource = citas;
        }

        private async void BtnGuardarEstadoCita_Click(object sender, RoutedEventArgs e)
        {
            var dgCitas = FindName("DgCitas") as DataGrid;
            dgCitas?.CommitEdit(DataGridEditingUnit.Cell, true);
            dgCitas?.CommitEdit(DataGridEditingUnit.Row, true);

            var boton = sender as Button;
            var cita = boton?.DataContext as Citas;
            if (cita == null || cita.Id <= 0)
            {
                MostrarMensajeFeedback(TxtMsgConfirmarCita, "Selecciona una cita válida.", esError: true);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Cambiar la cita #{cita.Id} a {cita.Estado}?",
                "Cambiar estado de cita",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            var (ok, msg) = await _controller.CambiarEstadoCita(cita.Id, cita.Estado);
            MostrarMensajeFeedback(TxtMsgConfirmarCita, msg, esError: !ok);
        }

        // ─────────────────────────────────────────
        // ENVIAR CORREO
        // ─────────────────────────────────────────

        private async void BtnEnviarCorreo_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgCorreo.Visibility = Visibility.Collapsed;

            string correo = TxtCorreoDestino.Text.Trim();

            if (!int.TryParse(TxtIdCitaCorreo.Text.Trim(), out int citaId) || citaId <= 0)
            {
                MostrarMensajeFeedback(TxtMsgCorreo, "Ingresa un ID de cita válido.", esError: true);
                return;
            }

            // Construir objeto Citas mínimo con el ID
            var cita = new Citas { Id = citaId };

            var (ok, msg) = await _controller.EnviarCorreo(correo, cita);
            MostrarMensajeFeedback(TxtMsgCorreo, msg, esError: !ok);

            if (ok)
            {
                TxtCorreoDestino.Clear();
                TxtIdCitaCorreo.Clear();
            }
        }

        // ─────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────

        private void MostrarMensaje(TextBlock bloque, string texto, bool esError)
        {
            bloque.Text = texto;
            bloque.Foreground = new System.Windows.Media.SolidColorBrush(
                esError
                    ? (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF5350")
                    : (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F0F7"));
        }

        private void MostrarMensajeFeedback(TextBlock bloque, string texto, bool esError)
        {
            bloque.Text = texto;
            bloque.Foreground = new System.Windows.Media.SolidColorBrush(
                esError
                    ? (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF5350")
                    : (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#66BB6A"));
            bloque.Visibility = Visibility.Visible;
        }
    }
}
