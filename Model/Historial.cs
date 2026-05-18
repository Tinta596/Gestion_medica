using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion.Model
{
    public class HistorialClinico
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamiento { get; set; }
        public string NotasAdicionales { get; set; }

        public HistorialClinico historialClinico(int id, int idUsuario, DateTime fechaConsulta, string diagnostico, string tratamiento, string notasAdicionales)
        {
            return new HistorialClinico
            {
                Id = id,
                IdUsuario = idUsuario,
                FechaConsulta = fechaConsulta,
                Diagnostico = diagnostico,
                Tratamiento = tratamiento,
                NotasAdicionales = notasAdicionales
            };
        }
    }
}