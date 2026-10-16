using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynaptumLearn.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSchoolMembershipIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchoolMemberships_SchoolId",
                table: "SchoolMemberships");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolMemberships_SchoolId_UserId",
                table: "SchoolMemberships",
                columns: new[] { "SchoolId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchoolMemberships_SchoolId_UserId",
                table: "SchoolMemberships");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolMemberships_SchoolId",
                table: "SchoolMemberships",
                column: "SchoolId");
        }
    }
}
