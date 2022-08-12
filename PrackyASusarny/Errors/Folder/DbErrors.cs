namespace PrackyASusarny.Errors.Folder;

public class DbException : Exception
{
    public DbException(string message) : base(message)
    {
    }
}

public class DbInsertException : DbException
{
    public DbInsertException(string message) : base(message)
    {
    }
}

public class DbConcurrencyException : DbException
{
    public DbConcurrencyException(string message) : base(message)
    {
    }
}

public class DbUpdateException : DbException
{
    public DbUpdateException(string message) : base(message)
    {
    }
}