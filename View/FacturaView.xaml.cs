using Gestion.Controller;
using Gestion.Model;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Gestion.Views;
namespace Gestion.Views
{
    public partial class FacturaView : Window
    {
        private readonly FacturaController _controller;
        private readonly Usuario _usuarioActual;

        private readonly List<StackPanel> _paneles = new List<StackPanel>();

        public FacturaView(Usuario usuario)
        {
            InitializeComponent();
            _controller = new FacturaController();
            _usuarioActual = usuario;

            _paneles.Add(PanelConsultar);
            _paneles.Add(PanelCrear);
            _paneles.Add(PanelPorUsuario);
            _paneles.Add(PanelPdf);
            _paneles.Add(PanelCancelarFactura);

            MostrarPanel(PanelConsultar, "Consultar Factura");
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

        private void NavConsultar_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelConsultar, "Consultar Factura");

        private void NavCrear_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelCrear, "Crear Factura");

        private void NavUsuario_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelPorUsuario, "Facturas por Usuario");

        private void NavPdf_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelPdf, "Generar PDF");

        private void NavCancelar_Click(object sender, RoutedEventArgs e)
            => MostrarPanel(PanelCancelarFactura, "Cancelar Factura");

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginView().Show();
            this.Close();
        }

        // ─────────────────────────────────────────
        // CONSULTAR FACTURA
        // ─────────────────────────────────────────

        private async void BtnObtenerFactura_Click(object sender, RoutedEventArgs e)
        {
            PanelDetalleFactura.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtFacturaId.Text.Trim(), out int id) || id <= 0)
            {
                MessageBox.Show("Ingresa un ID de factura válido.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var factura = await LoadingDialogService.RunAsync(
                this,
                () => _controller.ObtenerFactura(id),
                "Consultando factura...");

            if (factura == null)
            {
                MessageBox.Show("Factura no encontrada.", "Sin resultados",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TxtDetId.Text = factura.Id.ToString();
            TxtDetUsuario.Text = factura.IdUsuario.ToString();
            TxtDetFecha.Text = factura.Fecha.ToString("dd/MM/yyyy HH:mm");
            TxtDetTotal.Text = $"$ {factura.Total:N2}";
            TxtDetEstado.Text = "Activa";

            PanelDetalleFactura.Visibility = Visibility.Visible;
        }

        // ─────────────────────────────────────────
        // CREAR FACTURA
        // ─────────────────────────────────────────

        private async void BtnCrearFactura_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgCrearFactura.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtNuevaFacturaUsuario.Text.Trim(), out int usuarioId) || usuarioId <= 0)
            {
                MostrarFeedback(TxtMsgCrearFactura, "ID de usuario inválido.", esError: true);
                return;
            }

            // Construir lista de ítems desde los campos del formulario
            var items = new List<FacturaItem>();

            var item1 = ConstruirItem(TxtItem1Desc.Text, TxtItem1Precio.Text, TxtItem1Cantidad.Text);
            if (item1 != null) items.Add(item1);

            var item2 = ConstruirItem(TxtItem2Desc.Text, TxtItem2Precio.Text, TxtItem2Cantidad.Text);
            if (item2 != null) items.Add(item2);

            if (items.Count == 0)
            {
                MostrarFeedback(TxtMsgCrearFactura, "Agrega al menos un ítem con descripción, precio y cantidad.", esError: true);
                return;
            }

            var factura = new Factura
            {
                IdUsuario = usuarioId,
                Items = items
            };

            int facturaId = await LoadingDialogService.RunAsync(
                this,
                () => _controller.CrearFactura(factura),
                "Creando factura...");

            if (facturaId <= 0)
            {
                MostrarFeedback(TxtMsgCrearFactura, "No se pudo crear la factura. Revisa los datos.", esError: true);
                return;
            }

            MostrarFeedback(TxtMsgCrearFactura, $"Factura creada exitosamente. ID: {facturaId}", esError: false);
            LimpiarFormularioCrear();
        }

        private FacturaItem ConstruirItem(string desc, string precioStr, string cantidadStr)
        {
            if (string.IsNullOrWhiteSpace(desc)) return null;

            if (!decimal.TryParse(precioStr.Trim(), out decimal precio) || precio <= 0) return null;
            if (!int.TryParse(cantidadStr.Trim(), out int cantidad) || cantidad <= 0) return null;

            return new FacturaItem
            {
                Descripcion = desc.Trim(),
                Precio = precio,
                Cantidad = cantidad
            };
        }

        private void LimpiarFormularioCrear()
        {
            TxtNuevaFacturaUsuario.Clear();
            TxtItem1Desc.Clear();
            TxtItem1Precio.Clear();
            TxtItem1Cantidad.Clear();
            TxtItem2Desc.Clear();
            TxtItem2Precio.Clear();
            TxtItem2Cantidad.Clear();
        }

        // ─────────────────────────────────────────
        // FACTURAS POR USUARIO
        // ─────────────────────────────────────────

        private async void BtnFacturasUsuario_Click(object sender, RoutedEventArgs e)
        {
            LvFacturasUsuario.ItemsSource = null;

            if (!int.TryParse(TxtUsuarioFacturasId.Text.Trim(), out int usuarioId) || usuarioId <= 0)
            {
                MessageBox.Show("Ingresa un ID de usuario válido.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var facturas = await LoadingDialogService.RunAsync(
                this,
                () => _controller.ObtenerFacturasUsuario(usuarioId),
                "Cargando facturas del usuario...");

            if (facturas == null || facturas.Count == 0)
            {
                MessageBox.Show("Este usuario no tiene facturas registradas.", "Sin resultados",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            LvFacturasUsuario.ItemsSource = facturas;
        }

        // ─────────────────────────────────────────
        // GENERAR PDF
        // ─────────────────────────────────────────

        private void BtnExplorarRuta_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Guardar factura como PDF",
                Filter = "PDF files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = "factura"
            };

            if (dialog.ShowDialog() == true)
                TxtPdfRuta.Text = dialog.FileName;
        }

        private async void BtnGenerarPdf_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgPdf.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtPdfFacturaId.Text.Trim(), out int id) || id <= 0)
            {
                MostrarFeedback(TxtMsgPdf, "ID de factura inválido.", esError: true);
                return;
            }

            string ruta = TxtPdfRuta.Text.Trim();
            if (string.IsNullOrWhiteSpace(ruta))
            {
                MostrarFeedback(TxtMsgPdf, "Selecciona la ruta de exportación.", esError: true);
                return;
            }

            bool ok = await LoadingDialogService.RunAsync(
                this,
                () => _controller.GenerarPdf(id, ruta),
                "Generando el PDF de la factura...");
            MostrarFeedback(TxtMsgPdf,
                ok ? $"PDF generado correctamente en:\n{ruta}" : "No se pudo generar el PDF.",
                esError: !ok);
        }

        // ─────────────────────────────────────────
        // CANCELAR FACTURA
        // ─────────────────────────────────────────

        private async void BtnCancelarFactura_Click(object sender, RoutedEventArgs e)
        {
            TxtMsgCancelarFactura.Visibility = Visibility.Collapsed;

            if (!int.TryParse(TxtCancelarFacturaId.Text.Trim(), out int id) || id <= 0)
            {
                MostrarFeedback(TxtMsgCancelarFactura, "ID de factura inválido.", esError: true);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Confirmas la cancelación de la factura #{id}? Esta acción es permanente.",
                "Confirmar cancelación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            bool ok = await LoadingDialogService.RunAsync(
                this,
                () => _controller.CancelarFactura(id),
                "Cancelando factura...");
            MostrarFeedback(TxtMsgCancelarFactura,
                ok ? "Factura cancelada correctamente." : "No se pudo cancelar la factura.",
                esError: !ok);

            if (ok) TxtCancelarFacturaId.Clear();
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
