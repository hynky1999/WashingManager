using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.ServiceInterfaces;

public interface IReservationsService
{
    void CreateReservation(LocalDateTime start, LocalDateTime end, User user, IBorrowableEntityService entity);
    Borrow? MakeBorrowFromReservation(Reservation reservation);
    void CancelReservation(Reservation reservation);
    void DeleteUnTakenReservation(Reservation reservation);
    Reservation[] GetAllReservations(User user);
    Reservation[] GetUpcomingReservations();
}