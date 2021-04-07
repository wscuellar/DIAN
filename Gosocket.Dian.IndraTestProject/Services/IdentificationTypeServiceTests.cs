using Gosocket.Dian.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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