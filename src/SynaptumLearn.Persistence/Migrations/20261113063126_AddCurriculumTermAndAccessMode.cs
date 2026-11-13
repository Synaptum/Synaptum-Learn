using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynaptumLearn.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumTermAndAccessMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Topics_GradeSubjectId",
                table: "Topics");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Topics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Term",
                table: "Topics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumAccessMode",
                table: "Schools",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_GradeSubjectId_Term_Order",
                table: "Topics",
                columns: new[] { "GradeSubjectId", "Term", "Order" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Topics_Order",
                table: "Topics",
                sql: "[Order] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Topics_Term",
                table: "Topics",
                sql: "[Term] BETWEEN 1 AND 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Topics_GradeSubjectId_Term_Order",
                table: "Topics");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Topics_Order",
                table: "Topics");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Topics_Term",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "Term",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "CurriculumAccessMode",
                table: "Schools");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_GradeSubjectId",
                table: "Topics",
                column: "GradeSubjectId");
        }
    }
}
