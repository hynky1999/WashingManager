namespace App.Data.Constants;

/// <summary>
/// Interface for Reservation Constants
/// Good for testing without changing constants of main app
/// </summary>
public interface IReservationConstant
{
    /// <summary>
    /// Maximum duration of reservation
    /// </summary>
    public Duration MaxReservationDur { get; }

    /// <summary>
    /// Minimum duration of reservation
    /// </summary>
    public Duration MinReservationDur { get; }

    /// <summary>
    /// Minimum duration from now to reservation start
    /// </summary>
    public Duration MinDurBeforeReservation { get; }

    /// <summary>
    /// Minimum duration from now to reservation start when user can cancel reservation
    /// </summary>
    public Duration MinReservationCancelDur { get; }

    /// <summary>
    /// Duration of postpone when user fails to return the reservation
    /// </summary>
    public Duration ReservationPostponeDur { get; }

    /// <summary>
    /// Amount of time to give to user to choose a suggestion
    /// If set to 0 user would not have enough time to choose a suggestion
    /// and the <see cref="MinDurBeforeReservation"/> would not be respected
    /// </summary>
    public Duration SuggestReservationDurForBorrow { get; }

    /// <summary>
    /// Maximum amount of reservations a user can have at the same time
    /// </summary>
    public int MaxReservationsAtTime { get; }
}

/// <summary>
/// Implementation of <see cref="IReservationConstant"/>
/// </summary>
public class ReservationConstant : IReservationConstant
{
    /// <summary>
    /// See <see cref="IReservationConstant.MaxReservationDur"/>
    /// </summary>
    public Duration MaxReservationDur => Duration.FromHours(24 * 4);

    /// <summary>
    /// See <see cref="IReservationConstant.MinReservationDur"/>
    /// </summary>
    public Duration MinReservationDur => Duration.FromMinutes(1);

    /// <summary>
    /// See <see cref="IReservationConstant.MinDurBeforeReservation"/>
    /// </summary>
    public Duration MinDurBeforeReservation => Duration.FromMinutes(0);

    /// <summary>
    /// See <see cref="IReservationConstant.MinReservationCancelDur"/>
    /// </summary>
    public Duration MinReservationCancelDur => Duration.FromHours(0);

    /// <summary>
    /// See <see cref="IReservationConstant.ReservationPostponeDur"/>
    /// </summary>
    public Duration ReservationPostponeDur => Duration.FromMinutes(2);

    /// <summary>
    /// See <see cref="IReservationConstant.SuggestReservationDurForBorrow"/>
    /// </summary>
    public Duration SuggestReservationDurForBorrow => Duration.FromMinutes(1);

    /// <summary>
    /// See <see cref="IReservationConstant.MaxReservationsAtTime"/>
    /// </summary>
    public int MaxReservationsAtTime => 4;
}