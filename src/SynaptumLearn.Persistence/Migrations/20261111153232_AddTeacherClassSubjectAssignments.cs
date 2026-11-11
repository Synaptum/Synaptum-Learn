using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynaptumLearn.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherClassSubjectAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeacherClassSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherSubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherClassSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherClassSubjects_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherClassSubjects_TeacherSubjects_TeacherSubjectId",
                        column: x => x.TeacherSubjectId,
                        principalTable: "TeacherSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClassSubjects_ClassId",
                table: "TeacherClassSubjects",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClassSubjects_SchoolId_ClassId",
                table: "TeacherClassSubjects",
                columns: new[] { "SchoolId", "ClassId" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClassSubjects_SchoolId_TeacherSubjectId_ClassId",
                table: "TeacherClassSubjects",
                columns: new[] { "SchoolId", "TeacherSubjectId", "ClassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClassSubjects_TeacherSubjectId",
                table: "TeacherClassSubjects",
                column: "TeacherSubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherClassSubjects");
        }
    }
}
