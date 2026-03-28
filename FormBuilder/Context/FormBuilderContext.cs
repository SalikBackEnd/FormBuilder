using FormBuilder.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Context
{
    public class FormBuilderContext : DbContext
    {
        public FormBuilderContext(DbContextOptions<FormBuilderContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<FormField> FormFields { get; set; }
        public DbSet<FormSubmission> FormSubmissions { get; set; }
        public DbSet<FormSubmissionValue> FormSubmissionValues { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Form properties and index
            modelBuilder.Entity<Form>()
                .HasIndex(f => f.PublicSlug)
                .IsUnique();

            modelBuilder.Entity<Form>()
                .Property(f => f.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Configure Relationships (No navigation properties in entities, so we use empty WithMany)
            // User -> Forms
            modelBuilder.Entity<Form>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Form -> Fields
            modelBuilder.Entity<FormField>()
                .HasOne<Form>()
                .WithMany()
                .HasForeignKey(f => f.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Form -> Submissions
            modelBuilder.Entity<FormSubmission>()
                .HasOne<Form>()
                .WithMany()
                .HasForeignKey(s => s.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Submission -> Values
            modelBuilder.Entity<FormSubmissionValue>()
                .HasOne<FormSubmission>()
                .WithMany()
                .HasForeignKey(v => v.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Refresh Tokens
            modelBuilder.Entity<RefreshToken>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
