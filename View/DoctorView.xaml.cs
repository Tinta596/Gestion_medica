using Gestion.Controller;
using Gestion.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gestion.Views
{
    public partial class DoctorView : Window
    {
        private readonly DoctorController _controller;
        private readonly Usuario _usuarioActual;
        private int _pacienteSeleccionadoId;

        private readonly List<StackPanel> _paneles = new List<StackPanel>();

        private class EspecialidadItem
        {
            public string Nombre { get; set; }
            public TipoEspecialidad Valor { get; set; }
        }

        public DoctorView(Usuario usuario)
        {
            InitializeComponent();
            _controller = new DoctorController();
            _usuarioActual = usuario;

            _paneles.Add(PanelPacientes);
            _paneles.Add(PanelHistorial);
            _paneles.Add(PanelProgramar);
            _paneles.Add(PanelActualizar);

            CargarEspecialidades();
            MostrarPanel(PanelPacientes, "Pacientes");
            Loaded += async (loadedSender, loadedArgs) => await CargarPacientesAsync();
        }

        private void CargarEspecialidades()
        {
            var items = Enum.GetValues(typeof(TipoEspecialidad))
                            .Cast<TipoEspecialidad>()
                            .Select(e => new EspecialidadItem { Nombre = e.ToString(), Valor = e })
                            .ToList();
            CbCitaEspecialidad.ItemsSource = items;
        }

        // ─────────────────────────────────────────
        // NAVEGACIÓN
        // ─────────────────────────────────────────

        private void MostrarPanel(StackPanel panel, string titulo)
        {
            foreach (var p in _paneles)
                p.Visibility = Visibility.Collapsed;

            panel.Visibility = Visibility.Visible;
            TxtPageTitle.Text = titulo;
        }

        private async void NavPacientes_Click(object sender, RoutedEventArgs e)
        {
            MostrarPanel(PanelPacientes, "Pacientes");
            await CargarPacientesAsync();
        }

        private void NavHistorial_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelHistorial, "Historial Clínico");

        private void NavProgramar_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelProgramar, "Programar Cita");

        private void NavActualizar_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelActualizar, "Actualizar Cita");

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginView().Show();
            this.Close();
        }

        // ─────────────────────────────────────────
        // DATOS DEL PACIENTE
        // ─────────────────────────────────────────

        private async void BtnBuscarPacientes_Click(object sender, RoutedEventArgs e)
            => await CargarPacientesAsync();

        private async System.Threading.Tasks.Task CargarPacientesAsync()
        {
            var dgPacientes = FindName("DgPacientes") as DataGrid;
            var txtSinPacientes = FindName("TxtSinPacientes") as TextBlock;
            var txtBuscarPacienteNombre = FindName("TxtBuscarPacienteNombre") as TextBox;

            if (dgPacientes == null || txtSinPacientes == null)
                return;

            dgPacientes.ItemsSource = null;
            txtSinPacientes.Visibility = Visibility.Collapsed;

            var (ok, msg, pacientes) = await LoadingDialogService.RunAsync(
                this,
                () => _controller.ListarPacientes(txtBuscarPacienteNombre?.Text),
                "Buscando pacientes...");
            if (!ok || pacientes == null || pacientes.Count == 0)
            {
                txtSinPacientes.Text = ok ? "No hay pacientes registrados con ese nombre." : msg;
                txtSinPacientes.Visibility = Visibility.Visible;
                return;
            }

            dgPacientes.ItemsSource = pacientes;
        }

        private async void BtnDatosPaciente_Click(object sender, RoutedEventArgs e)
        {
            PanelDatosPaciente.Visibility = Visibility.Collapsed;
            PanelCitasPaciente.Visibility = Visibility.Collapsed;
            TxtMsgCitasPaciente.Visibility = Visibility.Collapsed;

            var boton = sender as Button;
            var pacienteSeleccionado = boton?.DataContext as Usuario;
            int id = pacienteSeleccionado?.Id ?? 0;

            if (id <= 0)
            {
                MessageBox.Show("Selecciona un paciente válido.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (ok, msg, paciente) = await LoadingDialogService.RunAsync(
                this,
                () => _controller.DatosPaciente(id),
                "Cargando datos del paciente...");

            if (!ok)
            {
                MessageBox.Show(msg, "No encontrado",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TxtPacienteNombre.Text = paciente.Nombre;
            TxtPacienteCorreo.Text = paciente.Correo;
            TxtPacienteTelefono.Text = paciente.Telefono ?? "—";
            _pacienteSeleccionadoId = paciente.Id;
            PanelDatosPaciente.Visibility = Visibility.Visible;

            await CargarCitasPacienteAsync(paciente.Id);
        }

        private async System.Threading.Tasks.Task CargarCitasPacienteAsync(int pacienteId)
        {
            var dgCitasPaciente = FindName("DgCitasPaciente") as DataGrid;
            var txtSinCitasPaciente = FindName("TxtSinCitasPaciente") as TextBlock;

            if (dgCitasPaciente == null || txtSinCitasPaciente == null)
                return;

            dgCitasPaciente.ItemsSource = null;
            txtSinCitasPaciente.Visibility = Visibility.Collapsed;
            TxtMsgCitasPaciente.Visibility = Visibility.Collapsed;

            var (ok, msg, citas) = await LoadingDialogService.RunAsync(
                this,
                () => _controller.CitasPaciente(pacienteId),
                "Cargando citas del paciente...");
            if (!ok || citas == null || citas.Count == 0)
            {
                txtSinCitasPaciente.Text = ok ? "Este paciente no tiene citas asignadas." : msg;
                txtSinCitasPaciente.Visibility = Visibility.Visible;
                PanelCitasPaciente.Visibility = Visibility.Visible;
                return;
            }

            dgCitasPaciente.ItemsSource = citas;
            PanelCitasPaciente.Visibility = Visibility.Visible;
        }

        private async void BtnCompletarCitaPaciente_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var cita = boton?.DataContext as Citas;
            if (cita == null || cita.Id <= 0)
            {
                MostrarFeedback(TxtMsgCitasPaciente, "Selecciona una cita válida.", esError: true);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Marcar como completada la cita de {cita.IdEspecialidad} del {cita.FechaHora:dd/MM/yyyy HH:mm}?",
                "Completar cita",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            var (ok, msg) = await LoadingDialogService.RunAsync(
                this,
                () => _controller.CompletarCita(cita.Id),
                "Actualizando la cita...");
            MostrarFeedback(TxtMsgCitasPaciente, msg, esError: !ok);

            if (ok && _pacienteSeleccionadoId > 0)
                await CargarCitasPacienteAsync(_pacienteSeleccionadoId);
        }

        // ─────────────────────────────────────────
        // HISTORIAL CLÍNICO
        // ─────────────────────────────────────────

        private async void BtnCargarHistorial_Click(object sender, RoutedEventArgs e)
        {
            LvHistorial.ItemsSource = null;
            TxtHistorialVacio.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtHistorialPacienteId.Text.Trim(), out int id) || id <= 0)
            {
                MessageBox.Show("Ingresa un ID de paciente válido.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (ok, msg, historial) = await LoadingDialogService.RunAsync(
                this,
                () => _controller.HistorialPaciente(id),
                "Consultando el historial clínico...");

            if (!ok || historial == null || historial.Count == 0)
            {
                TxtHistorialVacio.Visibility = Visibility.Visible;
                return;
            }

            LvHistorial.ItemsSource = historial;
        }

        private async void BtnCrearHistorial_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgNuevoHist.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtNuevoHistPacienteId.Text.Trim(), out int pacienteId) || pacienteId <= 0)
            {
                MostrarFeedback(TxtMsgNuevoHist, "Ingresa un ID de paciente válido.", esError: true);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtNuevoHistDiagnostico.Text))
            {
                MostrarFeedback(TxtMsgNuevoHist, "El diagnóstico es obligatorio.", esError: true);
                return;
            }

            var fecha = DpNuevoHistFecha.SelectedDate ?? DateTime.Now;
            if (fecha > DateTime.Now)
            {
                MostrarFeedback(TxtMsgNuevoHist, "La fecha de consulta no puede ser futura.", esError: true);
                return;
            }

            var historial = new HistorialClinico
            {
                IdUsuario = pacienteId,
                FechaConsulta = fecha,
                Diagnostico = TxtNuevoHistDiagnostico.Text.Trim(),
                Tratamiento = TxtNuevoHistTratamiento.Text.Trim(),
                NotasAdicionales = TxtNuevoHistNotas.Text.Trim()
            };

            var (ok, msg) = await LoadingDialogService.RunAsync(
                this,
                () => _controller.CrearHistorialClinico(historial),
                "Guardando el historial clínico...");
            MostrarFeedback(TxtMsgNuevoHist, msg, esError: !ok);

            if (ok)
            {
                TxtNuevoHistPacienteId.Clear();
                TxtNuevoHistDiagnostico.Clear();
                TxtNuevoHistTratamiento.Clear();
                TxtNuevoHistNotas.Clear();
                DpNuevoHistFecha.SelectedDate = null;

                // Si el listado superior está mostrando este paciente, refrescamos.
                if (int.TryParse(TxtHistorialPacienteId.Text.Trim(), out int idActual) && idActual == pacienteId)
                {
                    var (okList, _, lista) = await LoadingDialogService.RunAsync(
                        this,
                        () => _controller.HistorialPaciente(pacienteId),
                        "Actualizando el historial...");
                    if (okList && lista != null)
                    {
                        LvHistorial.ItemsSource = lista;
                        TxtHistorialVacio.Visibility = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        // ─────────────────────────────────────────
        // PROGRAMAR CITA
        // ─────────────────────────────────────────

        private async void BtnProgramarCita_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgCita.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(TxtCitaUsuarioId.Text))
            {
                MostrarFeedback(TxtMsgCita, "Ingresa el ID del paciente.", esError: true);
                return;
            }

            if (!int.TryParse(TxtCitaUsuarioId.Text.Trim(), out int usuarioId) || usuarioId <= 0)
            {
                MostrarFeedback(TxtMsgCita, "ID de usuario inválido.", esError: true);
                return;
            }

            if (DpCitaFecha.SelectedDate == null)
            {
                MostrarFeedback(TxtMsgCita, "Selecciona una fecha para la cita.", esError: true);
                return;
            }

            if (DpCitaFecha.SelectedDate.Value <= DateTime.Now)
            {
                MostrarFeedback(TxtMsgCita, "La fecha debe ser futura.", esError: true);
                return;
            }

            var especialidadItem = CbCitaEspecialidad.SelectedItem as EspecialidadItem;
            if (especialidadItem == null)
            {
                MostrarFeedback(TxtMsgCita, "Selecciona una especialidad.", esError: true);
                return;
            }

            var cita = new Citas
            {
                IdUsuario = usuarioId,
                FechaHora = DpCitaFecha.SelectedDate.Value,
                IdEspecialidad = especialidadItem.Valor,
                Estado = EstadoCita.Pendiente,
                Motivo = TxtCitaMotivo.Text.Trim()
            };

            var (ok, msg) = await LoadingDialogService.RunAsync(
                this,
                () => _controller.ProgramarCita(cita),
                "Programando la cita...");
            MostrarFeedback(TxtMsgCita, msg, esError: !ok);

            if (ok)
            {
                TxtCitaUsuarioId.Clear();
                TxtCitaMotivo.Clear();
                DpCitaFecha.SelectedDate = null;
                CbCitaEspecialidad.SelectedIndex = -1;

                // Refrescar lista de citas del paciente si está visible.
                if (_pacienteSeleccionadoId == usuarioId)
                    await CargarCitasPacienteAsync(_pacienteSeleccionadoId);
            }
        }

        // ─────────────────────────────────────────
        // ACTUALIZAR CITA
        // ─────────────────────────────────────────

        private async void BtnActualizarCita_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgActualizar.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtActualizarCitaId.Text.Trim(), out int citaId) || citaId <= 0)
            {
                MostrarFeedback(TxtMsgActualizar, "ID de cita inválido.", esError: true);
                return;
            }

            if (DpActualizarFecha.SelectedDate == null)
            {
                MostrarFeedback(TxtMsgActualizar, "Selecciona la nueva fecha.", esError: true);
                return;
            }

            var cita = new Citas
            {
                Id = citaId,
                FechaHora = DpActualizarFecha.SelectedDate.Value,
                Observaciones = TxtActualizarObservaciones.Text.Trim()
            };

            var (ok, msg) = await LoadingDialogService.RunAsync(
                this,
                () => _controller.ActualizarCita(cita),
                "Actualizando la cita...");
            MostrarFeedback(TxtMsgActualizar, msg, esError: !ok);

            if (ok)
            {
                TxtActualizarCitaId.Clear();
                TxtActualizarObservaciones.Clear();
                DpActualizarFecha.SelectedDate = null;
            }
        }

        // ─────────────────────────────────────────
        // HELPER
        // ─────────────────────────────────────────

        private void MostrarFeedback(TextBlock bloque, string texto, bool esError)
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
