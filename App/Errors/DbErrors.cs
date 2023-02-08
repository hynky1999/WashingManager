namespace App.Errors;

/// <summary>
/// Class which represents a DB or resource error
/// We don't want to use EF exceptions because we could change the DB provider in future
/// This allows use to keep same error handling in the application
/// </summary>
public class DbException : Exception
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public DbException(string message) : base(message)
    {
    }
}

/// <summary>
/// Insert Error
/// </summary>
public class DbInsertException : DbException
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public DbInsertException(string message) : base(message)
    {
    }
}

/// <summary>
/// Concurrency Error
/// </summary>
public class DbConcurrencyException : DbException
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public DbConcurrencyException(string message) : base(message)
    {
    }
}

/// <summary>
/// Update Error
/// </summary>
public class DbUpdateException : DbException
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public DbUpdateException(string message) : base(message)
    {
    }
}