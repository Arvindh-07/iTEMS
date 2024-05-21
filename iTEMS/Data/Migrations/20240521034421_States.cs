using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTEMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class States : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "States",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "States",
                table: "Employees");
        }
    }
}
