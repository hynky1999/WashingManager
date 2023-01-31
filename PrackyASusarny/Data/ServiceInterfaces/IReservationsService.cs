using System.Security.Claims;
using AntDesign.TableModels;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IReservationsService
{
    Task<Reservation?> CreateReservationAsync(LocalDateTime start,
        LocalDateTime end,
        ClaimsPrincipal user, BorrowableEntity entity);

    Task<Borrow?> MakeBorrowFromReservationAsync(Reservation reservation);
    Task CancelReservationAsync(Reservation reservation);

    Task<Reservation[]> GetReservationsAsync(ClaimsPrincipal user,
        QueryModel<Reservation> queryModel);

    Task<int> GetReservationsCountAsync(ClaimsPrincipal user,
        QueryModel<Reservation> queryModel);

    Task<Reservation[]> GetReservationsAsync<T>(
        QueryModel<Reservation> queryModel) where T : BorrowableEntity;

    Task<int> GetReservationsCountAsync<T>(QueryModel<Reservation> queryModel)
        where T : BorrowableEntity;

    Task<Reservation[]> GetUpcomingReservationsByEntityAsync(
        BorrowableEntity entity);

    Task<(LocalDateTime start, LocalDateTime end, T be)[]>
        SuggestReservation<T>(Duration length, int limit = 3)
        where T : BorrowableEntity;
}