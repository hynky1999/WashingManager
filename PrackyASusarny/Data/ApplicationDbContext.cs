using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Auth.Utils;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data;

public class
    ApplicationDbContext : IdentityDbContext<ApplicationUser, Role, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext()
    {
    }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<WashingMachine> WashingMachines => Set<WashingMachine>();
    public DbSet<DryingRoom> DryingRooms => Set<DryingRoom>();

    public DbSet<BorrowableEntityUsage<WashingMachine>> WashingMachineUsage =>
        Set<BorrowableEntityUsage<WashingMachine>>();

    public DbSet<BorrowableEntityUsage<DryingRoom>> DryingRoomUsage =>
        Set<BorrowableEntityUsage<DryingRoom>>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public virtual DbSet<Borrow> Borrows => Set<Borrow>();
    public DbSet<Manual> Manuals => Set<Manual>();
    public DbSet<BorrowPerson> BorrowPeople => Set<BorrowPerson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Borrow>().UseXminAsConcurrencyToken();
        modelBuilder.Entity<BorrowableEntity>().UseXminAsConcurrencyToken();
        modelBuilder.Entity<Reservation>().UseXminAsConcurrencyToken();
    }
}