using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class AssociateDocumentServiceTests
    {
        [TestMethod()]
        public void GetEventsByTrackIdTest()
        {
            //arrange
            
            string trackid = "561d4764aac2cb4163c370f24143853810ad12af22d424134eef81e26c1096d67c4f995ff5871263758164c1dd73343d";
            AssociateDocumentService current = new AssociateDocumentService();

            //act
            var result = current.GetEventsByTrackId(trackid);
            //assert

        }
    }
}