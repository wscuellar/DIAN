using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Moq;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Domain;
using System.Linq.Expressions;
using Gosocket.Dian.Domain.Common;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianContributorServiceTests
    {

        /*
        private readonly Mock<IContributorService> _contributorService = new Mock<IContributorService>();
        private readonly Mock<IRadianContributorRepository> _radianContributorRepository = new Mock<IRadianContributorRepository>();
        private readonly Mock<IRadianContributorTypeRepository> _radianContributorTypeRepository = new Mock<IRadianContributorTypeRepository>();
        private readonly Mock<IRadianContributorFileRepository> _radianContributorFileRepository = new Mock<IRadianContributorFileRepository>();
        private readonly Mock<IRadianTestSetResultManager> _radianTestSetResultManager = new Mock<IRadianTestSetResultManager>();
        private readonly Mock<IRadianOperationModeRepository> _radianOperationModeRepository = new Mock<IRadianOperationModeRepository>();
        private readonly Mock<IContributorOperationsService> _contributorOperationService = new Mock<IContributorOperationsService>();
        private readonly Mock<IRadianContributorFileHistoryRepository> _radianContributorFileHistoryRepository = new Mock<IRadianContributorFileHistoryRepository>();
        private RadianContributorService _current;


        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianContributorService(_contributorService.Object, _contributorOperationService.Object, _radianContributorRepository.Object, _radianContributorTypeRepository.Object, _radianContributorFileRepository.Object, _radianTestSetResultManager.Object, _radianOperationModeRepository.Object, _radianContributorFileHistoryRepository.Object);
        }


        [TestMethod]
        [DataRow("El usuario no existe en el sistema!!!", DisplayName = "Usuario no existe")]
        public void RegistrationValidation_ContributorNotExist_Test(string expected)
        {
            //arrange
            string userCode = string.Empty;
            Domain.Common.RadianContributorType radianContributorType = Domain.Common.RadianContributorType.ElectronicInvoice;
            Domain.Common.RadianOperationMode radianOperationMode = Domain.Common.RadianOperationMode.Direct;
            _contributorService.Setup(t => t.GetByCode(userCode)).Returns((Contributor)null);

            //add
            Domain.Entity.ResponseMessage result = _current.RegistrationValidation(userCode, radianContributorType, radianOperationMode);

            //assert
            Assert.AreEqual(expected, result.Message);
        }


        [TestMethod()]
        [DataRow(1, "El participante no cuenta con un software propio activo en el sistema", DisplayName = "Participante sin software habilitado sin valor de software")]
        [DataRow(2, "El participante no cuenta con un software propio activo en el sistema", DisplayName = "Participante sin software habilitado con lista vacia")]
        [DataRow(3, "El participante ya se encuentra registrado en RADIAN", DisplayName = "El participante no debe encontrarse registrado en el sistema RADIAN como Facturador Electrónico")]
        [DataRow(4, "¿Está seguro que desea habilitar  la trasmisión de eventos al RADIAN como Facturador Electrónico?", DisplayName = "Usuario Validado como Facturador electronico, con usuario nuevo")]
        [DataRow(5, "¿Está seguro que desea habilitar  la trasmisión de eventos al RADIAN como Facturador Electrónico?", DisplayName = "Usuario Validado como Facturador electronico, con usuario cancelado")]
        public void RegistrationValidation_QualificationRequest_ElectronicBiller_Direct_Test(int input, string expected)
        {
            //arrange
            string userCode = string.Empty;
            Domain.Common.RadianContributorType radianContributorType = Domain.Common.RadianContributorType.ElectronicInvoice;
            Domain.Common.RadianOperationMode radianOperationMode = Domain.Common.RadianOperationMode.Direct;
            switch (input)
            {
                case 1:
                    Contributor contributor = new Contributor();
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor.Id)).Returns(new List<ContributorOperations>());
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor);
                    break;
                case 2:
                    Contributor contributor2 = new Contributor();
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor2.Id)).Returns((List<ContributorOperations>)null);
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor2);
                    break;
                case 3:
                    //tiene software con estatus activo
                    Contributor contributor3 = new Contributor();
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor3);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor3.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });

                    //usuario registrado
                    RadianContributor radianContributor = new RadianContributor() { RadianState = "Registrado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor });
                    break;
                case 4:
                    //tiene software con estatus activo
                    Contributor contributor4 = new Contributor();
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor4);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor4.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });
                    //usuario no registrado
                    RadianContributor radianContributor4 = null;
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>());
                    break;
                case 5:
                    //tiene software con estatus activo
                    Contributor contributor5 = new Contributor();
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor5);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor5.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });
                    //usuario registrado pero cancelado
                    RadianContributor radianContributor5 = new RadianContributor() { RadianState = RadianState.Cancelado.GetDescription() };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor5 });
                    break;
            }

            //add
            Domain.Entity.ResponseMessage result = _current.RegistrationValidation(userCode, radianContributorType, radianOperationMode);

            //assert
            Assert.AreEqual(expected, result.Message);
        }

        [TestMethod()]
        [DataRow(1, "El participante ya se encuentra registrado en RADIAN", DisplayName = "Participante ya esta registrado")]
        [DataRow(2, "¿Está seguro que desea habilitar  la trasmisión de eventos al RADIAN como Facturador Electrónico?", DisplayName = "Usuario Validado como Facturador electronico, con usuario nuevo")]
        [DataRow(3, "¿Está seguro que desea habilitar  la trasmisión de eventos al RADIAN como Facturador Electrónico?", DisplayName = "Usuario Validado como Facturador electronico, con usuario cancelado")]
        public void RegistrationValidation_QualificationRequest_ElectronicBiller_Indirect_Test(int input, string expected)
        {
            //arrange
            string userCode = string.Empty;
            Domain.Common.RadianContributorType radianContributorType = Domain.Common.RadianContributorType.ElectronicInvoice;
            Domain.Common.RadianOperationMode radianOperationMode = Domain.Common.RadianOperationMode.Indirect;
            Contributor contributor = new Contributor();
            _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor);

            switch (input)
            {
                case 1:
                    //usuario registrado
                    RadianContributor radianContributor = new RadianContributor() { RadianState = "Registrado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor });
                    break;
                case 2:
                    //usuario no registrado
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>());
                    break;
                case 3:
                    //usuario registrado pero cancelado
                    RadianContributor radianContributor3 = new RadianContributor() { RadianState = "Cancelado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor3 });
                    break;
            }

            //add
            Domain.Entity.ResponseMessage result = _current.RegistrationValidation(userCode, radianContributorType, radianOperationMode);

            //assert
            Assert.AreEqual(expected, result.Message);
        }



        [TestMethod()]
        [DataRow(1, "El participante no cuenta con un software propio activo en el sistema", DisplayName = "Proveedor tecnologico sin software activo")]
        [DataRow(2, "El participante ya se encuentra registrado en RADIAN", DisplayName = "Proveedor tecnologico ya registrado")]
        [DataRow(3, "El participante no es un proveedor tecnológico habilitado", DisplayName = "Participante no proveedor")]
        [DataRow(4, "¿Está seguro que desea habilitar la trasmisión de eventos al RADIAN como Proveedor Tecnológico?", DisplayName = "Participante proveedor tecnologico habilitado")]
        public void RegistrationValidation_QualificationRequest_TechnologyProvider_Test(int input, string expected)
        {
            //arrange
            string userCode = string.Empty;
            Domain.Common.RadianContributorType radianContributorType = Domain.Common.RadianContributorType.TechnologyProvider;
            Domain.Common.RadianOperationMode radianOperationMode = Domain.Common.RadianOperationMode.Direct;
            switch (input)
            {
                case 1:
                    Contributor contributor = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Provider
                    };
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor);
                    break;
                case 2:
                    //Provedor activo con software activo
                    Contributor contributor2 = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Provider,
                        Status = true
                    };
                    //ya registrado en radian
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor2);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor2.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });
                    RadianContributor radianContributor = new RadianContributor() { RadianState = "Registrado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor });
                    break;
                case 3:
                    //Provedor activo con software inactivo
                    Contributor contributor3 = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Provider,
                        Status = false
                    };
                    //ya registrado en radian pero cancelado
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor3);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor3.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });
                    RadianContributor radianContributor3 = new RadianContributor() { RadianState = "Cancelado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor3 });
                    break;
                case 4:
                    //Provedor activo con software activo
                    Contributor contributor4 = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Provider,
                        Status = true
                    };
                    //ya registrado en radian pero cancelado
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor4);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor4.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });

                    RadianContributor radianContributor4 = new RadianContributor() { RadianState = "Cancelado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor4 });
                    break;
            }


            //add
            Domain.Entity.ResponseMessage result = _current.RegistrationValidation(userCode, radianContributorType, radianOperationMode);

            //assert
            Assert.AreEqual(expected, result.Message);
        }


        [TestMethod()]
        [DataRow(1, "El participante no cuenta con un software propio activo en el sistema", DisplayName = "Sistema de negociacion sin software activo")]
        [DataRow(2, "El participante ya se encuentra registrado en RADIAN", DisplayName = "Sistema de negociacion ya registrado")]
        [DataRow(3, "¿Está seguro que desea operar como Sistema de Negociación?", DisplayName = "Sistema de negociacion habilitado")]
        public void RegistrationValidation_QualificationRequest_TradingSystem_Test(int input, string expected)
        {
            //arrange
            string userCode = string.Empty;
            Domain.Common.RadianContributorType radianContributorType = Domain.Common.RadianContributorType.TradingSystem;
            Domain.Common.RadianOperationMode radianOperationMode = Domain.Common.RadianOperationMode.Direct;
            switch (input)
            {
                case 1:
                    Contributor contributor = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Biller
                    };
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor);
                    break;
                case 2:
                    //Provedor activo con software activo
                    Contributor contributor2 = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Provider,
                        Status = true
                    };
                    //ya registrado en radian
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor2);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor2.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });
                    RadianContributor radianContributor = new RadianContributor() { RadianState = "Registrado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor });
                    break;
                case 3:
                    //Provedor activo con software activo
                    Contributor contributor4 = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Provider,
                        Status = true
                    };
                    //ya registrado en radian pero cancelado
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor4);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor4.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });
                    RadianContributor radianContributor4 = new RadianContributor() { RadianState = "Cancelado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor4 });
                    break;

            }

            //add
            Domain.Entity.ResponseMessage result = _current.RegistrationValidation(userCode, radianContributorType, radianOperationMode);

            //assert
            Assert.AreEqual(expected, result.Message);
        }


        [TestMethod()]
        [DataRow(1, "El participante no cuenta con un software propio activo en el sistema", DisplayName = "Factor sin software activo")]
        [DataRow(2, "El participante ya se encuentra registrado en RADIAN", DisplayName = "Factor ya registrado")]
        [DataRow(3, "¿Está seguro que desea operar como Factor?", DisplayName = "Factor Habilitado")]
        public void RegistrationValidation_QualificationRequest_Factor_Test(int input, string expected)
        {
            //arrange
            string userCode = string.Empty;
            Domain.Common.RadianContributorType radianContributorType = Domain.Common.RadianContributorType.Factor;
            Domain.Common.RadianOperationMode radianOperationMode = Domain.Common.RadianOperationMode.Direct;
            switch (input)
            {
                case 1:
                    Contributor contributor = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Biller
                    };
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor);
                    break;
                case 2:
                    //Provedor activo con software activo
                    Contributor contributor2 = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Provider,
                        Status = true
                    };
                    //ya registrado en radian
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor2);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor2.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });
                    RadianContributor radianContributor = new RadianContributor() { RadianState = "Registrado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor });
                    break;
                case 3:
                    //Provedor activo con software activo
                    Contributor contributor4 = new Contributor()
                    {
                        ContributorTypeId = (int)Domain.Common.ContributorType.Provider,
                        Status = true
                    };
                    //ya registrado en radian pero cancelado
                    _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor4);
                    _contributorOperationService.Setup(t => t.GetContributorOperations(contributor4.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });
                    RadianContributor radianContributor4 = new RadianContributor() { RadianState = "Cancelado" };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>() { radianContributor4 });
                    break;

            }

            //add
            Domain.Entity.ResponseMessage result = _current.RegistrationValidation(userCode, radianContributorType, radianOperationMode);

            //assert
            Assert.AreEqual(expected, result.Message);
        }

        [TestMethod]
        public void RegistrationValidation_Fail_Test()
        {
            //arrange 
            string userCode = string.Empty;
            Domain.Common.RadianContributorType radianContributorType = Domain.Common.RadianContributorType.Zero;
            Domain.Common.RadianOperationMode radianOperationMode = Domain.Common.RadianOperationMode.Direct;
            Contributor contributor = new Contributor()
            {
                ContributorTypeId = (int)Domain.Common.ContributorType.Biller
            };
            _contributorService.Setup(t => t.GetByCode(userCode)).Returns(contributor);
            _contributorOperationService.Setup(t => t.GetContributorOperations(contributor.Id)).Returns(new List<ContributorOperations>()
                    {
                        new ContributorOperations()
                        {
                            OperationModeId = (int)Domain.Common.OperationMode.Own,
                            Software = new Software(){ Status = true}
                        }
                    });
            _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new List<RadianContributor>());
            //add
            Domain.Entity.ResponseMessage result = _current.RegistrationValidation(userCode, radianContributorType, radianOperationMode);

            //assert
            Assert.AreEqual("No se logro realizar la validación del participante a Registrar!!!", result.Message);

        }

        */
    }
}