using Gestion.Controller;
using Gestion.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gestion.Views
{
    public partial class UsuarioView : Window
    {
        private readonly UsuarioController _controller;
        private readonly AdminController _adminController;
        private readonly FacturaController _facturaController;
        private readonly Usuario _usuarioActual;
        private readonly List<StackPanel> _paneles = new List<StackPanel>();

        // ── Tarifas por especialidad ──────────────────────────────────────────
        // Clave = texto del ComboBoxItem en el XAML (debe coincidir exactamente)
        private static readonly Dictionary<string, List<(string Etiqueta, decimal Valor)>> _tarifas =
            new Dictionary<string, List<(string, decimal)>>
            {
                ["Cardiología"] = new List<(string, decimal)> { ("Consulta básica — $80.000",   80000m),
                                            ("Consulta + ECG — $130.000",  130000m),
                                            ("Consulta + Eco — $200.000",  200000m) },
                ["Dermatología"] = new List<(string, decimal)> { ("Consulta básica — $65.000",   65000m),
                                            ("Consulta + biopsia — $150.000", 150000m) },
                ["Pediatría"] = new List<(string, decimal)> { ("Consulta básica — $55.000",   55000m),
                                            ("Consulta + vacuna — $90.000",  90000m) },
                ["Neurología"] = new List<(string, decimal)> { ("Consulta básica — $90.000",   90000m),
                                            ("Consulta + EEG — $180.000",  180000m) },
                ["Ginecología"] = new List<(string, decimal)> { ("Consulta básica — $70.000",   70000m),
                                            ("Consulta + ecografía — $140.000", 140000m) },
                ["Ortopedia"] = new List<(string, decimal)> { ("Consulta básica — $75.000",   75000m),
                                            ("Consulta + Rx — $120.000",   120000m) },
                ["Medicina General"] = new List<(string, decimal)> { ("Consulta básica — $45.000",   45000m),
                                            ("Consulta prioritaria — $65.000", 65000m) },
            };

        // ─────────────────────────────────────────────────────────────────────
        // CONSTRUCTOR
        // ─────────────────────────────────────────────────────────────────────

        public UsuarioView(Usuario usuario)
        {
            InitializeComponent();
            _controller = new UsuarioController();
            _adminController = new AdminController();
            _facturaController = new FacturaController();
            _usuarioActual = usuario;

            _paneles.Add(PanelMisCitas);
            _paneles.Add(PanelBuscarCita);
            _paneles.Add(PanelEstado);
            _paneles.Add(PanelFacturas);
            _paneles.Add(PanelEstadisticas);
            _paneles.Add(PanelCancelar);

            MostrarPanel(PanelMisCitas, "Mis Citas");

            // Guardar ID en el TextBox oculto (compatibilidad)
            if (usuario != null)
                TxtMiUsuarioId.Text = usuario.Id.ToString();

            // ► Cargar citas automáticamente al abrir la ventana
            Loaded += async (loadedSender, loadedArgs) => await CargarMisCitasAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // NAVEGACIÓN
        // ─────────────────────────────────────────────────────────────────────

        private void MostrarPanel(StackPanel panel, string titulo)
        {
            foreach (var p in _paneles)
                p.Visibility = Visibility.Collapsed;

            panel.Visibility = Visibility.Visible;
            TxtPageTitle.Text = titulo;
        }

        private async void NavMisCitas_Click(object sender, RoutedEventArgs e)
        {
            MostrarPanel(PanelMisCitas, "Mis Citas Asignadas");
            await CargarMisCitasAsync();
        }

        private void NavBuscarCita_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelBuscarCita, "Buscar Cita");

        private void NavEstado_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelEstado, "Estado de Cita");

        private async void NavFacturas_Click(object sender, RoutedEventArgs e)
        {
            MostrarPanel(PanelFacturas, "Mis Facturas");
            await CargarMisFacturasAsync();
        }

        private async void NavEstadisticas_Click(object sender, RoutedEventArgs e)
        {
            MostrarPanel(PanelEstadisticas, "Estadísticas");
            await CargarEstadisticasAsync();
        }

        private void NavCancelar_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelCancelar, "Cancelar Cita");

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginView().Show();
            this.Close();
        }

        // ─────────────────────────────────────────────────────────────────────
        // MIS CITAS  (carga automática + botón "Actualizar lista")
        // ─────────────────────────────────────────────────────────────────────

        private async void BtnMisCitas_Click(object sender, RoutedEventArgs e)
            => await CargarMisCitasAsync();

        /// <summary>
        /// Carga las citas del usuario actual directamente desde _usuarioActual.Id.
        /// Se llama al abrir la ventana y también desde el botón "Actualizar lista".
        /// </summary>
        private async System.Threading.Tasks.Task CargarMisCitasAsync()
        {
            LvMisCitas.ItemsSource = null;
            TxtSinCitas.Visibility = Visibility.Collapsed;
            PanelDetalleCita.Visibility = Visibility.Collapsed;

            if (_usuarioActual == null || _usuarioActual.Id <= 0)
            {
                TxtSinCitas.Visibility = Visibility.Visible;
                TxtSinCitas.Text = "No hay sesión de usuario activa.";
                return;
            }

            var (ok, _, citas) = await _controller.MisCitas(_usuarioActual.Id);

            if (!ok || citas == null || citas.Count == 0)
            {
                TxtSinCitas.Visibility = Visibility.Visible;
                TxtSinCitas.Text = "No tienes citas programadas.";
                return;
            }

            LvMisCitas.ItemsSource = citas;
        }

        private void LvMisCitas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cita = LvMisCitas.SelectedItem as Citas;
            if (cita == null)
            {
                PanelDetalleCita.Visibility = Visibility.Collapsed;
                return;
            }

            TxtDetalleCitaId.Text = cita.Id.ToString();
            TxtDetalleCitaFecha.Text = cita.FechaHora.ToString("dd/MM/yyyy HH:mm");
            TxtDetalleCitaEspecialidad.Text = cita.IdEspecialidad.ToString();
            TxtDetalleCitaEstado.Text = cita.EstadoTexto;
            TxtDetalleCitaPrecio.Text = cita.Precio.ToString("C0");
            PanelDetalleCita.Visibility = Visibility.Visible;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ESPECIALIDAD → cargar precios en el ComboBox
        // ─────────────────────────────────────────────────────────────────────

        private void CmbEspecialidad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CmbPrecioCita.Items.Clear();
            CmbPrecioCita.IsEnabled = false;
            TxtNotaPrecio.Text = "Selecciona primero una especialidad para ver las tarifas.";

            var item = CmbEspecialidad.SelectedItem as ComboBoxItem;
            if (item == null) return;

            string especialidad = item.Content.ToString();

            if (!_tarifas.TryGetValue(especialidad, out var tarifas)) return;

            foreach (var (etiqueta, _) in tarifas)
                CmbPrecioCita.Items.Add(new ComboBoxItem { Content = etiqueta });

            CmbPrecioCita.IsEnabled = true;
            CmbPrecioCita.SelectedIndex = 0;       // preseleccionar la primera opción
            TxtNotaPrecio.Text = $"{tarifas.Count} tarifa(s) disponibles para {especialidad}.";
        }

        /// <summary>Devuelve el valor decimal del ítem seleccionado en CmbPrecioCita.</summary>
        private decimal ObtenerPrecioSeleccionado()
        {
            var espItem = CmbEspecialidad.SelectedItem as ComboBoxItem;
            if (espItem == null) return 0;

            var precioItem = CmbPrecioCita.SelectedItem as ComboBoxItem;
            if (precioItem == null) return 0;

            string especialidad = espItem.Content.ToString();
            string etiqueta = precioItem.Content.ToString();

            if (!_tarifas.TryGetValue(especialidad, out var tarifas)) return 0;

            foreach (var (et, valor) in tarifas)
                if (et == etiqueta) return valor;

            return 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // BUSCAR Y AGENDAR CITA
        // ─────────────────────────────────────────────────────────────────────

        private async void BtnBuscarCita_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgBuscarCita.Visibility = Visibility.Collapsed;

            // — Validar especialidad
            if (CmbEspecialidad.SelectedItem == null)
            {
                MostrarFeedback(TxtMsgBuscarCita, "Selecciona una especialidad.", esError: true);
                return;
            }

            // — Validar precio seleccionado
            decimal precio = ObtenerPrecioSeleccionado();
            if (precio <= 0)
            {
                MostrarFeedback(TxtMsgBuscarCita, "Selecciona un precio de consulta.", esError: true);
                return;
            }

            // — Validar fecha
            if (DpFechaCita.SelectedDate == null)
            {
                MostrarFeedback(TxtMsgBuscarCita, "Selecciona una fecha.", esError: true);
                return;
            }

            // — Validar hora y minutos
            if (CmbHora.SelectedItem == null || CmbMinutos.SelectedItem == null)
            {
                MostrarFeedback(TxtMsgBuscarCita, "Selecciona la hora.", esError: true);
                return;
            }

            // — Construir DateTime completo
            string horaTexto = ((ComboBoxItem)CmbHora.SelectedItem).Content.ToString();
            string minutosTexto = ((ComboBoxItem)CmbMinutos.SelectedItem).Content.ToString();
            int hora = int.Parse(horaTexto.Split(':')[0]);
            int minutos = int.Parse(minutosTexto);

            DateTime fecha = DpFechaCita.SelectedDate.Value.Date
                                .AddHours(hora).AddMinutes(minutos);

            if (fecha <= DateTime.Now)
            {
                MostrarFeedback(TxtMsgBuscarCita, "La fecha y hora deben ser futuras.", esError: true);
                return;
            }

            // — Verificar disponibilidad
            var especialidad = (TipoEspecialidad)CmbEspecialidad.SelectedIndex;
            var (_, _, disponible) = await _adminController.VerificarDisponibilidad(fecha, especialidad);

            if (!disponible)
            {
                MostrarFeedback(TxtMsgBuscarCita,
                    "El horario no está disponible. Elige otra fecha u hora.", esError: true);
                return;
            }

            // — Crear y guardar la cita
            var nuevaCita = new Citas
            {
                IdUsuario = _usuarioActual.Id,
                FechaHora = fecha,
                IdEspecialidad = especialidad,
                Estado = EstadoCita.Pendiente,
                Precio = precio
            };

            var doctorCtrl = new DoctorController();
            var (okCita, msgCita) = await doctorCtrl.ProgramarCita(nuevaCita);

            if (!okCita)
            {
                MostrarFeedback(TxtMsgBuscarCita, $"No se pudo agendar: {msgCita}", esError: true);
                return;
            }

            var factura = new Factura
            {
                IdUsuario = _usuarioActual.Id,
                IdCita = nuevaCita.Id,
                Items = new List<FacturaItem>
                {
                    new FacturaItem
                    {
                        Descripcion = "Consulta de " + especialidad,
                        Precio = precio,
                        Cantidad = 1
                    }
                }
            };

            int facturaId = await _facturaController.CrearFactura(factura);

            MostrarFeedback(TxtMsgBuscarCita,
                $"✔ Cita agendada para el {fecha:dd/MM/yyyy} a las {fecha:HH:mm}. " +
                (facturaId > 0
                    ? $"Factura #{facturaId} generada. Precio: ${precio:N0}"
                    : $"Precio: ${precio:N0}. No se pudo generar la factura automáticamente."),
                esError: false);

            // — Limpiar formulario
            DpFechaCita.SelectedDate = null;
            CmbEspecialidad.SelectedIndex = -1;
            CmbPrecioCita.Items.Clear();
            CmbPrecioCita.IsEnabled = false;
            CmbHora.SelectedIndex = -1;
            CmbMinutos.SelectedIndex = -1;
            TxtNotaPrecio.Text = "Selecciona primero una especialidad para ver las tarifas.";

            // — Refrescar lista de citas en background
            await CargarMisCitasAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // MIS FACTURAS
        // ─────────────────────────────────────────────────────────────────────

        private async void BtnMisFacturas_Click(object sender, RoutedEventArgs e)
            => await CargarMisFacturasAsync();

        private async System.Threading.Tasks.Task CargarMisFacturasAsync()
        {
            LvFacturas.ItemsSource = null;
            TxtSinFacturas.Visibility = Visibility.Collapsed;
            TxtMsgFactura.Visibility = Visibility.Collapsed;
            BtnGenerarPdf.IsEnabled = false;

            if (_usuarioActual == null || _usuarioActual.Id <= 0)
            {
                TxtSinFacturas.Text = "No hay sesión de usuario activa.";
                TxtSinFacturas.Visibility = Visibility.Visible;
                return;
            }

            var facturas = await _facturaController.ObtenerFacturasUsuario(_usuarioActual.Id);

            if (facturas == null || facturas.Count == 0)
            {
                TxtSinFacturas.Text = "No tienes facturas registradas.";
                TxtSinFacturas.Visibility = Visibility.Visible;
                return;
            }

            LvFacturas.ItemsSource = facturas;
            BtnGenerarPdf.IsEnabled = true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GENERAR PDF
        // ─────────────────────────────────────────────────────────────────────

        private async void BtnGenerarPdf_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgFactura.Visibility = Visibility.Collapsed;

            if (LvFacturas.SelectedItem == null)
            {
                MostrarFeedback(TxtMsgFactura, "Selecciona una factura de la lista primero.", esError: true);
                return;
            }

            var facturaSeleccionada = (Factura)LvFacturas.SelectedItem;

            var dialogo = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Factura_{facturaSeleccionada.Id}",
                DefaultExt = ".pdf",
                Filter = "PDF documents (.pdf)|*.pdf"
            };

            if (dialogo.ShowDialog() != true) return;

            bool ok = await _facturaController.GenerarPdf(facturaSeleccionada.Id, dialogo.FileName);

            if (!ok)
            {
                MostrarFeedback(TxtMsgFactura, "❌ Error al generar el PDF.", esError: true);
                return;
            }

            MostrarFeedback(TxtMsgFactura, $"✔ PDF guardado en: {dialogo.FileName}", esError: false);

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dialogo.FileName,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MostrarFeedback(TxtMsgFactura, $"PDF guardado, pero no se pudo abrir automáticamente: {ex.Message}", esError: true);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ESTADO DE CITA
        // ─────────────────────────────────────────────────────────────────────

        private async void BtnEstadoCita_Click(object sender, RoutedEventArgs e)
        {
            PanelResultadoEstado.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtEstadoCitaId.Text.Trim(), out int citaId) || citaId <= 0)
            {
                MessageBox.Show("Ingresa un ID de cita válido.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (ok, msg, estado) = await _controller.ConsultarEstadoCita(citaId);

            if (!ok)
            {
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TxtEstadoValor.Text = estado.ToString();
            PanelResultadoEstado.Visibility = Visibility.Visible;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ESTADÍSTICAS
        // ─────────────────────────────────────────────────────────────────────

        private async void BtnPromedioCitas_Click(object sender, RoutedEventArgs e)
            => await CargarEstadisticasAsync();

        private async System.Threading.Tasks.Task CargarEstadisticasAsync()
        {
            PanelPromedio.Visibility = Visibility.Collapsed;
            PanelEstadosCitas.Visibility = Visibility.Collapsed;

            var (ok, msg, promedio) = await _controller.PromedioCitas();

            if (!ok)
            {
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TxtPromedioValor.Text = promedio.ToString();
            PanelPromedio.Visibility = Visibility.Visible;

            if (_usuarioActual == null || _usuarioActual.Id <= 0) return;

            var (okEstados, msgEstados, resumen) = await _controller.ResumenEstados(_usuarioActual.Id);
            if (!okEstados || resumen == null)
            {
                MessageBox.Show(msgEstados, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int pendientes = resumen.ContainsKey(EstadoCita.Pendiente) ? resumen[EstadoCita.Pendiente] : 0;
            int confirmadas = resumen.ContainsKey(EstadoCita.Confirmada) ? resumen[EstadoCita.Confirmada] : 0;
            int canceladas = resumen.ContainsKey(EstadoCita.Cancelada) ? resumen[EstadoCita.Cancelada] : 0;
            int completadas = resumen.ContainsKey(EstadoCita.Completada) ? resumen[EstadoCita.Completada] : 0;
            int total = pendientes + confirmadas + canceladas + completadas;
            double porcentajeCanceladas = total > 0 ? (canceladas * 100.0 / total) : 0;

            TxtPendientesValor.Text = pendientes.ToString();
            TxtConfirmadasValor.Text = confirmadas.ToString();
            TxtCanceladasValor.Text = canceladas.ToString();
            TxtCompletadasValor.Text = completadas.ToString();
            TxtCanceladasResumen.Text = string.Format("{0} de {1} citas canceladas ({2:0}%)", canceladas, total, porcentajeCanceladas);
            BarraCanceladas.Value = porcentajeCanceladas;

            LvCitasCanceladas.ItemsSource = null;
            TxtSinCanceladas.Visibility = Visibility.Collapsed;

            var (okCitas, msgCitas, citas) = await _controller.MisCitas(_usuarioActual.Id);
            if (!okCitas || citas == null)
            {
                TxtSinCanceladas.Text = msgCitas;
                TxtSinCanceladas.Visibility = Visibility.Visible;
            }
            else
            {
                var citasCanceladas = new List<Citas>();
                foreach (var cita in citas)
                {
                    if (cita.Estado == EstadoCita.Cancelada)
                        citasCanceladas.Add(cita);
                }

                if (citasCanceladas.Count == 0)
                {
                    TxtSinCanceladas.Text = "No tienes citas canceladas.";
                    TxtSinCanceladas.Visibility = Visibility.Visible;
                }
                else
                {
                    LvCitasCanceladas.ItemsSource = citasCanceladas;
                }
            }

            PanelEstadosCitas.Visibility = Visibility.Visible;
        }

        // ─────────────────────────────────────────────────────────────────────
        // CANCELAR CITA
        // ─────────────────────────────────────────────────────────────────────

        private async void BtnCancelarCita_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgCancelar.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtCancelarCitaId.Text.Trim(), out int citaId) || citaId <= 0)
            {
                MostrarFeedback(TxtMsgCancelar, "ID de cita inválido.", esError: true);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Seguro que deseas cancelar la cita #{citaId}?",
                "Confirmar cancelación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            var (ok, msg) = await _controller.CancelarCita(citaId);
            MostrarFeedback(TxtMsgCancelar, msg, esError: !ok);

            if (ok)
            {
                TxtCancelarCitaId.Clear();
                // Refrescar lista de citas automáticamente
                await CargarMisCitasAsync();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPER
        // ─────────────────────────────────────────────────────────────────────

        private void MostrarFeedback(TextBlock bloque, string texto, bool esError)
        {
            bloque.Text = texto;
            bloque.Foreground = new SolidColorBrush(
                esError
                    ? (Color)ColorConverter.ConvertFromString("#EF5350")
                    : (Color)ColorConverter.ConvertFromString("#66BB6A"));
            bloque.Visibility = Visibility.Visible;
        }
    }
}
