using Microsoft.EntityFrameworkCore;
using NgrApi.Models;

namespace NgrApi.Data;

/// <summary>
/// Entity Framework Core database context for the NGR application
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientDemographics> PatientDemographics => Set<PatientDemographics>();
    public DbSet<PatientProgramAssignment> PatientProgramAssignments => Set<PatientProgramAssignment>();
    public DbSet<Encounter> Encounters => Set<Encounter>();
    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
    public DbSet<CareProgram> CarePrograms => Set<CareProgram>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserProgramRole> UserProgramRoles => Set<UserProgramRole>();
    public DbSet<PatientAlias> PatientAliases => Set<PatientAlias>();
    public DbSet<PatientFile> PatientFiles => Set<PatientFile>();
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<HelpPage> HelpPages => Set<HelpPage>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
    public DbSet<SavedReport> SavedReports => Set<SavedReport>();
    public DbSet<ReportExecution> ReportExecutions => Set<ReportExecution>();
    public DbSet<ReportDownloadLog> ReportDownloadLogs => Set<ReportDownloadLog>();
    public DbSet<SavedDownloadDefinition> SavedDownloadDefinitions => Set<SavedDownloadDefinition>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<EmrFieldMapping> EmrFieldMappings => Set<EmrFieldMapping>();
    public DbSet<InstitutionMrnCrosswalk> InstitutionMrnCrosswalks => Set<InstitutionMrnCrosswalk>();
    public DbSet<SftpConfig> SftpConfigs => Set<SftpConfig>();
    public DbSet<UserEvent> UserEvents => Set<UserEvent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FeedRun> FeedRuns => Set<FeedRun>();
    public DbSet<FeedFieldMapping> FeedFieldMappings => Set<FeedFieldMapping>();
    public DbSet<DeletionTombstone> DeletionTombstones => Set<DeletionTombstone>();
    public DbSet<MigrationRun> MigrationRuns => Set<MigrationRun>();
    public DbSet<DatabaseLock> DatabaseLocks => Set<DatabaseLock>();
    public DbSet<DatabaseLockSkippedForm> DatabaseLockSkippedForms => Set<DatabaseLockSkippedForm>();
    public DbSet<ImpersonationSession> ImpersonationSessions => Set<ImpersonationSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Patient Configuration
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patients");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RegistryId).IsUnique();
            entity.Property(e => e.RegistryId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.MedicalRecordNumber).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.HasIndex(e => e.CffId).IsUnique();
            entity.Property(e => e.CffId).IsRequired();
            entity.Property(e => e.LastNameAtBirth).HasMaxLength(100);
            entity.Property(e => e.BiologicalSexAtBirth).HasMaxLength(20);
            entity.Property(e => e.SsnLast4).HasMaxLength(4);
            entity.Property(e => e.Diagnosis).HasMaxLength(500);
            entity.Property(e => e.VitalStatus).HasMaxLength(20).HasDefaultValue("Alive");
            entity.Property(e => e.ConsentWithdrawn).HasDefaultValue(false);
            entity.Property(e => e.IsDeceased).HasDefaultValue(false);
            entity.Property(e => e.IsMigrated).HasDefaultValue(false);
            entity.Property(e => e.SourceSystemId).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.CareProgram)
                  .WithMany()
                  .HasForeignKey(e => e.CareProgramId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Demographics)
                  .WithOne(d => d.Patient)
                  .HasForeignKey<PatientDemographics>(d => d.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PatientDemographics Configuration
        modelBuilder.Entity<PatientDemographics>(entity =>
        {
            entity.ToTable("PatientDemographics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address1).HasMaxLength(200);
            entity.Property(e => e.Address2).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.ZipCode).HasMaxLength(10);
            entity.Property(e => e.Ethnicity).HasMaxLength(50);
            entity.Property(e => e.Race).HasMaxLength(50);
            entity.Property(e => e.InsuranceType).HasMaxLength(50);
            entity.Property(e => e.InsuranceProvider).HasMaxLength(200);
        });

        // PatientProgramAssignment Configuration
        modelBuilder.Entity<PatientProgramAssignment>(entity =>
        {
            entity.ToTable("PatientProgramAssignments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PatientId, e.ProgramId }).IsUnique();
            entity.Property(e => e.LocalMRN).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IsPrimaryProgram).HasDefaultValue(false);
            entity.Property(e => e.EnrollmentDate).IsRequired();
            entity.Property(e => e.RemovalReason).HasMaxLength(200);
            entity.Property(e => e.RemovedBy).HasMaxLength(255);

            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.ProgramAssignments)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Program)
                  .WithMany()
                  .HasForeignKey(e => e.ProgramId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PatientAlias Configuration
        modelBuilder.Entity<PatientAlias>(entity =>
        {
            entity.ToTable("PatientAliases");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AliasType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.AliasValue).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.Aliases)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PatientFile Configuration
        modelBuilder.Entity<PatientFile>(entity =>
        {
            entity.ToTable("PatientFiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalFileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.StoredFileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.BlobPath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FileExtension).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FileType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OtherFileTypeDescription).HasMaxLength(200);
            entity.Property(e => e.UploadedBy).HasMaxLength(255).IsRequired();
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsMigrated).HasDefaultValue(false);
            entity.Property(e => e.ContentHash).HasMaxLength(64);

            entity.HasOne(e => e.Patient)
                  .WithMany()
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Program)
                  .WithMany()
                  .HasForeignKey(e => e.ProgramId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Encounter Configuration
        modelBuilder.Entity<Encounter>(entity =>
        {
            entity.ToTable("Encounters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EncounterTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EncounterTypeName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EncounterDate).IsRequired();
            entity.Property(e => e.EncounterYear).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(4000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.Encounters)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Program)
                  .WithMany()
                  .HasForeignKey(e => e.ProgramId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // FormDefinition Configuration
        modelBuilder.Entity<FormDefinition>(entity =>
        {
            entity.ToTable("FormDefinitions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Code, e.Version }).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Version).IsRequired();
            entity.Property(e => e.FormType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EncounterTypeCode).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsShared).HasDefaultValue(false);
            entity.Property(e => e.AutoComplete).HasDefaultValue(false);
            entity.Property(e => e.SchemaJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.ValidationRulesJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.UiSchemaJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // FormSubmission Configuration
        modelBuilder.Entity<FormSubmission>(entity =>
        {
            entity.ToTable("FormSubmissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompletionStatus).HasMaxLength(20).IsRequired().HasDefaultValue("Incomplete");
            entity.Property(e => e.LockStatus).HasMaxLength(20).IsRequired().HasDefaultValue("Unlocked");
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.LastUpdateSource).HasMaxLength(20).HasDefaultValue("User");
            entity.Property(e => e.RequiresReview).HasDefaultValue(false);
            entity.Property(e => e.FormDataJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.TransplantOrgan).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.FormDefinition)
                  .WithMany()
                  .HasForeignKey(e => e.FormDefinitionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Patient)
                  .WithMany()
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Encounter)
                  .WithMany(enc => enc.FormSubmissions)
                  .HasForeignKey(e => e.EncounterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Program)
                  .WithMany()
                  .HasForeignKey(e => e.ProgramId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // CareProgram Configuration
        modelBuilder.Entity<CareProgram>(entity =>
        {
            entity.ToTable("CarePrograms");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProgramId).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.ProgramId).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ProgramType).HasMaxLength(50).IsRequired().HasDefaultValue("Adult");
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.Address1).HasMaxLength(200);
            entity.Property(e => e.Address2).HasMaxLength(200);
            entity.Property(e => e.ZipCode).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsOrphanHoldingProgram).HasDefaultValue(false);
            entity.Property(e => e.IsTrainingProgram).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Ignore(e => e.DisplayTitle); // Calculated, not stored

            // Seed the Orphaned Record Holding program (02-004)
            entity.HasData(new CareProgram
            {
                Id = 1,
                ProgramId = 3000,
                Code = "ORH",
                Name = "Orphaned Record Holding",
                ProgramType = "Orphaned-Record Holding",
                IsActive = true,
                IsOrphanHoldingProgram = true,
                IsTrainingProgram = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });
        });

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OktaId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.OktaId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Role Configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // UserProgramRole Configuration
        modelBuilder.Entity<UserProgramRole>(entity =>
        {
            entity.ToTable("UserProgramRoles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ProgramId, e.RoleId }).IsUnique();
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.AssignedBy).HasMaxLength(255);
            entity.Property(e => e.DeactivatedBy).HasMaxLength(255);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.ProgramRoles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Program)
                  .WithMany()
                  .HasForeignKey(e => e.ProgramId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Role)
                  .WithMany()
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Content Configuration
        modelBuilder.Entity<Content>(entity =>
        {
            entity.ToTable("Contents");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Body).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Announcement Configuration
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.ToTable("Announcements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(4000).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TargetAudience).HasMaxLength(50).IsRequired();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // HelpPage Configuration
        modelBuilder.Entity<HelpPage>(entity =>
        {
            entity.ToTable("HelpPages");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Content).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.ContextKey).HasMaxLength(100);
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ContactRequest Configuration
        modelBuilder.Entity<ContactRequest>(entity =>
        {
            entity.ToTable("ContactRequests");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReferenceId).IsUnique();
            entity.Property(e => e.ReferenceId).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ProgramNumber).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Message).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.AttachmentFileName).HasMaxLength(500);
            entity.Property(e => e.AttachmentBlobPath).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // SavedReport Configuration
        modelBuilder.Entity<SavedReport>(entity =>
        {
            entity.ToTable("SavedReports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Scope).HasMaxLength(20).IsRequired();
            entity.Property(e => e.QueryDefinitionJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.ReportType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OwnerEmail).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ReportExecution Configuration
        modelBuilder.Entity<ReportExecution>(entity =>
        {
            entity.ToTable("ReportExecutions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ExecutedBy).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ResultDataJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.ExecutedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ParametersJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.OutputMode).HasMaxLength(20).IsRequired().HasDefaultValue("screen");
            entity.Property(e => e.FileFormat).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Success");
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);

            entity.HasOne(e => e.SavedReport)
                  .WithMany()
                  .HasForeignKey(e => e.SavedReportId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ReportDownloadLog Configuration
        modelBuilder.Entity<ReportDownloadLog>(entity =>
        {
            entity.ToTable("ReportDownloadLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ReportType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UserEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.UserRole).HasMaxLength(50);
            entity.Property(e => e.Format).HasMaxLength(10).IsRequired();
            entity.Property(e => e.DownloadedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // SavedDownloadDefinition Configuration
        modelBuilder.Entity<SavedDownloadDefinition>(entity =>
        {
            entity.ToTable("SavedDownloadDefinitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.OwnerEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ParametersJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ImportJob Configuration
        modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.ToTable("ImportJobs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.BlobPath).HasMaxLength(1000);
            entity.Property(e => e.MappingJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ResultsJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ErrorsJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Program)
                  .WithMany()
                  .HasForeignKey(e => e.ProgramId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FormDefinition)
                  .WithMany()
                  .HasForeignKey(e => e.FormDefinitionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // EmrFieldMapping Configuration
        modelBuilder.Entity<EmrFieldMapping>(entity =>
        {
            entity.ToTable("EmrFieldMappings");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProgramId, e.CsvColumnName, e.FormType }).IsUnique();
            entity.Property(e => e.CsvColumnName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FormType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FieldPath).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DataType).HasMaxLength(20).IsRequired().HasDefaultValue("string");
            entity.Property(e => e.TransformHint).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Program)
                  .WithMany()
                  .HasForeignKey(e => e.ProgramId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired(false);
        });

        // InstitutionMrnCrosswalk Configuration
        modelBuilder.Entity<InstitutionMrnCrosswalk>(entity =>
        {
            entity.ToTable("InstitutionMrnCrosswalks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProgramId, e.MedicalRecordNumber }).IsUnique();
            entity.Property(e => e.MedicalRecordNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CffId).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Program)
                  .WithMany()
                  .HasForeignKey(e => e.ProgramId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Patient)
                  .WithMany()
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SftpConfig Configuration
        modelBuilder.Entity<SftpConfig>(entity =>
        {
            entity.ToTable("SftpConfigs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProgramId).IsUnique();
            entity.Property(e => e.Host).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EncryptedPassword).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.RemoteDirectory).HasMaxLength(500).IsRequired().HasDefaultValue("/");
            entity.Property(e => e.FilePattern).HasMaxLength(100).IsRequired().HasDefaultValue("*.csv");
            entity.Property(e => e.ScheduleCron).HasMaxLength(100).IsRequired().HasDefaultValue("0 2 * * *");
            entity.Property(e => e.LastRunStatus).HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.UpdatedBy).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Program)
                  .WithMany()
                  .HasForeignKey(e => e.ProgramId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserEvent Configuration (12-004: User Interaction Analytics)
        modelBuilder.Entity<UserEvent>(entity =>
        {
            entity.ToTable("UserEvents");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OccurredAt);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Page).HasMaxLength(500);
            entity.Property(e => e.Component).HasMaxLength(200);
            entity.Property(e => e.PropertiesJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.OccurredAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // AuditLog Configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UserEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.OldValues).HasColumnType("nvarchar(max)");
            entity.Property(e => e.NewValues).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ActingAdminId).HasMaxLength(255);
        });

        // DatabaseLock Configuration
        modelBuilder.Entity<DatabaseLock>(entity =>
        {
            entity.ToTable("DatabaseLocks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReportingYear).IsUnique();
            entity.Property(e => e.ExecutionMode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.InitiatedBy).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.InitiatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // DatabaseLockSkippedForm Configuration
        modelBuilder.Entity<DatabaseLockSkippedForm>(entity =>
        {
            entity.ToTable("DatabaseLockSkippedForms");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DatabaseLockId, e.FormSubmissionId });
            entity.Property(e => e.SessionUserId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.SkipReason).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ResolutionType).HasMaxLength(50);
            entity.Property(e => e.SkippedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.DatabaseLock)
                  .WithMany(l => l.SkippedForms)
                  .HasForeignKey(e => e.DatabaseLockId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FormSubmission)
                  .WithMany()
                  .HasForeignKey(e => e.FormSubmissionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // FeedRun Configuration (13-001)
        modelBuilder.Entity<FeedRun>(entity =>
        {
            entity.ToTable("FeedRuns");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.RunType).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TriggeredBy).HasMaxLength(255).IsRequired();
            entity.Property(e => e.BlobPath).HasMaxLength(1000);
            entity.Property(e => e.ReconciliationJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // FeedFieldMapping Configuration (13-001)
        modelBuilder.Entity<FeedFieldMapping>(entity =>
        {
            entity.ToTable("FeedFieldMappings");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.NgrEntity, e.NgrProperty, e.Version }).IsUnique();
            entity.Property(e => e.NgrEntity).HasMaxLength(100).IsRequired();
            entity.Property(e => e.NgrProperty).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CffColumnName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DataType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TransformHint).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // DeletionTombstone Configuration (13-002)
        modelBuilder.Entity<DeletionTombstone>(entity =>
        {
            entity.ToTable("DeletionTombstones");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.RetainUntil); // for purge queries
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SourceSystemId).HasMaxLength(100);
            entity.Property(e => e.DeletedReason).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DeletedBy).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DeletedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // MigrationRun Configuration (13-006)
        modelBuilder.Entity<MigrationRun>(entity =>
        {
            entity.ToTable("MigrationRuns");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Phase);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Phase).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TriggeredBy).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ErrorsJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ValidationReportJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ImpersonationSession Configuration
        modelBuilder.Entity<ImpersonationSession>(entity =>
        {
            entity.ToTable("ImpersonationSessions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AdminUserId);
            entity.HasIndex(e => e.TargetUserId);
            entity.Property(e => e.AdminUserId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.AdminEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.TargetUserId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.TargetUserEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.TargetUserName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.TargetUserGroupsJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.EndReason).HasMaxLength(50);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
