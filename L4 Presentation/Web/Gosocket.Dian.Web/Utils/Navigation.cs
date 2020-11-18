using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Utils
{
    public static class Navigation
    {
        public enum NavigationEnum
        {
            Undefined,
            ConfigurationIndex,
            Dashboard,
            AddNumberRange,
            BigContributors,
            Contingencies,
            DocumentIndex,
            DocumentValidatorIndex,
            DocumentValidatorCategorias,
            DocumentValidatorReglas,
            DocumentValidatorEsquemas,
            DocumentValidatorListas,
            DocumentValidatorPlugins,
            DocumentValidatorTipoDoc,
            DocumentValidatorCheck,
            DocumentValidator,
            DocumentValidatorBI,
            DocumentDetails,
            DocumentExport,
            DocumentList,
            DocumentSent,
            DocumentReceived,
            DocumentProvider,
            ClientIndex,
            Client,
            Provider,
            Software,
            Adquirentes,
            Bi,
            BiGlobals,
            BiOFE,
            BiNSU,
            LegalRepresentative,
            ProviderFileType,
            ContributorFileType,
            HFE,
            NumberRange,
            ProviderAuthorized,
            Invoice,
            TestSet,
            UserView,
            StatisticsBI,
            Users,
            RADIAN,
            RadianContributorFileType,
            RadianSetPruebas,
            AdminRadian,
            /// <summary>
            /// Opción de Menu, creación de Usuarios externos
            /// </summary>
            ExternalUsersCreate
        }
    }
}