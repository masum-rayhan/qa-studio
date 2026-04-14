using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QAStudio.Infrastructure.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddRecordingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecordingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TargetUrl = table.Column<string>(type: "text", nullable: true),
                    StepsJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetEnvironmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecordingSessions_Environments_TargetEnvironmentId",
                        column: x => x.TargetEnvironmentId,
                        principalTable: "Environments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecordingSessions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSessions_CreatedBy",
                table: "RecordingSessions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSessions_Status",
                table: "RecordingSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RecordingSessions_TargetEnvironmentId",
                table: "RecordingSessions",
                column: "TargetEnvironmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecordingSessions");
        }
    }
}
