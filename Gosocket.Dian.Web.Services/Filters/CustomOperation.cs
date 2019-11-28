using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Gosocket.Dian.Web.Services.Filters
{
    public class CustomOperation : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {

            MessageInspector inspector = dispatchOperation.Parent.MessageInspectors
                .Where(x => x is MessageInspector)
                .FirstOrDefault() as MessageInspector;

            if (inspector != null)
            {
                inspector.AddOperation(operationDescription);
            }
            else
            {
                inspector = new MessageInspector(operationDescription);
                dispatchOperation.Parent.MessageInspectors.Add(inspector);
            }
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}