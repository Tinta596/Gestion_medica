using Gestion.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Interface
{
    public interface IUsuario
    {
        Task<List<Citas>> ObtenerCita(Citas citas);
        Task<bool> CancelarCita(int Id);
        Task<EstadoCita> ObtenerEstadoCita(int id);
        Task<int> PromedioCitasPorPaciente();
        Task<Dictionary<EstadoCita, int>> ObtenerResumenEstadosUsuario(int idUsuario);

    }
}
