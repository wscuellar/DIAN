using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Web.Controllers;
using Gosocket.Dian.Domain;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class IdentificationTypeServiceTests
    {
        private IdentificationTypeService _currentController; 

        [TestInitialize]
        public void TestInitialize()
        {
            _currentController = new IdentificationTypeService();
        }

        [TestMethod()]
        public void ListTest()
        {
            //arrange
            //act
            var viewResult = _currentController.List();
            //assert
            Assert.IsInstanceOfType(viewResult, typeof(IEnumerable<IdentificationType>)); 
        }
    }
}