namespace Router.Models
{
    public static class Traffic
    {
        public static List<TrafficItem> Requests { get; set; } = new List<TrafficItem>();

        public static List<Func<TrafficItem>> Responses { get; set; }
    }

    public class TrafficItem
    {
        public string Url { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public int Milliseconds { get; set; }
    }
}
