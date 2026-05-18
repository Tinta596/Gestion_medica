using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace GestionMedica.DB
{
    public class DBUsuario
    {
        private readonly string conexionString =
            "server=localhost;" +
            "user=root;" +
            "password=;" +
            "database=gestion_citas_bd;" +
            "port=3307;";

        public async Task<MySqlConnection> AbrirConexion()
        {
            
            var conexion = new MySqlConnection(conexionString);
            try
            {
                await conexion.OpenAsync();
                
            }
            catch (MySqlException mex)
            {
                
                throw;
            }

            return conexion;
        }
    }
}
