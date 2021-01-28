using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Managers;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class RadianTestSetAppliedService : IRadianTestSetAppliedService
    {
        private readonly IRadianTestSetResultManager _radianTestSetResultManager;

        public RadianTestSetAppliedService(IRadianTestSetResultManager radianTestSetResultManager)
        {
            _radianTestSetResultManager = radianTestSetResultManager;
        }

        public List<RadianTestSetResult> GetAllTestSetResult()
        {
            return _radianTestSetResultManager.GetAllTestSetResult().ToList();
        }

        /// <summary>
        /// Obtener un TestSetResult
        /// </summary>
        /// <param name="partitionKey">nit contributor / SenderCode </param>
        /// <param name="rowKey">RadianContributorTypeId + | + SoftwareId</param>
        /// <returns></returns>
        public RadianTestSetResult GetTestSetResult(string partitionKey, string rowKey)
        {
            return _radianTestSetResultManager.GetTestSetResult(partitionKey, rowKey);
        }

        /// <summary>
        /// Este metodo crea o actualiza un registro en estado En proceso, si es nuevo, es obligatorio los atributos
        /// ContributorTypeId, SoftwareId y SenderCode que es el nit del radianContributor
        /// </summary>
        /// <param name="radianTestSet">El objeto con los valores a insertar o modificar</param>
        /// <returns>Un bool true si fue OK, false si ocurrio algun error</returns>
        public bool InsertOrUpdateTestSet(RadianTestSetResult radianTestSet)
        {

            bool result = false;

            // Validate ContributorTypeId its correct
            if (string.IsNullOrEmpty(radianTestSet.ContributorTypeId))
                return result;
            // validate SoftwareId its correct
            if (string.IsNullOrEmpty(radianTestSet.SoftwareId))
                return result;
            // Validate nit its correct
            if (string.IsNullOrEmpty(radianTestSet.SenderCode))
                return result;


            // validate partitionKey, its empty fill with sender code
            if (string.IsNullOrEmpty(radianTestSet.PartitionKey))
            {
                radianTestSet.PartitionKey = radianTestSet.SenderCode;
            }
            // VAlidate RowKey and create its not exist
            if (string.IsNullOrEmpty(radianTestSet.RowKey))
            {
                radianTestSet.RowKey = radianTestSet.ContributorTypeId + '|' + radianTestSet.SoftwareId;
            }

            // Verifico si existe o no
            if (GetTestSetResult(radianTestSet.PartitionKey, radianTestSet.RowKey) == null)
            {
                radianTestSet.State = "En proceso";
            }
            
            return _radianTestSetResultManager.InsertOrUpdateTestSetResult(radianTestSet);
        }


        public bool ResetPreviousCounts(string testSetId)
        {
            return _radianTestSetResultManager.ResetPreviousCounts(testSetId);
        }
    }
}
