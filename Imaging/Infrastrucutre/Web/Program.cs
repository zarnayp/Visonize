
using DupploPulse.UsImaging.Infrastructure.CompositionRoot;

namespace DupploPulse.UsImaging.Infrastructure.Web
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string executableDirectory = AppDomain.CurrentDomain.BaseDirectory; Directory.SetCurrentDirectory(executableDirectory);
            Directory.SetCurrentDirectory(executableDirectory);

            var composer = new Composer();

            WebApp webApp = new WebApp(composer.RenderingService); 
        }
    }
}