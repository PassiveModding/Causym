using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using static Causym.Services.ModuleExtender.TranslateModuleHandler;

namespace Causym
{
    public class DataContext : DbContext
    {
        public DbSet<Licensing.Quantifiable.Profile> UseUsers { get; set; }

        public DbSet<Licensing.Quantifiable.License> UseLicenses { get; set; }

        public DbSet<Licensing.Timed.License> TimeLicenses { get; set; }

        public DbSet<Licensing.Timed.Profile> TimeUsers { get; set; }

        public DbSet<GuildConfiguration> Guilds { get; set; }

        public DbSet<Translation.TranslateGuild> TranslateGuilds { get; set; }

        public DbSet<Translation.TranslatePair> TranslatePairs { get; set; }

        public DbSet<Language> LanguageOverrides { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseNpgsql(Config.PostgresConnectionString());
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "database.sqlite")}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Licensing.Quantifiable.Profile>(e =>
            {
                e.HasKey(x => new { x.Service, x.Id });
            });
            modelBuilder.Entity<Licensing.Quantifiable.License>(e =>
            {
                e.HasKey(x => new { x.Service, x.Key });
            });
            modelBuilder.Entity<Licensing.Timed.Profile>(e =>
            {
                e.HasKey(x => new { x.Service, x.Id });
            });
            modelBuilder.Entity<Licensing.Timed.License>(e =>
            {
                e.HasKey(x => new { x.Service, x.Key });
            });
            modelBuilder.Entity<GuildConfiguration>(e =>
            {
                e.HasKey(x => x.GuildId);
            });
            modelBuilder.Entity<Translation.TranslateGuild>(e =>
            {
                e.HasKey(x => x.GuildId);
            });
            modelBuilder.Entity<Translation.TranslatePair>(e =>
            {
                e.HasKey(x => new { x.GuildId, x.Source });
            });
        }
    }
}
