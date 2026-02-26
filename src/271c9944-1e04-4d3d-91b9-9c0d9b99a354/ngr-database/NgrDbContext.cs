using Microsoft.EntityFrameworkCore;
using Ngr.Database.Entities;

namespace Ngr.Database;

/// <summary>
/// Entity Framework Core DbContext for the NGR database.
/// </summary>
public class NgrDbContext : DbContext
{
    public NgrDbContext(DbContextOptions<NgrDbContext> options) : base(options)
    {
    }

    // Lookup tables
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<EncounterType> EncounterTypes => Set<EncounterType>();
    public DbSet<FormStatus> FormStatuses => Set<FormStatus>();

    // Care program tables
    public DbSet<CareProgram> CarePrograms => Set<CareProgram>();

    // User tables
    public DbSet<User> Users => Set<User>();
    public DbSet<ProgramUser> ProgramUsers => Set<ProgramUser>();

    // Patient tables
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientProgram> PatientPrograms => Set<PatientProgram>();
    public DbSet<PatientDemographics> PatientDemographics => Set<PatientDemographics>();
    public DbSet<PatientMergeHistory> PatientMergeHistory => Set<PatientMergeHistory>();

    // Encounter tables
    public DbSet<Encounter> Encounters => Set<Encounter>();

    // Form tables
    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
    public DbSet<FormFieldHistory> FormFieldHistory => Set<FormFieldHistory>();

    // Import tables
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<ImportError> ImportErrors => Set<ImportError>();

    // Content tables
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    // Reporting tables
    public DbSet<SavedReport> SavedReports => Set<SavedReport>();
    public DbSet<StandardReport> StandardReports => Set<StandardReport>();

    // Audit tables
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Data warehouse sync tables
    public DbSet<DataWarehouseSyncStatus> DataWarehouseSyncStatus => Set<DataWarehouseSyncStatus>();
    public DbSet<ChangeTracking> ChangeTracking => Set<ChangeTracking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema
        modelBuilder.HasDefaultSchema("ngr");

        // Apply all entity configurations
        ConfigureRoles(modelBuilder);
        ConfigureEncounterTypes(modelBuilder);
        ConfigureFormStatuses(modelBuilder);
        ConfigureCarePrograms(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureProgramUsers(modelBuilder);
        ConfigurePatients(modelBuilder);
        ConfigurePatientPrograms(modelBuilder);
        ConfigurePatientDemographics(modelBuilder);
        ConfigurePatientMergeHistory(modelBuilder);
        ConfigureEncounters(modelBuilder);
        ConfigureFormDefinitions(modelBuilder);
        ConfigureFormSubmissions(modelBuilder);
        ConfigureFormFieldHistory(modelBuilder);
        ConfigureImportJobs(modelBuilder);
        ConfigureImportErrors(modelBuilder);
        ConfigureContents(modelBuilder);
        ConfigureAnnouncements(modelBuilder);
        ConfigureSavedReports(modelBuilder);
        ConfigureStandardReports(modelBuilder);
        ConfigureAuditLogs(modelBuilder);
        ConfigureDataWarehouseSyncStatus(modelBuilder);
        ConfigureChangeTracking(modelBuilder);
    }

    private void ConfigureRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureEncounterTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EncounterType>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });
    }

    private void ConfigureFormStatuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormStatus>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });
    }

    private void ConfigureCarePrograms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CareProgram>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.OktaId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureProgramUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProgramUser>(entity =>
        {
            entity.HasIndex(e => new { e.ProgramId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.ProgramId);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.CareProgram)
                .WithMany(c => c.ProgramUsers)
                .HasForeignKey(e => e.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ProgramUsers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.ProgramUsers)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigurePatients(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasIndex(e => e.RegistryId).IsUnique();
            entity.HasIndex(e => new { e.LastName, e.FirstName });
            entity.HasIndex(e => e.DateOfBirth);
            entity.HasIndex(e => e.IsDeceased);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigurePatientPrograms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientProgram>(entity =>
        {
            entity.HasIndex(e => new { e.PatientId, e.ProgramId }).IsUnique();
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.ProgramId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.PatientPrograms)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CareProgram)
                .WithMany(c => c.PatientPrograms)
                .HasForeignKey(e => e.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigurePatientDemographics(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientDemographics>(entity =>
        {
            entity.HasIndex(e => e.PatientId).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Patient)
                .WithOne(p => p.Demographics)
                .HasForeignKey<PatientDemographics>(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigurePatientMergeHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientMergeHistory>(entity =>
        {
            entity.HasIndex(e => e.SurvivorPatientId);
            entity.Property(e => e.MergedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.SurvivorPatient)
                .WithMany(p => p.MergeHistoriesAsSurvivor)
                .HasForeignKey(e => e.SurvivorPatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureEncounters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Encounter>(entity =>
        {
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.ProgramId);
            entity.HasIndex(e => e.EncounterDate);
            entity.HasIndex(e => e.EncounterYear);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Encounters)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CareProgram)
                .WithMany(c => c.Encounters)
                .HasForeignKey(e => e.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EncounterType)
                .WithMany(et => et.Encounters)
                .HasForeignKey(e => e.EncounterTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureFormDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormDefinition>(entity =>
        {
            entity.HasIndex(e => new { e.Code, e.Version }).IsUnique();
            entity.HasIndex(e => e.Code);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.EncounterType)
                .WithMany(et => et.FormDefinitions)
                .HasForeignKey(e => e.EncounterTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureFormSubmissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormSubmission>(entity =>
        {
            entity.HasIndex(e => e.FormDefinitionId);
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.EncounterId);
            entity.HasIndex(e => e.ProgramId);
            entity.HasIndex(e => e.StatusId);
            entity.HasIndex(e => e.SubmittedAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.FormDefinition)
                .WithMany(fd => fd.FormSubmissions)
                .HasForeignKey(e => e.FormDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.FormSubmissions)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Encounter)
                .WithMany(enc => enc.FormSubmissions)
                .HasForeignKey(e => e.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CareProgram)
                .WithMany(c => c.FormSubmissions)
                .HasForeignKey(e => e.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FormStatus)
                .WithMany(fs => fs.FormSubmissions)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureFormFieldHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormFieldHistory>(entity =>
        {
            entity.HasIndex(e => e.FormSubmissionId);
            entity.HasIndex(e => e.ChangedAt);
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.FormSubmission)
                .WithMany(fs => fs.FieldHistory)
                .HasForeignKey(e => e.FormSubmissionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureImportJobs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.HasIndex(e => e.ProgramId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.CareProgram)
                .WithMany(c => c.ImportJobs)
                .HasForeignKey(e => e.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureImportErrors(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportError>(entity =>
        {
            entity.HasIndex(e => e.ImportJobId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.ImportJob)
                .WithMany(ij => ij.ImportErrors)
                .HasForeignKey(e => e.ImportJobId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureContents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsPublished);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureAnnouncements(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureSavedReports(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SavedReport>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ProgramId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.SavedReports)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CareProgram)
                .WithMany(c => c.SavedReports)
                .HasForeignKey(e => e.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureStandardReports(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StandardReport>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureAuditLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.ProgramId);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CareProgram)
                .WithMany(c => c.AuditLogs)
                .HasForeignKey(e => e.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureDataWarehouseSyncStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataWarehouseSyncStatus>(entity =>
        {
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.LastSyncAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureChangeTracking(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChangeTracking>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.IsProcessed });
            entity.HasIndex(e => e.ChangedAt);
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
