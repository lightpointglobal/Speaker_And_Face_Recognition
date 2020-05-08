using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SpeechAndFaceRecognizerWebCore.Application.Data.Migrations
{
    public partial class InitializeDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MicrosoftFaceIdentificationPersonGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    UserData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicrosoftFaceIdentificationPersonGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Login = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MicrosoftFaceIdentificationPersons",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    UserData = table.Column<string>(nullable: true),
                    PersonGroupId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicrosoftFaceIdentificationPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MicrosoftFaceIdentificationPersons_MicrosoftFaceIdentificationPersonGroups_PersonGroupId",
                        column: x => x.PersonGroupId,
                        principalTable: "MicrosoftFaceIdentificationPersonGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MicrosoftFaceIdentificationPersons_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MicrosoftSpeekerIdentificationProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    NeedEnrollmentsCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicrosoftSpeekerIdentificationProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MicrosoftSpeekerIdentificationProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MicrosoftFaceIdentificationPersons_PersonGroupId",
                table: "MicrosoftFaceIdentificationPersons",
                column: "PersonGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MicrosoftFaceIdentificationPersons_UserId",
                table: "MicrosoftFaceIdentificationPersons",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MicrosoftSpeekerIdentificationProfiles_UserId",
                table: "MicrosoftSpeekerIdentificationProfiles",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MicrosoftFaceIdentificationPersons");

            migrationBuilder.DropTable(
                name: "MicrosoftSpeekerIdentificationProfiles");

            migrationBuilder.DropTable(
                name: "MicrosoftFaceIdentificationPersonGroups");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
