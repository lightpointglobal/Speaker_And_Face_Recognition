using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SpeechAndFaceRecognizerWebCore.Application.Data.Migrations
{
    public partial class SomeChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MicrosoftFaceIdentificationPersonFaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PersonId = table.Column<Guid>(nullable: false),
                    Data = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicrosoftFaceIdentificationPersonFaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MicrosoftFaceIdentificationPersonFaces_MicrosoftFaceIdentificationPersons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "MicrosoftFaceIdentificationPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MicrosoftFaceIdentificationPersonFaces_PersonId",
                table: "MicrosoftFaceIdentificationPersonFaces",
                column: "PersonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MicrosoftFaceIdentificationPersonFaces");
        }
    }
}
