using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LeaveManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    Description = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: false
                    ),
                    Version = table.Column<long>(
                        type: "bigint",
                        rowVersion: true,
                        nullable: false,
                        defaultValue: 1L
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(
                        type: "nvarchar(256)",
                        maxLength: 256,
                        nullable: false
                    ),
                    PasswordHash = table.Column<string>(
                        type: "nvarchar(256)",
                        maxLength: 256,
                        nullable: false
                    ),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Version = table.Column<long>(
                        type: "bigint",
                        rowVersion: true,
                        nullable: false,
                        defaultValue: 1L
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(
                        type: "nvarchar(256)",
                        maxLength: 256,
                        nullable: false
                    ),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Version = table.Column<long>(
                        type: "bigint",
                        rowVersion: true,
                        nullable: false,
                        defaultValue: 1L
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(
                        type: "nvarchar(1000)",
                        maxLength: 1000,
                        nullable: false
                    ),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: true
                    ),
                    Version = table.Column<long>(
                        type: "bigint",
                        rowVersion: true,
                        nullable: false,
                        defaultValue: 1L
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.CheckConstraint("CK_LeaveRequest_Dates", "[FromDate] <= [ToDate]");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[]
                {
                    "Id",
                    "CreatedAt",
                    "CreatedBy",
                    "Description",
                    "Name",
                    "UpdatedAt",
                    "UpdatedBy",
                },
                values: new object[,]
                {
                    {
                        new Guid("11111111-1111-1111-1111-111111111111"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        "System administrator with full access",
                        "Admin",
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                    },
                    {
                        new Guid("22222222-2222-2222-2222-222222222222"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        "Standard employee with leave request access",
                        "Employee",
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                    },
                }
            );

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[]
                {
                    "Id",
                    "CreatedAt",
                    "CreatedBy",
                    "Email",
                    "IsActive",
                    "PasswordHash",
                    "UpdatedAt",
                    "UpdatedBy",
                },
                values: new object[,]
                {
                    {
                        new Guid("33333333-3333-3333-3333-333333333333"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        "admin@example.com",
                        true,
                        "$2a$11$3vXlOCQR/gqj7YmrMtB4kO968zgOdmGOuNNe.4NNLxOZJhQZhPMKW",
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                    },
                    {
                        new Guid("44444444-4444-4444-4444-444444444444"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        "employee@example.com",
                        true,
                        "$2a$11$PzcRSJlBLr7JabRCnpsGq.0l5c5mD4DfloZgWgMtCDEmJ4CY/RQeG",
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                    },
                }
            );

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[]
                {
                    "Id",
                    "CreatedAt",
                    "CreatedBy",
                    "IsActive",
                    "Name",
                    "UpdatedAt",
                    "UpdatedBy",
                    "UserId",
                },
                values: new object[,]
                {
                    {
                        new Guid("77777777-7777-7777-7777-777777777777"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        true,
                        "Admin User",
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        new Guid("33333333-3333-3333-3333-333333333333"),
                    },
                    {
                        new Guid("88888888-8888-8888-8888-888888888888"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        true,
                        "Test Employee",
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        new Guid("44444444-4444-4444-4444-444444444444"),
                    },
                }
            );

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[]
                {
                    "Id",
                    "CreatedAt",
                    "CreatedBy",
                    "RoleId",
                    "UpdatedAt",
                    "UpdatedBy",
                    "UserId",
                },
                values: new object[,]
                {
                    {
                        new Guid("55555555-5555-5555-5555-555555555555"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        new Guid("11111111-1111-1111-1111-111111111111"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        new Guid("33333333-3333-3333-3333-333333333333"),
                    },
                    {
                        new Guid("66666666-6666-6666-6666-666666666666"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        new Guid("22222222-2222-2222-2222-222222222222"),
                        new DateTimeOffset(
                            new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            new TimeSpan(0, 0, 0, 0, 0)
                        ),
                        new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        new Guid("44444444-4444-4444-4444-444444444444"),
                    },
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsActive",
                table: "Employees",
                column: "IsActive"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UserId",
                table: "Employees",
                column: "UserId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_FromDate",
                table: "LeaveRequests",
                column: "FromDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_Status",
                table: "LeaveRequests",
                column: "Status"
            );

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ToDate",
                table: "LeaveRequests",
                column: "ToDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LeaveRequests");

            migrationBuilder.DropTable(name: "UserRoles");

            migrationBuilder.DropTable(name: "Employees");

            migrationBuilder.DropTable(name: "Roles");

            migrationBuilder.DropTable(name: "Users");
        }
    }
}
