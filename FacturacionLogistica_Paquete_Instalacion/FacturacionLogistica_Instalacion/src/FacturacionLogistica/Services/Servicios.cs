using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FacturacionLogistica.Domain;
using FacturacionLogistica.Persistence;
namespace FacturacionLogistica.Services;

public sealed class ConfiguracionService
{
 public Configuracion Cargar(string ruta)
 {
  if(!File.Exists(ruta))throw new FileNotFoundException("No se encontró appsettings.json: "+ruta);
  try {
   using var doc=JsonDocument.Parse(File.ReadAllText(ruta)); var t=doc.RootElement.GetProperty("Tarifas");
   var c=new Configuracion{PrecioBaseKg=t.GetProperty("PrecioBaseKg").GetDecimal(),TasaIva=t.GetProperty("TasaIva").GetDecimal(),CostoFijoEnvio=t.GetProperty("CostoFijoEnvio").GetDecimal()};
   if(doc.RootElement.TryGetProperty("BaseDatos",out var db)&&db.TryGetProperty("Archivo",out var f))c.ArchivoBaseDatos=f.GetString()??"facturacion.db";
   return c;
  }catch(Exception ex)when(ex is JsonException or KeyNotFoundException or InvalidOperationException or ArgumentOutOfRangeException){throw new InvalidDataException("appsettings.json tiene estructura o valores inválidos: "+ex.Message,ex);}
 }
}
public sealed class PaqueteParser
{
 static readonly Regex Codigo=new(@"^(COD|PACK)-[A-Z0-9-]{2,20}$",RegexOptions.IgnoreCase|RegexOptions.Compiled);
 public (Paquete[] Paquetes,ErrorRegistro[] Errores) Parsear(string lote)
 {
  Paquete[] validos=new Paquete[5]; ErrorRegistro[] errores=new ErrorRegistro[5];int nv=0,ne=0,nlinea=0;
  string[] lineas=(lote??"").Replace("\r\n","\n").Replace('\r','\n').Split('\n');
  foreach(string original in lineas){
   nlinea++;string linea=original.Trim();
   if(linea.Length==0){AddError(ref errores,ref ne,new(){NumeroLinea=nlinea,TextoOriginal=original,Motivo="Línea vacía."});continue;}
   try{
    string[] c=linea.Split(';');if(c.Length!=5)throw new FormatException($"Se esperaban 5 campos y llegaron {c.Length}.");
    for(int i=0;i<c.Length;i++)c[i]=c[i].Trim();
    string codigo=c[0].ToUpperInvariant(),zona=c[2].Replace("  "," ").ToUpperInvariant();
    if(!Codigo.IsMatch(codigo))throw new FormatException("Código inválido: use PACK-xxx o COD-xxx.");
    if(string.IsNullOrWhiteSpace(c[1]))throw new FormatException("Cliente vacío.");
    if(zona is not ("ZONA A" or "ZONA B" or "ZONA C"))throw new FormatException("Zona inválida: use Zona A, Zona B o Zona C.");
    if(!decimal.TryParse(c[3],NumberStyles.Number,CultureInfo.InvariantCulture,out decimal peso))throw new FormatException("Peso no numérico; use punto decimal.");
    if(!decimal.TryParse(c[4],NumberStyles.Number,CultureInfo.InvariantCulture,out decimal valor))throw new FormatException("Valor declarado no numérico; use punto decimal.");
    if(peso<=0)throw new FormatException("El peso debe ser mayor que cero.");if(valor<0)throw new FormatException("Valor declarado negativo.");
    AddPaquete(ref validos,ref nv,new(){Codigo=codigo,Cliente=c[1],Zona=zona,PesoKg=peso,ValorDeclarado=valor});
   }catch(Exception ex)when(ex is FormatException or ArgumentException or OverflowException){AddError(ref errores,ref ne,new(){NumeroLinea=nlinea,TextoOriginal=original,Motivo=ex.Message});}
  }
  Paquete[] p=new Paquete[nv];ErrorRegistro[] e=new ErrorRegistro[ne];for(int i=0;i<nv;i++)p[i]=validos[i];for(int i=0;i<ne;i++)e[i]=errores[i];return(p,e);
 }
 static void AddPaquete(ref Paquete[] a,ref int n,Paquete p){if(n==a.Length){var b=new Paquete[a.Length*2];for(int i=0;i<a.Length;i++)b[i]=a[i];a=b;}a[n++]=p;}
 static void AddError(ref ErrorRegistro[] a,ref int n,ErrorRegistro e){if(n==a.Length){var b=new ErrorRegistro[a.Length*2];for(int i=0;i<a.Length;i++)b[i]=a[i];a=b;}a[n++]=e;}
}
public sealed class CalculoService
{
 public DesgloseFinanciero Calcular(Paquete p,Configuracion c){
  decimal subtotal=p.PesoKg*c.PrecioBaseKg;
  decimal tasa=p.Zona switch{"ZONA A"=>.05m,"ZONA B"=>.10m,"ZONA C"=>.18m,_=>throw new InvalidOperationException("Zona no válida.")};
  decimal recargo=subtotal*tasa,extra=p.PesoKg>20?p.PesoKg-20:0;
  decimal penalizacion=extra*c.PrecioBaseKg*.25m,seguro=p.ValorDeclarado>1000?p.ValorDeclarado*.02m:0;
  decimal baseIva=subtotal+recargo+penalizacion+seguro+c.CostoFijoEnvio,iva=baseIva*c.TasaIva;
  decimal R(decimal x)=>Math.Round(x,2,MidpointRounding.AwayFromZero);
  return new(){SubtotalPeso=R(subtotal),RecargoZona=R(recargo),PenalizacionExcesoPeso=R(penalizacion),Seguro=R(seguro),CostoFijo=R(c.CostoFijoEnvio),BaseAntesIva=R(baseIva),Iva=R(iva),Total=R(baseIva+iva)};
 }
}
public sealed class PaqueteService
{
 readonly CalculoService _calculo;readonly PaqueteRepository _repo;readonly PaqueteParser _parser=new();
 public PaqueteService(CalculoService c,PaqueteRepository r){_calculo=c;_repo=r;}
 public ResultadoLote Procesar(string texto,Configuracion cfg){
  var parsed=_parser.Parsear(texto);Paquete[] a=new Paquete[5];int n=0;
  foreach(var p in parsed.Paquetes){if(n==a.Length){var b=new Paquete[a.Length*2];for(int i=0;i<a.Length;i++)b[i]=a[i];a=b;}p.Desglose=_calculo.Calcular(p,cfg);a[n++]=p;}
  for(int i=0;i<n-1;i++)for(int j=0;j<n-i-1;j++)if(a[j].Desglose.Total<a[j+1].Desglose.Total){var t=a[j];a[j]=a[j+1];a[j+1]=t;}
  Paquete[] salida=new Paquete[n];for(int i=0;i<n;i++)salida[i]=a[i];if(n>0)_repo.GuardarLote(salida);
  return new(){Paquetes=salida,Errores=parsed.Errores};
 }
 public IReadOnlyList<Paquete> Historial()=>_repo.ObtenerTodos();
}
public sealed class CsvService
{
 public void Exportar(string ruta,Paquete[] ps){
  using var w=new StreamWriter(ruta,false,new UTF8Encoding(true));
  w.WriteLine("Codigo,Cliente,Zona,PesoKg,ValorDeclarado,Subtotal,RecargoZona,Penalizacion,Seguro,CostoFijo,BaseAntesIva,IVA,Total,Fecha");
  foreach(var p in ps){string[] c={p.Codigo,p.Cliente,p.Zona,F(p.PesoKg),F(p.ValorDeclarado),F(p.Desglose.SubtotalPeso),F(p.Desglose.RecargoZona),F(p.Desglose.PenalizacionExcesoPeso),F(p.Desglose.Seguro),F(p.Desglose.CostoFijo),F(p.Desglose.BaseAntesIva),F(p.Desglose.Iva),F(p.Desglose.Total),p.FechaProcesamiento.ToString("O")};w.WriteLine(string.Join(",",c.Select(E)));}
 }
 static string F(decimal d)=>d.ToString("0.00",CultureInfo.InvariantCulture);
 static string E(string s)=>s.Contains(',')||s.Contains('"')||s.Contains('\n')||s.Contains('\r')?"\""+s.Replace("\"","\"\"")+"\"":s;
}
public sealed class IdiomaService
{
 readonly Dictionary<string,string> es=new(){{"title","Sistema de Facturación Logística"},{"input","Lote (una factura por línea):"},{"process","Procesar lote"},{"clear","Limpiar"},{"export","Exportar CSV"},{"history","Ver historial"},{"detail","Detalle del paquete"},{"total","Total del lote:"},{"code","Código"},{"client","Cliente"},{"zone","Zona"},{"weight","Peso (kg)"},{"declared","Valor declarado"},{"subtotal","Subtotal"},{"fee","Recargo zona"},{"penalty","Penalización"},{"insurance","Seguro"},{"fixed","Costo fijo"},{"tax","IVA"},{"grand","Total"},{"empty","Escribe al menos un registro."},{"done","Lote procesado."},{"nodata","No hay paquetes para exportar."},{"exported","CSV exportado correctamente."},{"errors","Errores de validación"},{"processError","No se pudo procesar el lote"},{"exportError","No se pudo exportar el CSV"},{"historyError","No se pudo consultar el historial"},{"valid","Válidos"},{"invalid","Inválidos"},{"language","Idioma:"},{"validTab","Paquetes procesados"},{"errorsTab","Errores de validación"},{"line","Línea"},{"original","Registro original"},{"reason","Motivo del error"},{"invalidFound","registros inválidos"},{"errorsTabHint","Consulta la pestaña Errores de validación para identificar y corregir cada registro."},{"records","Registros"}};
 readonly Dictionary<string,string> en=new(){{"title","Logistics Billing System"},{"input","Batch (one invoice per line):"},{"process","Process batch"},{"clear","Clear"},{"export","Export CSV"},{"history","View history"},{"detail","Package details"},{"total","Batch total:"},{"code","Code"},{"client","Customer"},{"zone","Zone"},{"weight","Weight (kg)"},{"declared","Declared value"},{"subtotal","Subtotal"},{"fee","Zone surcharge"},{"penalty","Overweight penalty"},{"insurance","Insurance"},{"fixed","Fixed cost"},{"tax","VAT"},{"grand","Total"},{"empty","Enter at least one record."},{"done","Batch processed."},{"nodata","There are no packages to export."},{"exported","CSV exported successfully."},{"errors","Validation errors"},{"processError","The batch could not be processed"},{"exportError","The CSV could not be exported"},{"historyError","The history could not be loaded"},{"valid","Valid"},{"invalid","Invalid"},{"language","Language:"},{"validTab","Processed packages"},{"errorsTab","Validation errors"},{"line","Line"},{"original","Original record"},{"reason","Error reason"},{"invalidFound","invalid records"},{"errorsTabHint","Open the Validation errors tab to identify and correct each record."},{"records","Records"}};
 public string T(string k,bool english)=>(english?en:es).GetValueOrDefault(k,k);
}
