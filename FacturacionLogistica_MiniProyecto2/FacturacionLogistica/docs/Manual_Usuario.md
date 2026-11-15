# Manual de usuario

## Inicio
Ejecutar `FacturacionLogistica.exe` o iniciar desde Visual Studio. `appsettings.json` debe estar junto al ejecutable; si falta, se informa el error.

## Idioma
Elegir Español o English en el ComboBox superior. Los textos y encabezados principales cambian al instante.

## Capturar y procesar
Pegar una factura por línea con cinco campos separados por punto y coma:
`PACK-001;Juan Perez;Zona A;12.5;150.0`

- Código: prefijo PACK- o COD-.
- Cliente: obligatorio.
- Zona: Zona A, Zona B o Zona C.
- Peso: mayor que cero.
- Valor declarado: cero o positivo.
- Usar punto decimal.

Pulsar **Procesar lote**. Los paquetes correctos aparecen ordenados por total descendente y se guardan en SQLite. Las líneas incorrectas se muestran con número y motivo; no cancelan las demás.

## Inspección
Seleccionar una fila para ver metadatos, subtotal, recargo, penalización, seguro, costo fijo, IVA y total.

## Historial y exportación
Pulsar **Ver historial** para consultar registros previos. Para exportar, pulsar **Exportar CSV**, elegir ruta y confirmar.

## Limpiar
El botón Limpiar vacía la captura y vista actual, pero no elimina el historial de la base de datos.

## Problemas frecuentes
- Falta JSON: copiar appsettings.json junto al EXE.
- Número inválido: usar punto decimal.
- Error de exportación: elegir carpeta con permisos de escritura.
- No abre en otro equipo: publicar para Windows x64; usar la publicación autocontenida.
