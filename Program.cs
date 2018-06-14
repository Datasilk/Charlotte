using System;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DasMulli.Win32.ServiceUtils;

namespace Charlotte
{
    class Program
    {
        private const string RunAsServiceFlag = "-start";
        private const string RegisterServiceFlag = "-register";
        private const string UnregisterServiceFlag = "-unregister";

        private const string ServiceName = "Charlotte";
        private const string ServiceDisplayName = "Charlotte";
        private const string ServiceDescription = "An offscreen Chromium web browser used to collect computed DOM elements for web pages";

        static void Main(string[] args)
        {
            //parse arguments
            if (args.Contains(RunAsServiceFlag))
            {
                RunAsService(args);
            }
            else if (args.Contains(RegisterServiceFlag))
            {
                RegisterService();
            }
            else if (args.Contains(UnregisterServiceFlag))
            {
                UnregisterService();
            }
            else if(args.Contains("-help"))
            {
                DisplayHelp();
            }
            else
            {
                var winService = new WinService(new string[] { });
                winService.Start(new string[] { }, null);
                Console.WriteLine("Press any key to stop hosting...");
                Console.ReadKey();
                winService.Stop();
            }
        }

        private static void RunAsService(string[] args)
        {
            var winService = new WinService(args.Where(a => a != RunAsServiceFlag).ToArray());
            var serviceHost = new Win32ServiceHost(winService);
            serviceHost.Run();
        }

        private static void RegisterService()
        {
            var remainingArgs = Environment.GetCommandLineArgs()
                .Where(arg => arg != RegisterServiceFlag)
                .Select(EscapeArgs).ToList();
            remainingArgs.Add(RunAsServiceFlag);

            var host = Process.GetCurrentProcess().MainModule.FileName;

            if (!host.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
            {
                // For self-contained apps, skip the dll path
                remainingArgs = remainingArgs.Skip(1).ToList();
            }

            var fullServiceCommand = host + " " + string.Join(" ", remainingArgs);

            // Do not use LocalSystem in production, but this is good for demos as LocalSystem will have access to some random git-clone path
            // Note that when the service is already registered and running, it will be reconfigured but not restarted
            var serviceDefinition = new ServiceDefinitionBuilder(ServiceName)
                .WithDisplayName(ServiceDisplayName)
                .WithDescription(ServiceDescription)
                .WithBinaryPath(fullServiceCommand)
                .WithCredentials(Win32ServiceCredentials.LocalSystem)
                .WithAutoStart(true)
                .Build();

            new Win32ServiceManager().CreateOrUpdateService(serviceDefinition, startImmediately: true);

            Console.WriteLine($@"Successfully registered and started service ""{ServiceDisplayName}"" (""{ServiceDescription}"")");
        }

        private static void UnregisterService()
        {
            new Win32ServiceManager()
                .DeleteService(ServiceName);

            Console.WriteLine($@"Successfully unregistered service ""{ServiceDisplayName}"" (""{ServiceDescription}"")");
        }

        private static void DisplayHelp()
        {
            Console.WriteLine(ServiceDescription);
            Console.WriteLine();
            Console.WriteLine("This application is intened to be run as windows service. Use one of the following options:");
            Console.WriteLine("  -register         Registers and starts this program as a windows service named \"" + ServiceDisplayName + "\"");
            Console.WriteLine("                    All additional arguments will be passed to the service.");
            Console.WriteLine("  -unregister       Removes the windows service creatd by --register.");
        }

        private static string EscapeArgs(string arg)
        {
            arg = Regex.Replace(arg, @"(\\*)" + "\"", @"$1$1\" + "\"");
            arg = "\"" + Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
            return arg;
        }
    }
}
