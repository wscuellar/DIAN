using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Gosocket.Dian.Web.Common;

namespace Gosocket.Dian.Web.Common
{
    public static class PrincipalExtensions
    {
        public static int? ContributorAcceptanceStatusId(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return null;
            if (claimsPrincipal.FindFirst(CustomClaimTypes.ContributorAcceptanceStatusId) == null)
                return null;
            return int.Parse(claimsPrincipal.FindFirst(CustomClaimTypes.ContributorAcceptanceStatusId)?.Value);
        }

        public static string ContributorCode(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return "Not found!";
            if (claimsPrincipal.FindFirst(CustomClaimTypes.ContributorCode) == null)
                return "0";
            return claimsPrincipal.FindFirst(CustomClaimTypes.ContributorCode)?.Value;
        }

        public static string ContributorBusinessName(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return "Not found!";
            if (claimsPrincipal.FindFirst(CustomClaimTypes.ContributorBusinesName) == null)
                return "Not found!";
            return claimsPrincipal.FindFirst(CustomClaimTypes.ContributorBusinesName)?.Value;
        }

        public static int ContributorId(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return 0;
            if (claimsPrincipal.FindFirst(CustomClaimTypes.ContributorId) == null)
                return 0;
            return int.Parse(claimsPrincipal.FindFirst(CustomClaimTypes.ContributorId).Value);
        }

        public static string ContributorName(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return "Not found!";
            if(claimsPrincipal.FindFirst(CustomClaimTypes.ContributorName) == null)
                return "Not found!";
            return claimsPrincipal.FindFirst(CustomClaimTypes.ContributorName)?.Value?.Capitalize();
        }

        public static int? ContributorOperationModeId(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return null;
            if (claimsPrincipal.FindFirst(CustomClaimTypes.ContributorOperationModeId) == null)
                return null;
            if (string.IsNullOrEmpty(claimsPrincipal.FindFirst(CustomClaimTypes.ContributorOperationModeId)?.Value))
                return null;
            return int.Parse(claimsPrincipal.FindFirst(CustomClaimTypes.ContributorOperationModeId)?.Value);
        }

        public static int? ContributorTypeId(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return null;
            if (claimsPrincipal.FindFirst(CustomClaimTypes.ContributorTypeId) == null)
                return null;
            if (string.IsNullOrEmpty(claimsPrincipal.FindFirst(CustomClaimTypes.ContributorTypeId)?.Value))
                return null;
            if (int.Parse(claimsPrincipal.FindFirst(CustomClaimTypes.ContributorTypeId)?.Value) == 0)
                return null;
            return int.Parse(claimsPrincipal.FindFirst(CustomClaimTypes.ContributorTypeId)?.Value);
        }

        public static bool GoToInvoicer(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return false;
            if (claimsPrincipal.FindFirst(CustomClaimTypes.GoToInvoicer) == null)
                return false;
            return bool.Parse(claimsPrincipal.FindFirst(CustomClaimTypes.GoToInvoicer)?.Value);
        }

        public static bool HasPrincipal(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return false;
            return true;
        }

        public static string UserName(this IPrincipal principal)
        {
            return principal ==  null ? string.Empty : principal.Identity.Name;
        }

        public static string UserCode(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return "Not found!";
            if (claimsPrincipal.FindFirst(CustomClaimTypes.UserCode) == null)
                return "0";
            return claimsPrincipal.FindFirst(CustomClaimTypes.UserCode)?.Value;
        }

        public static string UserFullName(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return "-";
            if (claimsPrincipal.FindFirst(CustomClaimTypes.UserFullName) == null)
                return "-";
            return claimsPrincipal.FindFirst(CustomClaimTypes.UserFullName)?.Value?.Capitalize();
        }

        public static string UserEmail(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return "Not found!";
            if (claimsPrincipal.FindFirst(CustomClaimTypes.UserEmail) == null)
                return "@";
            return claimsPrincipal.FindFirst(CustomClaimTypes.UserEmail)?.Value;
        }

        public static string Rol(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return "Not found!";
            return claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
        }

        public static bool ShowTestSet(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return false;
            if (claimsPrincipal.FindFirst(CustomClaimTypes.ShowTestSet) == null)
                return false;
            return bool.Parse(claimsPrincipal.FindFirst(CustomClaimTypes.ShowTestSet).Value);
        }

        public static bool IsInAnyRole(this IPrincipal principal, params string[] roles)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            if (roles == null)
                throw new ArgumentNullException(nameof(roles));

            return roles.Any(r => principal.IsInRole(r));
        }

        public static int IdentificationTypeId(this IPrincipal principal)
        {
            if (!(principal is ClaimsPrincipal claimsPrincipal))
                return 0;
            if (claimsPrincipal.FindFirst(CustomClaimTypes.IdentificationTypeId) == null)
                return 0;
            return int.Parse(claimsPrincipal.FindFirst(CustomClaimTypes.IdentificationTypeId).Value);
        }

    }
}