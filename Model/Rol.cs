using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion.Model
{
    public class Rol
    {
        public int Id { get; set; }
        public TipoRol TipoRol { get; set; }


        public Rol roles(int id, TipoRol tipoRoles)
        {
            return new Rol
            {
                Id = id,
                TipoRol = tipoRoles
            };
        }
    }

    public enum TipoRol
    {
        Administrador = 1,
        Doctor = 2,
        Paciente = 3
    }
    public enum TipoEspecialidad
    {
        Cardiologia = 1,
        Dermatologia = 2,
        Oftalmologia = 3,
        Ortopedia = 4,
        Pediatria = 5,
        Neurologia = 6,
        General = 7
    }
}