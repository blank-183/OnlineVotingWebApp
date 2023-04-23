using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineVotingWebApp.Models;

namespace OnlineVotingWebApp.Data;

public partial class OnlineVotingDbContext : IdentityDbContext
{
    public OnlineVotingDbContext()
    {
    }

    public OnlineVotingDbContext(DbContextOptions<OnlineVotingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
    public virtual DbSet<VoteEvent> VoteEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Sex).IsFixedLength();
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__Address_AddressId");

            entity.HasOne(d => d.ApplicationUser)
            .WithOne(e => e.Address)
            .HasForeignKey<Address>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.ActivityLogId).HasName("PK__ActivityLog_ActivityLogId");

            entity.HasOne(d => d.ApplicationUser)
            .WithMany(e => e.ActivityLogs)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VoteEvent>(entity =>
        {
            entity.HasKey(e => e.VoteEventId).HasName("PK__VoteEven__9EF87FEC1BBBED19");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
