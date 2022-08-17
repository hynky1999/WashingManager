namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IBorrowPersonService
{
    public Task<int> GetIdByNameAndSurnameAsync(string name, string surname);
}