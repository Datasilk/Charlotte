using System.ServiceModel;

namespace WebBrowser.Wcf
{
    [ServiceContract]
    public interface IBrowser
    {
        [OperationContract]
        string Collect(string url);
    }
}
