using Gestion.Interface;
using Gestion.Model;
using Gestion.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GestionMedica.Services.Usuarios
{
    public class UsuarioServices : IUsuario
    {
        private readonly UsuarioRepository _repository;

        public UsuarioServices()
        {
            _repository = new UsuarioRepository();
        }

        public async Task<List<Citas>> ObtenerCita(Citas citas)
        {
            try
            {
                return await _repository.ObtenerCitasAsync(citas.IdUsuario);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener citas {ex}");
                return new List<Citas>();
            }
        }

        public async Task<EstadoCita> ObtenerEstadoCita(int id)
        {
            try
            {
                return await _repository.ObtenerEstadoCitaAsync(id) ?? EstadoCita.Pendiente;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error] ObtenerEstadoCita(id={id}): {ex.Message}");
                return EstadoCita.Pendiente;
            }
        }


        public async Task<int> PromedioCitasPorPaciente()
        {
            try
            {
                return await _repository.ObtenerPromedioCitasAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error] PromedioCitasPorPaciente: {ex.Message}");
                return 0;
            }
        }

        public async Task<Dictionary<EstadoCita, int>> ObtenerResumenEstadosUsuario(int idUsuario)
        {
            try
            {
                return await _repository.ObtenerResumenEstadosAsync(idUsuario);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error] ObtenerResumenEstadosUsuario(idUsuario={idUsuario}): {ex.Message}");
                return Enum.GetValues(typeof(EstadoCita))
                    .Cast<EstadoCita>()
                    .ToDictionary(estado => estado, estado => 0);
            }
        }

        public async Task<bool> CancelarCita(int id)
        {
            try
            {
                return await _repository.CancelarCitaAsync(id);
            }
            catch
            {
                return false;
            }
        }
    }
}
