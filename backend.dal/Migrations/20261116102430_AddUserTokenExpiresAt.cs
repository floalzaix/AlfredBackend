using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.dal.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTokenExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "UserTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_ExpiresAt",
                table: "UserTokens",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTokens_ExpiresAt",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "UserTokens");
        }
    }
}
