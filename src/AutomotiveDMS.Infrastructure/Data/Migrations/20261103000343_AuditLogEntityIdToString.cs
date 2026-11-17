using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomotiveDMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditLogEntityIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ChangedDate",
                table: "AuditLogs",
                column: "ChangedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ChangedDate",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);
        }
    }
}
