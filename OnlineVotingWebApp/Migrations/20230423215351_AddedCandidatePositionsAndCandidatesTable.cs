using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVotingWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddedCandidatePositionsAndCandidatesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidatePositions",
                columns: table => new
                {
                    CandidatePositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatePositionName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatePosition_CandidatePositionId", x => x.CandidatePositionId);
                });

            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    CandidateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatePositionId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    MiddleName = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    LastName = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Party = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Photo = table.Column<string>(type: "varchar(max)", unicode: false, maxLength: 2147483647, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Candidate_CandidateId", x => x.CandidateId);
                    table.ForeignKey(
                        name: "FK_Candidates_CandidatePositions_CandidatePositionId",
                        column: x => x.CandidatePositionId,
                        principalTable: "CandidatePositions",
                        principalColumn: "CandidatePositionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_CandidatePositionId",
                table: "Candidates",
                column: "CandidatePositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candidates");

            migrationBuilder.DropTable(
                name: "CandidatePositions");
        }
    }
}
