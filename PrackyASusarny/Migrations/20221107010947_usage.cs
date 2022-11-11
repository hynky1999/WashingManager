using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrackyASusarny.Migrations
{
    public partial class usage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DryingRoomUsage",
                columns: table => new
                {
                    DayId = table.Column<int>(type: "integer", nullable: false),
                    Hour0Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour1Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour2Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour3Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour4Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour5Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour6Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour7Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour8Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour9Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour10Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour11Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour12Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour13Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour14Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour15Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour16Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour17Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour18Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour19Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour20Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour21Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour22Total = table.Column<long>(type: "bigint", nullable: false),
                    Hour23Total = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DryingRoomUsage", x => x.DayId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DryingRoomUsage");
        }
    }
}
