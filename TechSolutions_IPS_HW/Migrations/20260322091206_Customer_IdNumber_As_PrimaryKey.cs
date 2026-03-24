using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechSolutions_IPS_HW.Migrations
{
    /// <inheritdoc />
    public partial class Customer_IdNumber_As_PrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Customers",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_FirstName_Surname_IdNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Customers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Customers",
                table: "Customers",
                column: "IdNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Customers",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "Customers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Customers",
                table: "Customers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_FirstName_Surname_IdNumber",
                table: "Customers",
                columns: new[] { "FirstName", "Surname", "IdNumber" });
        }
    }
}
