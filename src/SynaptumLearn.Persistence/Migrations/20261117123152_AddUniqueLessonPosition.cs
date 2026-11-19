using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynaptumLearn.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueLessonPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lessons_TopicId",
                table: "Lessons");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_TopicId_Order",
                table: "Lessons",
                columns: new[] { "TopicId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lessons_TopicId_Order",
                table: "Lessons");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_TopicId",
                table: "Lessons",
                column: "TopicId");
        }
    }
}
