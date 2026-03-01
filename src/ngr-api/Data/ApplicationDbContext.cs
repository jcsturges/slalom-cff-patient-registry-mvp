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
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

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
            entity.Property(e => e.IsDeceased).HasDefaultValue(false);
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

            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.ProgramAssignments)
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
            entity.Property(e => e.EncounterTypeCode).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
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
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FormDataJson).HasColumnType("nvarchar(max)").IsRequired();
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
        });
    }
}
