using Router.Models;
using System.Net;
using System.Text;

namespace Router.Common
{
    public static class Charlotte
    {
        public static string GetDOM(string url, bool session, string macros, out int requestIdOut, int requestId = 0)
        {
            if (requestId == 0) { requestId = 1 + new Random().Next(99999); }
            requestIdOut = requestId;
            var logPrefix = requestId + ": ";
            try
            {
                //find unused instance of Charlotte to collect the DOM from
                Log.WriteLine(logPrefix + "New request (<a href=\"" + url + "\" target=\"_blank\">" + url + "</a>)" + (session ? " with session" : ""));
                ConfigCharlotteInstance? instance = null;
                var start = DateTime.Now;
                while (instance == null)
                {
                    instance = App.Config.Charlotte.Instances.Where(a => a.UsesCookies == session)
                        .OrderBy(a => a.Started).FirstOrDefault();
                    if ((DateTime.Now - start).TotalSeconds > 10)
                    {
                        var err = "Timeout when waiting for available Charlotte instance";
                        Log.WriteLine(logPrefix + err);
                        return "Error: " + err;
                    }
                    Thread.Sleep(500);
                }

                var msg = "using instance " + instance.Id + " (" + instance.Url + ")";
                Log.WriteLine(logPrefix + msg);
                Console.WriteLine(msg);
                //instance is in use //////////////////////////
                instance.InUse = true;
                instance.Started = DateTime.Now;
                var result = "";

                try
                {
                    var postData = new StringBuilder();
                    postData.Append(string.Format("{0}={1}&", WebUtility.HtmlEncode("url"), WebUtility.HtmlEncode(url)));
                    postData.Append(string.Format("{0}={1}", WebUtility.HtmlEncode("macros"), WebUtility.HtmlEncode(macros)));
                    StringContent postContent = new StringContent(postData.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
                    HttpClient client = new HttpClient();
                    HttpResponseMessage message = client.PostAsync(instance.Url, postContent).GetAwaiter().GetResult();
                    result = message.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var size = ((result.Length * 2) / 1024.0);
                    Log.WriteLine(logPrefix + "response size = " + size.ToString("N1") + "kb");
                }
                catch (Exception ex)
                {
                    instance.InUse = false;
                    msg = ex.Message + "\n" + ex.StackTrace;
                    Log.WriteLine(logPrefix + msg);
                    return "Error: " + msg;
                }

                //reset instance use ///////////////////////////
                instance.InUse = false;
                return result;
            }
            catch (Exception ex)
            {
                var msg = ex.Message + "\n" + ex.StackTrace;
                Log.WriteLine(logPrefix + msg);
                return "Error: " + msg;
            }
        }

    }
}
