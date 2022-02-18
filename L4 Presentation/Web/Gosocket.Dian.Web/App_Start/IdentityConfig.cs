using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Web.Common;
using Gosocket.Dian.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Gosocket.Dian.Web
{
	public class EmailService : IIdentityMessageService
	{
		public Task SendAsync(IdentityMessage message)
		{
			// Plug in your email service here to send an email.
			MailMessage correo = new MailMessage();
			correo.From = new MailAddress("camilo.lizarazo87@gmail.com");
			correo.To.Add("calizarazo@indracompany.com");
			correo.Subject = "asunto";
			correo.Body = "<h1>mensaje</1>";
			correo.IsBodyHtml = true;
			correo.Priority = MailPriority.Normal;


			SmtpClient smtp = new SmtpClient();
			smtp.Host = "smtp.gmail.com";
			smtp.Port = 25;
			smtp.UseDefaultCredentials = true;
			smtp.Credentials = new System.Net.NetworkCredential("camilo.lizarazo87@gmail.com", "87101172447");
			smtp.Send(correo);
			return Task.FromResult(0);
		}
	}

	public class SmsService : IIdentityMessageService
	{
		public Task SendAsync(IdentityMessage message)
		{
			// Plug in your SMS service here to send a text message.
			return Task.FromResult(0);
		}
	}

	// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
	public class ApplicationUserManager : UserManager<ApplicationUser>
	{

		public ApplicationUserManager(IUserStore<ApplicationUser> store)
			: base(store)
		{
		}

		public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
		{
			var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
			// Configure validation logic for usernames
			manager.UserValidator = new UserValidator<ApplicationUser>(manager)
			{
				AllowOnlyAlphanumericUserNames = false,
				RequireUniqueEmail = true
			};

			// Configure validation logic for passwords
			manager.PasswordValidator = new PasswordValidator
			{
				RequiredLength = 6,
				RequireNonLetterOrDigit = true,
				RequireDigit = true,
				RequireLowercase = true,
				RequireUppercase = true,
			};

			// Configure user lockout defaults
			manager.UserLockoutEnabledByDefault = true;
			manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
			manager.MaxFailedAccessAttemptsBeforeLockout = 5;

			// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
			// You can write your own provider and plug it in here.
			manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
			{
				MessageFormat = "Your security code is {0}"
			});
			manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
			{
				Subject = "Security Code",
				BodyFormat = "Your security code is {0}"
			});
			manager.EmailService = new EmailService();
			manager.SmsService = new SmsService();
			var dataProtectionProvider = options.DataProtectionProvider;
			if (dataProtectionProvider != null)
			{
				manager.UserTokenProvider =
					new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
			}
			return manager;
		}
	}

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {        
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }
        private ContributorService contributorService = new ContributorService();

		public override async Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
		{
			var current = await user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);

			// User claims
			current.AddClaim(new Claim(CustomClaimTypes.UserCode, user.Code));
			current.AddClaim(new Claim(CustomClaimTypes.UserFullName, user.Name));

			if (string.IsNullOrEmpty(user.ContributorCode))
				return current;

            var dianAuthTableManager = new TableManager("AuthToken");
            var auth = dianAuthTableManager.Find<AuthToken>(user.Code, user.ContributorCode);

            Contributor currentContributor = user.Contributors.FirstOrDefault(x => x.Code == user.ContributorCode);

            currentContributor = currentContributor == null ? contributorService.GetByCode(user.ContributorCode): currentContributor;

            // Contributor claims
            current.AddClaim(new Claim(CustomClaimTypes.ContributorAcceptanceStatusId, currentContributor.AcceptanceStatusId.ToString()));
            current.AddClaim(new Claim(CustomClaimTypes.ContributorId, currentContributor.Id.ToString()));
            current.AddClaim(new Claim(CustomClaimTypes.ContributorBusinesName, currentContributor.BusinessName));
            current.AddClaim(new Claim(CustomClaimTypes.ContributorCode, currentContributor.Code));
            current.AddClaim(new Claim(CustomClaimTypes.ContributorName, currentContributor.Name));
            current.AddClaim(new Claim(CustomClaimTypes.ContributorOperationModeId, currentContributor.OperationModeId.ToString()));
            current.AddClaim(new Claim(CustomClaimTypes.ContributorTypeId, currentContributor.ContributorTypeId.ToString()));
            current.AddClaim(new Claim(CustomClaimTypes.IdentificationTypeId, user.IdentificationTypeId.ToString()));

			var otherCont= this.contributorService.GetOtherDocElecContributor(currentContributor.Id);

			// Invoicer claims
			bool goToInvoicer = false;
			if (currentContributor != null)
			{
				if(currentContributor.AcceptanceStatusId == (int)Domain.Common.ContributorStatus.Registered || currentContributor.AcceptanceStatusId == (int)Domain.Common.ContributorStatus.Enabled)
				{
					{ goToInvoicer = (currentContributor.ContributorOperations.FirstOrDefault(x => x.OperationModeId == (int)Domain.Common.OperationMode.Free && !x.Deleted) != null); }
				}
                if (otherCont.Any(x => x.State == "Registrado") || otherCont.Any(x => x.State == "Habilitado") || otherCont.Any(x => x.State == "En pruebas"))
                {
					if (!goToInvoicer)
					{
						foreach (var item in otherCont)
						{
							goToInvoicer = this.contributorService.GetOtherDocElecContributorOperations(item.Id);
						}
					}

				}
			}

			current.AddClaim(new Claim(CustomClaimTypes.GoToInvoicer, goToInvoicer.ToString()));


			// Test set clamis
			if (currentContributor != null)
				current.AddClaim(new Claim(CustomClaimTypes.ShowTestSet, (currentContributor.AcceptanceStatusId == (int)Domain.Common.ContributorStatus.Registered && currentContributor.OperationModeId == (int)Domain.Common.OperationMode.Own).ToString()));

			return current;
		}

		public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
		{
			return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
		}
	}
}
