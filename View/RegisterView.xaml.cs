using Gestion.Controller;
using Gestion.Model;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gestion.Views
{
    public partial class RegisterView : Window
    {
        private readonly AdminController _controller;

        public RegisterView()
        {
            InitializeComponent();
            _controller = new AdminController();
        }

        private async void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            TxtMensaje.Visibility = Visibility.Collapsed;

            string nombre = TxtNombre.Text.Trim();
            string apellido = TxtApellido.Text.Trim();
            string correo = TxtCorreo.Text.Trim();
            string password = TxtContrasena.Password;
            string confirmar = TxtConfirmarContrasena.Password;
            string telefono = TxtTelefono.Text.Trim();

            // Validaciones
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarMensaje("El nombre es obligatorio.", esError: true);
                TxtNombre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                MostrarMensaje("El apellido es obligatorio.", esError: true);
                TxtApellido.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(correo) || !correo.Contains("@"))
            {
                MostrarMensaje("Ingresa un correo electrónico válido.", esError: true);
                TxtCorreo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                MostrarMensaje("La contraseña debe tener al menos 6 caracteres.", esError: true);
                TxtContrasena.Focus();
                return;
            }

            if (password != confirmar)
            {
                MostrarMensaje("Las contraseñas no coinciden.", esError: true);
                TxtConfirmarContrasena.Clear();
                TxtConfirmarContrasena.Focus();
                return;
            }

            BtnRegistrar.IsEnabled = false;
            BtnRegistrar.Content = "Registrando...";

            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Correo = correo,
                Contrasena = password,
                Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono,
                IdRol = (int)TipoRol.Paciente
            };

            var (ok, msg) = await LoadingDialogService.RunAsync(
                this,
                () => _controller.CrearUsuario(nuevoUsuario),
                "Creando tu cuenta...");

            BtnRegistrar.IsEnabled = true;
            BtnRegistrar.Content = "CREAR CUENTA";

            if (!ok)
            {
                MostrarMensaje($"Error: {msg}", esError: true);
                return;
            }

            MostrarMensaje("¡Cuenta creada exitosamente! Redirigiendo...", esError: false);
            await Task.Delay(1800);

            var login = new LoginView();
            login.Show();
            this.Close();
        }

        private void BtnIrLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginView();
            login.Show();
            this.Close();
        }

        private void MostrarMensaje(string texto, bool esError)
        {
            TxtMensaje.Text = texto;
            TxtMensaje.Foreground = new SolidColorBrush(
                esError
                    ? (Color)ColorConverter.ConvertFromString("#EF5350")
                    : (Color)ColorConverter.ConvertFromString("#66BB6A"));
            TxtMensaje.Visibility = Visibility.Visible;
        }
    }
}