using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Shared.Features.WMManagment;

namespace PrackyASusarny.Data;

public class BorrowService : ModelService<BorrowService>
{
    private BorrowPersonService _borrowPersonService; 
    
    public BorrowService(IDbContextFactory<ApplicationDbContext> dbFactory, BorrowPersonService borrowPersonService) : base(dbFactory)
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

        if (borrow.BorrowPersonID == 0)
        {
            var id = await _borrowPersonService.GetIDByNameAndSurname(borrow.BorrowPerson.Name, borrow.BorrowPerson.Surname);
            borrow.BorrowPersonID = id;
        }
        // Add Same check for WashingMachine but should't be needed
        
        // Register to context
        dbContext.ChangeTracker.TrackGraph(borrow, CreateBorrowTraversal );
        // Since we use concurency token this will fail if status was modified.
        await dbContext.SaveChangesAsync();
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