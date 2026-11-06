// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Felix Klueckmann
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.engine
{


	/**
	 * Represents spans of simulation time. Each span of simulation time is
	 * represented by an individual object of this class and offers its own methods
	 * for arithmetic operations and comparison. Ensures that only valid spans of
	 * time are generated.
	 */
	public class TimeSpan : IComparable
	{

		/**
		 * ZERO defines the value for scheduling an object immediately, even
		 * replacing the current SimProcess.
		 */
		public static readonly TimeSpan ZERO = new TimeSpan(0, TimeUnit.MICROSECONDS);

		/**
		 * The span of time in the unit of epsilon
		 */
		private readonly long _durationInEpsilon;

		/// <summary>
		/// Constructs a TimeSpan object with the given time value in the time unit of the given parameter. It
		/// represents a time span in simulation time. Note that trying to create a TimeSpan object with a negative
		/// value will stop the simulation immediately. The simulation will also stop immediately if the TimeSpan
		/// is larger than Long.MAX_VALUE-1 (in the unit of epsilon).
		/// </summary>
		/// <param name="duration">The time value of this TimeSpan</param>
		/// <param name="unit">the TimeUnit</param>
		public TimeSpan(long duration, TimeUnit unit) {
			if (unit == null) { // no time unit given
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't create TimeSpan object! Simulation aborted.",
								"Class : TimeSpan  Constructor : TimeSpan(long, TimeUnit)",
								"Time unit passed is null",
								"Make sure to pass a non-null time unit. \nNote that before " +
								  "connecting model and experiment, TimeSpans must explicitly\n" +
								  "refer to a time unit as the reference unit is not yet defined," +
								  "e.g. use \nTimeSpan(long time, TimeUnit unit) instead of" +
								  "TimeInstant(long time).",
								null)));
			}
			if (duration < 0) { // points of time must be postive
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't create TimeSpan object! Simulation aborted.",
								"Class : TimeSpan  Constructor : TimeSpan(long, TimeUnit)",
								"the value passed for instantiation is negative : "
										+ duration,
								"Negative values for simulation time are illegal.",
								null)));
			}
			_durationInEpsilon = TimeOperations.getEpsilon().Convert(duration, unit);
			if (_durationInEpsilon == long.MaxValue) {
				/*The timeSpan is too big.
				(The method TimeUnit.convert(duration,unit)returns Long.MAX_VALUE if
				the result of the conversion is to big*/

				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't create TimeSpan object! Simulation aborted.",
								"Class : TimeSpan  Constructor : TimeSpan(long,TimeUnit)",
								"the TimeSpan is too big. ",
								"Can only create TimeSpan objects which are smaller than Long.MAX_VALUE (in the TimeUnit of epsilon).",
								null)));
			}
			TimeOperations.timeObjectsCreated = true;
		}

		/// <summary>
		/// Constructs a TimeSpan object with the given time value in the time unit of the reference time. It
		/// represents a time span in simulation time. Note that trying to create a TimeSpan object with a negative
		/// value will stop the simulation immediately. The simulation will also stop immediately if the TimeSpan
		/// is larger than Long.MAX_VALUE-1 (in the unit of epsilon).
		/// </summary>
		/// <param name="duration">The time value of this TimeSpan in the time unit of the reference time.</param>
		public TimeSpan(long duration): this(duration, TimeOperations.getReferenceUnit()) {
			
		}

		/// <summary>
		/// Constructs a TimeSpan object with the given time value in the given timeUnit. It represents a time span
		/// in simulation time. Note that trying to create a TimeSpan object with a negative value will stop the
		/// simulation immediately. The simulation will also stop immediately if the TimeSpan is larger than
		/// Long.MAX_VALUE-1 (in the unit of epsilon).
		/// </summary>
		/// <param name="duration">The time value of this TimeSpan in the time unit of the reference time.</param>
		/// <param name="unit">the time unit</param>
		public TimeSpan(double duration, TimeUnit unit) {
			if (unit == null) { // no time unit given
				throw (new SimAbortedException(
						new		ErrorMessage(
								null,
								"Can't create TimeSpan object! Simulation aborted.",
								"Class : TimeSpan  Constructor : TimeSpan(double, TimeUnit)",
								"Time unit passed is null",
								"Make sure to pass a non-null time unit. \nNote that before " +
								  "connecting model and experiment, TimeSpans must explicitly\n" +
								  "refer to a time unit as the reference unit is not yet defined," +
								  "e.g. use \nTimeSpan(double time, TimeUnit unit) instead of" +
								  "TimeInstant(double time).",
								null)));
			}
			_durationInEpsilon = (long)(duration * TimeOperations.getEpsilon().Convert(1, unit));
			// A positive duration that underflows to 0 after truncation would create a
			// zero-span, causing zero-time reschedules and an infinite scheduler loop.
			// Clamp to 1 epsilon so sub-epsilon positive durations advance time.
			if (_durationInEpsilon == 0 && duration > 0)
				_durationInEpsilon = 1;

			if (_durationInEpsilon < 0) { // points of time must be postive
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't create TimeSpan object! Simulation aborted.",
								"Class : TimeSpan  Constructor : TimeSpan(long, TimeUnit)",
								"the value passed for instantiation is negative : "
										+ _durationInEpsilon,
								"Negative values for simulation time are illegal.",
								null)));
			}
			if (_durationInEpsilon == long.MaxValue) {
				/*The timeSpan is too big.
				(The method TimeUnit.convert(duration,unit)returns Long.MAX_VALUE if
				the result of the conversion is to big*/

				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't create TimeSpan object! Simulation aborted.",
								"Class : TimeSpan  Constructor : TimeSpan(long,TimeUnit)",
								"the TimeSpan is too big. ",
								"Can only create TimeSpan objects which are smaller than Long.MAX_VALUE (in the TimeUnit of epsilon).",
								null)));
			}
			TimeOperations.timeObjectsCreated = true;
		}

		/// <summary>
		/// Constructs a TimeSpan object with the given time value in the time unit of the reference time. It
		/// represents a time span in simulation time. Note that trying to create a TimeSpan object with a negative
		/// value will stop the simulation immediately.The simulation will also stop immediately if the TimeSpan is
		/// larger than Long.MAX_VALUE-1 (in the unit of epsilon).
		/// </summary>
		/// <param name="duration">The time value of this TimeSpan in the time unit of the reference time.</param>
		public TimeSpan(double duration): this(duration, TimeOperations.getReferenceUnit()) {
			
		}

		

		internal TimeSpan(Builder builder) {
			_durationInEpsilon = builder.durationInEpsilon;
		}

		/// <summary>Returns the value of the TimeSpan object as a long type in the time unit of epsilon</summary>
		/// <returns>the time value of the TimeSpan object as a long type in the time unit of epsilon</returns>
		public long getTimeInEpsilon() {
			return _durationInEpsilon;
		}

		/// <summary>
		/// Returns the value of this TimeSpan object as a long type in the time unit given as a parameter. If the
		/// parameter has a coarser granularity than epsilon the returned value will be truncated, so lose
		/// precision.
		/// </summary>
		/// <param name="unit">: the TimeUnit</param>
		/// <returns>
		/// the time value of the TimeSpan object as a long type in the time unit given as a parameter or
		/// Long.MIN_VALUE if conversion would negatively overflow, or Long.MAX_VALUE if it would positively
		/// overflow.
		/// </returns>
		public long getTimeTruncated(TimeUnit unit) {
			return unit.Convert(_durationInEpsilon, TimeOperations.getEpsilon());
		}

		/// <summary>
		/// Returns the value of this TimeSpan object as a long type in the time unit of the reference time. If the
		/// parameter has a coarser granularity than epsilon the returned value will be truncated, so lose
		/// precision.
		/// </summary>
		/// <returns>
		/// the time value of the TimeSpan object as a long type in the time unit given as a parameter or
		/// Long.MIN_VALUE if conversion would negatively overflow, or Long.MAX_VALUE if it would positively
		/// overflow.
		/// </returns>
		public long getTimeTruncated() {
			return getTimeTruncated(TimeOperations.getReferenceUnit());
		}

		/// <summary>
		/// Returns the value of this TimeSpan object as a long type in the time unit given as a parameter. If the
		/// parameter has a coarser granularity than epsilon the returned value will be rounded, so lose precision.
		/// </summary>
		/// <param name="unit">: the TimeUnit</param>
		/// <returns>
		/// the time value of the TimeSpan object as a long type in the time unit given as a parameter or
		/// Long.MIN_VALUE if conversion would negatively overflow, or Long.MAX_VALUE if it would positively
		/// overflow.
		/// </returns>
		public long getTimeRounded(TimeUnit unit) 
		{
			if (unit.CompareTo(TimeOperations.getEpsilon()) > 0) {
				//unit has a coarser granularity than epsilon
				long halfAUnitInEpsilon = TimeOperations.getEpsilon().Convert(1, unit) / 2;
				long durationInUnitTruncated = getTimeTruncated(unit);
				long difference = _durationInEpsilon
						- TimeOperations.getEpsilon().Convert(durationInUnitTruncated,
								unit);
				// if the time value in the unit Epsilon is bigger than
				if (difference >= halfAUnitInEpsilon) {
					return durationInUnitTruncated + 1;
				}
				return durationInUnitTruncated;
			}
			else {
				//unit has a finer granularity or is equal than epsilon
				return getTimeTruncated(unit);
			}

		}

		/// <summary>
		/// Returns the value of this TimeSpan object as a long type in the time unit of the reference time. If the
		/// parameter has a coarser granularity than epsilon the returned value will be rounded, so lose precision.
		/// </summary>
		/// <returns>
		/// the time value of the TimeSpan object as a long type in the time unit given as a parameter or
		/// Long.MIN_VALUE if conversion would negatively overflow, or Long.MAX_VALUE if it would positively
		/// overflow.
		/// </returns>
		public long getTimeRounded() {
			return getTimeRounded(TimeOperations.getReferenceUnit());
		}

		/// <summary>
		/// Returns the value of this TimeSpan object as a double type in the time unit given as a parameter.
		/// </summary>
		/// <returns>
		/// the time value of the TimeSpan object as a double type in the time unit given as a parameter
		/// </returns>
		public double getTimeAsDouble(TimeUnit unit) {
			return _durationInEpsilon
					/ (double)TimeOperations.getEpsilon().Convert(1, unit);
		}

		/// <summary>
		/// Returns the value of this TimeSpan object as a double type in the time unit of the reference time.
		/// </summary>
		/// <returns>
		/// the time value of the TimeSpan object as a double type in the time unit given as a parameter
		/// </returns>
		public double getTimeAsDouble() {
			return getTimeAsDouble(TimeOperations.getReferenceUnit());
		}


		/// <summary>
		/// Checks if the first of two spans of simulation time is longer than the second. Note that this is a
		/// static method available through calling the class <c>TimeSpan</c> i.e. <c>TimeSpan.isLonger(a,b)</c>
		/// where a and b are valid TimeSpan objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is longer than b</returns>
		public static bool isLonger(TimeSpan a, TimeSpan b) {
			return (a._durationInEpsilon > b._durationInEpsilon);
		}

		/// <summary>
		/// Checks if the first of two spans of simulation time is longer than the second or equal to the second.
		/// Note that this is a static method available through calling the class <c>TimeSpan</c> i.e.
		/// <c>TimeSpan.isLonger(a,b)</c> where a and b are valid TimeSpan objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is longer than b or equal to b.</returns>
		public static bool isLongerOrEqual(TimeSpan a, TimeSpan b) {
			return (isLonger(a, b) || isEqual(a, b));
		}

		/// <summary>
		/// Checks if the first of two spans of simulation time is shorter than the second. Note that this is a
		/// static method available through calling the class <c>TimeSpan</c> i.e. <c>TimeSpan.isShorter(a,b)</c>
		/// where a and b are valid TimeSpan objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is shorter than b</returns>
		public static bool isShorter(TimeSpan a, TimeSpan b) {
			return (a._durationInEpsilon < b._durationInEpsilon);
		}

		/// <summary>
		/// Checks if the first of two spans of simulation time is shorter than the second or equal to the second.
		/// Note that this is a static method available through calling the class <c>TimeSpan</c> i.e.
		/// <c>TimeSpan.isShorterOrEqual(a,b)</c> where a and b are valid TimeSpan objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is shorter than b or equal to b.</returns>
		public static bool isShorterOrEqual(TimeSpan a, TimeSpan b) {
			return (isShorter(a, b) || isEqual(a, b));
		}

		/// <summary>Indicates whether TimeSpan a is equal to TimeSpan b, i.e. they are of equal length.</summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>true if a is equal to b; false otherwise.</returns>
		public static bool isEqual(TimeSpan a, TimeSpan b) {
			return (a._durationInEpsilon == b._durationInEpsilon);
		}

		/// <summary>
		/// Indicates whether this TimeSpan is equal to the given parameter. Returns true if the obj argument is a
		/// TimeSpan and is of equal length as this TimeSpan; false otherwise. This method overrides
		/// object.equals()
		/// </summary>
		/// <param name="obj">the reference object with which to compare.</param>
		/// <returns>
		/// true if the obj argument is a TimeSpan and is of equal length as this TimeSpan; false otherwise.
		/// </returns>
		public override bool Equals(object? obj) {
			if (obj is not TimeSpan span) return false;
			return isEqual(this, span);
		}

		public bool equals(object? obj) => Equals(obj);

		public override int GetHashCode() {
			return (int)(this._durationInEpsilon ^ (this._durationInEpsilon >> 32));
		}

		public bool isZero() {
			return _durationInEpsilon == 0L;
		}

		/// <summary>
		/// Compares the given TimeSpan to this TimeSpan. This method implements the Comparable&lt;TimeSpan&gt;
		/// Interface
		/// </summary>
		/// <param name="anotherTimeSpan">The TimeSpan to be compared to this TimeSpan</param>
		/// <returns>
		/// Returns a negative integer, zero, or a positive integer as this TimeSpan is shorter than, equal to, or
		/// longer than the given parameter.
		/// </returns>
		public int compareTo(TimeSpan anotherTimeSpan) {
			long difference = this.getTimeInEpsilon()
					- anotherTimeSpan.getTimeInEpsilon();
			// if the given parameter is longer than this return -1
			if (difference < 0)
				return -1;
			// if the given parameter is shorter than this return 1
			if (difference > 0)
				return 1;
			// if they are equal return 0
			return 0;
		}

		/// <summary>Returns the String Representation of this TimeSpan.</summary>
		/// <remarks>See also object.toString()</remarks>
		//@Override
		public string toString() {
			return TimeOperations.formatTimeSpan(this);
		}

		public override string ToString() => toString();

		/// <summary>
		/// Returns the String Representation of this TimeSpan according to the TimeFormatter, truncating digits
		/// after the decimal point if necessary.
		/// </summary>
		/// <param name="digits">Maximum number of digits after decimal point</param>
		/// <remarks>See also object.toString() See also TimeFormatter</remarks>
		public string toString(int digits) {

			string result = TimeOperations.formatTimeSpan(this);

			if (result.LastIndexOf(".") >= 0) {
				result = result.Substring(0, Math.Max(result.Length - 1, result.LastIndexOf(".") + digits));
			}
			return result;
		}

		public int CompareTo(object? obj)
		{
			if (obj is TimeSpan ts) return compareTo(ts);
			throw new ArgumentException("Not a TimeSpan");
		}
	}


    /**
		 *Use the Builder to create TimeSpans specified as the sum of durations
		 * with different TimeUnits Example (timeSpan with the length of one hour
		 * and 30 minutes): new TimeSpan.Builder().hours(1).minutes(30).build();
		 * 
		 */
    public  class Builder
    {
        public long durationInEpsilon;

		
        public Builder()
        {
            durationInEpsilon = 0L;
        }

        public Builder nanoseconds(long nanoseconds)
        {
            if (nanoseconds < 0)
            { // points of time must be postive
                throw (new SimAbortedException(
                        new ErrorMessage(
                                null,
                                "Can't set the value of nanoseconds of the Builder for TimeSpan object! Simulation aborted.",
                                "Class : TimeSpan  Builder : Builder.nanoseconds(long)",
                                "the value passed for the setting of nanoseconds is negative : "
                                        + nanoseconds,
                                "Negative values for simulation time are illegal.",
                                null)));
            }
            durationInEpsilon += TimeOperations.getEpsilon().Convert(
                    nanoseconds,TimeUnit.NANOSECONDS);
            return this;
        }

        public Builder microseconds(long microseconds)
        {
            if (microseconds < 0)
            { // points of time must be positive
                throw (new SimAbortedException(
                        new ErrorMessage(
                                null,
                                "Can't set the value of microseconds of the Builder for TimeSpan object! Simulation aborted.",
                                "Class : TimeSpan  Builder : Builder.microseconds(long)",
                                "the value passed for the setting of microseconds is negative : "
                                        + microseconds,
                                "Negative values for simulation time are illegal.",
                                null)));
            }
            durationInEpsilon += TimeOperations.getEpsilon().Convert(
                    microseconds,TimeUnit.MICROSECONDS);
            return this;
        }

        public Builder milliseconds(long milliseconds)
        {
            if (milliseconds < 0)
            { // points of time must be postive
                throw (new SimAbortedException(
                        new ErrorMessage(
                                null,
                                "Can't set the value of milliseconds of the Builder for TimeSpan object! Simulation aborted.",
                                "Class : TimeSpan  Builder : Builder.milliseconds(long)",
                                "the value passed for the setting of milliseconds is negative : "
                                        + milliseconds,
                                "Negative values for simulation time are illegal.",
                                null)));
            }
            durationInEpsilon += TimeOperations.getEpsilon().Convert(
                    milliseconds,TimeUnit.MILLISECONDS);
            return this;
        }

        public Builder seconds(long seconds)
        {
            if (seconds < 0)
            { // points of time must be postive
                throw (new SimAbortedException(
                        new ErrorMessage(
                                null,
                                "Can't set the value of seconds of the Builder for TimeSpan object! Simulation aborted.",
                                "Class : TimeSpan  Builder : Builder.seconds(long)",
                                "the value passed for the setting of seconds is negative : "
                                        + seconds,
                                "Negative values for simulation time are illegal.",
                                null)));
            }
            durationInEpsilon += TimeOperations.getEpsilon().Convert(seconds,
                   TimeUnit.SECONDS);
            return this;
        }

        public Builder minutes(long minutes)
        {
            if (minutes < 0)
            { // points of time must be postive
                throw (new SimAbortedException(
                        new ErrorMessage(
                                null,
                                "Can't set the value of minutes of the Builder for TimeSpan object! Simulation aborted.",
                                "Class : TimeSpan  Builder : Builder.minutes(long)",
                                "the value passed for the setting of minutes is negative : "
                                        + minutes,
                                "Negative values for simulation time are illegal.",
                                null)));
            }
            durationInEpsilon += TimeOperations.getEpsilon().Convert(minutes,
                   TimeUnit.MINUTES);
            return this;
        }

        public Builder hours(long hours)
        {
            if (hours < 0)
            { // points of time must be postive
                throw (new SimAbortedException(
                        new ErrorMessage(
                                null,
                                "Can't set the value of hours of the Builder for TimeSpan object! Simulation aborted.",
                                "Class : TimeSpan  Builder : Builder.hours(long)",
                                "the value passed for the setting of hours is negative : "
                                        + hours,
                                "Negative values for simulation time are illegal.",
                                null)));
            }
            durationInEpsilon += TimeOperations.getEpsilon().Convert(hours,
                   TimeUnit.HOURS);
            return this;
        }

        public Builder days(long days)
        {
            if (days < 0)
            { // points of time must be postive
                throw (new SimAbortedException(
                        new ErrorMessage(
                                null,
                                "Can't set the value of days of the Builder for TimeSpan object! Simulation aborted.",
                                "Class : TimeSpan  Builder : Builder.days(long)",
                                "the value passed for the setting of days is negative : "
                                        + days,
                                "Negative values for simulation time are illegal.",
                                null)));
            }
            durationInEpsilon += TimeOperations.getEpsilon()
                    .Convert(days,TimeUnit.DAYS);
            return this;
        }
        /**
         * Use this method to create TimeSpan objects with the builder pattern.
         * 
         */
        public TimeSpan build()
        {
            return new TimeSpan(this);
        }
    }
}
