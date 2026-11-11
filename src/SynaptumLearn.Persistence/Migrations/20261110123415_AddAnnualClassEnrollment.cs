using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynaptumLearn.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnualClassEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYear = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.CheckConstraint("CK_Classes_AcademicYear", "[AcademicYear] BETWEEN 2000 AND 2100");
                    table.ForeignKey(
                        name: "FK_Classes_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearnerClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYear = table.Column<int>(type: "int", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerClasses", x => x.Id);
                    table.CheckConstraint("CK_LearnerClasses_AcademicYear", "[AcademicYear] BETWEEN 2000 AND 2100");
                    table.CheckConstraint("CK_LearnerClasses_Dates", "[LeftAt] IS NULL OR [LeftAt] >= [EnrolledAt]");
                    table.ForeignKey(
                        name: "FK_LearnerClasses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LearnerClasses_LearnerProfiles_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "LearnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_GradeId",
                table: "Classes",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_SchoolId_GradeId_AcademicYear_Name",
                table: "Classes",
                columns: new[] { "SchoolId", "GradeId", "AcademicYear", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnerClasses_ClassId",
                table: "LearnerClasses",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerClasses_LearnerId",
                table: "LearnerClasses",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerClasses_SchoolId_LearnerId_AcademicYear",
                table: "LearnerClasses",
                columns: new[] { "SchoolId", "LearnerId", "AcademicYear" },
                unique: true,
                filter: "[LeftAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerClasses");

            migrationBuilder.DropTable(
                name: "Classes");
        }
    }
}
