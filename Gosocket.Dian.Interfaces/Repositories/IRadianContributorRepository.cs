﻿using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{

    public interface IRadianContributorRepository
    {
        int AddOrUpdate(RadianContributor radianContributor);
        List<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0);
        void RemoveRadianContributor(RadianContributor radianContributor);
        
    }

}