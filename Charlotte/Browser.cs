using System.Text;
using System.Text.Json;
using System.Reflection;
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
        public Browser(){
            //Initialize Cef
            var cef = new CefSettings();
            cef.CefCommandLineArgs.Add("enable-media-stream", "0");
            //cef.CefCommandLineArgs.Add("disable-image-loading", null);
            cef.CefCommandLineArgs.Add("disable-javascript-access-clipboard", null);
            cef.CefCommandLineArgs.Add("disable-gpu", null);
            cef.CefCommandLineArgs.Add("disable-gpu-vsync", null);
            cef.CefCommandLineArgs.Add("disable-software-rasterizer", null);
            cef.CefCommandLineArgs.Add("disable-accelerated-2d-canvas", null);

            cef.CefCommandLineArgs.Add("persist-session-cookies", "0");
            //cef.CefCommandLineArgs.Add("disable-spell-checking", null);
            //cef.CefCommandLineArgs.Add("disable-pdf-extension", null);
            cef.LogSeverity = LogSeverity.Error;
            cef.CachePath = App.MapPath("cefsharp");
            cef.LogFile = App.MapPath("cefsharp/cefsharp.log");
            if(App.Environment == Environment.production)
            {
                cef.BrowserSubprocessPath = App.MapPath("/");
            }
            cef.LocalesDirPath = App.MapPath("locales");
            Cef.Initialize(cef);
        }
        
        public string Collect(string url)
        {
            var html = "";
            var log = new StringBuilder();
            var errors = new StringBuilder();
            var redirecting = false;
            ChromiumWebBrowser browser = null;
            try
            {
                //Create Browser Instance
                var settings = new BrowserSettings()
                {
                    ImageLoading = CefState.Disabled,
                    WebGl = CefState.Disabled,
                    WindowlessFrameRate = 5
                };
                browser = new ChromiumWebBrowser(url, settings);
                browser.RequestHandler = new RequestHandler();

                //Frame Load Start Event
                browser.FrameLoadStart += delegate (object? sender, FrameLoadStartEventArgs e)
                {
                    log.AppendLine("Started loading: " + e.Url + (e.Frame.IsMain ? " (main frame)" : " (iframe)"));
                    //Console.WriteLine("Started loading: " + e.Url + (e.Frame.IsMain ? " (main frame)" : " (iframe)"));
                    if(e.Frame.IsMain == false) { // && e.Url != "about:blank") {
                        //Console.WriteLine("delete iframe");
                        e.Frame.Delete();
                    }
                };

                //Frame Load End Event
                browser.FrameLoadEnd += delegate (object? sender, FrameLoadEndEventArgs e)
                {
                    log.AppendLine("End loading (" + e.HttpStatusCode + "): " + e.Url);
                    //Console.WriteLine("End loading (" + e.HttpStatusCode + "): " + e.Url);
                    if (html != "") { return; }
                    if(e.Frame.Identifier != browser.GetMainFrame().Identifier) { return; }
                    if (redirecting == false || (redirecting == true && e.Frame.Url == url))
                    {
                        redirecting = false;

                        Task lazytask = Task.Run(() => {
                            var lazyjs = File.ReadAllText(Path + "check-lazyload.js");
                            object result = EvaluateScript(browser, lazyjs);
                            try
                            {
                                if ((bool)result == true)
                                {
                                    browser.EvaluateScriptAsync(File.ReadAllText(Path + "lazyload.js"));
                                    Thread.Sleep(2000);
                                }

                                //finally, extract the DOM in JSON format
                                //Console.WriteLine("Run extractDOM.js on URL: " + e.Url);
                                Task task = Task.Run(() => {
                                    var js = File.ReadAllText(Path + "extractDOM.js");
                                    result = EvaluateScript(browser, js);
                                    try
                                    {
                                        html = JsonSerializer.Serialize(result, new JsonSerializerOptions()
                                        {
                                            MaxDepth = 256
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        html = ex.Message + "\n" + ex.StackTrace + "\n\n\n" + result;
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                html = ex.Message + "\n" + ex.StackTrace;
                            }
                        });
                    }
                };

                //Frame Load Error Event
                browser.LoadError += delegate (object? sender, LoadErrorEventArgs e)
                {
                    log.AppendLine(e.ErrorCode.ToString() + " (" + e.FailedUrl + "): /n" + e.ErrorText + "/n");
                };

                //Address Change Event
                browser.AddressChanged += delegate (object? sender, AddressChangedEventArgs e)
                {
                    log.AppendLine("Address Changed: " + e.Address);
                    //Console.WriteLine("Address Changed: " + e.Address);
                    if (e.Address != url)
                    {
                        redirecting = true;
                        url = e.Address;
                        //Console.WriteLine("redirecting = true");
                    }
                };

                //Frame Loading State Change Event
                browser.LoadingStateChanged += delegate (object? sender, LoadingStateChangedEventArgs e)
                {
                    log.AppendLine("Loading State changed: " + e.IsLoading);
                };

                //Frame Load Start Event
                browser.StatusMessage += delegate (object? sender, StatusMessageEventArgs e)
                {
                    log.AppendLine("Status Message: " + e.Value);
                };

                //check for html response (with 10 second timeout)
                var i = 0;
                while (i++ <= (10 * 2))
                {
                    if (html != "")
                    {
                        //Console.WriteLine("downloaded " + browser.Address);
                        browser.Dispose();
                        return html;
                    }
                    Thread.Sleep(1000 / 2);
                }

                if (html == "")
                {
                    //return log since response timed out
                    browser.Dispose();
                    Console.WriteLine("/////////////////////////////////////////////////////////////");
                    Console.WriteLine("Timeout >> " + log.ToString());
                    return "log: " + log.ToString();
                }
            }
            catch (Exception ex)
            {
                //return error information
                try
                {
                    browser.Dispose();
                }
                catch (Exception) { }
                return "error: " + ex.Message + "\n\n" + ex.StackTrace;
            }

            return "";
        }

        public string Login(string url, string user, string pass, string macros)
        {
            return "";
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
            catch(Exception ex)
            {
                return null;
            }
        }

        private static string Path
        {
            get {
                //return Assembly.GetExecutingAssembly().Location.Replace("Charlotte.exe", ""); 
                return App.RootPath + "/";
            }
        }
    }
}
