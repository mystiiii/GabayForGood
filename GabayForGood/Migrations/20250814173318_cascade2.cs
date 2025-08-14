using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GabayForGood.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class cascade2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUpdates_Projects_ProjectId",
                table: "ProjectUpdates");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUpdates_Projects_ProjectId",
                table: "ProjectUpdates",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUpdates_Projects_ProjectId",
                table: "ProjectUpdates");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUpdates_Projects_ProjectId",
                table: "ProjectUpdates",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
