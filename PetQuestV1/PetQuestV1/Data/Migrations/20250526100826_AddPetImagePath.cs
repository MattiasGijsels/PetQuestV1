using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetQuestV1.Migrations
{
    /// <inheritdoc />
    public partial class AddPetImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Pets");
        }
    }
}
