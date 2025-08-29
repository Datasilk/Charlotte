using System.Text;
using System.Text.Json;
using System.ServiceModel;
using CefSharp;
using CefSharp.OffScreen;

namespace Charlotte
{
    [ServiceContract]
    public interface IBrowser
    {
        [OperationContract]
        string Collect(string url);
    }

    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Browser : IBrowser
    {
        private string checklazyjs { get; set; } = "";
        private string lazyjs { get; set; } = "";
        private string extractDOMjs { get; set; } = "";


        public Browser()
        {
            // Create required directories

            Console.WriteLine("Initialize Browser");
            var cefsharpDir = App.MapPath("cefsharp");
            if (!Directory.Exists(cefsharpDir))
            {
                Directory.CreateDirectory(cefsharpDir);
            }

            // Setup locales directory paths
            string runtimeLocalesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", "win-x64", "native", "locales");
            var localesDir = App.MapPath("locales");
            //Console.WriteLine($"Runtime locales directory: {runtimeLocalesDir}");

            // Try alternative path if runtime locales not found
            if (!Directory.Exists(runtimeLocalesDir))
            {
                string altLocalesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "locales");
                if (Directory.Exists(altLocalesDir))
                {
                    runtimeLocalesDir = altLocalesDir;
                    Console.WriteLine($"Using alternative locales directory: {runtimeLocalesDir}");
                }
                else
                {
                    Console.WriteLine("ERROR: Could not find any locales directory to copy from!");
                }
            }

            // Create locales directory if needed
            if (!Directory.Exists(localesDir))
            {
                Directory.CreateDirectory(localesDir);
                Console.WriteLine($"Created locales directory: {localesDir}");
            }

            // Copy missing locale files in a single pass
            if (Directory.Exists(runtimeLocalesDir))
            {
                int filesCopied = 0;
                foreach (var file in Directory.GetFiles(runtimeLocalesDir))
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(localesDir, fileName);

                    // Copy file if it doesn't exist
                    if (!File.Exists(destFile))
                    {
                        File.Copy(file, destFile, false);
                        filesCopied++;
                    }
                }

                if (filesCopied > 0)
                {
                    Console.WriteLine($"Copied {filesCopied} locale files");
                }
            }

            //Initialize Cef
            Console.WriteLine("Initializing CefSharp...");
            var cef = new CefSettings();

            // Disable Direct Composition to fix AMD VideoProcessor error
            cef.CefCommandLineArgs.Add("disable-direct-composition", "1");
            cef.CefCommandLineArgs.Add("disable-direct-composition-layers", "1");
            cef.CefCommandLineArgs.Add("use-angle", "gles");

            // Disable specific GPU features that might cause issues
            cef.CefCommandLineArgs.Add("disable-gpu-vsync", "1");
            cef.CefCommandLineArgs.Add("disable-accelerated-video-decode", "1");
            cef.CefCommandLineArgs.Add("disable-accelerated-video-encode", "1");

            // Use software compositing instead of GPU
            cef.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            cef.CefCommandLineArgs.Add("disable-gpu-memory-buffer-compositor-resources", "1");

            // Other settings
            cef.CefCommandLineArgs.Add("enable-media-stream", "0");
            cef.CefCommandLineArgs.Add("disable-javascript-access-clipboard", "1");
            cef.CefCommandLineArgs.Add("persist-session-cookies", "0");
            cef.CefCommandLineArgs.Add("disable-image-loading", "1"); // Disable image loading to improve performance

            // Suppress USB and GCM errors - more aggressive approach
            cef.CefCommandLineArgs.Add("disable-features", "UsbDeviceDetection,GCMAPIProvisioning,NetworkService,NetworkServiceInProcess");
            cef.CefCommandLineArgs.Add("disable-usb-keyboard-detect", "1");
            cef.CefCommandLineArgs.Add("disable-cloud-import", "1");
            cef.CefCommandLineArgs.Add("disable-notifications", "1");
            cef.CefCommandLineArgs.Add("disable-background-networking", "1");
            cef.CefCommandLineArgs.Add("disable-component-update", "1");
            cef.CefCommandLineArgs.Add("disable-domain-reliability", "1");
            cef.CefCommandLineArgs.Add("disable-sync", "1");
            cef.CefCommandLineArgs.Add("disable-spell-checking", null);
            cef.CefCommandLineArgs.Add("disable-pdf-extension", null);

