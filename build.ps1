<#
.SYNOPSIS
    Ejecuta los tests, compila MediaGrab y prepara en dist/ los archivos para GitHub Releases.

.DESCRIPTION
    Genera en dist/:
      MediaGrab-vX.Y.Z-win-x64.exe          Todo incluido: funciona sin instalar nada (recomendado).
      MediaGrab-vX.Y.Z-win-x64-ligero.exe   Mucho más pequeño; necesita .NET Desktop Runtime 10.
      SHA256SUMS.txt                        Hashes para verificar las descargas.
      RELEASE_NOTES.md                      Notas de la versión (de CHANGELOG.md) listas para pegar.

    Requiere el SDK de .NET 10 (https://dotnet.microsoft.com/download).

.EXAMPLE
    .\build.ps1

.EXAMPLE
    .\build.ps1 -SkipTests
#>
[CmdletBinding()]
param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$project = Join-Path $root 'src\MediaGrab\MediaGrab.csproj'
$solution = Join-Path $root 'MediaGrab.slnx'
$dist = Join-Path $root 'dist'
$runtime = 'win-x64'

function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($args[0]) falló (código $LASTEXITCODE)."
    }
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'No se encontró el SDK de .NET. Instala el SDK de .NET 10: https://dotnet.microsoft.com/download'
}

$version = ([xml](Get-Content $project -Raw)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
$tag = "v$version"
Write-Host "MediaGrab $tag" -ForegroundColor Cyan

# 1. Tests.
if (-not $SkipTests) {
    Write-Host 'Ejecutando los tests...' -ForegroundColor Cyan
    Invoke-DotNet test $solution -c Release --nologo --verbosity quiet
}

# 2. Publicar las dos variantes en una carpeta temporal.
$staging = Join-Path ([IO.Path]::GetTempPath()) ("MediaGrab-" + [guid]::NewGuid())
$variants = @(
    @{ Name = "MediaGrab-$tag-$runtime.exe"; SelfContained = 'true'; Extra = @('-p:EnableCompressionInSingleFile=true') },
    @{ Name = "MediaGrab-$tag-$runtime-ligero.exe"; SelfContained = 'false'; Extra = @() }
)

if (Test-Path $dist) {
    Remove-Item $dist -Recurse -Force
}
New-Item -ItemType Directory -Path $dist | Out-Null

try {
    foreach ($variant in $variants) {
        Write-Host "Publicando $($variant.Name)..." -ForegroundColor Cyan
        $output = Join-Path $staging $variant.Name
        # SelfContained como propiedad de MSBuild: "--self-contained false" no se aplica de forma fiable.
        Invoke-DotNet publish $project -c Release "-p:RuntimeIdentifier=$runtime" "-p:SelfContained=$($variant.SelfContained)" `
            -p:DebugType=None -p:DebugSymbols=false @($variant.Extra) -o $output --nologo --verbosity quiet
        Copy-Item (Join-Path $output 'MediaGrab.exe') (Join-Path $dist $variant.Name)
    }
}
finally {
    Remove-Item $staging -Recurse -Force -ErrorAction SilentlyContinue
}

# 3. Hashes.
$hashes = Get-ChildItem $dist -File | ForEach-Object {
    '{0}  {1}' -f (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), $_.Name
}
[IO.File]::WriteAllLines((Join-Path $dist 'SHA256SUMS.txt'), [string[]]$hashes)

# 4. Notas de la versión: la sección de CHANGELOG.md de esta versión, la tabla de descargas y los hashes.
$changelog = Get-Content (Join-Path $root 'CHANGELOG.md') -Encoding UTF8
$start = [array]::FindIndex([string[]]$changelog, [Predicate[string]] { param($line) $line -like "## *$version*" })
if ($start -ge 0) {
    $end = [array]::FindIndex([string[]]$changelog, $start + 1, [Predicate[string]] { param($line) $line -like '## *' })
    if ($end -lt 0) { $end = $changelog.Count }
    $section = (($changelog[($start + 1)..($end - 1)]) -join "`n").Trim() -split "`n"

    $notes = @("## MediaGrab $tag", '') + $section + @(
        '',
        '### Descarga',
        '| Archivo | Para quién |',
        '|---|---|',
        "| ``MediaGrab-$tag-$runtime.exe`` | **Recomendado.** Funciona en cualquier Windows 10/11 de 64 bits sin instalar nada. |",
        "| ``MediaGrab-$tag-$runtime-ligero.exe`` | Mucho más pequeño, pero necesita el [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0). |",
        '',
        'MediaGrab necesita también [yt-dlp](https://github.com/yt-dlp/yt-dlp); si no está instalado, la propia app ofrece instalarlo.',
        '',
        '> Windows SmartScreen puede avisar la primera vez porque el ejecutable no está firmado: pulsa *Más información* → *Ejecutar de todas formas*.',
        '',
        '### SHA-256',
        '```'
    ) + $hashes + @('```')

    [IO.File]::WriteAllLines((Join-Path $dist 'RELEASE_NOTES.md'), [string[]]$notes, (New-Object System.Text.UTF8Encoding($false)))
}
else {
    Write-Warning "CHANGELOG.md no tiene una sección para $version; no se generan notas de la versión."
}

Write-Host ''
Write-Host "MediaGrab $tag listo en $dist" -ForegroundColor Green
Get-ChildItem $dist -File | ForEach-Object { '  {0,-40} {1,10:N1} MB' -f $_.Name, ($_.Length / 1MB) }
