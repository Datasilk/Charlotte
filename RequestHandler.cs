using System.Linq;
using CefSharp;

namespace Charlotte
{
    public class RequestHandler : CefSharp.Handler.RequestHandler
    {
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            var found = Settings.BlacklistedDomains.Where(a => request.Url.Contains(a)).FirstOrDefault();
            if (found != null && found != "")
            {
                return new CustomResourceRequestHandler();
            }

            return null;
        }
    }

    public class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {

            request.Url = "";
            request.Dispose();

            return CefReturnValue.Cancel;
        }
    }
}
