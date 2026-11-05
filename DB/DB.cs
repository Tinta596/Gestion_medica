using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace GestionMedica.DB
{
    public class DBUsuario
    {
        public async Task<MySqlConnection> AbrirConexion()
        {
            var config = ConfigurationManager.ConnectionStrings["GestionMedica"];
            if (config == null || string.IsNullOrWhiteSpace(config.ConnectionString))
                throw new ConfigurationErrorsException("Falta la cadena de conexión 'GestionMedica'.");

            var builder = new MySqlConnectionStringBuilder(config.ConnectionString);
            if (string.IsNullOrWhiteSpace(builder.UserID) ||
                builder.UserID.Equals("root", StringComparison.OrdinalIgnoreCase))
            {
                throw new ConfigurationErrorsException(
                    "Configura un usuario MySQL de aplicación con permisos limitados; no uses root.");
            }

            if (string.IsNullOrWhiteSpace(builder.Password))
                throw new ConfigurationErrorsException("Falta la contraseña del usuario MySQL de aplicación.");

            var conexion = new MySqlConnection(builder.ConnectionString);
            try
            {
                await conexion.OpenAsync();
                return conexion;
            }
            catch
            {
                conexion.Dispose();
                throw;
            }
        }
    }
}
