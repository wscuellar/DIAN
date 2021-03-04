using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.FreeBiller;
using Gosocket.Dian.Web.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosocket.Dian.TestProject2.Controllers.FreeBiller
{
    [TestClass]
    public class CreateUserTest
    {
        private FakeRepository fakeRepository;
        public CreateUserTest()
        {
            fakeRepository = new FakeRepository();
        }

        private readonly UserService userService = new UserService();

       // [TestMethod]
        public void CreateUser_ValidationTypeDoc()
        {
            UserFreeBillerModel model = this.CreatingWrongModel();
            var results = TestModelHelper.Validate(model);

            if (Convert.ToInt32(model.TypeDocId) <= 0)
            {
                results.Add(this.AddValidationModel("Por favor seleccione el Tipo de Documento"));
            }

            Assert.IsTrue(results.Count > 0);
        }

       // [TestMethod]
        public void CreateUser_ValidationExistingUser() 
        {
            UserFreeBillerModel model = this.CreatingRightModel();
            var results = TestModelHelper.Validate(model);

            ApplicationUser vUserDB = this.fakeRepository.FindUserByIdentificationAndTypeId(Convert.ToInt32(model.TypeDocId), model.NumberDoc);
            if (vUserDB != null)
            {
                results.Add(this.AddValidationModel("Ya existe un Usuario con el Tipo de Documento y Documento suministrados"));
            }

            Assert.IsTrue(results.Count > 0);
        }

        //[TestMethod]
        public void CreateUser_CreateUserOK()
        {
            UserFreeBillerModel model = this.CreatingRightModel();
            var validationResults = TestModelHelper.Validate(model);
            var result = this.fakeRepository.CreateOk(new ApplicationUser { Code = "111222333", Name = "pruebas pruebas" }, "123456");

            if (result.Errors.Count() <= 0)
            {
                // Revisar calse estatica para colocar el valor del nuevo rol.
                var resultRole = this.fakeRepository.AddToRoleOk(model.Id, "TODOS");

                if (resultRole.Errors.Count() > 0)
                {
                    validationResults.Add(AddValidationModel("El Usario no puedo ser asignado al role 'Usuario Facturador Gratuito'"));
                }
            }
            else {
                validationResults.Add(AddValidationModel("No se pudo Registrar el Usuario!'"));
            }
            Assert.AreEqual(0, validationResults.Count);
        }

      //  [TestMethod]
        public void CreateUser_CreateUserFailUser()
        {
            UserFreeBillerModel model = this.CreatingRightModel();
            var validationResults = TestModelHelper.Validate(model);
            var result = this.fakeRepository.CreateFail(new ApplicationUser { Code = "111222333", Name = "pruebas pruebas" }, "123456");

            if (result.Errors.Count() <= 0)
            {
                // Revisar calse estatica para colocar el valor del nuevo rol.
                var resultRole = this.fakeRepository.AddToRoleOk(model.Id, "TODOS");

                if (resultRole.Errors.Count() > 0)
                {
                    validationResults.Add(AddValidationModel("El Usario no puedo ser asignado al role 'Usuario Facturador Gratuito'"));
                }
            }
            else
            {
                validationResults.Add(AddValidationModel("No se pudo Registrar el Usuario!'"));
            }
            Assert.IsTrue(validationResults.Count > 0);
        }

        //[TestMethod]
        public void CreateUser_CreateUserFailRole()
        {
            UserFreeBillerModel model = this.CreatingRightModel();
            var validationResults = TestModelHelper.Validate(model);
            var result = this.fakeRepository.CreateOk(new ApplicationUser { Code = "111222333", Name = "pruebas pruebas" }, "123456");

            if (result.Errors.Count() > 0)
            {
                // Revisar calse estatica para colocar el valor del nuevo rol.
                var resultRole = this.fakeRepository.AddToRoleFail(model.Id, "TODOS");

                if (resultRole.Errors.Count() > 0)
                {
                    validationResults.Add(AddValidationModel("El Usario no puedo ser asignado al role 'Usuario Facturador Gratuito'"));
                }
            }
            else
            {
                validationResults.Add(AddValidationModel("No se pudo Registrar el Usuario!'"));
            }
            Assert.IsTrue(validationResults.Count > 0);
        }


        private new ValidationResult AddValidationModel(string message) 
        {
            return new ValidationResult(message);
        }

        private UserFreeBillerModel CreatingWrongModel()
        {
            return new UserFreeBillerModel
            {
                Id = string.Empty,
                FullName = string.Empty,
                DescriptionTypeDoc = string.Empty,
                DescriptionProfile = string.Empty,
                NumberDoc = string.Empty,
                LastUpdate = null,
                IsActive = false
            };
        }

        private UserFreeBillerModel CreatingRightModel()
        {
            return new UserFreeBillerModel
            {
                Id = "7713a5d0-fe76-4e9a-b704-d05a7d7b7f07",
                FullName = "Pepito Perez",
                DescriptionTypeDoc = "1",
                DescriptionProfile = "Todos",
                NumberDoc = "111222333",
                LastUpdate = DateTime.Now,
                IsActive = true
            };
        }
    }
}
