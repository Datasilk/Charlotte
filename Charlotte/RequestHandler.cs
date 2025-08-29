using System.Linq;
using CefSharp;

namespace Charlotte
{
    public class RequestHandler : CefSharp.Handler.RequestHandler
    {

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, CefSharp.IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            if (Settings.BlacklistedDomains == null) return new CustomResourceRequestHandler();
            var found = Settings.BlacklistedDomains.Where(a => request.Url.Contains(a)).FirstOrDefault();
            if (found != null && found != "")
            {
                return new CustomResourceRequestHandler();
            }

            return null;
        }

        //protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, CefSharp.IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        //{
        //    if(frame.IsMain == false)
        //    {
        //        //do not load iframes
        //        return false;
        //    }
        //    return true;
        //}
    }

    public class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, CefSharp.IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            request.Url = "";
            request.Dispose();
            return CefReturnValue.Cancel;
        }
    }
}
