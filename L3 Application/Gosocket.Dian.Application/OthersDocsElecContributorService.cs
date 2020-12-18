using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class OthersDocsElecContributorService : IOthersDocsElecContributorService
    {
        private SqlDBContext sqlDBContext;
        private readonly IContributorService _contributorService;
        private readonly IOthersDocsElecContributorRepository _othersDocsElecContributorRepository;

        public OthersDocsElecContributorService(IContributorService contributorService, IOthersDocsElecContributorRepository othersDocsElecContributorRepository)
        {
            _contributorService = contributorService;
            _othersDocsElecContributorRepository = othersDocsElecContributorRepository;
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }


        public List<Gosocket.Dian.Domain.Sql.OtherDocElecOperationMode> GetOperationModes()
        {
            using (var context = new SqlDBContext())
            {
                return context.OtherDocElecOperationModes.ToList();
            }
        }

        public NameValueCollection Summary(string userCode)
        {
            NameValueCollection collection = new NameValueCollection();
            Domain.Contributor contributor = _contributorService.GetByCode(userCode);
            List<OtherDocElecContributor> LContributors = _othersDocsElecContributorRepository.List(t => t.ContributorId == contributor.Id && t.State != "Cancelado").Results;
            if (LContributors.Any())
                foreach (var radianContributor in LContributors)
                {
                    string key = Enum.GetName(typeof(Domain.Common.RadianContributorType), radianContributor.OtherDocElecContributorTypeId);
                    collection.Add(key + "_OtherDocElecContributorTypeId", radianContributor.OtherDocElecContributorTypeId.ToString());
                    collection.Add(key + "_OtherDocElecOperationModeId", radianContributor.OtherDocElecOperationModeId.ToString());
                }
            if (contributor == null) return collection;
            collection.Add("ContributorId", contributor.Id.ToString());
            collection.Add("ContributorTypeId", contributor.ContributorTypeId.ToString());
            collection.Add("Active", contributor.Status.ToString());
            return collection;
        }

        /// <summary>
        /// Cancelar un registro en la tabla OtherDocElecContributor
        /// </summary>
        /// <param name="contributorId">OtherDocElecContributorId</param>
        /// <param name="description">Motivo por el cual se hace la cancelación</param>
        /// <returns></returns>
        public ResponseMessage CancelRegister(int contributorId, string description)
        {
            ResponseMessage result = new ResponseMessage();

            var contributor = sqlDBContext.OtherDocElecContributors.FirstOrDefault(c => c.Id == contributorId);

            if (contributor != null)
            {
                contributor.State = "Cancelado";
                contributor.Update = DateTime.Now;
                contributor.Description = description;

                sqlDBContext.Entry(contributor).State = System.Data.Entity.EntityState.Modified;

                int re1 = sqlDBContext.SaveChanges();
                result.Code = System.Net.HttpStatusCode.OK.GetHashCode();
                result.Message = "Se cancelo el registro exitosamente";

                if(re1 > 0) //Update operations state
                {
                    var contriOpera = sqlDBContext.OtherDocElecContributorOperations.FirstOrDefault(c => c.OtherDocElecContributorId == contributorId);
                    Guid softId;

                    if (contriOpera != null)
                    {
                        softId = contriOpera.SoftwareId;

                        contriOpera.Deleted = true;

                        sqlDBContext.Entry(contriOpera).State = System.Data.Entity.EntityState.Modified;

                        int re2 = sqlDBContext.SaveChanges();
                        
                        if (re2 > 0) //Update operations SUCCESS
                        {
                            var contriSoftware = sqlDBContext.OtherDocElecSoftwares.FirstOrDefault(c => c.OtherDocElecContributorId == contributorId && c.Id == softId);

                            if (contriSoftware != null)
                            {
                                contriSoftware.Deleted = true;
                                contriSoftware.Status = true;
                                contriSoftware.Updated = DateTime.Now;

                                sqlDBContext.Entry(contriOpera).State = System.Data.Entity.EntityState.Modified;

                                int re3 = sqlDBContext.SaveChanges();

                                if (re3 > 0) //Update Software SUCCESS
                                {

                                }
                            }
                        }
                    }

                }
            }
            else
            {
                //sqlDBContext.Entry(contributor).State = System.Data.Entity.EntityState.Added;
                result.Code = System.Net.HttpStatusCode.NotFound.GetHashCode();
                result.Message = System.Net.HttpStatusCode.NotFound.ToString();
            }

            return result;

        }
    }
}
