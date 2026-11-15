# Manual técnico

## Arquitectura
- `Domain/Modelos.cs`: entidades y modelos fuertemente tipados.
- `Services/Servicios.cs`: configuración JSON, parser, cálculo, procesamiento, CSV e idioma.
- `Persistence/PaqueteRepository.cs`: persistencia SQLite.
- `UI/FormPrincipal.cs`: controles WinForms y eventos.

La UI recibe acciones y presenta resultados. Los servicios validan y calculan. El repositorio persiste. CsvService exporta.

## Flujo
1. Program carga appsettings.json.
2. ConfiguracionService crea Configuracion.
3. Repositorio inicializa la tabla SQLite.
4. Formulario recibe el lote.
5. PaqueteParser separa líneas/campos, limpia y valida; guarda errores aparte.
6. PaqueteService procesa en arreglo inicial de cinco y duplica capacidad al llenarse.
7. CalculoService aplica fórmulas.
8. PaqueteService ordena con burbuja descendente.
9. Repositorio guarda el lote en transacción.
10. CsvService exporta con `using`.

## Algoritmos
Parseo: Split por líneas y `;`, Trim, ToUpperInvariant, regex de código, TryParse con InvariantCulture. Los errores de una línea no detienen el lote.
Arreglo: `Paquete[5]`; al llenarse, nuevo arreglo al doble y copia manual.
Ordenamiento: burbuja descendente comparando `Desglose.Total`.
Cálculo: zona A 5%, B 10%, C 18%; exceso de 20 kg al 25% del precio base por kg; seguro 2% si valor > $1,000; costo fijo e IVA.

## Base de datos
SQLite mediante Microsoft.Data.Sqlite. El repositorio crea tabla, inserta lote en transacción y consulta historial.

## Publicar
En Windows con .NET 8:
```powershell
dotnet restore
dotnet build .\FacturacionLogistica.sln -c Release
dotnet publish .\src\FacturacionLogistica\FacturacionLogistica.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

## Mantenimiento
Tarifas: appsettings.json. Validación: PaqueteParser. Cálculos: CalculoService. CSV: CsvService. Idiomas: IdiomaService. BD: PaqueteRepository. UI: FormPrincipal.

## GitHub
Crear repositorio y subir el proyecto. Clonar:
`git clone https://github.com/USUARIO/REPOSITORIO.git`
Sustituir URL de ejemplo por la URL real. No subir datos reales de clientes ni credenciales.
