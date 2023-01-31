using AntDesign.TableModels;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IBorrowService
{
    Task<Money> GetPriceAsync(Borrow borrow);
    Task EndBorrowAsync(Borrow borrowC);
    Task<Borrow?> AddBorrowAsync(Borrow borrowC);

    Task<Borrow[]> GetBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity;

    Task<int> CountBorrowsByBEAsync<T>(QueryModel<Borrow> qM)
        where T : BorrowableEntity;

    Task<Borrow?> GetBorrowAsync(Reservation res);
}