using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Backend.DAL;

#nullable disable

namespace backend.dal.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261124090000_AddHomes")]
    public partial class AddHomes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Homes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homes", x => x.Id);
                    table.ForeignKey("FK_Homes_Users_CreatedByUserId", x => x.CreatedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HomeMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeMembers", x => x.Id);
                    table.ForeignKey("FK_HomeMembers_Homes_HomeId", x => x.HomeId, "Homes", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_HomeMembers_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_Homes_CreatedByUserId", "Homes", "CreatedByUserId");
            migrationBuilder.CreateIndex("IX_HomeMembers_HomeId_UserId", "HomeMembers", new[] { "HomeId", "UserId" }, unique: true);
            migrationBuilder.CreateIndex("IX_HomeMembers_UserId", "HomeMembers", "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HomeMembers");
            migrationBuilder.DropTable(name: "Homes");
        }
    }
}
