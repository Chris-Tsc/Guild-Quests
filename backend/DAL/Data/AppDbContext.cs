
using DAL.Identity;
using DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class AppDbContext
        : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<DailyQuest> DailyQuests => Set<DailyQuest>();
        public DbSet<GuildQuest> GuildQuests => Set<GuildQuest>();
        public DbSet<Events> Events => Set<Events>();
        public DbSet<SkillNode> SkillNodes => Set<SkillNode>();
        public DbSet<QuestOption> QuestOptions => Set<QuestOption>();
        public DbSet<PlayerGuildQuest> PlayerGuildQuests => Set<PlayerGuildQuest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique in-game name
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.InGameName)
                .IsUnique();

            // Player - Identity User relationship
            modelBuilder.Entity<Player>()
                .HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(p => p.AppUserId)
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
        }
    }
}
