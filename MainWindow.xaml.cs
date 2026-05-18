using Gestion.Views;
using System.Windows;

namespace Gestion
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // ─────────────────────────────────────────
        // NAVEGACIÓN DESDE LA PANTALLA DE BIENVENIDA
        // ─────────────────────────────────────────

        private void BtnIrLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginView();
            login.Show();
            this.Close();
        }

        private void BtnIrRegistro_Click(object sender, RoutedEventArgs e)
        {
            var register = new RegisterView();
            register.Show();
            this.Close();
        }
    }
}