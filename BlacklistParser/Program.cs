using System.Text.Json;

//Console.WriteLine($"Launched from {Environment.CurrentDirectory}");
//Console.WriteLine($"Physical location {AppDomain.CurrentDomain.BaseDirectory}");
//Console.WriteLine($"AppContext.BaseDir {AppContext.BaseDirectory}");

var root = AppDomain.CurrentDomain.BaseDirectory;
var domains = new List<string>();

//add blacklist_domains to list first
if (File.Exists(root + "blacklist_domains.json"))
{
    var list = JsonSerializer.Deserialize<string[]>(File.ReadAllText(root + "blacklist_domains.json"));
    if(list != null)
    {
        domains.AddRange(list);
    }
}

//open & parse adservers.txt
if (File.Exists(root + "lists/adservers.txt"))
{   
    var text = File.ReadAllText(root + "lists/adservers.txt");
    var lines = text.Split("\n");
    var i = 0;
    foreach (var line in lines)
    {
        if(i < 10 || line == "")
        {
            i++; continue;
        }
        var domain = line.Split(" ")[1];
        if (domains.Contains(domain)) { continue; }
        domains.Add(domain);

        //var line2 = line.Split(" ", 2)[1].Split(".");
        //if(line2.Length > 1)
        //{
        //    string domain; //line2[line2.Length - 2] + "." + line2[line2.Length - 1];
        //    if(line2[line2.Length - 2] == "co" && line2.Length > 2)
        //    {
        //        domain = line2[line2.Length - 3] + "." + line2[line2.Length - 2] + "." + line2[line2.Length - 1];
        //    }
        //    else
        //    {
        //        domain = line2[line2.Length - 2] + "." + line2[line2.Length - 1];
        //    }
        //    if (domains.Contains(domain)) { continue; }
        //    domains.Add(domain);
        //}
    }
    domains = domains.OrderBy(a => a).ToList();

    if(!Directory.Exists(root + "output"))
    {
        Directory.CreateDirectory(root + "output");
    }
    File.WriteAllText(root + "output/blacklist.json", JsonSerializer.Serialize(domains));
}