using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gosocket.Dian.TestProject.Functions.Validator
{
    [TestClass]
    public class AllTest
    {
       
        public async Task TestSuccessValidateDocument()
        {
            var trackId = "000b1180-a9a5-45b3-85cf-cd5b020d0651";
            var draft = "true";
            var request = new { trackId, draft };
            var url = ConfigurationManager.GetValue("ValidateDocumentUrl");
            var result = await ApiHelpers.ExecuteRequestAsync<List<GlobalDocValidatorTracking>>(url, request);
            Assert.IsTrue(result != null);
        }
    }
}
