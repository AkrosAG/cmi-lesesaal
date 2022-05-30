using CMI.Manager.Lesesaal;
using Topshelf;

namespace CMI.Host.Lesesaal
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<LesesaalService>(s =>
                {
                    s.ConstructUsing(name => new LesesaalService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("The Lesesaal service allows Access to the Lesesaal database.");
                x.SetDisplayName("CMI Lesesaal Service");
                x.SetServiceName("CMIViaducService");
            });
        }
    }
}