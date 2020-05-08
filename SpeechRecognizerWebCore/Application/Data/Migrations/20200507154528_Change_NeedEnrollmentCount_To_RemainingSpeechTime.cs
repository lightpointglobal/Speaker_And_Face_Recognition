using Microsoft.EntityFrameworkCore.Migrations;

namespace SpeechAndFaceRecognizerWebCore.Application.Data.Migrations
{
    public partial class Change_NeedEnrollmentCount_To_RemainingSpeechTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedEnrollmentsCount",
                table: "MicrosoftSpeekerIdentificationProfiles");

            migrationBuilder.AddColumn<double>(
                name: "RemainingSpeechTime",
                table: "MicrosoftSpeekerIdentificationProfiles",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingSpeechTime",
                table: "MicrosoftSpeekerIdentificationProfiles");

            migrationBuilder.AddColumn<int>(
                name: "NeedEnrollmentsCount",
                table: "MicrosoftSpeekerIdentificationProfiles",
                nullable: false,
                defaultValue: 0);
        }
    }
}
