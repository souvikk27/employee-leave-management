using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(e => e.Email).IsUnique();

        builder.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);

        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(e => e.Version).IsConcurrencyToken().HasDefaultValue(1);

        builder
            .HasMany(e => e.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.IsActive);

        var sysId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var fixedDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var adminUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var employeeUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        builder.HasData(
            new User(
                adminUserId,
                fixedDate,
                fixedDate,
                sysId,
                sysId,
                "admin@example.com",
                "$2a$11$3vXlOCQR/gqj7YmrMtB4kO968zgOdmGOuNNe.4NNLxOZJhQZhPMKW",
                true
            ),
            new User(
                employeeUserId,
                fixedDate,
                fixedDate,
                sysId,
                sysId,
                "employee@example.com",
                "$2a$11$PzcRSJlBLr7JabRCnpsGq.0l5c5mD4DfloZgWgMtCDEmJ4CY/RQeG",
                true
            )
        );
    }
}
