using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "GroupMembers");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "GroupMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "GroupMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GroupMembers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RespondedAt",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GroupMembers");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "GroupMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
