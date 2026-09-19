using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.Description).HasMaxLength(500);

        // Client-owned concurrency token (see LeaveRequestConfiguration): the domain
        // increments Version and EF Core must persist it, so no ValueGeneratedOnAddOrUpdate.
        builder.Property(e => e.Version).IsConcurrencyToken().HasDefaultValue(1);

        builder
            .HasMany(e => e.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        var sysId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var fixedDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var employeeRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        builder.HasData(
            new Role(
                adminRoleId,
                fixedDate,
                fixedDate,
                sysId,
                sysId,
                "Admin",
                "System administrator with full access"
            ),
            new Role(
                employeeRoleId,
                fixedDate,
                fixedDate,
                sysId,
                sysId,
                "Employee",
                "Standard employee with leave request access"
            )
        );
    }
}
