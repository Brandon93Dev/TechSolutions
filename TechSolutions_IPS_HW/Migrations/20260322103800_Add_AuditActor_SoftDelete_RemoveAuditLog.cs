using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechSolutions_IPS_HW.Migrations
{
    /// <inheritdoc />
    public partial class Add_AuditActor_SoftDelete_RemoveAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "PerformedBy",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuditEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "AuditEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PerformerActorId",
                table: "AuditEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectActorId",
                table: "AuditEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectUserId",
                table: "AdminNotifications",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Clean orphaned FKs before adding constraints
            migrationBuilder.Sql(
                "UPDATE [AdminNotifications] SET [SubjectUserId] = NULL WHERE [SubjectUserId] IS NOT NULL AND [SubjectUserId] NOT IN (SELECT [Id] FROM [AspNetUsers])");
            migrationBuilder.Sql(
                "DELETE FROM [AdminNotifications] WHERE [RecipientUserId] NOT IN (SELECT [Id] FROM [AspNetUsers])");

            migrationBuilder.CreateTable(
                name: "AuditActors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditActors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditActors_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_CustomerId",
                table: "AuditEntries",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_PerformerActorId",
                table: "AuditEntries",
                column: "PerformerActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_SubjectActorId",
                table: "AuditEntries",
                column: "SubjectActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_TimestampUtc",
                table: "AuditEntries",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_SubjectUserId",
                table: "AdminNotifications",
                column: "SubjectUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditActors_UserId",
                table: "AuditActors",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_AspNetUsers_RecipientUserId",
                table: "AdminNotifications",
                column: "RecipientUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_AspNetUsers_SubjectUserId",
                table: "AdminNotifications",
                column: "SubjectUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_AuditActors_PerformerActorId",
                table: "AuditEntries",
                column: "PerformerActorId",
                principalTable: "AuditActors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_AuditActors_SubjectActorId",
                table: "AuditEntries",
                column: "SubjectActorId",
                principalTable: "AuditActors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_Customers_CustomerId",
                table: "AuditEntries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminNotifications_AspNetUsers_RecipientUserId",
                table: "AdminNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_AdminNotifications_AspNetUsers_SubjectUserId",
                table: "AdminNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_AuditActors_PerformerActorId",
                table: "AuditEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_AuditActors_SubjectActorId",
                table: "AuditEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_Customers_CustomerId",
                table: "AuditEntries");

            migrationBuilder.DropTable(
                name: "AuditActors");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_CustomerId",
                table: "AuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_PerformerActorId",
                table: "AuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_SubjectActorId",
                table: "AuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_TimestampUtc",
                table: "AuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_AdminNotifications_SubjectUserId",
                table: "AdminNotifications");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "PerformerActorId",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "SubjectActorId",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "PerformedBy",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SubjectUserId",
                table: "AdminNotifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ChangedBy",
                table: "AuditLogs",
                column: "ChangedBy");
        }
    }
}
