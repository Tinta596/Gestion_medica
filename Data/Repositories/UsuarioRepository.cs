using Gestion.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gestion.Data.Repositories
{
    public sealed class UsuarioRepository : MySqlRepository
    {
        public Task<List<Citas>> ObtenerCitasAsync(int usuarioId)
        {
            return QueryAsync(UsuarioQueries.ObtenerCitas,
                command => command.Parameters.AddWithValue("@IdUsuario", usuarioId),
                reader => MapCita(reader));
        }

        public Task<EstadoCita?> ObtenerEstadoCitaAsync(int citaId)
        {
            return QuerySingleOrDefaultAsync<EstadoCita?>(UsuarioQueries.ObtenerEstadoCita,
                command => command.Parameters.AddWithValue("@Id", citaId),
                reader => (EstadoCita)Convert.ToInt32(reader["Estado"]));
        }

        public async Task<int> ObtenerPromedioCitasAsync()
        {
            object result = await ScalarAsync(UsuarioQueries.ObtenerPromedioCitas, null);
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public Task<Dictionary<EstadoCita, int>> ObtenerResumenEstadosAsync(int usuarioId)
        {
            var resumen = Enum.GetValues(typeof(EstadoCita))
                .Cast<EstadoCita>()
                .ToDictionary(estado => estado, estado => 0);

            return CargarResumenEstadosAsync(usuarioId, resumen);
        }

        private async Task<Dictionary<EstadoCita, int>> CargarResumenEstadosAsync(
            int usuarioId,
            Dictionary<EstadoCita, int> resumen)
        {
            var filas = await QueryAsync(UsuarioQueries.ObtenerResumenEstados,
                command => command.Parameters.AddWithValue("@IdUsuario", usuarioId),
                reader => new
                {
                    Estado = (EstadoCita)Convert.ToInt32(reader["Estado"]),
                    Total = Convert.ToInt32(reader["Total"])
                });

            foreach (var fila in filas)
                resumen[fila.Estado] = fila.Total;

            return resumen;
        }

        public async Task<bool> CancelarCitaAsync(int citaId)
        {
            int affected = await ExecuteAsync(UsuarioQueries.CancelarCita, command =>
            {
                command.Parameters.AddWithValue("@Estado", (int)EstadoCita.Cancelada);
                command.Parameters.AddWithValue("@Id", citaId);
            });

            return affected > 0;
        }
    }
}
