using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IBorrowService : ICrudService<Borrow>
{
    Task<Price> GetPriceAsync(Borrow borrow);
    Task<Borrow?> GetBorrowByWmAsync(WashingMachine wm);
    Task EndBorrowAsync(Borrow borrow);
}