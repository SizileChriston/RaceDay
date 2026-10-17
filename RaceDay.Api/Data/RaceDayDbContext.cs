using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<RaceEvent> Events { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<RaceRoute> Routes { get; set; }

    public DbSet<Enrolment> Enrolments { get; set; }

    public DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<RaceEvent>()
            .HasOne(raceEvent => raceEvent.Organiser)
            .WithMany(user => user.OrganisedEvents)
            .HasForeignKey(raceEvent => raceEvent.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasOne(category => category.Event)
            .WithMany(raceEvent => raceEvent.Categories)
            .HasForeignKey(category => category.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RaceRoute>()
            .HasOne(route => route.Event)
            .WithMany(raceEvent => raceEvent.Routes)
            .HasForeignKey(route => route.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrolment>()
            .HasOne(enrolment => enrolment.User)
            .WithMany(user => user.Enrolments)
            .HasForeignKey(enrolment => enrolment.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(enrolment => enrolment.Category)
            .WithMany(category => category.Enrolments)
            .HasForeignKey(enrolment => enrolment.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasIndex(enrolment => new
            {
                enrolment.UserId,
                enrolment.CategoryId
            })
            .IsUnique();

        modelBuilder.Entity<Result>()
            .HasOne(result => result.Enrolment)
            .WithOne(enrolment => enrolment.Result)
            .HasForeignKey<Result>(result => result.EnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasIndex(result => result.EnrolmentId)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .Property(category => category.DistanceKm)
            .HasPrecision(6, 2);

        modelBuilder.Entity<Category>()
            .Property(category => category.EntryFee)
            .HasPrecision(10, 2);

        modelBuilder.Entity<RaceRoute>()
            .Property(route => route.DistanceKm)
            .HasPrecision(6, 2);
    }
}