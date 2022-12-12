using Microsoft.AspNetCore.SignalR;
using Router.Common;

namespace Router.SignalR
{
    public class DashboardHub : Hub
    {
        public async Task UpdateConsole()
        {
            await Clients.Caller.SendAsync("update", "Charlotte's Web (Router) v" + App.Version + " running on " +
                string.Join(" and ", App.Addresses.Select(a => "<a href=\"" + a + "\" target=\"_blank\">" + a + "</a>"))
                );

            Log.Listeners.Add(Clients.Caller);
        }

        public async Task CheckUrl(string url, bool session, string macros)
        {
            var requestId = 1 + new Random().Next(99999);
            var logPrefix = requestId + ": ";
            Log.WriteLine(logPrefix + "Checking URL " + url + "..............................");
            var dom = Charlotte.GetDOM(url, session, macros, out requestId);
            await Clients.Caller.SendAsync("response", dom);
        }
    }
}
