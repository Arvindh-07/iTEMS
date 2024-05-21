using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iTEMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addstateandarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AreaByState",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaByState",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Employees");
        }
    }
}
