using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(e => e.Id);

        builder
            .HasOne(e => e.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(e => e.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

        var sysId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var fixedDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var employeeRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var adminUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var employeeUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        builder.HasData(
            new UserRole(
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                fixedDate,
                fixedDate,
                sysId,
                sysId,
                adminUserId,
                adminRoleId
            ),
            new UserRole(
                Guid.Parse("66666666-6666-6666-6666-666666666666"),
                fixedDate,
                fixedDate,
                sysId,
                sysId,
                employeeUserId,
                employeeRoleId
            )
        );
    }
}
