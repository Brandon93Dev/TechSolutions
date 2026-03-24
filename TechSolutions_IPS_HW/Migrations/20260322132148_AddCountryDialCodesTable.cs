using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechSolutions_IPS_HW.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryDialCodesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountryDialCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DialCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryDialCodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountryDialCodes_CountryName",
                table: "CountryDialCodes",
                column: "CountryName",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO CountryDialCodes (CountryName, DialCode) VALUES
                ('Algeria', '+213'),
                ('Angola', '+244'),
                ('Argentina', '+54'),
                ('Australia', '+61'),
                ('Austria', '+43'),
                ('Bahrain', '+973'),
                ('Bangladesh', '+880'),
                ('Belgium', '+32'),
                ('Bolivia', '+591'),
                ('Botswana', '+267'),
                ('Brazil', '+55'),
                ('Bulgaria', '+359'),
                ('Canada', '+1'),
                ('Chile', '+56'),
                ('China', '+86'),
                ('Colombia', '+57'),
                ('Costa Rica', '+506'),
                ('Croatia', '+385'),
                ('Cuba', '+53'),
                ('Czech Republic', '+420'),
                ('Denmark', '+45'),
                ('Dominican Republic', '+1'),
                ('Ecuador', '+593'),
                ('Egypt', '+20'),
                ('Ethiopia', '+251'),
                ('Finland', '+358'),
                ('France', '+33'),
                ('Germany', '+49'),
                ('Ghana', '+233'),
                ('Greece', '+30'),
                ('Hong Kong', '+852'),
                ('Hungary', '+36'),
                ('India', '+91'),
                ('Indonesia', '+62'),
                ('Ireland', '+353'),
                ('Israel', '+972'),
                ('Italy', '+39'),
                ('Jamaica', '+1'),
                ('Japan', '+81'),
                ('Kenya', '+254'),
                ('Kuwait', '+965'),
                ('Malaysia', '+60'),
                ('Mexico', '+52'),
                ('Morocco', '+212'),
                ('Mozambique', '+258'),
                ('Namibia', '+264'),
                ('Nepal', '+977'),
                ('Netherlands', '+31'),
                ('New Zealand', '+64'),
                ('Nigeria', '+234'),
                ('Norway', '+47'),
                ('Oman', '+968'),
                ('Pakistan', '+92'),
                ('Panama', '+507'),
                ('Paraguay', '+595'),
                ('Peru', '+51'),
                ('Philippines', '+63'),
                ('Poland', '+48'),
                ('Portugal', '+351'),
                ('Qatar', '+974'),
                ('Romania', '+40'),
                ('Russia', '+7'),
                ('Saudi Arabia', '+966'),
                ('Serbia', '+381'),
                ('Singapore', '+65'),
                ('Slovakia', '+421'),
                ('South Africa', '+27'),
                ('South Korea', '+82'),
                ('Spain', '+34'),
                ('Sri Lanka', '+94'),
                ('Sweden', '+46'),
                ('Switzerland', '+41'),
                ('Taiwan', '+886'),
                ('Tanzania', '+255'),
                ('Thailand', '+66'),
                ('Tunisia', '+216'),
                ('Turkey', '+90'),
                ('Uganda', '+256'),
                ('Ukraine', '+380'),
                ('United Arab Emirates', '+971'),
                ('United Kingdom', '+44'),
                ('United States', '+1'),
                ('Uruguay', '+598'),
                ('Venezuela', '+58'),
                ('Vietnam', '+84'),
                ('Zambia', '+260'),
                ('Zimbabwe', '+263');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryDialCodes");
        }
    }
}
