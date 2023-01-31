namespace PrackyASusarny.Data.Constants;

public interface IReservationConstant
{
    public Duration MaxReservationDur { get; }
    public Duration MinReservationDur { get; }
    public Duration MinDurBeforeReservation { get; }
    public Duration MinReservationCancelDur { get; }
    public Duration ReservationPostponeDur { get; }

    public Duration SuggestReservationDurForBorrow { get; }
    public int MaxReservationsAtTime { get; }
}

public class ReservationConstant : IReservationConstant
{
    public Duration MaxReservationDur => Duration.FromHours(24 * 4);
    public Duration MinReservationDur => Duration.FromMinutes(1);
    public Duration MinDurBeforeReservation => Duration.FromMinutes(0);
    public Duration MinReservationCancelDur => Duration.FromHours(0);
    public Duration ReservationPostponeDur => Duration.FromMinutes(2);
    public Duration SuggestReservationDurForBorrow => Duration.FromMinutes(1);
    public int MaxReservationsAtTime => 10000;
}