using Gestion.Controller;
using Gestion.Model;
using System.Windows;

namespace Gestion.Views
{
    public partial class LoginView : Window
    {
        private readonly AdminController _controller;

        public LoginView()
        {
            InitializeComponent();
            _controller = new AdminController();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar mensaje previo
            TxtError.Visibility = Visibility.Collapsed;
            TxtError.Text = string.Empty;

            string correo = TxtCorreo.Text.Trim();
            string contrasena = TxtContrasena.Password;

            // Validaciones básicas en la vista
            if (string.IsNullOrWhiteSpace(correo))
            {
                MostrarError("Por favor ingresa tu correo electrónico.");
                TxtCorreo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                MostrarError("Por favor ingresa tu contraseña.");
                TxtContrasena.Focus();
                return;
            }

            // Deshabilitar botón mientras se procesa
            BtnLogin.IsEnabled = false;
            BtnLogin.Content = "Verificando...";

            var (ok, msg, usuario) = await _controller.Login(correo, contrasena);

            BtnLogin.IsEnabled = true;
            BtnLogin.Content = "INGRESAR AL SISTEMA";

            if (!ok)
            {
                MostrarError(msg);
                TxtContrasena.Clear();
                TxtContrasena.Focus();
                return;
            }

            // Login exitoso: abrir vista según el rol
            AbrirVistaPorRol(usuario);
            this.Close();
        }

        private void AbrirVistaPorRol(Usuario usuario)
        {
            // Ajusta los valores de TipoRol según tu modelo
            switch ((TipoRol)usuario.IdRol)
            {
                case TipoRol.Administrador:
                    var adminView = new AdminView(usuario);
                    adminView.Show();
                    break;

                case TipoRol.Doctor:
                    var doctorView = new DoctorView(usuario);
                    doctorView.Show();
                    break;

                case TipoRol.Paciente:
                default:
                    var usuarioView = new UsuarioView(usuario);
                    usuarioView.Show();
                    break;
            }
        }

        private void MostrarError(string mensaje)
        {
            TxtError.Text = mensaje;
            TxtError.Visibility = Visibility.Visible;
        }
    }
}
