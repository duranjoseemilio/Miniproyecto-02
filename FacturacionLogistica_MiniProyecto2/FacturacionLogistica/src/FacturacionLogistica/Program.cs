using FacturacionLogistica.Persistence;
using FacturacionLogistica.Services;
using FacturacionLogistica.UI;
namespace FacturacionLogistica;
internal static class Program
{
 [STAThread] static void Main()
 {
  ApplicationConfiguration.Initialize();
  try {
   var cfg=new ConfiguracionService().Cargar(Path.Combine(AppContext.BaseDirectory,"appsettings.json"));
   var repo=new PaqueteRepository(Path.Combine(AppContext.BaseDirectory,cfg.ArchivoBaseDatos));
   repo.Inicializar();
   Application.Run(new FormPrincipal(cfg,new PaqueteService(new CalculoService(),repo)));
  } catch(Exception ex) { MessageBox.Show("No fue posible iniciar: "+ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error); }
 }
}
