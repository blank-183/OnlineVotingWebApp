using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public virtual DbSet<CandidatePosition> CandidatePositions { get; set; }
    public virtual DbSet<Candidate> Candidates { get; set; }
    public virtual DbSet<Vote> Votes { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }

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

        modelBuilder.Entity<CandidatePosition>(entity =>
        {
            entity.HasKey(e => e.CandidatePositionId).HasName("PK_CandidatePosition_CandidatePositionId");
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.CandidateId).HasName("PK__Candidate_CandidateId");

            entity.HasOne(d => d.CandidatePosition)
            .WithMany(e => e.Candidates)
            .HasForeignKey(e => e.CandidatePositionId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VoteEvent>(entity =>
        {
            entity.HasKey(e => e.VoteEventId).HasName("PK__VoteEven__9EF87FEC1BBBED19");
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.VoteId).HasName("PK__Vote__VoteId");

            entity.HasOne(d => d.ApplicationUser)
            .WithOne(e => e.Vote)
            .HasForeignKey<Vote>(e => e.VoterId)
            .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transaction__TransactionId");

            entity.HasOne(d => d.Candidate)
            .WithMany(e => e.Transactions)
            .HasForeignKey(e => e.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Vote)
            .WithMany(e => e.Transactions)
            .HasForeignKey(e => e.VoteId)
            .OnDelete(DeleteBehavior.Cascade);

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
