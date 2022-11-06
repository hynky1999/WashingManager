using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IBorrowableEntityService
{
    public Task ChangeStatus(BorrowableEntity be, Status status);
}