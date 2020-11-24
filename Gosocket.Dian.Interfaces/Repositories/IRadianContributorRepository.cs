﻿using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Linq.Expressions;

namespace Gosocket.Dian.Interfaces.Repositories
{

    public interface IRadianContributorRepository
    {

        RadianContributor Get(Expression<Func<RadianContributor, bool>> expression);
        PagedResult<RadianContributor> List(Expression<Func<RadianContributor, bool>> expression, int page = 0, int length = 0);

        int AddOrUpdate(RadianContributor radianContributor);
        
        void RemoveRadianContributor(RadianContributor radianContributor);
    }

}