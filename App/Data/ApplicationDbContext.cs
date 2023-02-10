using App.Auth.Models;
using App.Auth.Utils;
using App.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Data;

/// <summary>
/// Database context for the application.
/// </summary>
public class
    ApplicationDbContext : IdentityDbContext<ApplicationUser, Role, int>
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="options">DB options</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public ApplicationDbContext()
    {
    }

    /// <summary>
    /// Locations table
    /// </summary>
    public DbSet<Location> Locations => Set<Location>();

    /// <summary>
    /// WashingMachines table
    /// </summary>
    public DbSet<WashingMachine> WashingMachines => Set<WashingMachine>();

    /// <summary>
    /// DryingRooms table
    /// </summary>
    public DbSet<DryingRoom> DryingRooms => Set<DryingRoom>();

    /// <summary>
    /// WM Usage table
    /// </summary>
    public DbSet<BorrowableEntityUsage<WashingMachine>> WashingMachineUsage =>
        Set<BorrowableEntityUsage<WashingMachine>>();

    /// <summary>
    /// DR Usage table
    /// </summary>
    public DbSet<BorrowableEntityUsage<DryingRoom>> DryingRoomUsage =>
        Set<BorrowableEntityUsage<DryingRoom>>();

    /// <summary>
    /// Reservations table
    /// </summary>
    public DbSet<Reservation> Reservations => Set<Reservation>();

    /// <summary>
    /// Borrows table
    /// </summary>
    public virtual DbSet<Borrow> Borrows => Set<Borrow>();

    /// <summary>
    /// Manuals table
    /// </summary>
    public DbSet<Manual> Manuals => Set<Manual>();

    /// <summary>
    /// BorrowPeople table
    /// </summary>
    public DbSet<BorrowPerson> BorrowPeople => Set<BorrowPerson>();

    /// <summary>
    /// Sets up the database.
    /// Adds ConcurrencyToken to Borrow, BorrowableEntity and Reservation.
    /// ConcurrentToken is updated on every update of the entity, so concurrent updates will fail.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Borrow>().UseXminAsConcurrencyToken();
        modelBuilder.Entity<BorrowableEntity>().UseXminAsConcurrencyToken();
        modelBuilder.Entity<Reservation>().UseXminAsConcurrencyToken();
    }
}