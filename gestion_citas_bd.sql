-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1:3307:3307
-- Tiempo de generación: 19-05-2026 a las 15:37:23
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `gestion_citas_bd`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `citas`
--

CREATE TABLE `citas` (
  `Id` int(11) NOT NULL,
  `IdUsuario` int(11) NOT NULL,
  `FechaHora` datetime NOT NULL,
  `Estado` int(11) NOT NULL,
  `IdEspecialidad` int(11) NOT NULL,
  `Observaciones` varchar(255) DEFAULT NULL,
  `Precio` decimal(12,2) NOT NULL DEFAULT 0.00
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `citas`
--

INSERT INTO `citas` (`Id`, `IdUsuario`, `FechaHora`, `Estado`, `IdEspecialidad`, `Observaciones`, `Precio`) VALUES
(1, 4, '2026-05-15 09:30:00', 1, 2, NULL, 0.00),
(2, 4, '2026-05-21 09:15:00', 3, 1, NULL, 0.00),
(3, 4, '2026-05-29 09:30:00', 3, 4, NULL, 0.00),
(4, 4, '2026-05-27 00:00:00', 2, 6, NULL, 0.00),
(7, 4, '2026-05-22 10:30:00', 4, 5, NULL, 120000.00),
(10, 4, '2026-05-22 00:00:00', 1, 2, NULL, 0.00),
(11, 4, '2026-05-16 16:30:00', 1, 1, NULL, 65000.00);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `especialidadcatalogo`
--

CREATE TABLE `especialidadcatalogo` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `especialidadcatalogo`
--

INSERT INTO `especialidadcatalogo` (`Id`, `Nombre`) VALUES
(1, 'Cardiologia'),
(2, 'Dermatologia'),
(3, 'Oftalmologia'),
(4, 'Ortopedia'),
(5, 'Pediatria'),
(6, 'Neurologia'),
(7, 'General');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `factura`
--

CREATE TABLE `factura` (
  `Id` int(11) NOT NULL,
  `IdUsuario` int(11) NOT NULL,
  `Fecha` datetime NOT NULL,
  `Total` decimal(12,2) DEFAULT NULL,
  `Estado` varchar(32) DEFAULT NULL,
  `IdCita` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `factura`
--

INSERT INTO `factura` (`Id`, `IdUsuario`, `Fecha`, `Total`, `Estado`, `IdCita`) VALUES
(1, 4, '2026-05-14 14:19:24', 150000.00, 'Generada', NULL),
(2, 4, '2026-05-14 18:37:55', 120000.00, 'Generada', 7),
(3, 4, '2026-05-16 15:06:17', 65000.00, 'Generada', 11);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `facturaitem`
--

CREATE TABLE `facturaitem` (
  `Id` int(11) NOT NULL,
  `FacturaId` int(11) NOT NULL,
  `Descripcion` varchar(255) DEFAULT NULL,
  `Precio` decimal(12,2) NOT NULL,
  `Cantidad` int(11) NOT NULL,
  `Subtotal` decimal(12,2) GENERATED ALWAYS AS (`Precio` * `Cantidad`) STORED
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `facturaitem`
--

INSERT INTO `facturaitem` (`Id`, `FacturaId`, `Descripcion`, `Precio`, `Cantidad`) VALUES
(1, 2, 'Consulta de Pediatria', 120000.00, 1),
(2, 3, 'Consulta de Cardiologia', 65000.00, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `historial`
--

CREATE TABLE `historial` (
  `Id` int(11) NOT NULL,
  `Descripcion` text NOT NULL,
  `Fecha` datetime NOT NULL DEFAULT current_timestamp(),
  `IdPaciente` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `historial_clinico`
--

CREATE TABLE `historial_clinico` (
  `Id` int(11) NOT NULL,
  `IdUsuario` int(11) NOT NULL,
  `FechaConsulta` datetime NOT NULL DEFAULT current_timestamp(),
  `Diagnostico` text NOT NULL,
  `Tratamiento` text DEFAULT NULL,
  `NotasAdicionales` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `historial_clinico`
--

INSERT INTO `historial_clinico` (`Id`, `IdUsuario`, `FechaConsulta`, `Diagnostico`, `Tratamiento`, `NotasAdicionales`) VALUES
(1, 4, '2026-05-15 00:00:00', 'El usuario presento una sarpuyido con fiebre que orino sangre como tambien tuvo un desmayo', 'Lo tuvimos que dejar en urgencias ya que el paciente juan david barrios perneth tuvieron que ramitirlo para que el hicieran tratamiento con medicamento fuerte', 'Es el dio acetaminofen, y otro medicamento');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `rol`
--

CREATE TABLE `rol` (
  `Id` int(11) NOT NULL,
  `TipoRol` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `rol`
--

INSERT INTO `rol` (`Id`, `TipoRol`) VALUES
(1, 'Administrador'),
(2, 'Doctor'),
(3, 'Paciente');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuario`
--

CREATE TABLE `usuario` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(200) NOT NULL,
  `Apellido` varchar(200) NOT NULL,
  `Correo` varchar(100) NOT NULL,
  `Direccion` varchar(50) NOT NULL,
  `Contrasena` varchar(255) NOT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT 1,
  `IdRol` int(11) NOT NULL,
  `Telefono` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `usuario`
--

INSERT INTO `usuario` (`Id`, `Nombre`, `Apellido`, `Correo`, `Direccion`, `Contrasena`, `Activo`, `IdRol`, `Telefono`) VALUES
(1, 'Carlos', 'Administrador', 'admin@gestion.com', 'Oficina principal', '$2a$11$HHORVQJEUp7SJalxaTkYs.3jZ.MYeyAiutSTxetKkSQCLUo04ZAW2', 1, 1, NULL),
(2, 'Dr. Juan', 'Perez', 'doctor@gestion.com', 'Consultorio 1', '$2a$11$fV6Hp/XdYR5aQbHnI4uCt.OM6PvitBxHLNtC//kktbp7HWLp9TWx.', 1, 2, NULL),
(3, 'María', 'García', 'paciente@gestion.com', 'Carrera 7 #45-12', '$2a$11$KQnSbGoQFPzCfOeVNFeCbOHBIjz1f0NA4jKXjTQGzXKzDcxBQ9K5.', 0, 3, NULL),
(4, 'Juan david', 'Barrios pernet', 'juandavidbarriosperneth@gmail.com', '', '$2a$11$TDu5BijaJVIKwmCoo4gzv.aBn31YlKSLmHZzWMV3IQUQQIyHUBRYu', 1, 3, NULL),
(5, 'Manuel Ramirez Rojas', '', '532fabian@gmail.com', '', '$2a$11$wle02X/BQ1luhHXUntAmO.tY0/jD6WRG4xi.m9RZuCCUp0h1x9uYe', 1, 3, NULL),
(6, 'Juan', 'Pérez', 'doctor@test.com', 'Calle 1', '$2a$11$m7UCAuU4nOBtXQOqnNmVuupj1zvHYrsyzRm8BupCWCHpIajO5GCyW', 1, 2, '3001234567');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `citas`
--
ALTER TABLE `citas`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IdUsuario` (`IdUsuario`),
  ADD KEY `IdEspecialidad` (`IdEspecialidad`);

--
-- Indices de la tabla `especialidadcatalogo`
--
ALTER TABLE `especialidadcatalogo`
  ADD PRIMARY KEY (`Id`);

--
-- Indices de la tabla `factura`
--
ALTER TABLE `factura`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `FK_Invoice_Usuario` (`IdUsuario`),
  ADD KEY `FK_Factura_Cita` (`IdCita`);

--
-- Indices de la tabla `facturaitem`
--
ALTER TABLE `facturaitem`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `FacturaId` (`FacturaId`);

--
-- Indices de la tabla `historial`
--
ALTER TABLE `historial`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IdPaciente` (`IdPaciente`);

--
-- Indices de la tabla `historial_clinico`
--
ALTER TABLE `historial_clinico`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IdUsuario` (`IdUsuario`);

--
-- Indices de la tabla `rol`
--
ALTER TABLE `rol`
  ADD PRIMARY KEY (`Id`);

--
-- Indices de la tabla `usuario`
--
ALTER TABLE `usuario`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IdRol` (`IdRol`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `citas`
--
ALTER TABLE `citas`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT de la tabla `factura`
--
ALTER TABLE `factura`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `facturaitem`
--
ALTER TABLE `facturaitem`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT de la tabla `historial`
--
ALTER TABLE `historial`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `historial_clinico`
--
ALTER TABLE `historial_clinico`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT de la tabla `usuario`
--
ALTER TABLE `usuario`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `citas`
--
ALTER TABLE `citas`
  ADD CONSTRAINT `citas_ibfk_1` FOREIGN KEY (`IdUsuario`) REFERENCES `usuario` (`Id`),
  ADD CONSTRAINT `citas_ibfk_2` FOREIGN KEY (`IdEspecialidad`) REFERENCES `especialidadcatalogo` (`Id`);

--
-- Filtros para la tabla `factura`
--
ALTER TABLE `factura`
  ADD CONSTRAINT `FK_Factura_Cita` FOREIGN KEY (`IdCita`) REFERENCES `citas` (`Id`),
  ADD CONSTRAINT `FK_Invoice_Usuario` FOREIGN KEY (`IdUsuario`) REFERENCES `usuario` (`Id`);

--
-- Filtros para la tabla `facturaitem`
--
ALTER TABLE `facturaitem`
  ADD CONSTRAINT `facturaitem_ibfk_1` FOREIGN KEY (`FacturaId`) REFERENCES `factura` (`Id`);

--
-- Filtros para la tabla `historial`
--
ALTER TABLE `historial`
  ADD CONSTRAINT `historial_ibfk_1` FOREIGN KEY (`IdPaciente`) REFERENCES `usuario` (`Id`);

--
-- Filtros para la tabla `historial_clinico`
--
ALTER TABLE `historial_clinico`
  ADD CONSTRAINT `historial_clinico_ibfk_1` FOREIGN KEY (`IdUsuario`) REFERENCES `usuario` (`Id`);

--
-- Filtros para la tabla `usuario`
--
ALTER TABLE `usuario`
  ADD CONSTRAINT `usuario_ibfk_1` FOREIGN KEY (`IdRol`) REFERENCES `rol` (`Id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
