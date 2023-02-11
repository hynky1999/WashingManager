using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    public partial class bp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "startDate",
                table: "Borrows",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "endDate",
                table: "Borrows",
                newName: "End");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "BorrowPeople",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowPeople_UserID",
                table: "BorrowPeople",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowPeople_AspNetUsers_UserID",
                table: "BorrowPeople",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowPeople_AspNetUsers_UserID",
                table: "BorrowPeople");

            migrationBuilder.DropIndex(
                name: "IX_BorrowPeople_UserID",
                table: "BorrowPeople");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "BorrowPeople");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "Borrows",
                newName: "startDate");

            migrationBuilder.RenameColumn(
                name: "End",
                table: "Borrows",
                newName: "endDate");
        }
    }
}
