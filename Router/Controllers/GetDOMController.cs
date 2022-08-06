using Microsoft.AspNetCore.Mvc;
using Router.Models;
using System.Diagnostics;
using System.ServiceModel;
using WebBrowser.Wcf;

namespace Router.Controllers
{
    public class GetDOMController : BaseController
    {
        [HttpPost]
        public string Index(string url, bool session = false, string macros = "")
        {
            try
            {

                //find unused instance of Charlotte to collect the DOM from
                ConfigCharlotteInstance? instance = null;
                var start = DateTime.Now;
                while (instance == null)
                {
                    instance = App.Config.Charlotte.Instances.Where(a => a.InUse == false && a.UsesCookies == session)
                        .OrderBy(a => a.Started).FirstOrDefault();
                    if((DateTime.Now - start).TotalSeconds > 10)
                    { 
                        return "Error: Timeout when waiting for available Charlotte instance"; 
                    }
                    Thread.Sleep(500);
                }
                Console.WriteLine("found instance " + instance.Id + " (" + instance.Url + ")");
                //instance is in use //////////////////////////
                instance.InUse = true;
                instance.Started = DateTime.Now;
                var binding = new BasicHttpBinding()
                {
                    MaxReceivedMessageSize = 5 * 1024 * 1024, //5 MB
                };
                var endpoint = new EndpointAddress(new Uri(instance.Url));
                var channelFactory = new ChannelFactory<IBrowser>(binding, endpoint);
                var serviceClient = channelFactory.CreateChannel();
                var result = serviceClient.Collect(url);
                channelFactory.Close();

                //reset instance use ///////////////////////////
                instance.InUse = false;
                return result;
            }
            catch(Exception ex)
            {
                return "Error: " + ex.Message + "\n" + ex.StackTrace;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}