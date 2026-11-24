using System;
using Backend.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.dal.Migrations;

public partial class AlignHouseholdModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_HomeMembers_Homes_HomeId", table: "HomeMembers");
        migrationBuilder.DropForeignKey(name: "FK_HomeMembers_Users_UserId", table: "HomeMembers");
        migrationBuilder.DropForeignKey(name: "FK_Homes_Users_CreatedByUserId", table: "Homes");
        migrationBuilder.DropIndex(name: "IX_Homes_CreatedByUserId", table: "Homes");
        migrationBuilder.DropIndex(name: "IX_HomeMembers_HomeId_UserId", table: "HomeMembers");
        migrationBuilder.DropIndex(name: "IX_HomeMembers_UserId", table: "HomeMembers");
        migrationBuilder.DropPrimaryKey(name: "PK_HomeMembers", table: "HomeMembers");

        migrationBuilder.RenameTable(name: "Homes", newName: "Households");
        migrationBuilder.RenameTable(name: "HomeMembers", newName: "HouseholdMembers");
        migrationBuilder.RenameColumn(name: "HomeId", table: "HouseholdMembers", newName: "HouseholdId");

        migrationBuilder.DropColumn(name: "CreatedByUserId", table: "Households");
        migrationBuilder.DropColumn(name: "Id", table: "HouseholdMembers");
        migrationBuilder.AlterColumn<string>(name: "Name", table: "Households", type: "character varying(100)", maxLength: 100, nullable: false, oldClrType: typeof(string), oldType: "character varying(120)", oldMaxLength: 120);
        migrationBuilder.AddColumn<string>(name: "InvitationCode", table: "Households", type: "character varying(16)", maxLength: 16, nullable: true);

        migrationBuilder.AddPrimaryKey(name: "PK_HouseholdMembers", table: "HouseholdMembers", columns: new[] { "HouseholdId", "UserId" });
        migrationBuilder.CreateIndex(name: "IX_Households_InvitationCode", table: "Households", column: "InvitationCode", unique: true);
        migrationBuilder.CreateIndex(name: "IX_HouseholdMembers_UserId", table: "HouseholdMembers", column: "UserId");
        migrationBuilder.AddForeignKey(name: "FK_HouseholdMembers_Households_HouseholdId", table: "HouseholdMembers", column: "HouseholdId", principalTable: "Households", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey(name: "FK_HouseholdMembers_Users_UserId", table: "HouseholdMembers", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_HouseholdMembers_Households_HouseholdId", table: "HouseholdMembers");
        migrationBuilder.DropForeignKey(name: "FK_HouseholdMembers_Users_UserId", table: "HouseholdMembers");
        migrationBuilder.DropIndex(name: "IX_Households_InvitationCode", table: "Households");
        migrationBuilder.DropIndex(name: "IX_HouseholdMembers_UserId", table: "HouseholdMembers");
        migrationBuilder.DropPrimaryKey(name: "PK_HouseholdMembers", table: "HouseholdMembers");
        migrationBuilder.DropColumn(name: "InvitationCode", table: "Households");

        migrationBuilder.AddColumn<Guid>(name: "Id", table: "HouseholdMembers", type: "uuid", nullable: true);
        migrationBuilder.Sql("UPDATE \"HouseholdMembers\" SET \"Id\" = gen_random_uuid() WHERE \"Id\" IS NULL;");
        migrationBuilder.AlterColumn<Guid>(name: "Id", table: "HouseholdMembers", type: "uuid", nullable: false, oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);
        migrationBuilder.AddPrimaryKey(name: "PK_HouseholdMembers", table: "HouseholdMembers", column: "Id");
        migrationBuilder.RenameColumn(name: "HouseholdId", table: "HouseholdMembers", newName: "HomeId");
        migrationBuilder.RenameTable(name: "HouseholdMembers", newName: "HomeMembers");
        migrationBuilder.RenameTable(name: "Households", newName: "Homes");
        migrationBuilder.AlterColumn<string>(name: "Name", table: "Homes", type: "character varying(120)", maxLength: 120, nullable: false, oldClrType: typeof(string), oldType: "character varying(100)", oldMaxLength: 100);
        migrationBuilder.AddColumn<Guid>(name: "CreatedByUserId", table: "Homes", type: "uuid", nullable: true);
        migrationBuilder.Sql("UPDATE \"Homes\" h SET \"CreatedByUserId\" = m.\"UserId\" FROM \"HomeMembers\" m WHERE m.\"HomeId\" = h.\"Id\" AND m.\"Role\" = 'Admin';");
        migrationBuilder.AlterColumn<Guid>(name: "CreatedByUserId", table: "Homes", type: "uuid", nullable: false, oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);
        migrationBuilder.CreateIndex(name: "IX_Homes_CreatedByUserId", table: "Homes", column: "CreatedByUserId");
        migrationBuilder.CreateIndex(name: "IX_HomeMembers_HomeId_UserId", table: "HomeMembers", columns: new[] { "HomeId", "UserId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_HomeMembers_UserId", table: "HomeMembers", column: "UserId");
        migrationBuilder.AddForeignKey(name: "FK_Homes_Users_CreatedByUserId", table: "Homes", column: "CreatedByUserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(name: "FK_HomeMembers_Homes_HomeId", table: "HomeMembers", column: "HomeId", principalTable: "Homes", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey(name: "FK_HomeMembers_Users_UserId", table: "HomeMembers", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
    }
}
