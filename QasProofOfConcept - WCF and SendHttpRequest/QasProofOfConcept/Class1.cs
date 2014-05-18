using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Security.Permissions;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace QasProofOfConceptx
{
    public class WcfChannelFactory<T> : ChannelFactory<T> where T : class
    {
        public WcfChannelFactory(Binding binding) : base(binding) {}

        public T CreateBaseChannel()
        {
            return base.CreateChannel(this.Endpoint.Address, null);
        }

        public override T CreateChannel(EndpointAddress address, Uri via)
        {
            // This is where the magic happens. We don't really return a channel here;
            // we return WcfClientProxy.GetTransparentProxy(). That class will now
            // have the chance to intercept calls to the service.
            this.Endpoint.Address = address;            
            var proxy = new WcfClientProxy<T>(this);
            return proxy.GetTransparentProxy() as T;
        }
    }

    public class WcfClientProxy<T> : RealProxy where T : class
    {
        private WcfChannelFactory<T> _channelFactory;

        public WcfClientProxy(WcfChannelFactory<T> channelFactory)
            : base(typeof(T))
        {
            this._channelFactory = channelFactory;
        }

        public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage message)
        {
            // When a service method gets called, we intercept it here and call it below with methodBase.Invoke().

            var methodCall = message as IMethodCallMessage;
            var methodBase = methodCall.MethodBase;

            // We can't call CreateChannel() because that creates an instance of this class,
            // and we'd end up with a stack overflow. So, call CreateBaseChannel() to get the
            // actual service.
            T wcfService = this._channelFactory.CreateBaseChannel();

            try
            {
                var result = methodBase.Invoke(wcfService, methodCall.Args);

                return new ReturnMessage(
                      result, // Operation result
                      null, // Out arguments
                      0, // Out arguments count
                      methodCall.LogicalCallContext, // Call context
                      methodCall); // Original message
            }
            catch (FaultException fx)
            {
                // Need to specifically catch and rethrow FaultExceptions to bypass the CommunicationException catch.
                // This is needed to distinguish between Faults and underlying communication exceptions.
                throw;
            }
            catch (CommunicationException ex)
            {
                // Handle CommunicationException and implement retries here.
                throw new NotImplementedException();
            }            
            catch (Exception ex)
            {
                // Handle CommunicationException and implement retries here.
                throw;
            }            
        
        }
    }
}
