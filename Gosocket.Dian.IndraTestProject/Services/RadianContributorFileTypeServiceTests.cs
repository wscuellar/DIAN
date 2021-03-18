using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianContributorFileTypeServiceTests
    {
        private RadianContributorFileTypeService _current;

        private readonly Mock<IRadianContributorFileTypeRepository> _radianContributorFileTypeRepository = new Mock<IRadianContributorFileTypeRepository>();
        private readonly Mock<IRadianContributorTypeRepository> _radianContributorTypeRepository = new Mock<IRadianContributorTypeRepository>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianContributorFileTypeService(_radianContributorFileTypeRepository.Object, _radianContributorTypeRepository.Object);
        }

        [TestMethod()]
        public void FileTypeListTest()
        {
            //arrange  
            _ = _radianContributorFileTypeRepository.Setup(x => x.FileTypeCounter())
                .Returns(new List<KeyValue> {
                    new KeyValue() { Key = 7, value = 70 },
                    new KeyValue() { Key = 8, value = 80 }
                });
            _ = _radianContributorFileTypeRepository.Setup(x => x.List(It.IsAny<Expression<Func<RadianContributorFileType, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<RadianContributorFileType> {
                    new RadianContributorFileType() { Id = 7 },
                    new RadianContributorFileType() { Id = 8 }
                });
            //act
            var Result = _current.FileTypeList();
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<RadianContributorFileType>));
            Assert.IsTrue(Result.Count == 2);
        }

        [TestMethod()]
        public void ContributorTypeListTest()
        {
            //arrange  
            _ = _radianContributorTypeRepository.Setup(x => x.List(It.IsAny<Expression<Func<RadianContributorType, bool>>>()))
               .Returns(new List<RadianContributorType>  {
                    new RadianContributorType() { Id = 71 },
                    new RadianContributorType() { Id = 81 }
               });
            //act
            var Result = _current.ContributorTypeList();
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<RadianContributorType>));
            Assert.IsTrue(Result.Count == 2);
        }

        [TestMethod()]
        public void FilterTest()
        {
            string name = "nameFilterTest";
            string selectedRadianContributorTypeId = "7";
            //arrange  
            _ = _radianContributorFileTypeRepository.Setup(x => x.List(It.IsAny<Expression<Func<RadianContributorFileType, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
               .Returns(new List<RadianContributorFileType>  {
                    new RadianContributorFileType() { Id = 72 },
                    new RadianContributorFileType() { Id = 82 }
               });
            //act
            var Result = _current.Filter(name, selectedRadianContributorTypeId);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<RadianContributorFileType>));
            Assert.IsTrue(Result.Count == 2);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            //arrange  
            RadianContributorFileType entity = new RadianContributorFileType();
            _ = _radianContributorFileTypeRepository.Setup(x => x.AddOrUpdate(It.IsAny<RadianContributorFileType>())).Returns(777);
            //act
            var Result = _current.Update(entity);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsTrue(Result.Equals(777));
        }

        [TestMethod()]
        public void GetTest()
        {
            int id = 0;
            //arrange   
            _ = _radianContributorFileTypeRepository.Setup(x => x.Get(It.IsAny<int>())).Returns(new RadianContributorFileType() { Id = 8 });
            //act
            var Result = _current.Get(id); 
            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(RadianContributorFileType));
            Assert.IsTrue(Result.Id.Equals(8));
        }

        [TestMethod()]
        public void IsAbleForDeleteTest()
        {
            RadianContributorFileType radianContributorFileType = new RadianContributorFileType();
            //arrange   
            _ = _radianContributorFileTypeRepository.Setup(x => x.IsAbleForDelete(It.IsAny<RadianContributorFileType>())).Returns(true);
            //act
            var Result = _current.IsAbleForDelete(radianContributorFileType);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsTrue(Result);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            RadianContributorFileType radianContributorFileType = new RadianContributorFileType();
            //arrange   
            _ = _radianContributorFileTypeRepository.Setup(x => x.Delete(It.IsAny<RadianContributorFileType>())).Returns(2021);
            //act
            var Result = _current.Delete(radianContributorFileType);
            //assert
            Assert.IsNotNull(Result);
            Assert.IsTrue(Result.Equals(2021));
        }
    }
}