using System;
using System.Collections.Generic;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Domain.Entity;
using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Application.Managers
{
    public class DocumentTypeManager
    {
        private static readonly TableManager tableManager = new TableManager("GlobalDocumentType");
        private readonly string _category;
        private static DocumentTypeManager _instance = null;
        public DocumentTypeManager() { }

        public DocumentTypeManager(string category)
        {
            _category = category;
        }

        public static DocumentTypeManager Instance => _instance ?? (_instance = new DocumentTypeManager());

        public bool AddOrUpdate(GlobalDocumentType documentType)
        {
            return tableManager.InsertOrUpdate(documentType);
        }
        
        public GlobalDocumentType Get(string partitionKey, string rowKey)
        {
            return tableManager.Find<GlobalDocumentType>(partitionKey, rowKey);
        }

        public List<GlobalDocumentType> GetAll()
        {

            TableContinuationToken token = null;
            var rules = new List<GlobalDocumentType>();
            do
            {
                var data = tableManager.GetRangeRows<GlobalDocumentType>(1000, token);
                token = data.Item2;
                rules.AddRange(data.Item1);
            }
            while (token != null);
            return rules;
        }

        public List<GlobalDocumentType> List()
        {
            TableContinuationToken token = null;
            var documentTypes = new List<GlobalDocumentType>();
            do
            {
                var data = tableManager.GetRangeRows<GlobalDocumentType>($"{_category}", 1000, token);
                token = data.Item2;
                documentTypes.AddRange(data.Item1);
            }
            while (token != null);

            return documentTypes;
        }
    }
}
