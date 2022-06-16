using Gosocket.Dian.Application;
using Gosocket.Dian.Application.FreeBiller;
using Gosocket.Dian.Application.Managers;
using Gosocket.Dian.DataContext.Repositories;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services; 
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace Gosocket.Dian.Web
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            #region Repositories

            container.RegisterType<IRadianContributorFileHistoryRepository, RadianContributorFileHistoryRepository>();
            container.RegisterType<IRadianContributorFileRepository, RadianContributorFileRepository>();
            container.RegisterType<IRadianContributorFileStatusRepository, RadianContributorFileStatusRepository>();
            container.RegisterType<IRadianContributorRepository, RadianContributorRepository>();
            container.RegisterType<IRadianContributorTypeRepository, RadianContributorTypeRepository>();
            container.RegisterType<IRadianContributorFileTypeRepository, RadianContributorFileTypeRepository>();
            container.RegisterType<IRadianOperationModeRepository, RadianOperationModeRepository>();
            container.RegisterType<IRadianContributorOperationRepository, RadianContributorOperationRepository>();
            container.RegisterType<IRadianSoftwareRepository, RadianSoftwareRepository>();
            container.RegisterType<IPermissionRepository, PermissionRepository>();
            container.RegisterType<IOthersDocsElecContributorRepository, OthersDocsElecContributorRepository>(); 
            container.RegisterType<IOthersDocsElecContributorOperationRepository, OthersDocsElecContributorOperationRepository>();
            container.RegisterType<IOthersDocsElecSoftwareRepository, OthersDocsElecSoftwareRepository>();
            container.RegisterType<IEquivalentElectronicDocumentRepository, EquivalentElectronicDocumentRepository>();


            #endregion

            #region Services

            container.RegisterType<IContributorService, ContributorService>();
            container.RegisterType<IRadianContributorService, RadianContributorService>();
            container.RegisterType<IProfileService, ProfileService>();
            container.RegisterType<IRadianTestSetService, RadianTestSetService>();
            container.RegisterType<IRadianContributorFileTypeService, RadianContributorFileTypeService>();
            container.RegisterType<IRadianApprovedService, RadianAprovedService>();
            container.RegisterType<IRadianTestSetAppliedService, RadianTestSetAppliedService>();
            container.RegisterType<IRadianLoggerService, RadianLoggerService>();
            container.RegisterType<IContributorOperationsService, ContributorOperationsService>();
            container.RegisterType<IRadianApprovedService, RadianAprovedService>();
            container.RegisterType<IPermissionService, PermissionService>();
            container.RegisterType<IRadianTestSetResultService, RadianTestSetResultService>();
            container.RegisterType<IRadianCallSoftwareService, RadianCallSoftwareService>();
            container.RegisterType<IGlobalDocValidatorDocumentService, GlobalDocValidatorDocumentService>();
            container.RegisterType<IGlobalDocValidatorTrackingService, GlobalDocValidatorTrackingService>();
            container.RegisterType<IQueryAssociatedEventsService, QueryAssociatedEventsService>();
            container.RegisterType<IRadianPdfCreationService, RadianPdfCreationService>();
            container.RegisterType<IRadianGraphicRepresentationService, RadianGraphicRepresentationService>();
            container.RegisterType<IRadianSupportDocument, RadianSupportDocument>();
            container.RegisterType<IGlobalRadianOperationService, GlobalRadianOperationService>();
            container.RegisterType<IOthersElectronicDocumentsService, OthersElectronicDocumentsService>();
            container.RegisterType<IOthersDocsElecContributorService, OthersDocsElecContributorService>();
            container.RegisterType<IOthersDocsElecSoftwareService, OthersDocsElecSoftwareService>();
            container.RegisterType<IGlobalOtherDocElecOperationService, GlobalOtherDocElecOperationService>();
            container.RegisterType<ITestSetOthersDocumentsResultService, TestSetOthersDocumentsResultService>();
            container.RegisterType<IGlobalDocPayrollService, GlobalDocPayrollService>();
            container.RegisterType<IRadianPayrollGraphicRepresentationService, RadianPayrollGraphicRepresentationService>();
            container.RegisterType<IElectronicDocumentService, ElectronicDocumentService>();
            container.RegisterType<IAssociateDocuments, AssociateDocumentService>();
            container.RegisterType<IGlobalAuthorizationService, GlobalAuthorizationService>();

            #endregion

            #region Managers

            container.RegisterType<IRadianTestSetResultManager, RadianTestSetResultManager>();
            container.RegisterType<IRadianTestSetManager, RadianTestSetManager>();
            container.RegisterType<IRadianLoggerManager, RadianLoggerManager>();
            container.RegisterType<IGlobalDocValidationDocumentMetaService, GlobalDocValidationDocumentMetaService>();
            container.RegisterType<ITestSetOthersDocumentsResultManager, TestSetOthersDocumentsResultManager>();

            #endregion

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}