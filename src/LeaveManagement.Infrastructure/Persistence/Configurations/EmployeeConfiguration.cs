using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(256);

        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(e => e.Version).IsConcurrencyToken().HasDefaultValue(1);

        builder
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.IsActive);

        var sysId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var fixedDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var adminUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var employeeUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var adminEmployeeId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var employeeEmployeeId = Guid.Parse("88888888-8888-8888-8888-888888888888");

        builder.HasData(
            new Employee(
                adminEmployeeId,
                fixedDate,
                fixedDate,
                sysId,
                sysId,
                "Admin User",
                adminUserId,
                true
            ),
            new Employee(
                employeeEmployeeId,
                fixedDate,
                fixedDate,
                sysId,
                sysId,
                "Test Employee",
                employeeUserId,
                true
            )
        );
    }
}
