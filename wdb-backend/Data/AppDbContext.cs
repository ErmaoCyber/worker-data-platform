using Microsoft.EntityFrameworkCore;
using wdb_backend.Models;
using NotificationModel = wdb_backend.Models.Notification;

namespace wdb_backend.Data;

/// <summary>
/// Database context for the application.
/// Manages database connections and maps entity models to
/// database tables via Supabase (PostgreSQL).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Worker> Workers { get; set; }
    public DbSet<Employer> Employers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Field> Fields { get; set; }
    public DbSet<WorkerInfo> WorkerInfos { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<NotificationModel> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --------------------------------------------------------
        // Category
        // --------------------------------------------------------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CategoryName).HasColumnName("category");
            entity.HasIndex(c => c.CategoryName).IsUnique();
        });

        // --------------------------------------------------------
        // Field
        // --------------------------------------------------------
        modelBuilder.Entity<Field>(entity =>
        {
            entity.ToTable("fields");
            entity.HasKey(f => f.Id);
            entity.Property(f => f.FieldName).HasColumnName("field");

            entity.HasOne(f => f.Category)
                  .WithMany(c => c.Fields)
                  .HasForeignKey(f => f.CategoryId);

            entity.HasIndex(f => new { f.CategoryId, f.FieldName }).IsUnique();
            entity.HasIndex(f => new { f.CategoryId, f.Label }).IsUnique();
        });

        // --------------------------------------------------------
        // WorkerInfo
        // --------------------------------------------------------
        modelBuilder.Entity<WorkerInfo>(entity =>
        {
            entity.ToTable("worker_info");
            entity.HasKey(w => w.Id);

            entity.HasOne(w => w.Worker)
                  .WithMany()
                  .HasForeignKey(w => w.WorkerId);

            entity.HasOne(w => w.Field)
                  .WithMany()
                  .HasForeignKey(w => w.FieldId)
                  .IsRequired(false);

            entity.HasIndex(w => new { w.WorkerId, w.FieldId })
                  .IsUnique()
                  .HasFilter("field_id IS NOT NULL");

            entity.HasIndex(w => new { w.WorkerId, w.CustomLabel })
                  .IsUnique()
                  .HasFilter("custom_label IS NOT NULL");

            entity.ToTable(t => t.HasCheckConstraint(
                "chk_field_xor_custom",
                "(field_id IS NOT NULL) <> (custom_label IS NOT NULL)"
            ));
        });

        // --------------------------------------------------------
        // Request
        // --------------------------------------------------------
        modelBuilder.Entity<Request>(entity =>
        {
            entity.ToTable("request");
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.Employer)
                  .WithMany()
                  .HasForeignKey(r => r.EmployerId);

            entity.HasOne(r => r.Worker)
                  .WithMany()
                  .HasForeignKey(r => r.WorkerId);

            entity.HasMany(r => r.Permissions)
                  .WithOne(p => p.Request)
                  .HasForeignKey(p => p.RequestId);

            entity.ToTable(t => t.HasCheckConstraint(
                "chk_custom_request_status",
                "custom_request_status IN ('pending', 'approved', 'rejected')"
            ));
        });

        // --------------------------------------------------------
        // Permission
        // --------------------------------------------------------
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permission");
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.Request)
                  .WithMany(r => r.Permissions)
                  .HasForeignKey(p => p.RequestId);

            entity.HasOne(p => p.Field)
                  .WithMany()
                  .HasForeignKey(p => p.FieldId)
                  .IsRequired(false);

            entity.HasOne(p => p.WorkerInfo)
                  .WithMany(w => w.Permissions)
                  .HasForeignKey(p => p.InfoId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.ToTable(t => t.HasCheckConstraint(
                "chk_status",
                "status IN (0, 1, 2, 3)"
            ));

            entity.ToTable(t => t.HasCheckConstraint(
                "chk_approved_has_info",
                "status NOT IN (1, 3) OR info_id IS NOT NULL"
            ));

            entity.ToTable(t => t.HasCheckConstraint(
                "chk_field_or_info",
                "field_id IS NOT NULL OR info_id IS NOT NULL"
            ));
        });

        // --------------------------------------------------------
        // Notification
        // --------------------------------------------------------
        modelBuilder.Entity<NotificationModel>(entity =>
        {
            entity.ToTable("notification");
            entity.HasKey(n => n.Id);

            entity.HasOne(n => n.RecipientWorker)
                  .WithMany()
                  .HasForeignKey(n => n.RecipientWorkerId)
                  .IsRequired(false);

            entity.HasOne(n => n.RecipientEmployer)
                  .WithMany()
                  .HasForeignKey(n => n.RecipientEmployerId)
                  .IsRequired(false);

            entity.HasOne(n => n.Request)
                  .WithMany()
                  .HasForeignKey(n => n.RequestId)
                  .IsRequired(false);

            entity.ToTable(t => t.HasCheckConstraint(
                "chk_notification_type",
                "type IN ('NEW_REQUEST', 'DATA_ACCESSED', 'REQUEST_REVIEWED', 'ACCESS_REVOKED')"
            ));

            entity.ToTable(t => t.HasCheckConstraint(
                "chk_single_recipient",
                "(recipient_worker_id IS NOT NULL) <> (recipient_employer_id IS NOT NULL)"
            ));
        });

        // --------------------------------------------------------
        // Seed Data
        // --------------------------------------------------------
        SeedCategories(modelBuilder);
        SeedFields(modelBuilder);
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = new Guid("00000000-0000-0000-0000-000000000001"),
                CategoryName = "PersonalInformation"
            },
            new Category
            {
                Id = new Guid("00000000-0000-0000-0000-000000000002"),
                CategoryName = "MedicalInformation"
            },
            new Category
            {
                Id = new Guid("00000000-0000-0000-0000-000000000003"),
                CategoryName = "CareerInformation"
            },
            new Category
            {
                Id = new Guid("00000000-0000-0000-0000-000000000004"),
                CategoryName = "WorkplaceInformation"
            },
            new Category
            {
                Id = new Guid("00000000-0000-0000-0000-000000000005"),
                CategoryName = "FinancialInformation"
            },
            new Category
            {
                Id = new Guid("00000000-0000-0000-0000-000000000006"),
                CategoryName = "OtherInformation"
            }
        );
    }

    private static void SeedFields(ModelBuilder modelBuilder)
    {
        var personalId = new Guid("00000000-0000-0000-0000-000000000001");
        var medicalId = new Guid("00000000-0000-0000-0000-000000000002");
        var careerId = new Guid("00000000-0000-0000-0000-000000000003");
        var workplaceId = new Guid("00000000-0000-0000-0000-000000000004");
        var financialId = new Guid("00000000-0000-0000-0000-000000000005");

        modelBuilder.Entity<Field>().HasData(

            // ── PersonalInformation ──────────────────────────────
            new Field
            {
                Id = new Guid("10000000-0000-0000-0000-000000000001"),
                FieldName = "full_name",
                CategoryId = personalId,
                Label = "Full Name",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("10000000-0000-0000-0000-000000000002"),
                FieldName = "date_of_birth",
                CategoryId = personalId,
                Label = "Date of Birth",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("10000000-0000-0000-0000-000000000003"),
                FieldName = "gender",
                CategoryId = personalId,
                Label = "Gender",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("10000000-0000-0000-0000-000000000004"),
                FieldName = "nationality",
                CategoryId = personalId,
                Label = "Nationality",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("10000000-0000-0000-0000-000000000005"),
                FieldName = "address",
                CategoryId = personalId,
                Label = "Address",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("10000000-0000-0000-0000-000000000006"),
                FieldName = "phone_number",
                CategoryId = personalId,
                Label = "Phone Number",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("10000000-0000-0000-0000-000000000007"),
                FieldName = "emergency_contact",
                CategoryId = personalId,
                Label = "Emergency Contact",
                AllowedType = "text"
            },

            // ── MedicalInformation ───────────────────────────────
            new Field
            {
                Id = new Guid("20000000-0000-0000-0000-000000000001"),
                FieldName = "blood_type",
                CategoryId = medicalId,
                Label = "Blood Type",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("20000000-0000-0000-0000-000000000002"),
                FieldName = "allergies",
                CategoryId = medicalId,
                Label = "Allergies",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("20000000-0000-0000-0000-000000000003"),
                FieldName = "medical_notes",
                CategoryId = medicalId,
                Label = "Medical Notes",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("20000000-0000-0000-0000-000000000004"),
                FieldName = "medical_report",
                CategoryId = medicalId,
                Label = "Medical Report",
                AllowedType = "file"
            },
            new Field
            {
                Id = new Guid("20000000-0000-0000-0000-000000000005"),
                FieldName = "current_conditions",
                CategoryId = medicalId,
                Label = "Current Conditions",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("20000000-0000-0000-0000-000000000006"),
                FieldName = "medications",
                CategoryId = medicalId,
                Label = "Medications",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("20000000-0000-0000-0000-000000000007"),
                FieldName = "vaccination_records",
                CategoryId = medicalId,
                Label = "Vaccination Records",
                AllowedType = "file"
            },
            new Field
            {
                Id = new Guid("20000000-0000-0000-0000-000000000008"),
                FieldName = "lab_results",
                CategoryId = medicalId,
                Label = "Lab and Test Results",
                AllowedType = "file"
            },
            new Field
            {
                Id = new Guid("20000000-0000-0000-0000-000000000009"),
                FieldName = "insurance_info",
                CategoryId = medicalId,
                Label = "Insurance Information",
                AllowedType = "text"
            },

            // ── CareerInformation ────────────────────────────────
            new Field
            {
                Id = new Guid("30000000-0000-0000-0000-000000000001"),
                FieldName = "job_title",
                CategoryId = careerId,
                Label = "Job Title",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("30000000-0000-0000-0000-000000000002"),
                FieldName = "department",
                CategoryId = careerId,
                Label = "Department",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("30000000-0000-0000-0000-000000000003"),
                FieldName = "employee_id",
                CategoryId = careerId,
                Label = "Employee ID",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("30000000-0000-0000-0000-000000000004"),
                FieldName = "start_date",
                CategoryId = careerId,
                Label = "Start Date",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("30000000-0000-0000-0000-000000000005"),
                FieldName = "resume",
                CategoryId = careerId,
                Label = "Resume",
                AllowedType = "file"
            },

            // ── WorkplaceInformation ─────────────────────────────
            new Field
            {
                Id = new Guid("40000000-0000-0000-0000-000000000001"),
                FieldName = "work_rights",
                CategoryId = workplaceId,
                Label = "Work Rights",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("40000000-0000-0000-0000-000000000002"),
                FieldName = "certifications",
                CategoryId = workplaceId,
                Label = "Certifications",
                AllowedType = "file"
            },
            new Field
            {
                Id = new Guid("40000000-0000-0000-0000-000000000003"),
                FieldName = "ppe_requirements",
                CategoryId = workplaceId,
                Label = "PPE Requirements",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("40000000-0000-0000-0000-000000000004"),
                FieldName = "work_restrictions",
                CategoryId = workplaceId,
                Label = "Work Restrictions",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("40000000-0000-0000-0000-000000000005"),
                FieldName = "induction_records",
                CategoryId = workplaceId,
                Label = "Induction Records",
                AllowedType = "file"
            },

            // ── FinancialInformation ─────────────────────────────
            new Field
            {
                Id = new Guid("50000000-0000-0000-0000-000000000001"),
                FieldName = "bank_account",
                CategoryId = financialId,
                Label = "Bank Account",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("50000000-0000-0000-0000-000000000002"),
                FieldName = "tax_number",
                CategoryId = financialId,
                Label = "Tax Number",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("50000000-0000-0000-0000-000000000003"),
                FieldName = "kiwisaver_rate",
                CategoryId = financialId,
                Label = "KiwiSaver Rate",
                AllowedType = "text"
            },
            new Field
            {
                Id = new Guid("50000000-0000-0000-0000-000000000004"),
                FieldName = "payment_preference",
                CategoryId = financialId,
                Label = "Payment Preference",
                AllowedType = "text"
            }

            // OtherInformation: no preset fields
        );
    }
}
