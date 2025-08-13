using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GabayForGood.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class testing123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProjectUpdates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProjectUpdates");
        }
    }
}
