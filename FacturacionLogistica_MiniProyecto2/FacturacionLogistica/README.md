# Sistema de Procesamiento y Auditoría de Facturación
Aplicación C# WinForms para validar lotes, calcular tarifas, auditar, guardar historial SQLite y exportar CSV.

## Requisitos
Windows 10/11, Visual Studio 2022 con carga de trabajo Desarrollo de escritorio de .NET, .NET 8 SDK y acceso a NuGet.

## Ejecutar
Abrir `FacturacionLogistica.sln`, restaurar paquetes NuGet y ejecutar con F5.
Terminal:
```powershell
dotnet restore
dotnet build .\FacturacionLogistica.sln
dotnet run --project .\src\FacturacionLogistica\FacturacionLogistica.csproj
```

## Formato
Una línea por paquete: `PACK-001;Juan Perez;Zona A;12.5;150.0`
Campos: código, cliente, zona, peso kg y valor declarado. Códigos PACK- o COD-. Zonas A/B/C. Decimales con punto.

## Reglas
Subtotal = peso × precio base/kg. Recargo zona: A 5%, B 10%, C 18% del subtotal. Penalización = kg que exceden 20 × precio base × 25%. Seguro = 2% del valor declarado cuando supera $1,000. Se suma costo fijo. IVA se aplica a subtotal + recargos + penalización + seguro + costo fijo.

**Pendiente de confirmar con el docente:** base gravable exacta del IVA. La fórmula anterior es el criterio implementado. Los importes del JSON son demostrativos.

## Publicación Windows x64
```powershell
dotnet publish .\src\FacturacionLogistica\FacturacionLogistica.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```
Comprimir `publish` para obtener la aplicación portable autocontenida. No es un instalador MSI tradicional.

## GitHub
`git clone https://github.com/USUARIO/REPOSITORIO.git` (reemplazar por URL real).
