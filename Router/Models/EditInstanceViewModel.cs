namespace Router.Models
{
    public class EditInstanceViewModel
    {
        public string InstanceId { get; set; }
        public string Url { get; set; }
        public string ServerName { get;set; }
        public string Note { get; set; }
        public bool UsesCookies { get; set; }
    }
}
