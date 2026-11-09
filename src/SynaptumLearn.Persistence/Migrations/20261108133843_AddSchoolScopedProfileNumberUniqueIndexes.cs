using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynaptumLearn.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolScopedProfileNumberUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherProfiles_SchoolId",
                table: "TeacherProfiles");

            migrationBuilder.DropIndex(
                name: "IX_LearnerProfiles_SchoolId",
                table: "LearnerProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherProfiles_SchoolId_EmployeeNumber",
                table: "TeacherProfiles",
                columns: new[] { "SchoolId", "EmployeeNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerProfiles_SchoolId_LearnerNumber",
                table: "LearnerProfiles",
                columns: new[] { "SchoolId", "LearnerNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherProfiles_SchoolId_EmployeeNumber",
                table: "TeacherProfiles");

            migrationBuilder.DropIndex(
                name: "IX_LearnerProfiles_SchoolId_LearnerNumber",
                table: "LearnerProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherProfiles_SchoolId",
                table: "TeacherProfiles",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerProfiles_SchoolId",
                table: "LearnerProfiles",
                column: "SchoolId");
        }
    }
}
