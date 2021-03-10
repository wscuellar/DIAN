using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Sql;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Gosocket.Dian.Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    [Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {

        }

        public string Code { get; set; }

        public string CreatedBy { get; set; }

        /// <summary>
        /// Fecha de creación del Usuario
        /// </summary>
        public DateTime? CreationDate { get; set; }

        public int? CurrentContributorId { get; set; }

        public string IdentificationId { get; set; }

        public int IdentificationTypeId { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public string ContributorCode { get; set; }

        public virtual ICollection<Contributor> Contributors { get; set; }

        /// <summary>
        /// Activar o descativar el Usuario. Por ahora solo aplica para Usuarios externos
        /// </summary>
        public byte Active { get; set; }

        /// <summary>
        /// Descripcion o razon por la cual se Ativo/Inactivo el Usuario externo
        /// </summary>
        public string ActiveDescription { get; set; }

        /// <summary>
        /// Quien actualizo el Usuario
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Ultima actualización del Usuario
        /// </summary>
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// Nit del Representate Legal o Persona Natural registrado en el Rut que crea el Usuario externo
        /// </summary>
        public string CreatorNit { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("SqlConnection", throwIfV1Schema: false)
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // To remove the requests to the Migration History table
            //modelBuilder.Entity<ApplicationUser>()
            //    .HasMany(user => user.Contributors).;
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(r => r.Contributors)
                .WithMany() // No navigation property here
                .Map(m =>
                {
                    m.MapLeftKey("UserId");
                    m.MapRightKey("ContributorId");
                    m.ToTable("UserContributors");
                });

            //Contributor-OperationMode one-to-one 
            modelBuilder.Entity<Contributor>()
            .HasMany<ContributorOperations>(s => s.ContributorOperations)
            .WithRequired( c => c.Contributor)
            .HasForeignKey<int?>(s => s.ContributorId);
            //modelBuilder.Entity<Contributor>().Ignore();
            // To remove the plural names   
            //Database.SetInitializer(new CreateDatabaseIfNotExists<SqlDBContext>());

            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<SqlDBContext, MigrateDBConfiguration>());
            //modelBuilder.Entity<ApplicationUser>().HasKey<string>(l => l.Id);
            //modelBuilder.Entity<IdentityUserRole>().HasKey<string>(l => l.Id);
            //modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.Id);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<Contributor> Contributor { set; get; }
        public DbSet<ContributorOperations> ContributorOperation { set; get; }

        public DbSet<UsersFreeBillerProfile> UserFreeBillerProfile { set; get; }
    }
}