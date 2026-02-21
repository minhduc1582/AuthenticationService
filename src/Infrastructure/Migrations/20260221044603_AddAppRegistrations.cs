using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientSecretHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ClientSecretSalt = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    ExpirationUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastSecretRotatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRegistrations_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRegistrationScopes",
                columns: table => new
                {
                    AppRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRegistrationScopes", x => new { x.AppRegistrationId, x.ScopeId });
                    table.ForeignKey(
                        name: "FK_AppRegistrationScopes_AppRegistrations_AppRegistrationId",
                        column: x => x.AppRegistrationId,
                        principalTable: "AppRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppRegistrationScopes_Scopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "Scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Scopes",
                columns: new[] { "Id", "Description", "IsActive", "IsDefault", "Name" },
                values: new object[,]
                {
                    { new Guid("a2a55c37-3fcb-4f28-9e57-52ab4c0c4e01"), "Read access to API resources", true, true, "api.read" },
                    { new Guid("e74a4df1-aeb6-4b68-86ce-d1a99cd3ad36"), "Write access to API resources", true, false, "api.write" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRegistrations_ClientId",
                table: "AppRegistrations",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppRegistrations_OwnerUserId",
                table: "AppRegistrations",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRegistrationScopes_ScopeId",
                table: "AppRegistrationScopes",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_Scopes_Name",
                table: "Scopes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRegistrationScopes");

            migrationBuilder.DropTable(
                name: "AppRegistrations");

            migrationBuilder.DropTable(
                name: "Scopes");
        }
    }
}
