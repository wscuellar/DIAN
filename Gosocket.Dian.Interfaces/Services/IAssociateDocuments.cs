﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IAssociateDocuments
    {
        List<GlobalDocValidatorDocumentMeta> GetEventsByTrackId(string trackId);

    }
}
