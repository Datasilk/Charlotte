namespace Router.Models
{
    public class Config
    {
        public ConfigAdmin Admin { get; set; }
        public ConfigCharlotte Charlotte { get; set; }
    }

    public class ConfigAdmin
    {
        public string Username { get; set; }
        public string Pass { get; set; }
    }

    public class ConfigCharlotte
    {
        public List<ConfigCharlotteInstance> Instances { get; set; }
    }
    
    public class ConfigCharlotteInstance
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Server { get; set; }
        public string Note { get; set; }
    }
}
