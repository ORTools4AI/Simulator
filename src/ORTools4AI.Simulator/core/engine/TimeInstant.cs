// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Felix Klueckmann
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;
using System.Globalization;

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * Represents points in simulation time. Is used to indicate points in
     * simulation time at which the state of the model changes. Each point in
     * simulation time is represented by an individual object of this class and
     * offers its own methods for arithmetic operations.
     */

	public class TimeInstant : IComparable<TimeInstant> {

		/**
		 * A short cut to the default time zone (thanks to Marcin Kawelski).  
		 */
		private readonly static TimeZoneInfo DEFAULT_PREFERRED_TIMEZONE = TimeZoneInfo.Utc;

		/**
		 * The number of epsilons since the start of the epoch (January 1, 1970, 00:00:00 GMT).  
		 */
		private readonly long _timeInEpsilon;

		/**
		 * The preferred time zone for printing this time instant.
		 */
		private TimeZoneInfo _preferredTimeZone;

		/// <summary>
		/// Constructs a TimeInstant object with the given time value in the time unit of the given parameter. It
		/// represents a time instant in simulation time. Note that trying to create a TimeInstant object The
		/// simulation will also stop immediately if the TimeSpan is larger than Long.MAX_VALUE-1 (in the unit of
		/// epsilon).
		/// </summary>
		/// <param name="time">The time value of this TimeInstant in the time unit of unit.</param>
		/// <param name="unit">the TimeUnit</param>
		public TimeInstant(long time, TimeUnit unit) {
			if (unit == null) { // no time unit given
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't create TimeInstant object! Simulation aborted.",
								"Class : TimeInstant  Constructor : TimeInstant(long, TimeUnit)",
								"Time unit passed is null",
								"Make sure to pass a non-null time unit. \nNote that before " +
								  "connecting model and experiment, TimeInstants must explicitly\n" +
								  "refer to a time unit as the reference unit is not yet defined," +
								  "e.g. use \nTimeInstant(long time, TimeUnit unit) instead of" +
								  "TimeInstant(long time).",
								null)));
			}
			// System.out.println(time + " " + unit);
			// System.out.println("Format: "
			// + new SimpleDateFormat().format(new Date(time)));
			if (unit.CompareTo(TimeOperations.getEpsilon()) < 0) {
				// unit is a finer granularity than epsilon
				if (TimeOperations.getStartTime() != null) {// Start time has been
															// set
					Console.WriteLine("Starttime: "
							+ TimeOperations.getStartTime());
					long timeSinceStart = time // time since start...?
							- unit.Convert(TimeOperations.getStartTime()// Achtung-negative
																		// werden
																		// positiv
									.getTimeInEpsilon(), TimeOperations
									.getEpsilon());
					Console.WriteLine("TimeSinceStart: " + timeSinceStart);
					if (timeSinceStart != 0) {
						time = time
								- (timeSinceStart % unit.Convert(1, TimeOperations
										.getEpsilon()));
					}
				}
			}
			this._timeInEpsilon = TimeOperations.getEpsilon().Convert(time, unit);
			if (_timeInEpsilon == long.MaxValue) {
				/*The timeInstant is too big. 
				(The method TimeUnit.convert(duration,unit)returns Long.MAX_VALUE if
				the result of the conversion is to big*/

				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't create TimeInstant object! Simulation aborted.",
								"Class : TimeInstant  Constructor : TimeInstant(long,TimeUnit)",
								"the TimeInstant is too big. ",
								"Can only create TimeInstant objects which are smaller than Long.MAX_VALUE (in the TimeUnit of epsilon).",
								null)));
			}

			this._preferredTimeZone = DEFAULT_PREFERRED_TIMEZONE;
			TimeOperations.timeObjectsCreated = true;
		}

		/// <summary>
		/// Constructs a TimeInstant object with the given time value in the time unit of the reference time. It
		/// represents a time Instant in simulation time.
		/// </summary>
		/// <param name="timeInReferenceUnit">
		/// The time value of this TimeInstant in the time unit of the reference time.
		/// </param>
		public TimeInstant(long timeInReferenceUnit): this(timeInReferenceUnit, TimeOperations.getReferenceUnit())
		{
		}

		/// <summary>
		/// Constructs a TimeInstant object with the given time value in the time unit given as second parameter.
		/// It represents a time Instant in simulation time.
		/// </summary>
		/// <param name="time">The time value of this TimeInstant in the time unit of the reference time.</param>
		/// <param name="unit">the time unit</param>
		public TimeInstant(double time, TimeUnit unit) {
			if (unit == null) { // no time unit given
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't create TimeInstant object! Simulation aborted.",
								"Class : TimeInstant  Constructor : TimeInstant(double, TimeUnit)",
								"Time unit passed is null",
								"Make sure to pass a non-null time unit. \nNote that before " +
								  "connecting model and experiment, TimeInstants must explicitly\n" +
								  "refer to a time unit as the reference unit is not yet defined," +
								  "e.g. use \nTimeInstant(double time, TimeUnit unit) instead of" +
								  "TimeInstant(double time).",
								null)));
			}
			// System.out.println(time + " " + unit);
			// System.out.println("Format: "
			// + new SimpleDateFormat().format(new Date(time)));
			if (unit.CompareTo(TimeOperations.getEpsilon()) < 0) {
				// unit is a finer granularity than epsilon
				if (TimeOperations.getStartTime() != null) {// Start time has been
															// set
					System.Console.WriteLine("Starttime: "
							+ TimeOperations.getStartTime());
					double timeSinceStart = time // time since start...?
							- unit.Convert(TimeOperations.getStartTime()// Achtung-negative
																		// werden
																		// positiv
									.getTimeInEpsilon(), TimeOperations
									.getEpsilon());
                    System.Console.WriteLine("TimeSinceStart: " + timeSinceStart);
					if (timeSinceStart != 0) {
						time = time
								- (timeSinceStart % unit.Convert(1, TimeOperations
										.getEpsilon()));
					}
				}
			}
			this._timeInEpsilon = (long)(TimeOperations.getEpsilon().Convert(1, unit) * time);
			if (_timeInEpsilon == long.MaxValue) {
				/*The timeInstant is too big. 
				(The method TimeUnit.convert(duration,unit)returns Long.MAX_VALUE if
				the result of the conversion is to big*/

				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't create TimeInstant object! Simulation aborted.",
								"Class : TimeInstant  Constructor : TimeInstant(long,TimeUnit)",
								"the TimeInstant is too big. ",
								"Can only create TimeInstant objects which are smaller than Long.MAX_VALUE (in the TimeUnit of epsilon).",
								null)));
			}

			this._preferredTimeZone = DEFAULT_PREFERRED_TIMEZONE;
			TimeOperations.timeObjectsCreated = true;
		}

		/// <summary>
		/// Constructs a TimeInstant object with the given time value in the time unit of the reference time. It
		/// represents a time Instant in simulation time.
		/// </summary>
		/// <param name="timeInReferenceUnit">
		/// The time value of this TimeInstant in the time unit of the reference time.
		/// </param>
		public TimeInstant(double timeInReferenceUnit): this(timeInReferenceUnit, TimeOperations.getReferenceUnit()) {
		}

		/// <summary>Creates a simulation time instant from a date and time with a UTC offset.</summary>
		/// <param name="calendar">The date and time to convert.</param>
		/// <param name="applyPreferredTimeZoneFromCalendar">Whether to use the supplied offset for output.</param>
		public TimeInstant(DateTimeOffset calendar, bool applyPreferredTimeZoneFromCalendar)
			: this(calendar.ToUnixTimeMilliseconds(), TimeUnit.MILLISECONDS)
		{
			if (applyPreferredTimeZoneFromCalendar)
				this._preferredTimeZone = TimeZoneInfo.CreateCustomTimeZone(
					"custom", calendar.Offset, "custom", "custom");
		}

		public TimeInstant(DateTimeOffset calendar) : this(calendar, false) { }

		public TimeInstant(DateTime date)
			: this(new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc)).ToUnixTimeMilliseconds(),
				   TimeUnit.MILLISECONDS)
		{ }

		/// <summary>
		/// Returns the value of the TimeInstant object as a long type in the time unit of epsilon
		/// </summary>
		/// <returns>the time value of the TimeInstant object as a long type in the time unit of epsilon</returns>
		public long getTimeInEpsilon() {
			return _timeInEpsilon;
		}

		/// <summary>
		/// Returns the value of this TimeInstant object as a long type in the time unit given as a parameter. If
		/// the parameter has a coarser granularity than epsilon the returned value will be truncated, so lose
		/// precision.
		/// </summary>
		/// <returns>
		/// the time value of the TimeInstant object as a long type in the time unit given as a parameter or
		/// Long.MIN_VALUE if conversion would negatively overflow, or Long.MAX_VALUE if it would positively
		/// overflow.
		/// </returns>
		public long getTimeTruncated(TimeUnit unit) {
			return unit.Convert(_timeInEpsilon, TimeOperations.getEpsilon());
		}

		/// <summary>
		/// Returns the value of this TimeInstant object as a long type in the time unit of the reference time. If
		/// the parameter has a coarser granularity than epsilon the returned value will be truncated, so lose
		/// precision.
		/// </summary>
		/// <returns>
		/// the time value of the TimeInstant object as a long type in the time unit given as a parameter or
		/// Long.MIN_VALUE if conversion would negatively overflow, or Long.MAX_VALUE if it would positively
		/// overflow.
		/// </returns>
		public long getTimeTruncated() {
			return getTimeTruncated(TimeOperations.getReferenceUnit());
		}

		/// <summary>
		/// Returns the value of this TimeInstant object as a long type in the time unit given as a parameter. If
		/// the parameter has a coarser granularity than epsilon the returned value will be rounded, so lose
		/// precision.
		/// </summary>
		/// <param name="unit">the TimeUnit</param>
		/// <returns>
		/// the time value of the TimeInstant object as a long type in the time unit given as a parameter or
		/// Long.MIN_VALUE if conversion would negatively overflow, or Long.MAX_VALUE if it would positively
		/// overflow.
		/// </returns>
		public long getTimeRounded(TimeUnit unit) {
			if (unit.CompareTo(TimeOperations.getEpsilon()) > 0) {
				// unit has a coarser granularity than epsilon
				long halfAUnitInEpsilon = TimeOperations.getEpsilon().Convert(1,
						unit) / 2;
				long durationInUnitTruncated = getTimeTruncated(unit);
				long difference = _timeInEpsilon
						- TimeOperations.getEpsilon().Convert(
								durationInUnitTruncated, unit);
				// if the time value in the unit Epsilon is bigger than
				if (difference >= halfAUnitInEpsilon) {
					return durationInUnitTruncated + 1;
				}
				return durationInUnitTruncated;
			} else {
				// unit has a finer granularity or is equal than epsilon
				return getTimeTruncated(unit);
			}

		}

		/// <summary>
		/// Returns the value of this TimeInstant object as a long type in the time unit of the reference time. If
		/// the parameter has a coarser granularity than epsilon the returned value will be rounded, so lose
		/// precision.
		/// </summary>
		/// <returns>
		/// the time value of the TimeInstant object as a long type in the time unit given as a parameter or
		/// Long.MIN_VALUE if conversion would negatively overflow, or Long.MAX_VALUE if it would positively
		/// overflow.
		/// </returns>
		public long getTimeRounded() {
			return getTimeRounded(TimeOperations.getReferenceUnit());
		}

		/// <summary>
		/// Returns the value of this TimeInstant object as a Calender object. Note that the TimeZone of the
		/// Calender object returend is set to this TimeInstant's preferred TimeZone (which defaults to UTC unless
		/// set differently).
		/// </summary>
		/// <returns>a Calendar representation of this TimeInstant</returns>
		public DateTimeOffset getTimeAsCalender() {
			long ms = this.getTimeRounded(TimeUnit.MILLISECONDS);
			return TimeZoneInfo.ConvertTime(
				DateTimeOffset.FromUnixTimeMilliseconds(ms),
				this._preferredTimeZone);
		}

		/// <summary>Returns the value of this TimeInstant object as a DateTime (UTC).</summary>
		/// <returns>a DateTime representation of this TimeInstant</returns>
		public DateTime getTimeAsDate() {
			long ms = this.getTimeRounded(TimeUnit.MILLISECONDS);
			return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
		}

		/// <summary>
		/// Returns the value of this TimeInstant object as a double type in the time unit given as a parameter.
		/// </summary>
		/// <returns>
		/// the time value of the TimeInstant object as a double type in the time unit given as a parameter
		/// </returns>
		public double getTimeAsDouble(TimeUnit unit) {
			return _timeInEpsilon
					/ (double)TimeOperations.getEpsilon().Convert(1, unit);
		}

		/// <summary>
		/// Returns the value of this TimeInstant object as a double type in the time unit of the reference time.
		/// </summary>
		/// <returns>
		/// the time value of the TimeInstant object as a double type in the time unit given as a parameter
		/// </returns>
		public double getTimeAsDouble() {
			return getTimeAsDouble(TimeOperations.getReferenceUnit());
		}

		/// <returns>
		/// the time value of the TimeInstant object as a double type in the time unit given as a parameter
		/// </returns>
		/// <remarks>
		/// Replaced by getTimeAsDouble(). The value of this TimeInstant object as a double type in the time unit
		/// of the reference time.
		/// </remarks>
		public double getTimeValue() {
			return getTimeAsDouble();
		}

		/// <summary>The preferred time zone for printing this time instant.</summary>
		/// <returns>the time zone to use for printing this TimeInstant.</returns>
		public TimeZoneInfo getPreferredTimeZone() {
			return this._preferredTimeZone;
		}

		/// <summary>
		/// Sets the preferred time zone for printing this time instant. Note that this time zone does not affect
		/// the internal representation of this TimeInstant (stored as multiple of Experiment's epsilon since the
		/// start of the epoch, i.e. January 1, 1970, 00:00:00 GMT); the preferred TimeZone just servers
		/// TimeFormatters generating a convenient output.
		/// </summary>
		/// <param name="preferredTimeZone">
		/// The time value of this TimeInstant in the time unit of the reference time.
		/// </param>
		public void setPreferredTimeZone(TimeZoneInfo preferredTimeZone) {
			this._preferredTimeZone = preferredTimeZone;
		}

		/// <summary>
		/// Checks if the first of two points of simulation time is before the second. Before means, that the time
		/// value of TimeInstant a is smaller and hence "earlier" than TimeInstant b. Note that this is a static
		/// method available through calling the class <c>TimeInstant</c> i.e. <c>TimeInstant.isAfter(a,b)</c>
		/// where a and b are valid TimeInstant objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is before (earlier) than b</returns>
		public static bool isBefore(TimeInstant a, TimeInstant b) {
			return (a._timeInEpsilon < b._timeInEpsilon);
		}

		/// <summary>
		/// Checks if the first of two points of simulation time is after the second. After means, that the time
		/// value of TimeInstant a is larger and hence "later" than TimeInstant b. Note that this is a static
		/// method available through calling the class <c>TimeInstant</c> i.e. <c>TimeInstant.isAfter(a,b)</c>
		/// where a and b are valid TimeInstant objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is after (later) than b</returns>
		public static bool isAfter(TimeInstant a, TimeInstant b) {
			return (a._timeInEpsilon > b._timeInEpsilon);
		}

		/// <summary>
		/// Checks if the first of two points of simulation time is after the second or equal to the second. After
		/// means, that the time value of TimeInstant a is larger and hence after TimeInstant b. Equal means, that
		/// they both describe the same point in simulation time. Note that this is a static method available
		/// through calling the class <c>TimeInstant</c> i.e. <c>TimeInstant.isAfterOrEqual(a,b)</c> where a and b
		/// are valid timeInstant objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is after (later than )b or equal to b</returns>
		public static bool isAfterOrEqual(TimeInstant a, TimeInstant b) {
			return (isAfter(a, b) || isEqual(a, b));
		}

		/// <summary>
		/// Checks if the two TimeInstant parameters describe the same point of simulation time. Note that this is
		/// a static method available through calling the class <c>TimeInstant</c> i.e.
		/// <c>TimeInstant.isEqual(a,b)</c> where a and b are valid TimeInstant objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if both parameters describe same point of simulation time</returns>
		public static bool isEqual(TimeInstant a, TimeInstant b) {
			return (a._timeInEpsilon == b._timeInEpsilon);
		}

		/// <summary>
		/// Checks if the first of two points of simulation time is before the second or equal to the second.
		/// Before means, that the time value of TimeInstant a is smaller and hence before TimeInstant b. Equal
		/// means, that they both describe the same point in simulation time. Note that this is a static method
		/// available through calling the class <c>TimeInstant</c> i.e. <c>TimeInstant.isBeforeOrEqual(a,b)</c>
		/// where a and b are valid timeInstant objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is before (earlier than )b or equal to b</returns>
		public static bool isBeforeOrEqual(TimeInstant a, TimeInstant b) {
			return (isBefore(a, b) || isEqual(a, b));
		}

		/// <summary>
		/// Determines that last instant prior to this instant at which a new period of the unit provided "begins",
		/// which means smaller units are zero. Examples: Assume this TimeInstant is 28.12.2011 11:23:45:678.
		/// &lt;br\&gt; Calling <c>getBeginOf(TimeUnit.TimeUnit.DAYS)</c> yields 28.12.2011 00:00:00:000 (begin of
		/// current hour).&lt;br\&gt; Calling <c>getBeginOf(TimeUnit.TimeUnit.MINUTES)</c> yields 28.12.2011
		/// 11:23:00:000 (begin of current day).&lt;br\&gt; Note that days are interpreted with respect to this
		/// instant's preferred time zone.
		/// </summary>
		/// <param name="smallestUnit">
		/// the unit to begin (i.e. smaller units set to zero); must be TimeUnit.DAYS, TimeUnit.HOURS,
		/// TimeUnit.MINUTES, TimeUnit.SECONDS or TimeUnit.MILLISECONDS.
		/// </param>
		/// <returns>
		/// The TimeInstant at which the day/hour/minute/second/millisecond of the this TimeInstant has begun.
		/// </returns>
		public TimeInstant getBeginOf(TimeUnit smallestUnit) {
			long ms = this.getTimeTruncated(TimeUnit.MILLISECONDS);
			DateTimeOffset dto = TimeZoneInfo.ConvertTime(
				DateTimeOffset.FromUnixTimeMilliseconds(ms),
				this._preferredTimeZone);

			// Fall-through: each case also zeros all finer units below it
			int h = dto.Hour, min = dto.Minute, sec = dto.Second, msec = dto.Millisecond;
			if (smallestUnit == TimeUnit.DAYS)  { h = 0; min = 0; sec = 0; msec = 0; }
			else if (smallestUnit == TimeUnit.HOURS)   { min = 0; sec = 0; msec = 0; }
			else if (smallestUnit == TimeUnit.MINUTES) { sec = 0; msec = 0; }
			else if (smallestUnit == TimeUnit.SECONDS) { msec = 0; }

			dto = new DateTimeOffset(dto.Year, dto.Month, dto.Day, h, min, sec, msec, dto.Offset);
			return new TimeInstant(dto, true);
		}

		/// <summary>
		/// Returns a hash code value for the object. This method overrides object.hashCode() to support efficient
		/// treatment in HashMaps.
		/// </summary>
		/// <returns>: int: A hash code value for this TimeInstant.</returns>
		public override int GetHashCode() {
			return (int)(this._timeInEpsilon ^ (this._timeInEpsilon >> 32));
		}

		public override bool Equals(object? o) {
			if (o is not TimeInstant instant) return false;
			return isEqual(this, instant);
		}

		/// <summary>
		/// Compares the given TimeInstant to this TimeInstant. This method implements the
		/// Comparable&lt;TimeInstant&gt; Interface
		/// </summary>
		/// <param name="anotherInstant">: The TimeInstant to be compared to this TimeInstant</param>
		/// <returns>
		/// : int: Returns a negative integer, zero, or a positive integer as this TimeInstant is before, at the
		/// same time, or after the given TimeInstant.
		/// </returns>
		public int compareTo(TimeInstant anotherInstant) {
			long difference = this.getTimeInEpsilon()
					- anotherInstant.getTimeInEpsilon();
			if (difference < 0)
				return -1;
			if (difference > 0)
				return 1;
			return 0;
		}

		/// <summary>Returns the String Representation of this TimeInstant according to the TimeFormatter.</summary>
		/// <remarks>See also object.toString() See also TimeFormatter</remarks>
		public string toString() {
			return TimeOperations.formatTimeInstant(this);
		}

		public override string ToString() => toString();

		/// <summary>
		/// Returns the String Representation of this TimeInstant according to the TimeFormatter, truncating digits
		/// after the decimal point if necessary.
		/// </summary>
		/// <param name="digits">Maximum number of digits after decimal point</param>
		/// <remarks>See also object.toString() See also TimeFormatter</remarks>
		public string toString(int digits) {

			string result = TimeOperations.formatTimeInstant(this);

			if (result.LastIndexOf(".") >= 0) {
				result = result.Substring(0, Math.Max(result.Length - 1, 
					result.LastIndexOf(".") + digits));
			}
			return result;
		}

		public int CompareTo(TimeInstant other)
		{
			return this.compareTo(other);
		}
	}
}
