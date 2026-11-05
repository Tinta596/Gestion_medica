param(
    [Parameter(Mandatory = $true)]
    [string]$ConfigPath
)

$resolvedPath = (Resolve-Path -LiteralPath $ConfigPath).Path
Add-Type -AssemblyName System.Configuration
$map = New-Object System.Configuration.ExeConfigurationFileMap
$map.ExeConfigFilename = $resolvedPath
$config = [System.Configuration.ConfigurationManager]::OpenMappedExeConfiguration(
    $map,
    [System.Configuration.ConfigurationUserLevel]::None)

$connection = $config.ConnectionStrings.ConnectionStrings["GestionMedica"]
$settings = $config.AppSettings.Settings

if ($null -eq $connection -or
    $connection.ConnectionString -notmatch "(?i)(user\s*id|uid)\s*=\s*(?!root\b)[^;]+" -or
    $connection.ConnectionString -notmatch "(?i)(password|pwd)\s*=\s*[^;]+") {
    throw "Configura GestionMedica con un usuario MySQL distinto de root y una contraseña antes de proteger el archivo."
}

foreach ($key in @("Smtp.User", "Smtp.Password", "Smtp.Host", "Smtp.Port")) {
    if ($null -eq $settings[$key] -or [string]::IsNullOrWhiteSpace($settings[$key].Value)) {
        throw "Falta configurar el valor requerido '$key' en appSettings."
    }
}

foreach ($sectionName in @("connectionStrings", "appSettings")) {
    $section = $config.GetSection($sectionName)
    if (-not $section.SectionInformation.IsProtected) {
        $section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider")
        $section.SectionInformation.ForceSave = $true
    }
}

$config.Save([System.Configuration.ConfigurationSaveMode]::Modified)
Write-Output "Secciones connectionStrings y appSettings protegidas con DPAPI en: $resolvedPath"
