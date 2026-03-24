using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechSolutions_IPS_HW.Migrations
{
    /// <inheritdoc />
    public partial class Customer_IsDeleted_Field_Addition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_Customers_CustomerId",
                table: "AuditEntries");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CustomerChangeLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformerActorId = table.Column<int>(type: "int", nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerChangeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerChangeLogs_AuditActors_PerformerActorId",
                        column: x => x.PerformerActorId,
                        principalTable: "AuditActors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerChangeLogs_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChangeLogs_CustomerId",
                table: "CustomerChangeLogs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChangeLogs_PerformerActorId",
                table: "CustomerChangeLogs",
                column: "PerformerActorId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChangeLogs_TimestampUtc",
                table: "CustomerChangeLogs",
                column: "TimestampUtc");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_Customers_CustomerId",
                table: "AuditEntries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_Customers_CustomerId",
                table: "AuditEntries");

            migrationBuilder.DropTable(
                name: "CustomerChangeLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_Customers_CustomerId",
                table: "AuditEntries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}