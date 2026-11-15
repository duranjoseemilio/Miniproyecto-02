# Entregables 1 a 4

## 1. Respaldo del repositorio
Este ZIP contiene solución, código, configuración, README y documentación. Tras subir a GitHub, descargar también el ZIP oficial si se solicita.

## 2. Instalación
En Windows ejecutar:
```powershell
dotnet publish .\src\FacturacionLogistica\FacturacionLogistica.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```
Comprimir `publish`. Es una aplicación portable autocontenida, no un MSI tradicional.

## 3. Documentación
Se incluyen manual de usuario, manual técnico y README. Agregar URL real de GitHub y convertir a DOCX/PDF si el aula virtual no acepta Markdown.

## 4. Video
Se incluye guion. El video real debe grabarlo el equipo y durar máximo cinco minutos. Si se comparte por liga, crear PDF con la URL final.

## Lista de verificación
- [ ] Compila en Windows con .NET 8.
- [ ] JSON ausente/inválido se reporta.
- [ ] Lote mixto procesa válidos y rechaza inválidos.
- [ ] Usa arreglos, redimensionamiento manual y burbuja.
- [ ] Calcula zona, penalización, seguro e IVA.
- [ ] Cambia idioma y encabezados.
- [ ] Selección de fila muestra desglose.
- [ ] Exporta CSV.
- [ ] Guarda y consulta historial SQLite.
- [ ] Publica Windows x64.
- [ ] Se sube a GitHub y se sustituye URL de ejemplo.
- [ ] Se graba video de máximo cinco minutos.
