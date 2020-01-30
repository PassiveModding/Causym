using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Causym.Migrations
{
    public partial class InitDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelSnapshots",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false),
                    SnapshotTime = table.Column<DateTime>(nullable: false),
                    MessageCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSnapshots", x => new { x.GuildId, x.ChannelId, x.SnapshotTime });
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    Prefix = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "StatServers",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    MemberChannelId = table.Column<ulong>(nullable: true),
                    SnapshotsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatServers", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "StatSnapshots",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    SnapshotTime = table.Column<DateTime>(nullable: false),
                    MemberCount = table.Column<int>(nullable: false),
                    TotalMessageCount = table.Column<int>(nullable: false),
                    CachedMembers = table.Column<int>(nullable: false),
                    MembersOnline = table.Column<int>(nullable: false),
                    MembersDND = table.Column<int>(nullable: false),
                    MembersIdle = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatSnapshots", x => new { x.GuildId, x.SnapshotTime });
                });

            migrationBuilder.CreateTable(
                name: "TranslateGuilds",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    ReactionsEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslateGuilds", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "TranslatePairs",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    Source = table.Column<string>(maxLength: 100, nullable: false),
                    DestLang = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslatePairs", x => new { x.GuildId, x.Source });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelSnapshots");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "StatServers");

            migrationBuilder.DropTable(
                name: "StatSnapshots");

            migrationBuilder.DropTable(
                name: "TranslateGuilds");

            migrationBuilder.DropTable(
                name: "TranslatePairs");
        }
    }
}
