using System;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Router.Models;
using System.Diagnostics;

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
                    instance = App.Config.Charlotte.Instances.Where(a => a.UsesCookies == session)
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
                var result = "";

                try
                {
                    var postData = new StringBuilder();
                    postData.Append(String.Format("{0}={1}&", WebUtility.HtmlEncode("url"), WebUtility.HtmlEncode(url)));
                    postData.Append(String.Format("{0}={1}", WebUtility.HtmlEncode("macros"), WebUtility.HtmlEncode(macros)));
                    StringContent postContent = new StringContent(postData.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
                    HttpClient client = new HttpClient();
                    HttpResponseMessage message = client.PostAsync(instance.Url, postContent).GetAwaiter().GetResult();
                    result = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                catch(Exception ex)
                {
                    instance.InUse = false;
                    return "Error: " + ex.Message + "\n" + ex.StackTrace;
                }

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