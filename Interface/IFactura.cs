using Gestion.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Interface
{
    public interface IFactura
    {
        Task<Factura> ObtenerFactura(int id);
        Task<List<Factura>> ObtenerFacturasPorUsuario(int idUsuario);
        Task<int> CrearFactura(Factura factura);
        Task<bool> GenerarPdfFactura(int id, string path);
        Task<bool> CancelarFactura(int id);
    }
}
