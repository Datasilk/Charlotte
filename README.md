# Charlotte
An off-screen Chromium web browser used to extract the computed DOM from any web page using .NET Core WCF.

### Installation
1. Build project
2. execute `bin/x64/Debug/Charlotte.exe -register` to register the console application as a Windows Service, which will automatically start the WCF Hosted Service
3. Using WCF in your own project, connect to `http://localhost:7077/Browser` and retrieve the computed DOM element hierarchy as a JSON string:


**IBrowser.cs**
```
[ServiceContract]
public interface IBrowser
{
    [OperationContract]
    string Collect(string url);
}
```

**Client.cs**
```
public static class Client{
	static string Collect(string url){
		var binding = new BasicHttpBinding()
			{
				MaxReceivedMessageSize = 50 * 1024 * 1024 //50 MB
			};
		var endpoint = new EndpointAddress(new Uri("http://localhost:7077/Browser"));
		var channelFactory = new ChannelFactory<IBrowser>(binding, endpoint);
		var serviceClient = channelFactory.CreateChannel();
		var result = serviceClient.Collect(url);
		channelFactory.Close();
	}
}

```

4. Deserialize the JSON string into a POCO object using the following models:

```
public class Document
{
    public string[] a; //list of attribute names
    public Node dom;
}

public class Node
{
    public string t = ""; //tag name
    public int[] s = null; //array of style values [display (0 = none, 1 = block, 2 = inline), font-size, bold, italic]
    public Dictionary<int, string> a; //dictionary of element attributes [{attribute name index, value}, {...}]
    public List<Node> c; //list of child elements
    public string v; //optional #text element value
}
```

> NOTE: Get the Node attribute name from the Document.a array using the Dictionary key as an index


##### Why Charlotte?
Named after the spider in the children's story "Charlotte's Web". Charlotte prevented the slaughter of a piglette named Wilbur by weaving words of praise for Wilbur into her web, which in turn surprised the local farmers and convinced them to keep Wilbur alive.

Similarly, the Charlotte console app weaves a JSON object from the computed DOM of a web page, so that developers can keep the contents of an article instead of letting it go to the slaughter (when the publisher of an article decides to archive the URL).
