
namespace Gestion.Model
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Direccion { get; set; }
        public string Contrasena { get; set; }
        public bool Activo { get; set; } = true;
        public int IdRol { get; set; }
        public string RolTexto => IdRol == (int)TipoRol.Doctor ? "Doctor" :
                                  IdRol == (int)TipoRol.Paciente ? "Paciente" :
                                  IdRol == (int)TipoRol.Administrador ? "Administrador" : "Sin rol";
        public string Telefono { get; internal set; }

        public Usuario usuario(int id, string nombre, string apellido, string correo, string direccion, string contrasena, bool activo, int idRol)
        {
            return new Usuario
            {
                Id = id,
                Nombre = nombre,
                Apellido = apellido,
                Correo = correo,
                Direccion = direccion,
                Contrasena = contrasena,
                Activo = activo,
                IdRol = idRol
            };
        }

    }
}
