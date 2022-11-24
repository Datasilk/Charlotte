using Microsoft.AspNetCore.SignalR;

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
    }
}
