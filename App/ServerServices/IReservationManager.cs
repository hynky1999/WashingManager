using App.Data.Models;

namespace App.ServerServices;

/// <summary>
/// Interface for reservation manager that manages reservation over borrow and no borrow
/// </summary>
public interface IReservationManager
{
    /// <summary>
    /// Gets number of queues. Each queue represents one borrowable entity.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Creates a new queue for given borrowable entity.
    /// </summary>
    /// <param name="id">id of entity</param>
    /// <returns>Reference to created queue</returns>
    Task<ITimedQueue<Reservation>> Create(int id);

    /// <summary>
    /// Removes queue for given borrowable entity.
    /// </summary>
    /// <param name="id">id of entity</param>
    void Remove(int id);

    /// <summary>
    /// Triggers OnChange event for given queue.
    /// </summary>
    /// <param name="id">id of entity</param>
    /// <returns></returns>
    Task OnChange(int id);
}