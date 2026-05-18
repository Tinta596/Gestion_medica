using Gestion.Interface;
using Gestion.Model;
using GestionMedica.DB;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace GestionMedica.Services.Usuarios
{
    public class UsuarioServices : IUsuario
    {
        private readonly DBUsuario conexion;

        public UsuarioServices()
        {
            conexion = new DBUsuario();
        }

        public async Task<List<Citas>> ObtenerCita(Citas citas)
        {
            List<Citas> lista = new List<Citas>();

            using (var con = await conexion.AbrirConexion())
            {
                try
                {
                    string query = @"SELECT Id,
                                            IdUsuario,
                                            FechaHora,
                                            Estado,
                                            IdEspecialidad,
                                            Precio
                                     FROM Citas
                                     WHERE IdUsuario = @IdUsuario
                                     ORDER BY FechaHora DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", citas.IdUsuario);

                        using (var reader =
                            await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var cita = new Citas
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                                    Estado = (EstadoCita)Convert.ToInt32(reader["Estado"]),
                                    IdEspecialidad = (TipoEspecialidad)Convert.ToInt32(reader["IdEspecialidad"]),
                                    Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0m
                                };

                                lista.Add(cita);
                            }
                        }
                    }
                }
                catch (Exception ex) 
                {
                    Debug.WriteLine($"Error al obtner citas {ex}");   
                }
            }

            return lista;
        }

        public async Task<EstadoCita> ObtenerEstadoCita(int id)
        {
            EstadoCita estado = EstadoCita.Pendiente;

            using (var con = await conexion.AbrirConexion())
            {
                try
                {
                    string query = @"SELECT Estado
                                     FROM Citas
                                     WHERE Id = @Id";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                estado = (EstadoCita)Convert.ToInt32(reader["Estado"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error] ObtenerEstadoCita(id={id}): {ex.Message}");
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                }
            }

            return estado;
        }


        public async Task<int> PromedioCitasPorPaciente()
        {
            int promedio = 0;

            using (var con = await conexion.AbrirConexion())
            {
                try
                {
                    string query = @"SELECT AVG(TotalCitas)
                                     FROM (
                                         SELECT IdUsuario,
                                                COUNT(*) AS TotalCitas
                                         FROM Citas
                                         GROUP BY IdUsuario
                                     ) AS CitasPorUsuario";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        var resultado = await cmd.ExecuteScalarAsync();
                        if (resultado != null && resultado != DBNull.Value)
                        {
                            promedio = Convert.ToInt32(resultado);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"[Error] PromedioCitasPorPaciente: {ex.Message}");
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                }
            }

            return promedio;
        }

        public async Task<Dictionary<EstadoCita, int>> ObtenerResumenEstadosUsuario(int idUsuario)
        {
            var resumen = Enum.GetValues(typeof(EstadoCita))
                .Cast<EstadoCita>()
                .ToDictionary(estado => estado, estado => 0);

            using (var con = await conexion.AbrirConexion())
            {
                try
                {
                    string query = @"SELECT Estado, COUNT(*) AS Total
                                     FROM Citas
                                     WHERE IdUsuario = @IdUsuario
                                     GROUP BY Estado";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var estado = (EstadoCita)Convert.ToInt32(reader["Estado"]);
                                resumen[estado] = Convert.ToInt32(reader["Total"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error] ObtenerResumenEstadosUsuario(idUsuario={idUsuario}): {ex.Message}");
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
                }
            }

            return resumen;
        }

        public async Task<bool> CancelarCita(int id)
        {
            bool resultado = false;

            using (var con = await conexion.AbrirConexion())
            {
                try
                {
                    string query = @"UPDATE Citas
                                     SET Estado = @Estado
                                     WHERE Id = @Id";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Estado", (int)EstadoCita.Cancelada);
                        cmd.Parameters.AddWithValue("@Id", id);
                        int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                        resultado = filasAfectadas > 0;
                    }
                }
                catch (Exception ex)
                {
                    resultado = false;
                }
            }

            return resultado;
        }
    }
}
