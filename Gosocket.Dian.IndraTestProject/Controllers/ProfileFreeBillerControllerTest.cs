using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql.FreeBiller;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Controllers;
using Gosocket.Dian.Web.Models.FreeBiller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Gosocket.Dian.IndraTestProject.Controllers
{
    /// <summary>
    /// Summary description for ProfileFreeBillerControllerTest
    /// </summary>
    [TestClass]
    public class ProfileFreeBillerControllerTest
    {
        private ProfileFreeBillerController _currentController;

        private readonly Mock<IProfileService> _profileService = new Mock<IProfileService>();

        [TestInitialize]
        public void TestInitialize()
        {
            _currentController = new ProfileFreeBillerController(_profileService.Object);
        }

        [TestMethod]
        public void Index()
        {
            //arrange
            //act
            var viewResult = _currentController.Index() as ViewResult;
            //assert
            Assert.IsNotNull(viewResult);
        }


        [TestMethod]
        public void CreateProfile_Result_Test()
        {
            //arrange
            _profileService.Setup(x => x.GetOptionsByProfile(0)).Returns(GetLMenuOptions());

            //act
            var viewResult = _currentController.CreateProfile() as ViewResult;

            //Assert
            Assert.IsInstanceOfType(viewResult.Model, typeof(ProfileFreeBillerModel));
            Assert.IsNotNull(viewResult);
            Assert.IsTrue(((ProfileFreeBillerModel)viewResult.Model).MenuOptionsByProfile.Count > 0);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Create No Exitoso")]
        [DataRow(2, DisplayName = "Create Exitoso")]
        public void CreateProfilePost_Result_Test(int input)
        {
            JsonResult _jsonResult;
            ResponseMessage result;
            ProfileFreeBillerModel model = new ProfileFreeBillerModel
            {
                ValuesSelected = new string[] { "1", "2", "3", "4" }
            };
            //arrange
            _profileService.Setup(x => x.CreateNewProfile(It.IsAny<Profile>())).Returns(GetProfile());
            if (input.Equals(1))
            {
                //arrange
                _profileService.Setup(x => x.SaveOptionsMenuByProfile(It.IsAny<List<MenuOptionsByProfiles>>())).Returns(false);
                //act
                _jsonResult = _currentController.CreateProfile(model);
                result = _jsonResult.Data as ResponseMessage;
                //Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("alert", result.MessageType);
                Assert.AreEqual("El perfil no fue creado!", result.Message);
                Assert.AreEqual(200, result.Code);
            }
            if (input.Equals(2))
            {
                //arrange
                _profileService.Setup(x => x.SaveOptionsMenuByProfile(It.IsAny<List<MenuOptionsByProfiles>>())).Returns(true);
                //act
                _jsonResult = _currentController.CreateProfile(model);
                result = _jsonResult.Data as ResponseMessage;
                //Assert
                Assert.IsNotNull(result);
                Assert.AreEqual("confirmation", result.MessageType);
                Assert.AreEqual("El perfil fue creado exitosamente", result.Message);
                Assert.AreEqual(200, result.Code);
            }

        }

        #region Private
        private List<MenuOptions> GetLMenuOptions()
        {
            return new List<MenuOptions>
            {
                GetMenuOption(1,"Uno"),
                GetMenuOption(2,"Dos"),
                GetMenuOption(3,"Tres"),
                GetMenuOption(4,"Cuatro"),
            };
        }

        private MenuOptions GetMenuOption(int Id, string Name)
        {
            var Menu = new MenuOptions
            {
                Id = Id,
                Name = Name,
                ParentId = 1,
                IsActive = true,
                MenuLevel = Id,
                Checked = true,
                Children = new List<MenuOptions>()
            };

            Menu.Children.Add(new MenuOptions { Id = 1100 + Id, Name = "Sub1-" + Name, ParentId = 1, IsActive = true, MenuLevel = Id, Checked = true });
            Menu.Children.Add(new MenuOptions { Id = 1200 + Id, Name = "Sub2-" + Name, ParentId = 1, IsActive = true, MenuLevel = Id, Checked = true });
            return Menu;
        }

        private Profile GetProfile()
        {
            return new Profile
            {
                Id = 1,
                Name = "NameProfile",
                IsEditable = true
            };
        }
        #endregion

    }
}