using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IBorrowService
{
    Task<Price> GetPriceAsync(Borrow borrow);
    Task EndBorrowAsync(Borrow borrowC);
    Task AddBorrowAsync(Borrow borrowC);
}