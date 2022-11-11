using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrackyASusarny.Migrations
{
    public partial class foreignkeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowableEntity_Locations_LocationID",
                table: "BorrowableEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowableEntity_Manuals_ManualID",
                table: "BorrowableEntity");

            migrationBuilder.AlterColumn<int>(
                name: "LocationID",
                table: "BorrowableEntity",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowableEntity_Locations_LocationID",
                table: "BorrowableEntity",
                column: "LocationID",
                principalTable: "Locations",
                principalColumn: "LocationID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowableEntity_Manuals_ManualID",
                table: "BorrowableEntity",
                column: "ManualID",
                principalTable: "Manuals",
                principalColumn: "ManualID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowableEntity_Locations_LocationID",
                table: "BorrowableEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowableEntity_Manuals_ManualID",
                table: "BorrowableEntity");

            migrationBuilder.AlterColumn<int>(
                name: "LocationID",
                table: "BorrowableEntity",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowableEntity_Locations_LocationID",
                table: "BorrowableEntity",
                column: "LocationID",
                principalTable: "Locations",
                principalColumn: "LocationID");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowableEntity_Manuals_ManualID",
                table: "BorrowableEntity",
                column: "ManualID",
                principalTable: "Manuals",
                principalColumn: "ManualID");
        }
    }
}
