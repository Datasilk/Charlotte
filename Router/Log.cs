using Microsoft.AspNetCore.SignalR;

namespace Router
{
    public static class Log
    {
        public static List<IClientProxy> Listeners { get; set; } = new List<IClientProxy>();

        public static void WriteLine(string message)
        {
            if(Listeners.Count == 0) { return; }
            var remove = new List<IClientProxy>();
            foreach(var listener in Listeners)
            {
                try
                {
                    listener.SendAsync("update", message);
                }
                catch (Exception)
                {
                    remove.Add(listener);
                }
            }
            if(remove.Count > 0)
            {
                foreach(var r in remove)
                {
                    Listeners.Remove(r);
                }
                remove.Clear();
            }
        }
    }
}
