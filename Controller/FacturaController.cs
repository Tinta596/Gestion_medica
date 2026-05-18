using Gestion.Model;
using Gestion.Service;
using GestionMedica.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gestion.Controller
{
    public class FacturaController
    {
        private readonly FacturaServices facturaService;

        public FacturaController()
        {
            facturaService = new FacturaServices();
        }

        public async Task<Factura> ObtenerFactura(int id)
        {
            try
            {
                if (id <= 0)
                {
                    
                    return null;
                }

                var factura = await facturaService.ObtenerFactura(id);

                if (factura == null || factura.Id == 0)
                {
                    
                    return null;
                }

                return factura;
            }
            catch (Exception ex)
            {
                
                return null;
            }
        }

        public async Task<int> CrearFactura(Factura factura)
        {
            try
            {
                if (factura == null)
                {
                    
                    return 0;
                }

                if (factura.IdUsuario <= 0)
                {
                    
                    return 0;
                }

                if (factura.Items == null || factura.Items.Count == 0)
                {
                    
                    return 0;
                }

                foreach (var item in factura.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.Descripcion))
                    {
                        
                        return 0;
                    }

                    if (item.Precio <= 0)
                    {
                        
                        return 0;
                    }

                    if (item.Cantidad <= 0)
                    {
                        
                        return 0;
                    }
                }

                factura.Total = 0;

                foreach (var item in factura.Items)
                {
                    factura.Total += item.Subtotal;
                }

                factura.Fecha = DateTime.Now;

                int facturaId = await facturaService.CrearFactura(factura);

                if (facturaId <= 0)
                {
                    
                    return 0;
                }

                return facturaId;
            }
            catch (Exception ex)
            {
                
                return 0;
            }
        }

        public async Task<bool> CancelarFactura(int id)
        {
            try
            {
                if (id <= 0)
                {
                    
                    return false;
                }

                var factura = await facturaService.ObtenerFactura(id);

                if (factura == null || factura.Id == 0)
                {
                    
                    return false;
                }

                return await facturaService.CancelarFactura(id);
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        public async Task<bool> GenerarPdf(int id, string path)
        {
            try
            {
                if (id <= 0)
                {
                    
                    return false;
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    
                    return false;
                }

                return await facturaService.GenerarPdfFactura(id, path);
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        public async Task<List<Factura>> ObtenerFacturasUsuario(int idUsuario)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    
                    return new List<Factura>();
                }

                return await facturaService.ObtenerFacturasPorUsuario(idUsuario);
            }
            catch (Exception ex)
            {
                
                return new List<Factura>();
            }
        }
    }
}
