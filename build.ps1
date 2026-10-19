<#
.SYNOPSIS
    Compila MediaGrab y prepara en dist/ los archivos para subir a GitHub Releases.

.DESCRIPTION
    Genera en dist/:
      MediaGrab.exe               Ejecutable suelto.
      MediaGrab-vX.Y.Z.zip        Ejecutable + .config + licencia (descarga recomendada).
      SHA256SUMS.txt              Hashes para verificar las descargas.
      RELEASE_NOTES.md            Notas de la versión (de CHANGELOG.md) listas para pegar.

.EXAMPLE
    .\build.ps1
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$project = Join-Path $root 'src\MediaGrab\MediaGrab.csproj'
$output = Join-Path $root "src\MediaGrab\bin\$Configuration"
$dist = Join-Path $root 'dist'

function Find-MSBuild {
    $inPath = Get-Command 'msbuild.exe' -ErrorAction SilentlyContinue
    if ($inPath) {
        return $inPath.Source
    }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vswhere) {
        $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' |
            Select-Object -First 1
        if ($found) {
            return $found
        }
    }

    throw 'No se encontró MSBuild. Instala Visual Studio o "Build Tools for Visual Studio" con el workload de .NET de escritorio.'
}

# 1. Compilar.
$msbuild = Find-MSBuild
Write-Host "Compilando ($Configuration) con $msbuild" -ForegroundColor Cyan
& $msbuild $project -t:Rebuild "-p:Configuration=$Configuration" -nologo -verbosity:minimal
if ($LASTEXITCODE -ne 0) {
    throw "La compilación falló (código $LASTEXITCODE)."
}

$exe = Join-Path $output 'MediaGrab.exe'
$version = [version](Get-Item $exe).VersionInfo.FileVersion
$tag = 'v{0}.{1}.{2}' -f $version.Major, $version.Minor, $version.Build

# 2. Empaquetar.
if (Test-Path $dist) {
    Remove-Item $dist -Recurse -Force
}
New-Item -ItemType Directory -Path $dist | Out-Null

Copy-Item $exe $dist

$staging = Join-Path ([IO.Path]::GetTempPath()) ("MediaGrab-" + [guid]::NewGuid())
$packageDir = Join-Path $staging 'MediaGrab'
New-Item -ItemType Directory -Path $packageDir | Out-Null
Copy-Item $exe, "$exe.config", (Join-Path $root 'LICENSE') $packageDir

$zip = Join-Path $dist "MediaGrab-$tag.zip"
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($packageDir, $zip, [IO.Compression.CompressionLevel]::Optimal, $true)
Remove-Item $staging -Recurse -Force

# 3. Hashes.
$hashes = Get-ChildItem $dist -File | ForEach-Object {
    '{0}  {1}' -f (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), $_.Name
}
[IO.File]::WriteAllLines((Join-Path $dist 'SHA256SUMS.txt'), [string[]]$hashes)

# 4. Notas de la versión: la sección de CHANGELOG.md correspondiente a esta versión.
$changelog = Get-Content (Join-Path $root 'CHANGELOG.md') -Encoding UTF8
$start = [array]::FindIndex([string[]]$changelog, [Predicate[string]] { param($line) $line -like "## *$($tag.TrimStart('v'))*" })
if ($start -ge 0) {
    $end = [array]::FindIndex([string[]]$changelog, $start + 1, [Predicate[string]] { param($line) $line -like '## *' })
    if ($end -lt 0) { $end = $changelog.Count }
    $section = (($changelog[($start + 1)..($end - 1)]) -join "`n").Trim() -split "`n"
    $notes = @("## MediaGrab $tag", '') + $section +
        @('', '### SHA-256', '```') + $hashes + @('```')
    [IO.File]::WriteAllLines((Join-Path $dist 'RELEASE_NOTES.md'), [string[]]$notes, (New-Object System.Text.UTF8Encoding($false)))
}
else {
    Write-Warning "CHANGELOG.md no tiene una sección para $tag; no se generan notas de la versión."
}

Write-Host ''
Write-Host "MediaGrab $tag listo en $dist" -ForegroundColor Green
Get-ChildItem $dist -File | ForEach-Object { '  {0,-24} {1,8:N0} KB' -f $_.Name, ($_.Length / 1KB) }
