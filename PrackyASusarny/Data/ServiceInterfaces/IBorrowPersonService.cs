using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IBorrowPersonService : ICrudService<BorrowPerson>
{
    public Task<int> GetIDByNameAndSurname(string name, string surname);
}