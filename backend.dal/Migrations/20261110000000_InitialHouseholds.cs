using Backend.DAL;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Backend.DAL.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261110000000_InitialHouseholds")]
public class InitialHouseholds : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.CreateTable(
            name: "Households",
            columns: table => new {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                InvitationCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Households", x => x.Id));

        migrationBuilder.CreateTable(
            name: "HouseholdMembers",
            columns: table => new {
                HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
            },
            constraints: table => {
                table.PrimaryKey("PK_HouseholdMembers", x => new { x.HouseholdId, x.UserId });
                table.ForeignKey("FK_HouseholdMembers_Households_HouseholdId", x => x.HouseholdId,
                    "Households", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("UX_Households_InvitationCode", "Households", "InvitationCode", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropTable("HouseholdMembers");
        migrationBuilder.DropTable("Households");
    }
}
