using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Activation;

namespace Charlotte.Wcf
{
    public class BrowserServiceHostFactory : ServiceHostFactoryBase
    {
        private readonly IBrowser browser;

        public BrowserServiceHostFactory()
        {
            browser = new Browser();
        }

        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            var host = new BrowserServiceHost(browser, baseAddresses);

            foreach (var uri in baseAddresses)
            {
                WSHttpBinding b = new WSHttpBinding(SecurityMode.Message)
                {
                    Name = typeof(IBrowser).Name
                };
                host.AddServiceEndpoint(typeof(IBrowser), b, uri);
            }
            return host;
        }
    }

    public class BrowserServiceHost : ServiceHost
    {
        public BrowserServiceHost(IBrowser browser, params Uri[] baseAddresses) : base(browser, baseAddresses)
        {
            if (browser == null)
            {
                throw new ArgumentNullException(nameof(browser));
            }
            Description.Behaviors.Add(new BrowserInstanceProvider(browser));
        }
    }

    public class BrowserInstanceProvider : IInstanceProvider, IServiceBehavior
    {
        private readonly IBrowser browser;

        public BrowserInstanceProvider(IBrowser browser)
        {
            this.browser = browser;
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher cd = (ChannelDispatcher)channelDispatcherBase;
                foreach (EndpointDispatcher ed in cd.Endpoints)
                {
                    if (!ed.IsSystemEndpoint)
                    {
                        ed.DispatchRuntime.InstanceProvider = this;
                    }
                }
            }
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return browser;
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return GetInstance(instanceContext);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            (instance as IDisposable)?.Dispose();
        }
    }
}
