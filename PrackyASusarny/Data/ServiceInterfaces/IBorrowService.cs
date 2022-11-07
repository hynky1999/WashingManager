using AntDesign.TableModels;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IBorrowService
{
    Task<Price> GetPriceAsync(Borrow borrow);
    Task EndBorrowAsync(Borrow borrowC);
    Task AddBorrowAsync(Borrow borrowC);

    Task<Borrow[]> GetBorrowsByBEAsync<T>(QueryModel<Borrow> qM) where T : BorrowableEntity;
    Task<int> CountBorrowsByBEAsync<T>(QueryModel<Borrow> qM) where T : BorrowableEntity;
}