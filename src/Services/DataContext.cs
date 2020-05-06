using System;
using System.IO;
using Causym.Services;
using Microsoft.EntityFrameworkCore;

namespace Causym
{
    public class DataContext : DbContext
    {
        public DbSet<Modules.Configure.GuildConfiguration> Guilds { get; set; }

        // Translate Module
        public DbSet<Modules.Translation.TranslateGuild> TranslateGuilds { get; set; }

        public DbSet<Modules.Translation.TranslatePair> TranslatePairs { get; set; }

        // Statistics Module
        public DbSet<Modules.Statistics.StatisticsConfig> StatServers { get; set; }

        public DbSet<Modules.Statistics.StatisticsSnapshot> StatSnapshots { get; set; }

        public DbSet<Modules.Statistics.ChannelSnapshot> ChannelSnapshots { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(DbConnection.DbConnectionString);

            //optionsBuilder.UseSqlite(DbConnection.DbConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Modules.Configure.GuildConfiguration>(e =>
            {
                e.HasKey(x => x.GuildId);
            });

            // Translate Module
            modelBuilder.Entity<Modules.Translation.TranslateGuild>(e =>
            {
                e.HasKey(x => x.GuildId);
            });
            modelBuilder.Entity<Modules.Translation.TranslatePair>(e =>
            {
                e.HasKey(x => new { x.GuildId, x.Source });
            });

            // Statistics Module
            modelBuilder.Entity<Modules.Statistics.StatisticsConfig>(e =>
            {
                e.HasKey(x => x.GuildId);
            });

            modelBuilder.Entity<Modules.Statistics.StatisticsSnapshot>(e =>
            {
                e.HasKey(x => new { x.GuildId, x.SnapshotTime });
            });

            modelBuilder.Entity<Modules.Statistics.ChannelSnapshot>(e =>
            {
                e.HasKey(x => new { x.GuildId, x.ChannelId, x.SnapshotTime });
            });
        }
    }
}