using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Server.Models.Tasks;

namespace TaskManager.Server.Data.Configurations.Tasks;

public class TaskHistoryConfiguration
    : IEntityTypeConfiguration<TaskHistory>
{
    public void Configure(EntityTypeBuilder<TaskHistory> builder)
    {
        builder.ToTable("TaskHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Comment)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(x => x.TaskItem)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChangedBy)
            .WithMany(x => x.TaskHistoryEntries)
            .HasForeignKey(x => x.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}