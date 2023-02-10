using System.Security.Claims;
using AntDesign.TableModels;
using App.Data.Models;

namespace App.Data.ServiceInterfaces;

/// <summary>
/// Service for managing Reservations entities
/// </summary>
public interface IReservationsService
{
    /// <summary>
    /// Create a new reservation for the given user based on Instant
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="user"></param>
    /// <param name="entity"></param>
    /// <returns>Reservation if created successfully</returns>
    Task<Reservation?> CreateReservationAsync(Instant start,
        Instant end,
        ClaimsPrincipal user, BorrowableEntity entity);

    /// <summary>
    /// Takes a reservation and creates a borrow for that reservation if possible
    /// </summary>
    /// <param name="reservation"></param>
    /// <returns>Borrow if created</returns>
    Task<Borrow?> MakeBorrowFromReservationAsync(Reservation reservation);

    /// <summary>
    /// Cancels reservation
    /// </summary>
    /// <param name="reservation"></param>
    /// <returns></returns>
    Task CancelReservationAsync(Reservation reservation);

    /// <summary>
    /// Gets all reservations for the given user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="queryModel">constraints</param>
    /// <returns>Reservations of user</returns>
    Task<Reservation[]> GetReservationsAsync(ClaimsPrincipal user,
        QueryModel<Reservation> queryModel);

    /// <summary>
    /// <see cref="GetReservationsAsync"/>
    /// </summary>
    /// <param name="user"></param>
    /// <param name="queryModel"></param>
    /// <returns></returns>
    Task<int> GetReservationsCountAsync(ClaimsPrincipal user,
        QueryModel<Reservation> queryModel);

    /// <summary>
    /// Gets all reservations for the given entity
    /// </summary>
    /// <param name="queryModel">constrains</param>
    /// <typeparam name="T">entity type to get reservations for</typeparam>
    /// <returns></returns>
    Task<Reservation[]> GetReservationsAsync<T>(
        QueryModel<Reservation> queryModel) where T : BorrowableEntity;

    /// <summary>
    /// <see cref="GetReservationsAsync{T}"/> 
    /// </summary>
    /// <param name="queryModel"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<int> GetReservationsCountAsync<T>(QueryModel<Reservation> queryModel)
        where T : BorrowableEntity;

    /// <summary>
    ///  Gets all reservations for the given entity that start after current time
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>Reservations starting after current time</returns>
    Task<Reservation[]> GetUpcomingReservationsByEntityAsync(
        BorrowableEntity entity);

    /// <summary>
    /// Suggest a reservation times based on Duration length
    /// </summary>
    /// <param name="length"></param>
    /// <param name="limit">how much suggestions to show</param>
    /// <typeparam name="T">Type of BE to get suggestions for</typeparam>
    /// <returns>Suggested reservations</returns>
    Task<(Instant start, Instant end, T be)[]>
        SuggestReservation<T>(Duration length, int limit = 3)
        where T : BorrowableEntity;
}