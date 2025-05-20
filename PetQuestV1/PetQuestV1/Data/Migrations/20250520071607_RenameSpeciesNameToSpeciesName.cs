using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetQuestV1.Migrations
{
    /// <inheritdoc />
    public partial class RenameSpeciesNameToSpeciesName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Species",
                newName: "SpeciesName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SpeciesName",
                table: "Species",
                newName: "Name");
        }
    }
}
