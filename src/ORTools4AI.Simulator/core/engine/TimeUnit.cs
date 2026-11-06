// Modified for ORTools4AI.Simulator.

using System.Diagnostics;


namespace ORTools4AI.Simulator.core.engine
{
    public sealed class TimeUnit:IComparable<TimeUnit>
    {
        private string Value;

        private const string _DAYS = "DAYS";

        private const string _HOURS = "HOURS";

        private const string _MICROSECONDS = "MICROSECONDS";

        private const string _MILLISECONDS = "MILLISECONDS";

        private const string _MINUTES = "MINUTES";

        private const string _NANOSECONDS = "NANOSECONDS";

        private const string _SECONDS = "SECONDS";

        private const string _TICKS = "TICKS";

        private static readonly long C0 = 1L;


        private static readonly Dictionary<string, int> timeUnitOrder =
            new Dictionary<string, int>()
            {
                {_TICKS,1 },
                {_NANOSECONDS,2},
                {_MICROSECONDS,3},
                {_MILLISECONDS,4},
                {_SECONDS,5},
                {_MINUTES,6},
                {_HOURS,7 },
                {_DAYS,8}
            };
           

        private static readonly long NanosecondsPerTick = C0 * 100;

        private static readonly long NanosecondsPerMicrosecond = NanosecondsPerTick * 10;

        private static readonly long NanosecondsPerMillisecond = NanosecondsPerMicrosecond * 1000;

        private static readonly long NanosecondsPerSecond = NanosecondsPerMillisecond * 1000;

        private static readonly long NanosecondsPerMinute = NanosecondsPerSecond * 60;

        private static readonly long NanosecondsPerHour = NanosecondsPerMinute * 60;

        private static readonly long NanosecondsPerDay = NanosecondsPerHour * 24;

        private Func<long, long> _ToNanos;

        private Func<long, long> _ToTicks;

        private Func<long, long> _ToMicros;

        private Func<long, long> _ToMillis;

        private Func<long, long> _ToSeconds;

        private Func<long, long> _ToMinutes;

        private Func<long, long> _ToHours;

        private Func<long, long> _ToDays;

        private Func<long, long, int> _ExcessNanos;

        public static readonly TimeUnit NANOSECONDS = new TimeUnit("NANOSECONDS", (long duration) => duration, 
            (long duration) => duration / NanosecondsPerTick, 
            (long duration) => duration / NanosecondsPerMicrosecond, 
            (long duration) => duration / NanosecondsPerMillisecond, 
            (long duration) => duration / NanosecondsPerSecond, 
            (long duration) => duration / NanosecondsPerMinute, 
            (long duration) => duration / NanosecondsPerHour, 
            (long duration) => duration / NanosecondsPerDay,
            (long x, long y) => (int)(x - y * NanosecondsPerMillisecond));

        public static readonly TimeUnit TICKS = new TimeUnit("TICKS", 
            (long duration) => scale(duration, NanosecondsPerTick, long.MaxValue / NanosecondsPerTick), 
            (long duration) => duration, 
            (long duration) => duration / (NanosecondsPerMicrosecond / NanosecondsPerTick), 
            (long duration) => duration / (NanosecondsPerMillisecond / NanosecondsPerTick), 
            (long duration) => duration / (NanosecondsPerSecond / NanosecondsPerTick), 
            (long duration) => duration / (NanosecondsPerMinute / NanosecondsPerTick), 
            (long duration) => duration / (NanosecondsPerHour / NanosecondsPerTick), 
            (long duration) => duration / (NanosecondsPerDay / NanosecondsPerTick), 
            (long x, long y) => (int)(x * NanosecondsPerTick - y * NanosecondsPerMillisecond));

        public static readonly TimeUnit MICROSECONDS = new TimeUnit("MICROSECONDS", 
            (long duration) => scale(duration, NanosecondsPerMicrosecond, long.MaxValue / NanosecondsPerMicrosecond), 
            (long duration) => scale(duration, NanosecondsPerMicrosecond / NanosecondsPerTick, long.MaxValue / (NanosecondsPerMicrosecond / NanosecondsPerTick)), 
            (long duration) => duration, (long duration) => duration / (NanosecondsPerMillisecond / NanosecondsPerMicrosecond), 
            (long duration) => duration / (NanosecondsPerSecond / NanosecondsPerMicrosecond), 
            (long duration) => duration / (NanosecondsPerMinute / NanosecondsPerMicrosecond), 
            (long duration) => duration / (NanosecondsPerHour / NanosecondsPerMicrosecond), 
            (long duration) => duration / (NanosecondsPerDay / NanosecondsPerMicrosecond), 
            (long x, long y) => 0);

