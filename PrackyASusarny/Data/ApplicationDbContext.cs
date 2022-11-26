using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrackyASusarny.Auth.Models;
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

public class ApplicationUserClaimsPrincipalFactory :
    UserClaimsPrincipalFactory<ApplicationUser>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        IOptions<IdentityOptions> optionsAccessor) : base(userManager,
        optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity>
        GenerateClaimsAsync(ApplicationUser user)
    {
        ClaimsIdentity claims = await
            base.GenerateClaimsAsync(user);


        claims.AddClaim(new Claim(Claims.UserID, user.Id.ToString()));
        return claims;
    }
}