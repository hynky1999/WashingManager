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
            return new ZonedDateTime(new LocalDateTime(2022, 8, 14, 0, 0),
                DateTimeZone.Utc, Offset.Zero).ToInstant();
        }
    }
}