using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.dal.Migrations
{
    public partial class AddHomesAndMemberships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Homes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homes", home => home.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeMemberships",
                columns: table => new
                {
                    HomeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeMemberships", membership => new { membership.HomeId, membership.UserId });
                    table.ForeignKey(
                        name: "FK_HomeMemberships_Homes_HomeId",
                        column: membership => membership.HomeId,
                        principalTable: "Homes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeMemberships_Users_UserId",
                        column: membership => membership.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomeMemberships_UserId",
                table: "HomeMemberships",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HomeMemberships");
            migrationBuilder.DropTable(name: "Homes");
        }
    }
}
