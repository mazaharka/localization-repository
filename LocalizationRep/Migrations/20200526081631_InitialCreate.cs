using Microsoft.EntityFrameworkCore.Migrations;

namespace LocalizationRep.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileModel",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileModel", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Section",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(nullable: true),
                    ShortName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Section", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MainTable",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SectionID = table.Column<int>(nullable: false),
                    CommonID = table.Column<string>(nullable: true),
                    IOsID = table.Column<string>(nullable: true),
                    AndroidID = table.Column<string>(nullable: true),
                    TextRU = table.Column<string>(nullable: true),
                    TextEN = table.Column<string>(nullable: true),
                    TextUA = table.Column<string>(nullable: true),
                    IsFreezing = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainTable", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MainTable_Section_SectionID",
                        column: x => x.SectionID,
                        principalTable: "Section",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MainTable_SectionID",
                table: "MainTable",
                column: "SectionID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileModel");

            migrationBuilder.DropTable(
                name: "MainTable");

            migrationBuilder.DropTable(
                name: "Section");
        }
    }
}
