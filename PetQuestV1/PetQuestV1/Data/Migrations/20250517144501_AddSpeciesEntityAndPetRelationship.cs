using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetQuestV1.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeciesEntityAndPetRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Species_NameId",
                table: "Pets");

            migrationBuilder.RenameColumn(
                name: "NameId",
                table: "Pets",
                newName: "SpeciesId");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_NameId",
                table: "Pets",
                newName: "IX_Pets_SpeciesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Species_SpeciesId",
                table: "Pets",
                column: "SpeciesId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Species_SpeciesId",
                table: "Pets");

            migrationBuilder.RenameColumn(
                name: "SpeciesId",
                table: "Pets",
                newName: "NameId");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_SpeciesId",
                table: "Pets",
                newName: "IX_Pets_NameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Species_NameId",
                table: "Pets",
                column: "NameId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
