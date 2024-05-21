using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemoryVaultAPI.Migrations
{
    /// <inheritdoc />
    public partial class LikesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    LikerID = table.Column<int>(type: "int", nullable: false),
                    MemoryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => new { x.MemoryID, x.LikerID });
                    table.ForeignKey(
                        name: "FK_Likes_Accounts_LikerID",
                        column: x => x.LikerID,
                        principalTable: "Accounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Likes_Memories_MemoryID",
                        column: x => x.MemoryID,
                        principalTable: "Memories",
                        principalColumn: "MemoryID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Likes_LikerID",
                table: "Likes",
                column: "LikerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Likes");
        }
    }
}