        public static readonly TimeUnit MILLISECONDS = new TimeUnit("MILLISECONDS", 
            (long duration) => scale(duration, NanosecondsPerMillisecond, long.MaxValue / NanosecondsPerMillisecond), 
            (long duration) => scale(duration, NanosecondsPerMillisecond / NanosecondsPerTick, long.MaxValue / (NanosecondsPerMillisecond / NanosecondsPerTick)), 
            (long duration) => scale(duration, NanosecondsPerMillisecond / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerMillisecond / NanosecondsPerMicrosecond)), 
            (long duration) => duration, (long duration) => duration / (NanosecondsPerSecond / NanosecondsPerMillisecond), 
            (long duration) => duration / (NanosecondsPerMinute / NanosecondsPerMillisecond), 
            (long duration) => duration / (NanosecondsPerHour / NanosecondsPerMillisecond), 
            (long duration) => duration / (NanosecondsPerDay / NanosecondsPerMillisecond), 
            (long x, long y) => 0);

        public static readonly TimeUnit SECONDS = new TimeUnit("SECONDS", 
            (long duration) => scale(duration, NanosecondsPerSecond, long.MaxValue / NanosecondsPerSecond), 
            (long duration) => scale(duration, NanosecondsPerSecond / NanosecondsPerTick, long.MaxValue / (NanosecondsPerSecond / NanosecondsPerTick)), 
            (long duration) => scale(duration, NanosecondsPerSecond / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerSecond / NanosecondsPerMicrosecond)), 
            (long duration) => scale(duration, NanosecondsPerSecond / NanosecondsPerMillisecond, long.MaxValue / (NanosecondsPerSecond / NanosecondsPerMillisecond)), 
            (long duration) => duration, 
            (long duration) =>  duration / (NanosecondsPerMinute / NanosecondsPerSecond), 
            (long duration) => duration / (NanosecondsPerHour / NanosecondsPerSecond), 
            (long duration) => duration / (NanosecondsPerDay / NanosecondsPerSecond), 
            (long x, long y) => 0);

        public static readonly TimeUnit MINUTES = new TimeUnit("MINUTES", 
            (long duration) => scale(duration, NanosecondsPerMinute, long.MaxValue / NanosecondsPerMinute), 
            (long duration) => scale(duration, NanosecondsPerMinute / NanosecondsPerTick, long.MaxValue / (NanosecondsPerMinute / NanosecondsPerTick)), 
            (long duration) => scale(duration, NanosecondsPerMinute / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerMinute / NanosecondsPerMicrosecond)), 
            (long duration) => scale(duration, NanosecondsPerMinute / NanosecondsPerMillisecond, long.MaxValue / (NanosecondsPerMinute / NanosecondsPerMillisecond)), 
            (long duration) => scale(duration, NanosecondsPerMinute / NanosecondsPerSecond, long.MaxValue / (NanosecondsPerMinute / NanosecondsPerSecond)), 
            (long duration) => duration, 
            (long duration) => duration / (NanosecondsPerHour / NanosecondsPerMinute), 
            (long duration) => duration / (NanosecondsPerDay / NanosecondsPerMinute), 
            (long x, long y) => 0);

        public static readonly TimeUnit HOURS = new TimeUnit("HOURS", 
            (long duration) => scale(duration, NanosecondsPerHour, long.MaxValue / NanosecondsPerHour), 
            (long duration) => scale(duration, NanosecondsPerHour / NanosecondsPerTick, long.MaxValue / (NanosecondsPerHour / NanosecondsPerTick)), 
            (long duration) => scale(duration, NanosecondsPerHour / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerHour / NanosecondsPerMicrosecond)), 
            (long duration) => scale(duration, NanosecondsPerHour / NanosecondsPerMillisecond, long.MaxValue / (NanosecondsPerHour / NanosecondsPerMillisecond)), 
            (long duration) => scale(duration, NanosecondsPerHour / NanosecondsPerSecond, long.MaxValue / (NanosecondsPerHour / NanosecondsPerSecond)), 
            (long duration) => scale(duration, NanosecondsPerHour / NanosecondsPerMinute, long.MaxValue / (NanosecondsPerHour / NanosecondsPerMinute)), 
            (long duration) => duration, 
            (long duration) => duration / (NanosecondsPerDay / NanosecondsPerHour), 
            (long x, long y) => 0);

        public static readonly TimeUnit DAYS = new TimeUnit("DAYS", 
            (long duration) => scale(duration, NanosecondsPerDay, long.MaxValue / NanosecondsPerDay), 
            (long duration) => scale(duration, NanosecondsPerDay / NanosecondsPerTick, long.MaxValue / (NanosecondsPerDay / NanosecondsPerTick)), 
            (long duration) => scale(duration, NanosecondsPerDay / NanosecondsPerMicrosecond, long.MaxValue / (NanosecondsPerDay / NanosecondsPerMicrosecond)), 
            (long duration) => scale(duration, NanosecondsPerDay / NanosecondsPerMillisecond, long.MaxValue / (NanosecondsPerDay / NanosecondsPerMillisecond)), 
            (long duration) => scale(duration, NanosecondsPerDay / NanosecondsPerSecond, long.MaxValue / (NanosecondsPerDay / NanosecondsPerSecond)), 
            (long duration) => scale(duration, NanosecondsPerDay / NanosecondsPerMinute, long.MaxValue / (NanosecondsPerDay / NanosecondsPerMinute)), 
            (long duration) => scale(duration, NanosecondsPerDay / NanosecondsPerHour, long.MaxValue / (NanosecondsPerDay / NanosecondsPerHour)), 
            (long duration) => duration, 
            (long x, long y) => 0);

        private TimeUnit(string value, Func<long, long> toNanos, Func<long, long> toTicks, 
            Func<long, long> toMicros, Func<long, long> toMillis, Func<long, long> toSeconds, 
            Func<long, long> toMinutes, Func<long, long> toHours, 
            Func<long, long> toDays, Func<long, long, int> excessNanos)
        {
            Value = value;
            _ToNanos = toNanos;
            _ToTicks = toTicks;
            _ToMicros = toMicros;
            _ToMillis = toMillis;
            _ToSeconds = toSeconds;
            _ToMinutes = toMinutes;
            _ToHours = toHours;
            _ToDays = toDays;
            _ExcessNanos = excessNanos;
        }

        public void Sleep(long timeout)
        {
            if (timeout > 0)
            {
                Thread.Sleep((int)ToMilliseconds(timeout));
            }
        }

        public void TimedJoin(Thread thread, long timeout)
        {
            if (timeout > 0)
            {
                long num = ToMilliseconds(timeout);
                thread.Join((int)num);
            }
        }

        public void TimedWait(object obj, long timeout)
        {
            if (timeout > 0)
            {
                long num = ToMilliseconds(timeout);
                Monitor.Wait(obj, (int)num);
            }
        }

        public long ToNanoseconds(long duration)
        {
            return _ToNanos(duration);
        }

        public long ToTicks(long duration)
        {
            return _ToTicks(duration);
        }

        public long ToMicroseconds(long duration)
        {
            return _ToMicros(duration);
        }

        public long ToMilliseconds(long duration)
        {
            return _ToMillis(duration);
        }

        public long ToSeconds(long duration)
        {
            return _ToSeconds(duration);
        }

        public long ToMinutes(long duration)
        {
            return _ToMinutes(duration);
        }

        public long ToHours(long duration)
        {
            return _ToHours(duration);
        }

        public long ToDays(long duration)
        {
            return _ToDays(duration);
        }

        public long Convert(long sourceDuration, TimeUnit sourceUnit)
        {
            return Value switch
            {
                "DAYS" => sourceUnit._ToDays(sourceDuration),
                "HOURS" => sourceUnit.ToHours(sourceDuration),
                "MINUTES" => sourceUnit.ToMinutes(sourceDuration),
                "SECONDS" => sourceUnit.ToSeconds(sourceDuration),
                "MILLISECONDS" => sourceUnit.ToMilliseconds(sourceDuration),
                "MICROSECONDS" => sourceUnit.ToMicroseconds(sourceDuration),
                "TICKS" => sourceUnit.ToTicks(sourceDuration),
                "NANOSECONDS" => sourceUnit.ToNanoseconds(sourceDuration),
                _ => throw new ArgumentException("The time unit " + Value + " is unrecognized."),
            };
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || (object)GetType() != obj.GetType())
            {
                return false;
            }

            TimeUnit timeUnit = (TimeUnit)obj;
            return Value == timeUnit.Value;
        }

        public override string ToString()
        {
            return Value;
        }

        //public override int GetHashCode()
        //{
        //    throw new NotImplementedException();
        //    //return Hashing.Hash(Value);
        //}

        public static bool operator ==(TimeUnit left, TimeUnit right)
        {
            if ((object)left == right)
            {
                return true;
            }

            if ((object)right == null || (object)left == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(TimeUnit left, TimeUnit right)
        {
            return !(left == right);
        }

        private int ExcessNanos(long duration, long milliseconds)
        {
            return _ExcessNanos(duration, milliseconds);
        }

        private static long scale(long x, long y, long over)
        {
            if (x > over)
            {
                return long.MaxValue;
            }

            if (x < -over)
            {
                return long.MinValue;
            }

            return x * y;
        }

        public int CompareTo(TimeUnit? other)
        {
            Debug.Assert(other is not null);
            return timeUnitOrder[this.Value].CompareTo(timeUnitOrder[other.Value]);
        }
    }
}
