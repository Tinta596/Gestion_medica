using Gestion.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Data.Repositories
{
    public sealed class FacturaRepository : MySqlRepository
    {
        public Task<Usuario> ObtenerUsuarioAsync(int usuarioId)
        {
            return QuerySingleOrDefaultAsync(FacturaQueries.ObtenerUsuario,
                command => command.Parameters.AddWithValue("@Id", usuarioId),
                reader => new Usuario
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                    Apellido = reader["Apellido"]?.ToString() ?? string.Empty,
                    Correo = reader["Correo"]?.ToString() ?? string.Empty,
                    Telefono = reader["Telefono"] == DBNull.Value ? string.Empty : reader["Telefono"].ToString(),
                    Direccion = reader["Direccion"] == DBNull.Value ? string.Empty : reader["Direccion"].ToString()
                });
        }

        public Task<Citas> ObtenerCitaPorFacturaAsync(int facturaId)
        {
            return QuerySingleOrDefaultAsync(FacturaQueries.ObtenerCitaPorFactura,
                command => command.Parameters.AddWithValue("@IdFactura", facturaId),
                reader => MapCita(reader));
        }

        public async Task<Factura> ObtenerFacturaAsync(int facturaId)
        {
            var factura = await QuerySingleOrDefaultAsync(FacturaQueries.ObtenerFactura,
                command => command.Parameters.AddWithValue("@Id", facturaId),
                reader => new Factura
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                    IdCita = reader["IdCita"] != DBNull.Value ? Convert.ToInt32(reader["IdCita"]) : 0,
                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                    Total = reader["Total"] != DBNull.Value ? Convert.ToDecimal(reader["Total"]) : 0m,
                    Estado = reader["Estado"]?.ToString() ?? string.Empty,
                    Items = new List<FacturaItem>()
                });

            if (factura == null)
                return new Factura { Items = new List<FacturaItem>() };

            factura.Items = await QueryAsync(FacturaQueries.ObtenerItemsFactura,
                command => command.Parameters.AddWithValue("@Id", facturaId),
                reader => new FacturaItem
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    FacturaId = Convert.ToInt32(reader["FacturaId"]),
                    Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                    Precio = Convert.ToDecimal(reader["Precio"]),
                    Cantidad = Convert.ToInt32(reader["Cantidad"])
                });

            return factura;
        }

        public async Task<List<Factura>> ObtenerFacturasUsuarioAsync(int usuarioId)
        {
            await CrearFacturasFaltantesAsync(usuarioId);
            return await QueryAsync(FacturaQueries.ObtenerFacturasUsuario,
                command => command.Parameters.AddWithValue("@IdUsuario", usuarioId),
                reader => new Factura
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                    IdCita = reader["IdCita"] != DBNull.Value ? Convert.ToInt32(reader["IdCita"]) : 0,
                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                    Total = reader["Total"] != DBNull.Value ? Convert.ToDecimal(reader["Total"]) : 0m,
                    Estado = reader["Estado"]?.ToString() ?? string.Empty,
                    Items = new List<FacturaItem>()
                });
        }

        private async Task CrearFacturasFaltantesAsync(int usuarioId)
        {
            try
            {
                await ExecuteAsync(FacturaQueries.CrearFacturasFaltantes, command =>
                {
                    command.Parameters.AddWithValue("@IdUsuario", usuarioId);
                    command.Parameters.AddWithValue("@EstadoCancelada", (int)EstadoCita.Cancelada);
                });

                await ExecuteAsync(FacturaQueries.CrearItemsFaltantes,
                    command => command.Parameters.AddWithValue("@IdUsuario", usuarioId));
            }
            catch (MySqlException exception) when (exception.Number == 1054)
            {
            }
        }

        public async Task<int> CrearFacturaAsync(Factura factura)
        {
            using (var connection = await OpenConnectionAsync())
            using (var transaction = (MySqlTransaction)await connection.BeginTransactionAsync())
            {
                try
                {
                    int facturaId;
                    using (var command = new MySqlCommand(FacturaQueries.InsertarFactura, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@IdUsuario", factura.IdUsuario);
                        command.Parameters.AddWithValue("@IdCita", factura.IdCita > 0 ? (object)factura.IdCita : DBNull.Value);
                        command.Parameters.AddWithValue("@Fecha", factura.Fecha);
                        command.Parameters.AddWithValue("@Total", factura.Total);
                        command.Parameters.AddWithValue("@Estado", "Generada");
                        facturaId = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }

                    if (factura.Items != null)
                    {
                        foreach (var item in factura.Items)
                        {
                            using (var command = new MySqlCommand(FacturaQueries.InsertarItemFactura, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@FacturaId", facturaId);
                                command.Parameters.AddWithValue("@Descripcion", item.Descripcion);
                                command.Parameters.AddWithValue("@Precio", item.Precio);
                                command.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    return facturaId;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> CancelarFacturaAsync(int facturaId)
        {
            int affected = await ExecuteAsync(FacturaQueries.CancelarFactura, command =>
            {
                command.Parameters.AddWithValue("@Estado", "Cancelada");
                command.Parameters.AddWithValue("@Id", facturaId);
            });

            return affected > 0;
        }
    }
}
