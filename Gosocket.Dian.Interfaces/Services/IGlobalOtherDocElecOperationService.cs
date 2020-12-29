﻿using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IGlobalOtherDocElecOperationService
    {
        GlobalOtherDocElecOperation GetOperation(string code, Guid softwareId);
        bool Insert(GlobalOtherDocElecOperation item, OtherDocElecSoftware software);
        bool Update(GlobalOtherDocElecOperation item);
        bool IsActive(string code, Guid softwareId);
        List<GlobalOtherDocElecOperation> OperationList(string code);
        bool SoftwareAdd(GlobalSoftware item);
        GlobalOtherDocElecOperation EnableParticipant(string code, string softwareId);
        bool Delete(string code, string v);
    }
}