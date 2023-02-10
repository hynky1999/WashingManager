using App.Data.Constants;
using NodaTime;

namespace EFCoreTests;

public class Utils
{
    public class CustomizableClock : IClock
    {
        private Instant _time = Instant.FromUtc(2021, 1, 1, 0, 0);

        public Instant GetCurrentInstant()
        {
            return _time;
        }

        public void SetTime(Instant time)
        {
            _time = time;
        }
    }

    public class Clock14082022 : IClock
    {
        public Instant GetCurrentInstant()
        {
            return new LocalDateTime(2022, 8, 14, 2, 0, 0).InZoneLeniently(
                DateTimeZoneProviders.Tzdb["Europe/Prague"]).ToInstant();
        }
    }
    public class TestReservationConstant : IReservationConstant
    {
        public Duration MaxReservationDur => Duration.FromHours(10);
        public Duration MinReservationDur => Duration.FromMinutes(0);
        public Duration MinDurBeforeReservation => Duration.FromMinutes(0);
        public Duration MinReservationCancelDur => Duration.FromMinutes(0);
        public Duration ReservationPostponeDur => Duration.FromMinutes(30);
        public Duration SuggestReservationDurForBorrow { get; } = Duration.FromMinutes(1);
        public int MaxReservationsAtTime => 5;
    }

    public class TestRates : IRates
    {
        public int PricePerHalfHour => 10;
        public int FlatBorrowPrice => 100;
        public int PricePerOverRes => 20;
        public int NoBorrowPenalty => 30;
        public Currency DBCurrency => Currency.CZK;
    }
    
    public class TestUsageConstants : IUsageConstants
    {
        public ZonedDateTime CalculatedSince =>
            new(new LocalDateTime(2022, 8, 8, 0, 0), DateTimeZone.Utc,
                Offset.Zero);

        public DateTimeZone UsageTimeZone =>
            DateTimeZoneProviders.Tzdb["Europe/Prague"];
    }
}