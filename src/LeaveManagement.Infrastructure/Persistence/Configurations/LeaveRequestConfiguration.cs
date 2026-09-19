using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Reason).IsRequired().HasMaxLength(1000);

        builder.Property(e => e.Status).IsRequired();

        // Client-owned concurrency token: the domain increments Version on every
        // state change and EF Core persists it (original value in the WHERE clause).
        // ValueGeneratedOnAddOrUpdate must NOT be used here: without a database
        // trigger the store never computes a new value, so the client increment
        // would be silently dropped and concurrent updates would never conflict.
        builder.Property(e => e.Version).IsConcurrencyToken().HasDefaultValue(1);

        builder
            .HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EmployeeId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.FromDate);
        builder.HasIndex(e => e.ToDate);

        // Using check constraint for FromDate <= ToDate as specified in rules
        builder.ToTable(tb =>
            tb.HasCheckConstraint("CK_LeaveRequest_Dates", "[FromDate] <= [ToDate]")
        );
    }
}
