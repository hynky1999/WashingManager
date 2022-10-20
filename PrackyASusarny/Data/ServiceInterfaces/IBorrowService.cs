using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IBorrowService
{
    Task<Price> GetPriceAsync(Borrow borrow);
    Task<Borrow?> GetBorrowByWmAsync(WashingMachine wm);
    Task EndBorrowAsync(Borrow borrow);
}

public enum StatRange
{
    ByHour,
    ByDay
}