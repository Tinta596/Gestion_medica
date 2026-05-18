using System;
using System.Collections.Generic;

namespace Gestion.Model
{
    public class Factura
    {
        public int Id{ get; set; }
        public int IdUsuario { get; set; }
        public int IdCita { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public List<FacturaItem> Items { get; set; }
        public string Estado { get; internal set; }
    }
}