-- Reemplaza el valor de contraseña por una clave aleatoria fuerte antes de ejecutar.
-- Ejecuta este script con una cuenta administradora de MariaDB/MySQL.
CREATE USER IF NOT EXISTS 'gestion_app'@'localhost'
  IDENTIFIED BY 'REEMPLAZAR_POR_UNA_CLAVE_ALEATORIA';

GRANT SELECT, INSERT, UPDATE
  ON `gestion_citas_bd`.*
  TO 'gestion_app'@'localhost';

FLUSH PRIVILEGES;
