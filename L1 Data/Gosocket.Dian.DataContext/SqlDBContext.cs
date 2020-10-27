﻿using Gosocket.Dian.Domain;
using Gosocket.Dian.Infrastructure;
using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Gosocket.Dian.DataContext
{
    [DbConfigurationType(typeof(AzureDbConfiguration))]
    public class SqlDBContext : DbContext
    {

        public SqlDBContext()
           : base(ConfigurationManager.GetValue("SqlConnection"))
        {

        }

        public SqlDBContext(string conectionString)
            : base(conectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // To remove the requests to the Migration History table
            Database.SetInitializer<SqlDBContext>(null);
            base.OnModelCreating(modelBuilder);

            #region Contributor relations
            //Contributor-AcceptanceStatus one-to-one 
            modelBuilder.Entity<Contributor>()
            .HasRequired<AcceptanceStatus>(s => s.AcceptanceStatus)
            .WithMany()
            .HasForeignKey<int>(s => s.AcceptanceStatusId);

            //Contributor-OperationMode one-to-one 
            modelBuilder.Entity<Contributor>()
            .HasOptional<OperationMode>(s => s.OperationMode)
            .WithMany()
            .HasForeignKey<int?>(s => s.OperationModeId);

            //Contributor-ContributorType one-to-one 
            modelBuilder.Entity<Contributor>()
            .HasOptional<ContributorType>(s => s.ContributorType)
            .WithMany()
            .HasForeignKey<int?>(s => s.ContributorTypeId);

            //Contributor - Contributor one-to-many Provider as many Clients
            modelBuilder.Entity<Contributor>()
            .HasOptional<Contributor>(s => s.Provider)
            .WithMany(g => g.Clients)
            .HasForeignKey<int?>(s => s.ProviderId);

            // Contributor has many Software 
            modelBuilder.Entity<Contributor>()
                .HasMany<Software>(c => c.Softwares)
                .WithMany();
                
            #endregion

            #region ContributorFileHistory
            //ContributorFileHistory - ContributorFile one-to-many Provider as many Clients
            modelBuilder.Entity<ContributorFileHistory>()
            .HasRequired<ContributorFile>(s => s.ContributorFile)
            .WithMany()
            .HasForeignKey<Guid>(s => s.ContributorFileId);
            #endregion

            #region Software Relations
            modelBuilder.Entity<Software>()
            .HasRequired<Contributor>(s => s.Contributor)
            .WithMany(g => g.Softwares)
            .HasForeignKey<int>(s => s.ContributorId);
            #endregion

            #region ContributorOperations
            modelBuilder.Entity<ContributorOperations>()
                .ToTable("ContributorOperations")
                .HasKey(co => co.Id);

            modelBuilder.Entity<ContributorOperations>()
                .HasRequired(co => co.Contributor)
                .WithMany(c => c.ContributorOperations)
                .HasForeignKey(co => co.ContributorId);

            modelBuilder.Entity<ContributorOperations>()
               .HasRequired(co => co.OperationMode)
               .WithMany()
               .HasForeignKey(co => co.OperationModeId);

            modelBuilder.Entity<ContributorOperations>()
              .HasOptional(co => co.Provider)
              .WithMany()
              .HasForeignKey(co => co.ProviderId);

            modelBuilder.Entity<ContributorOperations>()
              .HasOptional(co => co.Software)
              .WithMany()
              .HasForeignKey(co => co.SoftwareId);
            #endregion
        }

        public DbSet<Software> Softwares { set; get; }
        public DbSet<AcceptanceStatus> AcceptanceStatuses { set; get; }
        public DbSet<AcceptanceStatusSoftware> AcceptanceStatusesSoftware { set; get; }
        public DbSet<IdentificationType> IdentificationTypes { set; get; }

        ////Files
        public DbSet<ContributorFile> ContributorFiles { set; get; }
        public DbSet<ContributorFileHistory> ContributorFileHistories { set; get; }
        public DbSet<ContributorFileType> ContributorFileTypes { set; get; }
        public DbSet<ContributorFileStatus> ContributorFileStatuses { set; get; }
        //Files

        public DbSet<Contributor> Contributors { set; get; }
        public DbSet<UserContributors> UserContributors { set; get; }
        public DbSet<ContributorType> ContributorType { set; get; }
        public DbSet<ContributorOperations> ContributorOperations { set; get; }
        public DbSet<OperationMode> OperationModes { set; get; }
        public DbSet<RadianContributor> RadianContributors { set; get; }
    }

    public class MigrateDBConfiguration : System.Data.Entity.Migrations.DbMigrationsConfiguration<SqlDBContext>
    {
        public MigrateDBConfiguration()
        {
            AutomaticMigrationsEnabled = true;
        }
    }
    public class AzureDbConfiguration : DbConfiguration
    {
        public AzureDbConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy(100, TimeSpan.FromMilliseconds(1000)));
        }
    }
}