            // Set up paths
            cef.CachePath = cefsharpDir; //cef.CachePath = ""; //incognito mode
            cef.LocalesDirPath = localesDir;
            cef.LogFile = Path.Combine(cefsharpDir, "cefsharp.log");

            if (App.Environment == Environment.production)
            {
                cef.BrowserSubprocessPath = App.MapPath("/");
            }
            // Also set the resources directory path
            string resourcesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes", "win-x64", "native");
            if (Directory.Exists(resourcesDir))
            {
                cef.ResourcesDirPath = resourcesDir;
            }

            // Disable logging
            //cef.LogSeverity = LogSeverity.Fatal; // Only show fatal errors
            //cef.CefCommandLineArgs.Add("disable-logging", "1");
            //cef.CefCommandLineArgs.Add("log-level", "3"); // 3 = Fatal

            try
            {
                Cef.Initialize(cef, performDependencyCheck: true);
                Console.WriteLine("CefSharp initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing CefSharp: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
                throw; // Re-throw to see the error in the console output
            }

            try
            {
                // Check if files exist before trying to read them
                string checkLazyPath = AppPath + "check-lazyload.js";
                string lazyPath = AppPath + "lazyload.js";
                string extractDOMPath = AppPath + "extractDOM.js";

                // Try alternative paths if files don't exist
                if (!File.Exists(checkLazyPath))
                {
                    string altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "check-lazyload.js");
                    if (File.Exists(altPath)) checkLazyPath = altPath;
                }

                if (!File.Exists(lazyPath))
                {
                    string altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lazyload.js");
                    if (File.Exists(altPath)) lazyPath = altPath;
                }

                if (!File.Exists(extractDOMPath))
                {
                    string altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extractDOM.js");
                    if (File.Exists(altPath)) extractDOMPath = altPath;
                }

                // Read the files from the determined paths
                checklazyjs = File.ReadAllText(checkLazyPath);
                lazyjs = File.ReadAllText(lazyPath);
                extractDOMjs = File.ReadAllText(extractDOMPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading JavaScript injection files: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
                throw; // Re-throw to see the error in the console output
            }
        }

