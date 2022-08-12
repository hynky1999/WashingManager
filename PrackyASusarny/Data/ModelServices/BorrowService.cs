using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Errors.Folder;
using DbUpdateException = Microsoft.EntityFrameworkCore.DbUpdateException;

namespace PrackyASusarny.Data;

public class BorrowService : ModelService<BorrowService>
{
    private BorrowPersonService _borrowPersonService;

    public BorrowService(IDbContextFactory<ApplicationDbContext> dbFactory, BorrowPersonService borrowPersonService) :
        base(dbFactory)
    {
        _borrowPersonService = borrowPersonService;
    }

    public async Task AddBorrow(Borrow borrow)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
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

    public async Task<Borrow?> GetBorrowByWm(WashingMachine? wm)
    {
        if (wm == null)
        {
            throw new ArgumentNullException("Washing Machine missing");
        }

        using var dbContext = await _dbFactory.CreateDbContextAsync();
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
        using var dbContext = await _dbFactory.CreateDbContextAsync();
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