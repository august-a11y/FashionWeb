using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fix_fk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserRoles_AppRoles_AppRoleId",
                table: "AppUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppUserRoles_AppRoles_RolesId",
                table: "AppUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppUserRoles_AppUsers_AppUserId",
                table: "AppUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppUserRoles_AppUsers_UsersId",
                table: "AppUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AppUserRoles_AppRoleId",
                table: "AppUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AppUserRoles_AppUserId",
                table: "AppUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AppUserRoles_RolesId",
                table: "AppUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AppUserRoles_UsersId",
                table: "AppUserRoles");

            migrationBuilder.DropColumn(
                name: "AppRoleId",
                table: "AppUserRoles");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "AppUserRoles");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AppUserRoles");

            migrationBuilder.DropColumn(
                name: "RolesId",
                table: "AppUserRoles");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "AppUserRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppRoleId",
                table: "AppUserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                table: "AppUserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AppUserRoles",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RolesId",
                table: "AppUserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UsersId",
                table: "AppUserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_AppRoleId",
                table: "AppUserRoles",
                column: "AppRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_AppUserId",
                table: "AppUserRoles",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_RolesId",
                table: "AppUserRoles",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_UsersId",
                table: "AppUserRoles",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserRoles_AppRoles_AppRoleId",
                table: "AppUserRoles",
                column: "AppRoleId",
                principalTable: "AppRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserRoles_AppRoles_RolesId",
                table: "AppUserRoles",
                column: "RolesId",
                principalTable: "AppRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserRoles_AppUsers_AppUserId",
                table: "AppUserRoles",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserRoles_AppUsers_UsersId",
                table: "AppUserRoles",
                column: "UsersId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
