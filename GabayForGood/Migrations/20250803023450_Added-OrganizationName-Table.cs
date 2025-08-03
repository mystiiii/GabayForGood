using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GabayForGood.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrganizationNameTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Organizations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Organizations");
        }
    }
}
