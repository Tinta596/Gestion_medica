using System;

namespace Gestion.Model
{
    public class Citas
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaHora { get; set; }
        public EstadoCita Estado { get; set; }
        public TipoEspecialidad IdEspecialidad { get; set; }
        public string Motivo { get; set; }
        public string Observaciones { get; set; }
        public decimal Precio { get; set; }
        public string Paciente { get; set; }
        public string EstadoTexto
        {
            get
            {
                return Estado.ToString();
            }
            set
            {

            }
        }
        public string EspecialidadTexto => IdEspecialidad.ToString();
    }

    public enum EstadoCita
    {
        Pendiente = 1,
        Confirmada = 2,
        Cancelada = 3,
        Completada = 4
    }
}