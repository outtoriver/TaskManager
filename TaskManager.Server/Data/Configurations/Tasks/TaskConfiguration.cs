using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Server.Models.Tasks;

namespace TaskManager.Server.Data.Configurations.Tasks;

public class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks", table =>
        {
            table.HasCheckConstraint(
                "CK_Tasks_Progress",
                "[Progress] >= 0 AND [Progress] <= 100");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Description)
            .HasMaxLength(10000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.Progress)
            .HasDefaultValue(0);

        builder.Property(x => x.IsImportant)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(x => x.CreatedBy)
            .WithMany(x => x.CreatedTasks)
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedTo)
            .WithMany(x => x.AssignedTasks)
            .HasForeignKey(x => x.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AssignedToId);

        builder.HasIndex(x => x.CreatedById);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.Priority);

        builder.HasIndex(x => x.DueDate);

        builder.HasIndex(x => x.IsImportant);
    }
}