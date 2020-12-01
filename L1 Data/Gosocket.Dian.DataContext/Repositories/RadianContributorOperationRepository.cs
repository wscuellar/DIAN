﻿using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class RadianContributorOperationRepository : IRadianContributorOperationRepository
    {
        private readonly SqlDBContext _sqlDBContext;
        private ResponseMessage responseMessage;

        public RadianContributorOperationRepository()
        {
            if (_sqlDBContext == null)
                _sqlDBContext = new SqlDBContext();
        }

        public List<RadianContributorOperation> List(Expression<Func<RadianContributorOperation, bool>> expression)
        {
            var query = _sqlDBContext.RadianContributorOperations.Include("RadianContributor").Include("Software")
                .Where(expression);
            return query.ToList();
        }

        public RadianContributorOperation Get(Expression<Func<RadianContributorOperation, bool>> expression)
        {
            var query = _sqlDBContext.RadianContributorOperations.Include("RadianContributor").Include("Software")
                .Where(expression);
            return query.FirstOrDefault();
        }

        public ResponseMessage Update(int radianContributorOperationId)
        {
            using (var context = new SqlDBContext())
            {
                var radianContributorOperationInstance = context.RadianContributorOperations
                    .FirstOrDefault(c => c.Id == radianContributorOperationId);

                if (radianContributorOperationInstance != null)
                {
                    radianContributorOperationInstance.Deleted = true;
                    context.Entry(radianContributorOperationInstance).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    responseMessage = new ResponseMessage("Datos actualizados correctamente", "Actualizado");
                }
                else
                {
                    responseMessage = new ResponseMessage("Registro no encontrado en la base de datos", "Nulo");
                }

                return responseMessage;
            }
        }

        public int Add(RadianContributorOperation contributorOperation)
        {
            int result = 0;
            using (var context = new SqlDBContext())
            {
                context.RadianContributorOperations.Add(contributorOperation);
                result = context.SaveChanges();
            }

            return result;
        }
    }
}