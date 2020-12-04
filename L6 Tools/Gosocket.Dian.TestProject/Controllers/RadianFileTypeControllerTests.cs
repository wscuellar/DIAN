using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Gosocket.Dian.Interfaces;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Castle.Components.DictionaryAdapter.Xml;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Domain;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianContributorFileTypeControllerTests
    {

        private RadianContributorFileTypeController _current;
        private readonly Mock<IRadianContributorFileTypeService> _RadianContributorFileTypeService = new Mock<IRadianContributorFileTypeService>();
        private readonly Mock<IRadianContributorService> _RadianContributorService = new Mock<IRadianContributorService>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianContributorFileTypeController(_RadianContributorFileTypeService.Object, _RadianContributorService.Object);
        }
    }
}