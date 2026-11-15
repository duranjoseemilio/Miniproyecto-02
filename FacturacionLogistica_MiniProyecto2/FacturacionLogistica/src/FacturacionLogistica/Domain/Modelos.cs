namespace FacturacionLogistica.Domain;
public sealed class Configuracion
{
 private decimal _precioBaseKg,_tasaIva,_costoFijoEnvio; private string _archivoBaseDatos="facturacion.db";
 public decimal PrecioBaseKg {get=>_precioBaseKg;set=>_precioBaseKg=value>=0?value:throw new ArgumentOutOfRangeException();}
 public decimal TasaIva {get=>_tasaIva;set=>_tasaIva=value is >=0 and <=1?value:throw new ArgumentOutOfRangeException();}
 public decimal CostoFijoEnvio {get=>_costoFijoEnvio;set=>_costoFijoEnvio=value>=0?value:throw new ArgumentOutOfRangeException();}
 public string ArchivoBaseDatos {get=>_archivoBaseDatos;set=>_archivoBaseDatos=string.IsNullOrWhiteSpace(value)?"facturacion.db":value.Trim();}
}
public sealed class DesgloseFinanciero
{
 public decimal SubtotalPeso {get;init;} public decimal RecargoZona {get;init;} public decimal PenalizacionExcesoPeso {get;init;}
 public decimal Seguro {get;init;} public decimal CostoFijo {get;init;} public decimal BaseAntesIva {get;init;} public decimal Iva {get;init;} public decimal Total {get;init;}
}
public sealed class Paquete
{
 private string _codigo="",_cliente="",_zona=""; private decimal _pesoKg,_valorDeclarado;
 public int Id {get;set;}
 public string Codigo {get=>_codigo;set=>_codigo=string.IsNullOrWhiteSpace(value)?throw new ArgumentException("Código obligatorio"):value.Trim().ToUpperInvariant();}
 public string Cliente {get=>_cliente;set=>_cliente=string.IsNullOrWhiteSpace(value)?throw new ArgumentException("Cliente obligatorio"):value.Trim();}
 public string Zona {get=>_zona;set=>_zona=string.IsNullOrWhiteSpace(value)?throw new ArgumentException("Zona obligatoria"):value.Trim().ToUpperInvariant();}
 public decimal PesoKg {get=>_pesoKg;set=>_pesoKg=value>0?value:throw new ArgumentOutOfRangeException(nameof(value),"El peso debe ser mayor que cero.");}
 public decimal ValorDeclarado {get=>_valorDeclarado;set=>_valorDeclarado=value>=0?value:throw new ArgumentOutOfRangeException(nameof(value));}
 public DateTime FechaProcesamiento {get;set;}=DateTime.Now; public DesgloseFinanciero Desglose {get;set;}=new();
}
public sealed class ErrorRegistro {public int NumeroLinea {get;init;} public string TextoOriginal {get;init;}=""; public string Motivo {get;init;}="";}
public sealed class ResultadoLote {public Paquete[] Paquetes {get;init;}=Array.Empty<Paquete>(); public ErrorRegistro[] Errores {get;init;}=Array.Empty<ErrorRegistro>(); public decimal TotalLote=>Paquetes.Sum(p=>p.Desglose.Total);}
