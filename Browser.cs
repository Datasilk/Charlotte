using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Reflection;
using System.ServiceModel;
using CefSharp;
using CefSharp.OffScreen;
using Newtonsoft.Json;

namespace Charlotte.Wcf
{
    [ServiceContract]
    public interface IBrowser
    {
        [OperationContract]
        string Collect(string url);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Browser : IBrowser
    {

        public Browser(){
            //Initialize Cef
            var cef = new CefSettings();
            cef.CefCommandLineArgs.Add("enable-media-stream", "0");
            cef.LogSeverity = LogSeverity.Error;
            Cef.Initialize(cef);
        }
        
        public string Collect(string url)
        {
            var html = "";
            var log = new StringBuilder();
            var errors = new StringBuilder();
            try
            {
                //Create Browser Instance
                var settings = new BrowserSettings()
                {
                    ImageLoading = CefState.Disabled,
                    WebGl = CefState.Disabled,
                    WindowlessFrameRate = 5
                };
                var browser = new ChromiumWebBrowser(url, settings);

                //Frame Load Start Event
                browser.FrameLoadStart += delegate (object sender, FrameLoadStartEventArgs e)
                {
                    log.AppendLine("Started loading: " + e.Url);
                };

                //Frame Load End Event
                browser.FrameLoadEnd += delegate (object sender, FrameLoadEndEventArgs e)
                {
                    log.AppendLine("End loading (" + e.HttpStatusCode + "): " + e.Url);
                    if (html != "") { return; }
                    if(e.Frame.Identifier != browser.GetMainFrame().Identifier) { return; }

                    Task task = Task.Run(() => {
                        var js = File.ReadAllText(Path + "extractDOM.js");
                        object result = EvaluateScript(browser, js);
                        try
                        {
                            html = JsonConvert.SerializeObject(result, Formatting.None);
                        }
                        catch(Exception ex)
                        {
                            html = ex.Message + "\n" + ex.StackTrace;
                        }
                        
                    });
                };

                //Frame Load Error Event
                browser.LoadError += delegate (object sender, LoadErrorEventArgs e)
                {
                    log.AppendLine(e.ErrorCode.ToString() + " (" + e.FailedUrl + "): /n" + e.ErrorText + "/n");
                };

                //Address Change Event
                browser.AddressChanged += delegate (object sender, AddressChangedEventArgs e)
                {
                    log.AppendLine("Address Changed: " + e.Address);
                };

                //Frame Loading State Change Event
                browser.LoadingStateChanged += delegate (object sender, LoadingStateChangedEventArgs e)
                {
                    log.AppendLine("Loading State changed: " + e.IsLoading);
                };

                //Frame Load Start Event
                browser.StatusMessage += delegate (object sender, StatusMessageEventArgs e)
                {
                    log.AppendLine("Status Message: " + e.Value);
                };

                //check for html response (with 15 second timeout)
                var i = 0;
                while (i++ <= 15 * 2)
                {
                    if (html != "")
                    {
                        Console.WriteLine("downloaded " + url);
                        browser.Dispose();
                        return html;
                    }
                    Thread.Sleep(1000 / 2);
                }

                if (html == "")
                {
                    //return log since response timed out
                    browser.Dispose();
                    return "log: " + log.ToString();
                }
            }
            catch (Exception ex)
            {
                //return error information
                return "error: " + ex.Message + "\n\n" + ex.StackTrace;
            }

            return "";
        }

        public void Dispose()
        {
            //Dispose Browser
            Cef.Shutdown();
        }

        private static object EvaluateScript(ChromiumWebBrowser browser, string script)
        {
            var task = browser.EvaluateScriptAsync(script);
            task.Wait();
            var response = task.Result;
            return response.Success ? (response.Result ?? "") : response.Message;
        }

        private static string Path
        {
            get { return Assembly.GetExecutingAssembly().Location.Replace("Charlotte.exe", ""); }
        }
    }
}
