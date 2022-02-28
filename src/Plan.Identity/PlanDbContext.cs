using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Plan.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plan.Identity
{
    public class PlanDbContext : IdentityDbContext<PlanUser, PlanRole, long>
    {
        public PlanDbContext(DbContextOptions options) : base(options)
        {
        }

        protected PlanDbContext()
        {
        }

        public DbSet<PlanDelay> PlanDelays { get; set; }

        public DbSet<PlanGroup> PlanGroups { get; set; }

        public DbSet<PlanItem> PlanItems { get; set; }

        public DbSet<PlanItemAnnex> PlanItemAnnices { get; set; }

        public DbSet<PlanItemUser> PlanItemUsers { get; set; }

        public DbSet<PlanNotifyJob> PlanNotifyJobs { get; set; }

        public DbSet<PlanApp> PlanApps { get; set; }

        public DbSet<PlanFriendKey> PlanFriendKeys { get; set; }

        public DbSet<PlanUserFriend> PlanUserFriends { get; set; }

        public DbSet<PlanAppKey> PlanAppKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PlanGroup>(b =>
            {
                b.HasIndex(x => x.Name);

                b.HasMany(x => x.Items)
                    .WithOne(x => x.Group)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<PlanItem>(b =>
            {
                b.HasIndex(x => x.Title);
                b.HasIndex(x => x.NowType);
            });
            builder.Entity<PlanItemUser>(b =>
            {
                b.HasKey(x => new { x.UserId, x.ItemId });
            });
            builder.Entity<PlanItemAnnex>(b =>
            {

            }); 
            builder.Entity<PlanApp>(b =>
            {
                b.HasQueryFilter(x => x.Enable);
                b.HasIndex(x => x.AppKey)
                    .IsUnique(true);
            });
            builder.Entity<PlanUserFriend>(b =>
            {
                b.HasKey(x => new { x.OwnerUserId, x.TargetUserId });
            }); 
            builder.Entity<PlanFriendKey>(b =>
            {
                b.HasIndex(x => x.Key);
            });
        }
    }
}
