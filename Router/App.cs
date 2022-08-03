using System.Text;
using System.Text.Json;
using System.IO;

namespace Router
{
    public enum Environment
    {
        development = 0,
        staging = 1,
        production = 2
    }

    public static class App
    {
        public static Models.Config Config { get; set; } = new Models.Config();
        public static string ConfigFilename { get; set; } = "";

        public static Environment Environment { get; set; } = Environment.development;
        public static bool IsDocker { get; set; }
        private static string _rootPath { get; set; } = "";

        public static string RootPath
        {
            get
            {
                if (string.IsNullOrEmpty(_rootPath))
                {
                    _rootPath = Path.GetFullPath(".").Replace("\\", "/");
                }
                return _rootPath;
            }
        }

        public static string MapPath(string path = "")
        {
            path = path.Replace("\\", "/");
            if (path.Substring(0, 1) == "/") { path = path.Substring(1); } //remove slash at beginning of string
            if (IsDocker)
            {
                return Path.Combine(RootPath, path).Replace("\\", "/");
            }
            else
            {
                return Path.Combine(RootPath.Replace("/", "\\"), path.Replace("/", "\\"));
            }
        }

        public static void SaveConfig()
        {
            File.WriteAllText(MapPath("/" + ConfigFilename), JsonSerializer.Serialize(Config));
        }
    }
}
