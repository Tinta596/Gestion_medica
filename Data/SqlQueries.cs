namespace Gestion.Data
{
    public static class AdministradorQueries
    {
        public const string InsertarUsuario = @"INSERT INTO Usuario (Nombre, Apellido, Correo, Direccion, Contrasena, Activo, IdRol)
                                 VALUES (@Nombre, @Apellido, @Correo, @Direccion, @Contrasena, @Activo, @IdRol)";

        public const string ActualizarUsuario = @"UPDATE Usuario
                                     SET Nombre=@Nombre,
                                         Apellido=@Apellido,
                                         Correo=@Correo,
                                         Direccion=@Direccion,
                                         Contrasena=@Contrasena,
                                         Activo=@Activo,
                                         IdRol=@IdRol
                                     WHERE Id=@Id";

        public const string ObtenerUsuarioPorId = "SELECT * FROM Usuario WHERE Id = @Id";

        public const string BuscarUsuariosPorNombre = @"SELECT Id, Nombre, Apellido, Correo, Direccion, Activo, IdRol, Telefono
                                     FROM Usuario
                                     WHERE IdRol IN (@IdRolDoctor, @IdRolPaciente)
                                       AND (
                                           @Filtro = ''
                                           OR Nombre LIKE @Busqueda
                                           OR Apellido LIKE @Busqueda
                                           OR CONCAT(Nombre, ' ', Apellido) LIKE @Busqueda
                                           OR Correo LIKE @Busqueda
                                       )
                                     ORDER BY IdRol, Nombre, Apellido";

        public const string ContarCitasEnHorario = @"SELECT COUNT(*)
                                     FROM Citas
                                     WHERE FechaHora = @FechaHora
                                       AND IdEspecialidad = @IdEspecialidad
                                       AND Estado != 3";

        public const string ObtenerDoctoresActivos = @"SELECT u.Id, u.Nombre, u.Apellido, u.Correo, u.Activo, u.IdRol
                                     FROM Usuario u
                                     INNER JOIN Rol r ON u.IdRol = r.Id
                                     WHERE r.Id = 2 AND u.Activo = 1";

        public const string ObtenerCitaCompletaPorId = @"SELECT c.Id,
                                            c.IdUsuario,
                                            c.FechaHora,
                                            c.Estado,
                                            c.IdEspecialidad,
                                            c.Precio,
                                            CONCAT(u.Nombre, ' ', COALESCE(u.Apellido,'')) AS Paciente
                                     FROM Citas c
                                     INNER JOIN Usuario u ON c.IdUsuario = u.Id
                                     WHERE c.Id = @Id";

        public const string ObtenerCredencialesActivas = @"SELECT Id, Nombre, Apellido, Correo, Contrasena, Activo, IdRol
                                     FROM Usuario
                                     WHERE Correo = @Correo AND Activo = 1";

        public const string DesactivarUsuario = @"UPDATE Usuario
                                     SET Activo = 0
                                     WHERE Id = @Id AND Activo = 1";

        public const string ObtenerCitasConPrecio = @"SELECT c.Id,
                                            c.IdUsuario,
                                            c.FechaHora,
                                            c.Estado,
                                            c.IdEspecialidad,
                                            c.Precio,
                                            CONCAT(u.Nombre, ' ', COALESCE(u.Apellido, '')) AS Paciente
                                     FROM Citas c
                                     INNER JOIN Usuario u ON c.IdUsuario = u.Id
                                     ORDER BY c.FechaHora DESC";

        public const string ObtenerCitasSinPrecio = @"SELECT c.Id,
                                            c.IdUsuario,
                                            c.FechaHora,
                                            c.Estado,
                                            c.IdEspecialidad,
                                            CONCAT(u.Nombre, ' ', COALESCE(u.Apellido, '')) AS Paciente
                                     FROM Citas c
                                     INNER JOIN Usuario u ON c.IdUsuario = u.Id
                                     ORDER BY c.FechaHora DESC";

        public const string ActualizarEstadoCita = @"UPDATE Citas
                                     SET Estado = @Estado
                                     WHERE Id = @Id";
    }

    public static class DoctorQueries
    {
        public const string ContarUsuarioActivo = "SELECT COUNT(*) FROM Usuario WHERE Id = @Id AND Activo = 1";
        public const string ContarEspecialidad = "SELECT COUNT(*) FROM especialidadcatalogo WHERE Id = @Id";

        public const string ObtenerPacientes = @"SELECT Id, Nombre, Apellido, Correo, Direccion, Activo, IdRol, Telefono
                                     FROM Usuario
                                     WHERE IdRol = @IdRolPaciente
                                       AND Activo = 1
                                       AND (
                                           @Filtro = ''
                                           OR Nombre LIKE @Busqueda
                                           OR Apellido LIKE @Busqueda
                                           OR CONCAT(Nombre, ' ', Apellido) LIKE @Busqueda
                                           OR Correo LIKE @Busqueda
                                       )
                                     ORDER BY Nombre, Apellido";

        public const string ObtenerHistorialClinico = "SELECT * FROM historial_clinico WHERE IdUsuario = @Id";

        public const string InsertarHistorialClinico = @"INSERT INTO historial_clinico
                                        (IdUsuario, FechaConsulta, Diagnostico, Tratamiento, NotasAdicionales)
                                     VALUES
                                        (@IdUsuario, @FechaConsulta, @Diagnostico, @Tratamiento, @NotasAdicionales);
                                     SELECT LAST_INSERT_ID();";

        public const string ObtenerDatosUsuario = "SELECT * FROM Usuario WHERE Id = @Id";

        public const string ObtenerCitasPaciente = @"SELECT Id, IdUsuario, FechaHora, Estado, IdEspecialidad, Precio
                                     FROM Citas
                                     WHERE IdUsuario = @IdUsuario
                                       AND Estado IN (@EstadoPendiente, @EstadoCompletada)
                                     ORDER BY FechaHora DESC";

        public const string ObtenerCitaPorId = @"SELECT Id, IdUsuario, FechaHora, Estado, IdEspecialidad, Precio
                             FROM Citas
                             WHERE Id = @Id";

        public const string CompletarCita = @"UPDATE Citas
                                     SET Estado = @Estado
                                     WHERE Id = @Id";

        public const string InsertarCitaConPrecio = @"INSERT INTO Citas (IdUsuario, FechaHora, Estado, IdEspecialidad, Precio)
                             VALUES (@IdUsuario, @FechaHora, @Estado, @IdEspecialidad, @Precio);
                             SELECT LAST_INSERT_ID();";

        public const string InsertarCitaSinPrecio = @"INSERT INTO Citas (IdUsuario, FechaHora, Estado, IdEspecialidad)
                             VALUES (@IdUsuario, @FechaHora, @Estado, @IdEspecialidad);
                             SELECT LAST_INSERT_ID();";

        public const string ActualizarCita = @"UPDATE Citas
                                     SET IdUsuario = @IdUsuario,
                                         FechaHora = @FechaHora,
                                         Estado = @Estado,
                                         IdEspecialidad = @IdEspecialidad
                                     WHERE Id = @Id";
    }

    public static class UsuarioQueries
    {
        public const string ObtenerCitas = @"SELECT Id,
                                            IdUsuario,
                                            FechaHora,
                                            Estado,
                                            IdEspecialidad,
                                            Precio
                                     FROM Citas
                                     WHERE IdUsuario = @IdUsuario
                                     ORDER BY FechaHora DESC";

        public const string ObtenerEstadoCita = @"SELECT Estado
                                     FROM Citas
                                     WHERE Id = @Id";

        public const string ObtenerPromedioCitas = @"SELECT AVG(TotalCitas)
                                     FROM (
                                         SELECT IdUsuario,
                                                COUNT(*) AS TotalCitas
                                         FROM Citas
                                         GROUP BY IdUsuario
                                     ) AS CitasPorUsuario";

        public const string ObtenerResumenEstados = @"SELECT Estado, COUNT(*) AS Total
                                     FROM Citas
                                     WHERE IdUsuario = @IdUsuario
                                     GROUP BY Estado";

        public const string CancelarCita = @"UPDATE Citas
                                     SET Estado = @Estado
                                     WHERE Id = @Id";
    }

    public static class FacturaQueries
    {
        public const string ObtenerUsuario = "SELECT Id, Nombre, Apellido, Correo, Telefono, Direccion FROM Usuario WHERE Id = @Id";

        public const string ObtenerCitaPorFactura = @"SELECT c.Id, c.IdUsuario, c.FechaHora, c.Estado,
                                c.IdEspecialidad, c.Motivo, c.Observaciones, c.Precio
                         FROM Citas c
                         INNER JOIN Factura f ON f.IdCita = c.Id
                         WHERE f.Id = @IdFactura";

        public const string ObtenerFactura = "SELECT Id, IdUsuario, IdCita, Fecha, Total, Estado FROM Factura WHERE Id = @Id";
        public const string ObtenerItemsFactura = "SELECT Id, FacturaId, Descripcion, Precio, Cantidad FROM FacturaItem WHERE FacturaId = @Id";
        public const string ObtenerFacturasUsuario = "SELECT Id, IdUsuario, IdCita, Fecha, Total, Estado FROM Factura WHERE IdUsuario = @IdUsuario";

        public const string CrearFacturasFaltantes = @"INSERT INTO Factura (IdUsuario, IdCita, Fecha, Total, Estado)
                                 SELECT c.IdUsuario,
                                        c.Id,
                                        NOW(),
                                        COALESCE(c.Precio, 0),
                                        CASE WHEN c.Estado = @EstadoCancelada THEN 'Cancelada' ELSE 'Generada' END
                                 FROM Citas c
                                 LEFT JOIN Factura f ON f.IdCita = c.Id
                                 WHERE c.IdUsuario = @IdUsuario
                                   AND f.Id IS NULL
                                   AND COALESCE(c.Precio, 0) > 0";

        public const string CrearItemsFaltantes = @"INSERT INTO FacturaItem (FacturaId, Descripcion, Precio, Cantidad)
                              SELECT f.Id,
                                     CONCAT('Consulta de ', c.IdEspecialidad),
                                     COALESCE(c.Precio, 0),
                                     1
                              FROM Factura f
                              INNER JOIN Citas c ON c.Id = f.IdCita
                              LEFT JOIN FacturaItem fi ON fi.FacturaId = f.Id
                              WHERE c.IdUsuario = @IdUsuario
                                AND fi.Id IS NULL
                                AND COALESCE(c.Precio, 0) > 0";

        public const string InsertarFactura = @"INSERT INTO Factura (IdUsuario, IdCita, Fecha, Total, Estado)
                               VALUES (@IdUsuario, @IdCita, @Fecha, @Total, @Estado);
                               SELECT LAST_INSERT_ID();";

        public const string InsertarItemFactura = @"INSERT INTO FacturaItem (FacturaId, Descripcion, Precio, Cantidad)
                               VALUES (@FacturaId, @Descripcion, @Precio, @Cantidad)";

        public const string CancelarFactura = "UPDATE Factura SET Estado = @Estado WHERE Id = @Id";
    }
}
