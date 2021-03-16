using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class OthersDocumentsControllerTests
    {

        private OthersDocumentsController _current;  

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new OthersDocumentsController();
        }


        [TestMethod()]
        public void Index()
        {
            var result = _current.Index();
             
            Assert.IsNotNull(result);
        }
        [TestMethod()]
        public void AddOrUpdateTest()
        {
            var result = _current.AddOrUpdate();

            Assert.IsNotNull(result);
        }
    }
}