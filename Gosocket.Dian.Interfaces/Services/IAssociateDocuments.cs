using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IAssociateDocuments
    {
        List<InvoiceWrapper> GetEventsByTrackId(string trackId);

    }
}
