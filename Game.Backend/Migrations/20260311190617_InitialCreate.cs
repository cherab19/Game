using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UsernameNormalized = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EmailNormalized = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leaderboards",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    Kills = table.Column<int>(type: "integer", nullable: false),
                    Deaths = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leaderboards", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_leaderboards_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lobbies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Region = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GameMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    HostPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lobbies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lobbies_players_HostPlayerId",
                        column: x => x.HostPlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LobbyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_games_lobbies_LobbyId",
                        column: x => x.LobbyId,
                        principalTable: "lobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lobby_members",
                columns: table => new
                {
                    LobbyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lobby_members", x => new { x.LobbyId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_lobby_members_lobbies_LobbyId",
                        column: x => x.LobbyId,
                        principalTable: "lobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lobby_members_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LobbyId = table.Column<Guid>(type: "uuid", nullable: true),
                    GameId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_events_games_GameId",
                        column: x => x.GameId,
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_events_lobbies_LobbyId",
                        column: x => x.LobbyId,
                        principalTable: "lobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_events_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_events_GameId_CreatedAtUtc",
                table: "events",
                columns: new[] { "GameId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_events_LobbyId_CreatedAtUtc",
                table: "events",
                columns: new[] { "LobbyId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_events_PlayerId",
                table: "events",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_games_LobbyId",
                table: "games",
                column: "LobbyId");

            migrationBuilder.CreateIndex(
                name: "IX_leaderboards_Score",
                table: "leaderboards",
                column: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_leaderboards_Wins",
                table: "leaderboards",
                column: "Wins");

            migrationBuilder.CreateIndex(
                name: "IX_lobbies_Code",
                table: "lobbies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lobbies_HostPlayerId",
                table: "lobbies",
                column: "HostPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_lobby_members_PlayerId",
                table: "lobby_members",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_players_EmailNormalized",
                table: "players",
                column: "EmailNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_UsernameNormalized",
                table: "players",
                column: "UsernameNormalized",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "leaderboards");

            migrationBuilder.DropTable(
                name: "lobby_members");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "lobbies");

            migrationBuilder.DropTable(
                name: "players");
        }
    }
}
