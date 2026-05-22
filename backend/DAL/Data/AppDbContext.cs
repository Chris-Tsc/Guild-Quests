using DAL.Identity;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<DailyQuest> DailyQuests => Set<DailyQuest>();
        public DbSet<GuildQuest> GuildQuests => Set<GuildQuest>();
        public DbSet<Events> Events => Set<Events>();
        public DbSet<SkillNode> SkillNodes => Set<SkillNode>();
        public DbSet<DailyQuestOption> DailyQuestOptions => Set<DailyQuestOption>();
        public DbSet<GuildQuestOption> GuildQuestOptions => Set<GuildQuestOption>();
        public DbSet<PlayerGuildQuest> PlayerGuildQuests => Set<PlayerGuildQuest>();
        public DbSet<PlayerDailyQuest> PlayerDailyQuests => Set<PlayerDailyQuest>();
        public DbSet<PlayerRolledGuildQuest> PlayerRolledGuildQuests => Set<PlayerRolledGuildQuest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique in-game name
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.InGameName)
                .IsUnique();

            // Unique username
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Player - Identity User relationship
            modelBuilder.Entity<Player>()
                .HasOne(p => p.AppUser)
                .WithOne(u => u.Player)
                .HasForeignKey<Player>(p => p.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // PlayerGuildQuest relationships
            modelBuilder.Entity<PlayerGuildQuest>()
                .HasOne(pgq => pgq.Player)
                .WithMany()
                .HasForeignKey(pgq => pgq.PlayerId);

            modelBuilder.Entity<PlayerGuildQuest>()
                .HasOne(pgq => pgq.GuildQuest)
                .WithMany()
                .HasForeignKey(pgq => pgq.GuildQuestId);

            modelBuilder.Entity<PlayerGuildQuest>()
                .HasIndex(pgq => new { pgq.PlayerId, pgq.DaytimeInfoUtc, pgq.GuildQuestId })
                .IsUnique();

            // PlayerDailyQuest relationships
            modelBuilder.Entity<PlayerDailyQuest>()
                .HasOne(pdq => pdq.Player)
                .WithMany()
                .HasForeignKey(pdq => pdq.PlayerId);

            modelBuilder.Entity<PlayerDailyQuest>()
                .HasOne(pdq => pdq.DailyQuest)
                .WithMany()
                .HasForeignKey(pdq => pdq.DailyQuestId);

            modelBuilder.Entity<DailyQuestOption>()
                .HasOne(o => o.DailyQuest)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.DailyQuestId);

            //PlayerRolledGuildQuest relationships
            modelBuilder.Entity<PlayerRolledGuildQuest>()
                .HasOne(pgbq => pgbq.Player)
                .WithMany()
                .HasForeignKey(pgbq => pgbq.PlayerId);

            modelBuilder.Entity<PlayerRolledGuildQuest>()
                .HasOne(pgbq => pgbq.GuildQuest)
                .WithMany()
                .HasForeignKey(pgbq => pgbq.GuildQuestId);

            modelBuilder.Entity<PlayerRolledGuildQuest>()
                .HasIndex(pgbq => new { pgbq.PlayerId, pgbq.DaytimeInfoUtc, pgbq.GuildQuestId })
                .IsUnique();

            modelBuilder.Entity<GuildQuestOption>()
                .HasOne(o => o.GuildQuest)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.GuildQuestId);
        }
    }
}
