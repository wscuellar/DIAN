using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IGlobalDocValidatorTrackingService
    {
        List<GlobalDocValidatorTracking> ListTracking(string eventDocumentKey);
    }
}
