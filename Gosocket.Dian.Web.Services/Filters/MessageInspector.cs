using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Gosocket.Dian.Web.Services.Filters
{
    public class MessageBehavior : IServiceBehavior
    {
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher epd in cd.Endpoints)
                {
                    epd.DispatchRuntime.MessageInspectors.Add(new MessageInspector());
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }

    public class MessageBehaviorBehaviorExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new MessageBehavior();
        }

        public override Type BehaviorType
        {
            get { return typeof(MessageBehavior); }
        }
    }

    public class MessageInspector : IDispatchMessageInspector
    {
        private static readonly TableManager tableManagerGlobalLogger = new TableManager("GlobalLogger");
        readonly List<string> targetOperations = new List<string>();

        public MessageInspector() { }

        public MessageInspector(OperationDescription operation)
        {
            this.AddOperation(operation);
        }

        public void AddOperation(OperationDescription operation)
        {
            this.targetOperations.Add(operation.Messages[0].Action);
        }

        public bool TargetOperationMatchesRequest(Message request)
        {
            string requestAction = request.Headers.Action;
            requestAction = requestAction.Substring(requestAction.LastIndexOf("/"));

            string targetOperation = "";
            foreach (string targetOperationPath in targetOperations)
            {
                targetOperation = targetOperationPath.Substring(targetOperationPath.LastIndexOf("/"));
                if (targetOperation.Equals(requestAction))
                    return true;
            }



            return false;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (TargetOperationMatchesRequest(request))
                return request;

            try
            {
                var ip = GetIP();
                string requestAction = request.Headers.Action;
                requestAction = requestAction.Substring(requestAction.LastIndexOf("/"));

                var logger = new GlobalLogger($"InvalidOperation-{DateTime.UtcNow.ToString("yyyyMMdd")}", Guid.NewGuid().ToString()) { Action = requestAction, Message = $"IP: {ip}" };
                tableManagerGlobalLogger.InsertOrUpdate(logger);
            }
            catch{}

            throw new FaultException("Invalid operation.", new FaultCode("Client"));
        }

        public void BeforeSendReply(ref Message reply, object correlationState) { }

        public static string GetIP()
        {

            string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            return ip.Split(':').FirstOrDefault();
        }

    }
}