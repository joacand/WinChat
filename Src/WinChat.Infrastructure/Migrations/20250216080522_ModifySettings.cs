using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WinChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ApplicationData");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "ApplicationData",
                newName: "SettingValue");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ApplicationData",
                newName: "SettingKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SettingValue",
                table: "ApplicationData",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "SettingKey",
                table: "ApplicationData",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ApplicationData",
                type: "TEXT",
                nullable: true);
        }
    }
}
