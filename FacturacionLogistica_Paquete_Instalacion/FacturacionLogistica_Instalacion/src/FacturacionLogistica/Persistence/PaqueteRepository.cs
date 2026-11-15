using Microsoft.Data.Sqlite;
using FacturacionLogistica.Domain;
namespace FacturacionLogistica.Persistence;
public sealed class PaqueteRepository
{
 readonly string _cs;
 public PaqueteRepository(string path){string full=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(full)!);_cs=new SqliteConnectionStringBuilder{DataSource=full}.ToString();}
 public void Inicializar(){
  using var c=new SqliteConnection(_cs);c.Open();using var q=c.CreateCommand();
  q.CommandText="CREATE TABLE IF NOT EXISTS Paquetes(Id INTEGER PRIMARY KEY AUTOINCREMENT,Codigo TEXT NOT NULL,Cliente TEXT NOT NULL,Zona TEXT NOT NULL,PesoKg REAL NOT NULL,ValorDeclarado REAL NOT NULL,Subtotal REAL NOT NULL,Recargo REAL NOT NULL,Penalizacion REAL NOT NULL,Seguro REAL NOT NULL,CostoFijo REAL NOT NULL,BaseIva REAL NOT NULL,Iva REAL NOT NULL,Total REAL NOT NULL,Fecha TEXT NOT NULL);";
  q.ExecuteNonQuery();
 }
 public void GuardarLote(Paquete[] ps){
  using var c=new SqliteConnection(_cs);c.Open();using var tx=c.BeginTransaction();
  try{foreach(var p in ps){using var q=c.CreateCommand();q.Transaction=tx;
   q.CommandText="INSERT INTO Paquetes(Codigo,Cliente,Zona,PesoKg,ValorDeclarado,Subtotal,Recargo,Penalizacion,Seguro,CostoFijo,BaseIva,Iva,Total,Fecha) VALUES($c,$n,$z,$p,$v,$s,$r,$x,$g,$f,$b,$i,$t,$d);";
   q.Parameters.AddWithValue("$c",p.Codigo);q.Parameters.AddWithValue("$n",p.Cliente);q.Parameters.AddWithValue("$z",p.Zona);q.Parameters.AddWithValue("$p",p.PesoKg);q.Parameters.AddWithValue("$v",p.ValorDeclarado);q.Parameters.AddWithValue("$s",p.Desglose.SubtotalPeso);q.Parameters.AddWithValue("$r",p.Desglose.RecargoZona);q.Parameters.AddWithValue("$x",p.Desglose.PenalizacionExcesoPeso);q.Parameters.AddWithValue("$g",p.Desglose.Seguro);q.Parameters.AddWithValue("$f",p.Desglose.CostoFijo);q.Parameters.AddWithValue("$b",p.Desglose.BaseAntesIva);q.Parameters.AddWithValue("$i",p.Desglose.Iva);q.Parameters.AddWithValue("$t",p.Desglose.Total);q.Parameters.AddWithValue("$d",p.FechaProcesamiento.ToString("O"));q.ExecuteNonQuery();
  }tx.Commit();}catch{tx.Rollback();throw;}
 }
 public IReadOnlyList<Paquete> ObtenerTodos(){
  var list=new List<Paquete>();using var c=new SqliteConnection(_cs);c.Open();using var q=c.CreateCommand();
  q.CommandText="SELECT Id,Codigo,Cliente,Zona,PesoKg,ValorDeclarado,Subtotal,Recargo,Penalizacion,Seguro,CostoFijo,BaseIva,Iva,Total,Fecha FROM Paquetes ORDER BY Id DESC";
  using var r=q.ExecuteReader();while(r.Read()){
   list.Add(new Paquete{Id=r.GetInt32(0),Codigo=r.GetString(1),Cliente=r.GetString(2),Zona=r.GetString(3),PesoKg=Convert.ToDecimal(r.GetDouble(4)),ValorDeclarado=Convert.ToDecimal(r.GetDouble(5)),
    Desglose=new DesgloseFinanciero{SubtotalPeso=Convert.ToDecimal(r.GetDouble(6)),RecargoZona=Convert.ToDecimal(r.GetDouble(7)),PenalizacionExcesoPeso=Convert.ToDecimal(r.GetDouble(8)),Seguro=Convert.ToDecimal(r.GetDouble(9)),CostoFijo=Convert.ToDecimal(r.GetDouble(10)),BaseAntesIva=Convert.ToDecimal(r.GetDouble(11)),Iva=Convert.ToDecimal(r.GetDouble(12)),Total=Convert.ToDecimal(r.GetDouble(13))},
    FechaProcesamiento=DateTime.Parse(r.GetString(14))});
  }return list;
 }
}
