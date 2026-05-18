using MySql.Data.MySqlClient;
using System;
using DotNetEnv;

namespace GestionMedica.DB
{
    public class DBUsuario
    {
        private readonly string conexionString;

        public DBUsuario()
        {
            // Cargar variables del archivo .env
            Env.Load();

            conexionString =
                $"server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
                $"user={Environment.GetEnvironmentVariable("DB_USER")};" +
                $"database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                $"password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
                $"port={Environment.GetEnvironmentVariable("DB_PORT")};";
        }

        public MySqlConnection AbrirConexion()
        {
            MySqlConnection conexion = new MySqlConnection(conexionString);

            try
            {
                conexion.Open();
                
            }
            catch (Exception ex)
            {
                
            }

            return conexion;
        }
    }
}
