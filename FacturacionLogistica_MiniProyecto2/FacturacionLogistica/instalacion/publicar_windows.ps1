$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "..\src\FacturacionLogistica\FacturacionLogistica.csproj"
$out = Join-Path $PSScriptRoot "publish"
if (Test-Path $out) { Remove-Item $out -Recurse -Force }
dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $out
Write-Host "Publicación terminada en: $out"
Write-Host "Comprime el contenido de esa carpeta para distribuir la aplicación."
