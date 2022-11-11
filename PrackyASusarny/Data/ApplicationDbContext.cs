using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, string>
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

    public virtual DbSet<Borrow> Borrows => Set<Borrow>();
    public DbSet<Manual> Manuals => Set<Manual>();
    public DbSet<BorrowPerson> BorrowPeople => Set<BorrowPerson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Borrow>().UseXminAsConcurrencyToken();
        modelBuilder.Entity<BorrowableEntity>().UseXminAsConcurrencyToken();
    }
}