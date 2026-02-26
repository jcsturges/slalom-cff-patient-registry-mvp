# NGR Database Schema

Entity Framework Core 8 database schema for the Next Generation Patient Registry (NGR).

## Overview

This project contains:
- **Entity classes**: C# POCO classes representing database tables
- **DbContext**: `NgrDbContext` with all entity configurations
- **Migrations**: SQL scripts for schema creation
- **Seed data**: Initial lookup data and sample records

## Technology Stack

- **Framework**: Entity Framework Core 8.0
- **Database**: Azure SQL Database
- **Language**: C# 12
- **Target**: .NET 8.0

## Database Schema

### Schema: `ngr`

All tables are created in the `ngr` schema.

### Entity Categories

1. **Lookup Tables**
   - Roles
   - EncounterTypes
   - FormStatuses

2. **Care Program Tables**
   - CarePrograms (136 CF care centers)

3. **User Tables**
   - Users (synced from Okta)
   - ProgramUsers (user-program-role assignments)

4. **Patient Tables**
   - Patients (master patient index)
   - PatientPrograms (roster)
   - PatientDemographics
   - PatientMergeHistory

5. **Encounter Tables**
   - Encounters

6. **Form Tables**
   - FormDefinitions (eCRF templates)
   - FormSubmissions (eCRF data)
   - FormFieldHistory (audit trail)

7. **Import Tables**
   - ImportJobs
   - ImportErrors

8. **Content Tables**
   - Contents
   - Announcements

9. **Reporting Tables**
   - SavedReports
   - StandardReports

10. **Audit Tables**
    - AuditLogs

11. **Data Warehouse Sync Tables**
    - DataWarehouseSyncStatus
    - ChangeTracking

## Connection String

The connection string should be stored in Azure Key Vault and retrieved at runtime:

```json
{
  "ConnectionStrings": {
    "NgrDatabase": "Server=tcp:{server}.database.windows.net,1433;Initial Catalog={database};Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

## Usage in ASP.NET Core

### Register DbContext

```csharp
// Program.cs
builder.Services.AddDbContext<NgrDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("NgrDatabase");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
    });
});
```

### Use in Services

```csharp
public class PatientService
{
    private readonly NgrDbContext _context;

    public PatientService(NgrDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Demographics)
            .Include(p => p.PatientPrograms)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

## Migrations

### Create Initial Migration

```bash
dotnet ef migrations add InitialCreate --project Ngr.Database
```

### Apply Migration to Database

```bash
dotnet ef database update --project Ngr.Database --connection "Server=..."
```

### Generate SQL Script

```bash
dotnet ef migrations script --project Ngr.Database --output Migrations/001_InitialSchema.sql
```

## Running SQL Scripts

The SQL scripts in the `Migrations` folder can be executed directly against Azure SQL Database:

```bash
# Using sqlcmd
sqlcmd -S {server}.database.windows.net -d {database} -U {username} -P {password} -i Migrations/001_InitialSchema.sql

# Using Azure Data Studio or SQL Server Management Studio
# Open and execute the script files in order
```

## Seed Data

The `002_SeedData.sql` script includes:
- 5 default roles (SystemAdmin, FoundationAnalyst, ProgramAdmin, ClinicalUser, ReadOnlyUser)
- 6 encounter types (Annual, Quarterly, Hospitalization, etc.)
- 4 form statuses (Draft, Submitted, Locked, Archived)
- 10 sample care programs
- 3 sample form definitions
- 3 sample standard reports
- Sample content and announcements

## Indexing Strategy

All tables include appropriate indexes for:
- Primary keys (clustered)
- Foreign keys (non-clustered)
- Unique constraints
- Frequently queried columns
- Composite indexes for common query patterns

## Performance Considerations

1. **Connection Pooling**: Enabled by default in EF Core
2. **Retry Logic**: Configured for transient fault handling
3. **AsNoTracking**: Use for read-only queries
4. **Pagination**: Implement for large result sets
5. **Compiled Queries**: Consider for frequently executed queries

## Security

1. **Encryption at Rest**: Azure SQL TDE enabled
2. **Encryption in Transit**: TLS 1.2+ enforced
3. **Authentication**: Azure AD authentication recommended
4. **Secrets**: Connection strings stored in Azure Key Vault
5. **Audit**: All data changes tracked in AuditLogs table

## Estimated Table Sizes

| Table | Estimated Rows (Year 1) | Estimated Size |
|-------|------------------------|----------------|
| Patients | 36,000 | 50 MB |
| Encounters | 4,120,000 | 2 GB |
| FormSubmissions | 8,000,000 | 500 GB |
| AuditLogs | 50,000,000 | 100 GB |
| PatientPrograms | 40,000 | 10 MB |

## Maintenance

### Regular Tasks

1. **Index Maintenance**: Rebuild fragmented indexes monthly
2. **Statistics Update**: Update statistics weekly
3. **Backup Verification**: Test restores quarterly
4. **Audit Log Archival**: Archive logs older than 1 year
5. **Change Tracking Cleanup**: Delete processed records older than 30 days

### Monitoring

Monitor these metrics:
- Database size and growth rate
- Query performance (slow queries)
- Index fragmentation
- Blocking and deadlocks
- Connection pool exhaustion

## Support

For questions or issues:
- Technical Lead: [Contact Info]
- Database Administrator: [Contact Info]
- Documentation: [Wiki Link]