        public string Collect(string url)
        {
            var html = "";
            var log = new StringBuilder();
            var errors = new StringBuilder();
            var redirecting = false;
            ChromiumWebBrowser browser = null;
            log.AppendLine("Opening Web Page: " + url);
            try
            {
                //Create Browser Instance with optimized settings
                var settings = new BrowserSettings()
                {
                    ImageLoading = CefState.Disabled,
                    WebGl = CefState.Disabled,
                    WindowlessFrameRate = 1,  // Reduce frame rate to minimum
                    Databases = CefState.Disabled,     // Disable databases for better performance
                    JavascriptAccessClipboard = CefState.Disabled,
                    RemoteFonts = CefState.Disabled,
                    TabToLinks = CefState.Disabled,
                };
                browser = new ChromiumWebBrowser(url, settings);

                //Frame Load Start Event
                browser.FrameLoadStart += delegate (object? sender, FrameLoadStartEventArgs e)
                {
                    Console.WriteLine("Started loading: " + e.Url + (e.Frame.IsMain ? " (main frame)" : " (iframe)"));
                    if (e.Frame.IsMain == false)
                    { // && e.Url != "about:blank") {
                        Console.WriteLine("delete iframe");
                        e.Frame.Delete();
                    }
                    else
                    {
                        log.AppendLine("Started loading frame: " + e.Url);
                    }
                };

                //Frame Load End Event
                browser.FrameLoadEnd += delegate (object? sender, FrameLoadEndEventArgs e)
                {
                    log.AppendLine("Frame load ended " + html);
                    if (html != "") { return; }
                    if (e.Frame.Identifier != browser.GetMainFrame().Identifier)
                    {
                        log.AppendLine("End loading iframe (" + e.HttpStatusCode + "): " + e.Url);
                        return;
                    }
                    else
                    {
                        log.AppendLine("End loading main frame (" + e.HttpStatusCode + "): " + e.Url);
                    }
                    if (redirecting == false || (redirecting == true && e.Frame.Url == url))
                    {
                        log.AppendLine("Extracting DOM...");
                        redirecting = false;
                        Task lazytask = Task.Run(ExtractDOM);
                    }
                };

                //Frame Load Error Event
                browser.LoadError += delegate (object? sender, LoadErrorEventArgs e)
                {
                    log.AppendLine("Load Error: " + e.ErrorCode.ToString() + " (" + e.FailedUrl + "): " + e.ErrorText);
                };

                //Address Change Event
                browser.AddressChanged += delegate (object? sender, AddressChangedEventArgs e)
                {
                    log.AppendLine("Address changed: " + e.Address);
                    if (e.Address != url)
                    {
                        log.Append("Redirected from " + url + " to " + e.Address);
                        redirecting = true;
                        url = e.Address;
                    }
                };

                //Frame Loading State Change Event
                browser.LoadingStateChanged += delegate (object? sender, LoadingStateChangedEventArgs e)
                {
                    log.AppendLine("Loading State changed: " + e.IsLoading);
                };

                //Frame Status Message Event
                browser.StatusMessage += delegate (object? sender, StatusMessageEventArgs e)
                {
                    log.AppendLine("Status Message: " + e.Value);
                };

                void ExtractDOM()
                {
                    //scroll to bottom of the page to trigger lazy loading of all images
                    object result = EvaluateScript(browser, checklazyjs);
                    try
                    {
                        if ((bool)result == true)
                        {
                            browser.EvaluateScriptAsync(lazyjs);
                            Thread.Sleep(2000);
                        }

                        //finally, extract the DOM in JSON format
                        Task task = Task.Run(() =>
                        {
                            result = EvaluateScript(browser, extractDOMjs);
                            try
                            {
                                html = JsonSerializer.Serialize(result, new JsonSerializerOptions()
                                {
                                    MaxDepth = 256
                                });

                                if (html == null || html.Trim() == "")
                                {
                                    html = "error: No HTML collected via JavaScript Injection into web page";
                                }
                            }
                            catch (Exception ex)
                            {
                                html = ex.Message + "\n" + ex.StackTrace + "\n\n\n" + result;
                            }
                            //try closing web browser
                            CloseBrowser(browser);
                        });
                    }
                    catch (Exception ex)
                    {
                        html = ex.Message + "\n" + ex.StackTrace;
                        //try closing web browser
                        CloseBrowser(browser);
                    }
                }

                //check for html response (with 20 second timeout)
                var i = 0;
                var maxWaitTime = 20 * 2; // 20 seconds

                while (i++ <= maxWaitTime)
                {
                    if (html != "")
                    {
                        return html;
                    }
                    Thread.Sleep(500); // 500ms sleep
                }

                if (html == "")
                {
                    //try closing web browser
                    CloseBrowser(browser);

                    //return log since response timed out
                    Console.WriteLine("No HTML Returned: " + log.ToString());
                    return "log: " + log.ToString();
                }
            }
            catch (Exception ex)
            {
                //try closing web browser
                if(browser != null) CloseBrowser(browser);
                //return error information
                return "error: " + ex.Message + "\n\n" + ex.StackTrace;
            }

            return "";
        }

        public string Login(string url, string user, string pass, string macros)
        {
            return "";
        }

        private void CloseBrowser(ChromiumWebBrowser browser)
        {
            try
            {
                browser.GetBrowserHost().CloseBrowser(true);
                browser.Dispose();
                Console.WriteLine("Browser Closed Successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Closing Browser: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
            }
        }

        public void Dispose()
        {
            //Dispose Browser
            Cef.Shutdown();
        }

        private static object EvaluateScript(ChromiumWebBrowser browser, string script)
        {
            try
            {
                var task = browser.EvaluateScriptAsync(script, null, true);
                //task.Wait();
                var response = task.Result;
                return response.Success ? (response.Result ?? "") : response.Message;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static string AppPath
        {
            get
            {
                var path = App.RootPath + "/";
                return path;
            }
        }
    }
}
