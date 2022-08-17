using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Errors.Folder;
using DbUpdateException = Microsoft.EntityFrameworkCore.DbUpdateException;

namespace PrackyASusarny.Data.EFCoreServices;

public class BorrowService : CrudService<Borrow>, IBorrowService
{
    private IBorrowPersonService _borrowPersonService;

    public BorrowService(IDbContextFactory<ApplicationDbContext> dbFactory, IBorrowPersonService borrowPersonService,
        ILogger<BorrowService> logger) :
        base(dbFactory, logger, context => context.Borrows, (borrow) => borrow.BorrowID)
    {
        _borrowPersonService = borrowPersonService;
    }

    public Task<Price> GetPriceAsync(Borrow borrow)
    {
        throw new NotImplementedException();
    }

    public Task<Borrow?> GetBorrowByWmAsync(WashingMachine wm)
    {
        throw new NotImplementedException();
    }

    public Task EndBorrowAsync(Borrow borrow)
    {
        throw new NotImplementedException();
    }

    public async Task AddBorrow(Borrow borrow)
    {
        using var dbContext = await DbFactory.CreateDbContextAsync();
        if (borrow.WashingMachine.Status != Status.Free)
        {
            throw new ArgumentException("Invalid Value");
        }

        borrow.WashingMachine.Status = Status.Taken;

        if (borrow.BorrowPerson.BorrowPersonID == 0)
        {
            var id = await _borrowPersonService.GetIDByNameAndSurname(borrow.BorrowPerson.Name,
                borrow.BorrowPerson.Surname);
            borrow.BorrowPerson.BorrowPersonID = id;
        }
        // Add Same check for WashingMachine but should't be needed

        // Register to context
        dbContext.ChangeTracker.TrackGraph(borrow, CreateBorrowTraversal);
        // Since we use concurency token this will fail if status was modified.
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new DbConcurrencyException(e.Message);
        }
        catch (DbUpdateException e)
        {
            throw new Errors.Folder.DbUpdateException(e.Message);
        }
    }

    public async Task<Price> GetPrice(Borrow borrow)
    {
        // This could be based on locale with facotry at price
        var time = borrow.startDate - DateTime.UtcNow;
        var pricePerTime = (time.Minutes / 30) * Rates.WMpricePerHalfHour;
        return new Price
        {
            Amount = pricePerTime,
            Currency = "Kč"
        };
    }

    public async Task<Borrow?> GetBorrowByWm(WashingMachine wm)
    {
        if (wm == null)
        {
            throw new ArgumentNullException("Washing Machine missing");
        }

        using var dbContext = await DbFactory.CreateDbContextAsync();
        var query = dbContext.Borrows.Where(b => b.WashingMachine.WashingMachineId == wm.WashingMachineId)
            .Include(b => b.BorrowPerson);
        var borrow = await query.FirstOrDefaultAsync();
        if (borrow is not null)
        {
            borrow.WashingMachine = wm;
        }

        return borrow;
    }

    public async Task EndBorrow(Borrow borrow)
    {
        using var dbContext = await DbFactory.CreateDbContextAsync();
        dbContext.Borrows.Attach(borrow);
        borrow.endDate = DateTime.UtcNow;
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new DbConcurrencyException(e.Message);
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    private void CreateBorrowTraversal(EntityEntryGraphNode node)
    {
        node.Entry.State = EntityState.Unchanged;
        if (node.Entry.Entity is Borrow)
        {
            node.Entry.State = EntityState.Added;
        }

        if (node.Entry.Entity is BorrowPerson {BorrowPersonID: 0})
        {
            node.Entry.State = EntityState.Added;
        }

        if (node.Entry.Entity is WashingMachine wm)
        {
            node.Entry.Property(nameof(WashingMachine.Status)).IsModified = true;
        }
    }
}