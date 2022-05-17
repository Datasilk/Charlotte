using System;
using DasMulli.Win32.ServiceUtils;
using System.ServiceModel;
using Charlotte.Wcf;

namespace Charlotte
{
    internal class WinService : IWin32Service, IDisposable
    {
        private readonly string[] args;
        private ServiceHost host;
        private Browser browser;

        public WinService(string[] args)
        {
            this.args = args;
        }

        public string ServiceName => "Charlotte";

        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            string[] withArgs;
            if (startupArguments.Length > 0)
            {
                withArgs = new string[args.Length + startupArguments.Length];
                Array.Copy(args, withArgs, args.Length);
                Array.Copy(startupArguments, 0, withArgs, args.Length, startupArguments.Length);
            }
            else
            {
                withArgs = args;
            }

            //start web browser
            browser = new Browser();

            //start WCF TCP Service Host
            var baseAddress = "http://localhost:7077/Browser";
            host = new BrowserServiceHost(browser, new Uri(baseAddress));
            //host.AddServiceEndpoint(typeof(Browser), new BasicHttpBinding(), "");
            host.Open();
            Console.WriteLine("WCF host opened at " + baseAddress);
        }

        public void Stop()
        {
            if(host != null)
            {
                host.Close();
                browser.Dispose();
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
