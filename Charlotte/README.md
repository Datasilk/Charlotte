# Charlotte
An off-screen Chromium web browser used to extract the computed DOM from any web page using .NET Core WCF.

### Installation
1. Build project
2. execute `bin/x64/Debug/Charlotte.exe -register` in PowerShell to register the console application as a Windows Service, which will automatically start the WCF Hosted Service
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
		return result;
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

### How Does It Work?
Charlotte uses CefSharp to load a web page within an off-screen Chromium web browser. 

Once the DOM is completely loaded, Charlotte injects script code from `extractDOM.js` into the web page, which is used to traverse the rendered DOM and generate a JSON representation of the DOM. Any style information extracted from each DOM element is actually the computed style for that element, which makes Charlotte so powerful. You don't need to decypher complex CSS files or `<style>` tags in order to figure out the styling for a given element on the page. That information is extracted for you and attached to each node within the JSON object.

### Customize The Output
You can modify `extractDOM.js` to extract more information about each DOM element. For example, you could include more style information. Unfortunately, the style object only accepts integers, so you'll need to use some creativity to extract string values from element styles and convert them into integers.

> NOTE: The style object (Node.s) is an integer array in order to save a significant amount of bytes when generating the JSON string.

> NOTE: Right now, Charlotte extracts only four integers from an element's style attribute; **display**, **font-size**, **bold**, and **italic**. The reason being is that these values can help determine the importance of a block of text based on if it's being displayed, the size of the font, and whether or not the text is bold and/or italic.

##### Why the name "Charlotte"?
Named after the spider in the children's story "Charlotte's Web". Charlotte prevented the slaughter of a piglette named Wilbur by weaving words of praise for Wilbur into her web, which in turn surprised the local farmers and convinced them to keep Wilbur alive.

Similarly, the Charlotte console app weaves a JSON object from the computed DOM of a web page, so that developers can keep the contents of an article instead of letting it go to the slaughter (when the publisher of an article decides to archive the URL, which would then return a 404 or 500 error).
