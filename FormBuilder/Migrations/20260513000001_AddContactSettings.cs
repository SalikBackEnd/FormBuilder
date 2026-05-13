using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.Migrations
{
    /// <inheritdoc />
    public partial class AddContactSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CollectSubmitterEmail",
                table: "Forms",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "CollectSubmitterName",
                table: "Forms",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SubmitterEmailRequired",
                table: "Forms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SubmitterNameRequired",
                table: "Forms",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollectSubmitterEmail",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "CollectSubmitterName",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "SubmitterEmailRequired",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "SubmitterNameRequired",
                table: "Forms");
        }
    }
}
