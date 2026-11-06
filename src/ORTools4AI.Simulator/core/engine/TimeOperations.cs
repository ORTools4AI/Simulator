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
     * TimeOperations is an utility class that provides arithmetic operations for
     * the time classes TimeInstant and TimeSpan. It also holds the time settings,
     * i.e. the granularity (epsilon) and the reference time unit.
     */
	public class TimeOperations
	{

		/**
		 * The granularity of simulation time. Default is MICROSECONDS.
		 */
		private static TimeUnit epsilon = TimeUnit.MICROSECONDS;
		
        /**
		 * The reference time unit specifying what is meant by the simulation time
		 * step of 1 in statements without an explicit declaration of a time unit
		 * like in <code>new TimeSpan(5)</code>.
		 */
        private static TimeUnit referenceUnit = TimeUnit.SECONDS;

		/**
		 * The point in simulation time when the experiment has started.
		 */
		private static TimeInstant startTime;

		/**
		 * The TimeFormatter to use for printing TimeInstants in this environment.
		 */
		private static TimeFormatter myTimeFormatter = getDefaultTimeFormatter();

		/**
		 * Flag to indicate that a user-defined <code>TimeFormatter</code> is used
		 * (<code>true</code>), while <code>false</code> means the default is applied.
		 */
		private static bool customTimeFormatterUsed = false;

		/**
		 * Flag set to true as soon as the first TimeInstant or TimeSpan object
		 * is created. Used to detect premature time-object creation before the
		 * experiment's epsilon is configured.
		 */
		internal static bool timeObjectsCreated = false;

		// Suppress default constuctor for noninstantiability
		private TimeOperations()
		{
		}

		/// <summary>
		/// Returns a new TimeSpan object representing the sum of the given TimeSpan objects. Note that the
		/// simulation will stop immediately if the resulting sum is larger than Long.MAX_VALUE-1 (in the unit of
		/// epsilon).
		/// </summary>
		/// <param name="a">TimeSpan a</param>
		/// <param name="b">TimeSpan b</param>
		/// <returns>A new TimeSpan as the sum of the TimeSpan parameters</returns>
		public static TimeSpan add(TimeSpan a, TimeSpan b)
		{
			if (long.MaxValue - a.getTimeInEpsilon() - b.getTimeInEpsilon() < 1)
			{
				// the resulting sum is too big
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't add TimeSpan objects! Simulation aborted.",
								"Class : TimeOperations  Methode : add(TimeSpan a, TimeSpan b)",
								"the resulting sum is too big. ",
								"Can only create TimeSpan objects which are smaller than Long.MAX_VALUE (in the TimeUnit of epsilon).",
								null)));
			}
			return new TimeSpan(a.getTimeInEpsilon() + b.getTimeInEpsilon(),
					epsilon);
		}

		/// <summary>
		/// Returns a new Time Instant determined from the instant passed to this method plus the time span passed
		/// to method. In other words, this method determines the instant that is a given span after the instant
		/// provided. Note that the simulation will stop immediately if the resulting instant is larger than
		/// <c>Long.MAX_VALUE-1</c> (in the unit of epsilon). The preferred time zone of the result is equal to the
		/// preferred time zone of parameter <c>b</c>.
		/// </summary>
		/// <param name="span">The TimeSpan to add...</param>
		/// <param name="instant">...to this TimeInstant.</param>
		/// <returns>A new TimeInstant determined from given TimeInstant plus the given TimeSpan</returns>
		public static TimeInstant add(TimeSpan span, TimeInstant instant)
		{
			return TimeOperations.add(instant, span);
		}

		/// <summary>
		/// Returns a new Time Instant determined from the instant passed to this method plus the time span passed
		/// to method. In other words, this method determines the instant that is a given span after the instant
		/// provided. Note that the simulation will stop immediately if the resulting instant is larger than
		/// <c>Long.MAX_VALUE-1</c> (in the unit of epsilon). The preferred time zone of the result is equal to the
		/// preferred time zone of instant passed to this method.
		/// </summary>
		/// <param name="instant">The TimeInstant, to which...</param>
		/// <param name="span">...this TimeSpan will be added</param>
		/// <returns>A new TimeInstant determined from given TimeInstant plus the given TimeSpan</returns>
		public static TimeInstant add(TimeInstant instant, TimeSpan span)
		{
			if (instant.getTimeInEpsilon() > 0)
			{
				// overflow is possible
				if (long.MaxValue - span.getTimeInEpsilon()
						- instant.getTimeInEpsilon() < 1)
				{
					// the resulting sum is too big
					throw (new SimAbortedException(
							new ErrorMessage(
									null,
									"Can't add TimeSpan and TimeInstant objects! Simulation aborted.",
									"Class : TimeOperations  Methode : add(TimeSpan a, TimeInstant b)",
									"the resulting sum is too big. ",
									"Can only create TimeInstant objects which are before Long.MAX_VALUE (in the TimeUnit of epsilon).",
									null)));
				}
			}
			TimeInstant result = new TimeInstant(span.getTimeInEpsilon()
					+ instant.getTimeInEpsilon(), epsilon);
			result.setPreferredTimeZone(instant.getPreferredTimeZone());
			return result;
		}

		/// <summary>
		/// Returns a new Time Instant determined from the instant passed to this method minus the time span passed
		/// to method. In other words, this method determines the instant that is a given span before the instant
		/// provided. Note that the simulation will stop immediately if the resulting instant is smaller than 0 (in
		/// the unit of epsilon). The preferred time zone of the result is equal to the preferred time zone of
		/// instant passed to this method.
		/// </summary>
		/// <param name="instant">The TimeInstant, from which...</param>
		/// <param name="span">...this TimeSpan will be subtracted</param>
		/// <returns>A new TimeInstant determined from given TimeInstant minus the given TimeSpan</returns>
		public static TimeInstant subtract(TimeInstant instant, TimeSpan span)
		{
			if (instant.getTimeInEpsilon() > 0)
			{
				// prior to zero?
				if (instant.getTimeInEpsilon() - span.getTimeInEpsilon() < 0)
				{

					// the resulting instant is prior to zero
					throw (new SimAbortedException(
							new ErrorMessage(
									null,
									"Can't subtract TimeInstant and  TimeSpan objects! Simulation aborted.",
									"Class : TimeOperations  Methode : subtract(TimeInstant a, TimeSpan b)",
									"the resulting smaller than 0.",
									"Can only create TimeInstant objects which are non-negative (in the TimeUnit of epsilon).",
									null)));
				}
			}
			TimeInstant result = new TimeInstant(instant.getTimeInEpsilon()
					- span.getTimeInEpsilon(), epsilon);
			result.setPreferredTimeZone(instant.getPreferredTimeZone());
			return result;
		}


		/// <summary>
		/// Returns a new TimeSpan object representing the absolute difference of the given TimeSpan objects.
		/// </summary>
		/// <param name="a">TimeSpan a</param>
		/// <param name="b">TimeSpan b</param>
		/// <returns>A new TimeSpan as the absolute difference of the TimeSpan parameters</returns>
		public static TimeSpan diff(TimeSpan a, TimeSpan b)
		{

			if (TimeSpan.isShorter(a, b))
			{
				return new TimeSpan(b.getTimeInEpsilon() - a.getTimeInEpsilon(),
						epsilon);
			}

			return new TimeSpan(a.getTimeInEpsilon() - b.getTimeInEpsilon(),
					epsilon);
		}

		/// <summary>
		/// Returns a new TimeSpan object representing the absolute difference of the given TimeInstant objects,
		/// i.e. the span of time between the two instants of time. Note that the simulation will stop immediately
		/// if the resulting sum is larger than <c>Long.MAX_VALUE-1</c> (in the unit of epsilon).
		/// </summary>
		/// <param name="a">TimeInstant a</param>
		/// <param name="b">TimeInstant b</param>
		/// <returns>A new TimeSpan as the difference of the TimeSpan parameters</returns>
		public static TimeSpan diff(TimeInstant a, TimeInstant b)
		{
			if (TimeInstant.isAfter(a, b))
			{
				// a is after b
				if (b.getTimeInEpsilon() < 0)
				{
					// the resulting difference could be too big.

					if (long.MaxValue - 1 + b.getTimeInEpsilon() < a
						.getTimeInEpsilon())
					{
						// the resulting difference is too big.
						throw (new SimAbortedException(
								new ErrorMessage(
										null,
										"Can't subtract TimeInstant objects! Simulation aborted.",
										"Class : TimeOperations  Methode : diff(TimeSpan a, TimeInstant b)",
										"the resulting difference is too big. ",
										"Can only create TimeSpan objects which are smaller than Long.MAX_VALUE (in the TimeUnit of epsilon).",
										null)));
					}
				}
				return new TimeSpan(a.getTimeInEpsilon() - b.getTimeInEpsilon(),
						epsilon);
			}
			else
			{
				// b is after a
				if (a.getTimeInEpsilon() < 0)
				{
					// the resulting difference could be too big.

					if (long.MaxValue + a.getTimeInEpsilon() < b
							.getTimeInEpsilon())
					{
						// the resulting difference is too big.
						throw (new SimAbortedException(
								new ErrorMessage(
										null,
										"Can't subtract TimeInstant objects! Simulation aborted.",
										"Class : TimeOperations  Methode : diff(TimeSpan a, TimeInstant b)",
										"the resulting difference is too big. ",
										"Can only create TimeSpan objects which are smaller than Long.MAX_VALUE (in the TimeUnit of epsilon).",
										null)));
					}
				}

				return new TimeSpan(b.getTimeInEpsilon() - a.getTimeInEpsilon(),
						epsilon);
			}

		}

		/// <summary>
		/// Returns a new TimeSpan object representing the product of the given TimeSpan and the factor of type
		/// double. Note that the simulation will stop immediately if the resulting product is larger than
		/// <c>Long.MAX_VALUE-1</c> (in the unit of epsilon).
		/// </summary>
		/// <param name="span">The span of time</param>
		/// <param name="factor">The scalar factor</param>
		/// <returns>A new TimeSpan as the product of span and factor</returns>
		public static TimeSpan multiply(TimeSpan span, double factor)
		{
			if (factor > 1)
			{
				// the resulting product could be too big
				if (long.MaxValue / factor < span.getTimeInEpsilon())
				{
					// the resulting product is too big
					throw (new SimAbortedException(
							new ErrorMessage(
									null,
									"Can't multiply TimeSpan and double value! Simulation aborted.",
									"Class : TimeOperations  Methode : multiply(TimeSpan span, double factor)",
									"the resulting product is too big. ",
									"Can only create TimeSpan objects which are shorter than Long.MAX_VALUE (in the TimeUnit of epsilon).",
									null)));
				}
			}
			return new TimeSpan((long)(span.getTimeInEpsilon() * factor), epsilon);
		}

		/// <summary>
		/// Returns a new TimeSpan object representing the product of the given TimeSpan and the factor of type
		/// double. Note that the simulation will stop immediately if the resulting product is larger than
		/// <c>Long.MAX_VALUE-1</c> (in the unit of epsilon).
		/// </summary>
		/// <param name="factor">The scalar factor</param>
		/// <param name="span">The span of time</param>
		/// <returns>A new TimeSpan as the product of span and factor</returns>
		public static TimeSpan multiply(double factor, TimeSpan span)
		{
			return TimeOperations.multiply(span, factor);
		}

		/// <summary>
		/// Returns a new TimeSpan object representing the quotient of the given TimeSpan objects.
		/// </summary>
		/// <param name="dividend">The dividend</param>
		/// <param name="divisor">The divisor</param>
		/// <returns>A new TimeSpan as the quotient of dividend and divisor</returns>
		public static double divide(TimeSpan dividend, TimeSpan divisor)
		{
			if (divisor.getTimeInEpsilon() == 0)
			{
				// cannot divide by zero
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't divide TimeSpan values! Simulation aborted.",
								"Class : TimeOperations  Methode : divide(TimeSpan dividend, TimeSpan divisor)",
								"Cannot devide by zero.",
								"Never try to devide by zero.", null)));
			}
			return ((double)dividend.getTimeInEpsilon() / (double)divisor
					.getTimeInEpsilon());

		}

		/// <summary>
		/// Returns a new TimeSpan object representing the quotient of the given TimeSpan and the divisor of type
		/// double. Note that the simulation will stop immediately if the resulting quotient is larger than
		/// Long.MAX_VALUE-1 (in the unit of epsilon).
		/// </summary>
		/// <param name="dividend">The dividend</param>
		/// <param name="divisor">The divisor</param>
		/// <returns>A new TimeSpan as the quotient of divident and divisor</returns>
		public static TimeSpan divide(TimeSpan dividend, double divisor)
		{
			if (divisor <= 0)
			{
				// cannot divide by zero
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't divide TimeSpan and double value! Simulation aborted.",
								"Class : TimeOperations  Methode : mdivide(TimeSpan dividend, double divisor)",
								"Cannot devide by zero.",
								"Never try to devide by zero.", null)));
			}
			if (divisor < 1)
			{
				// the resulting quotient could be too big
				if (long.MaxValue * divisor < dividend.getTimeInEpsilon())
				{
					// the resulting product is too big
					throw (new SimAbortedException(
							new ErrorMessage(
									null,
									"Can't divide TimeSpan and double value! Simulation aborted.",
									"Class : TimeOperations  Methode : mdivide(TimeSpan dividend, double divisor)",
									"the resulting quotient is too big. ",
									"Can only create TimeSpan objects which are shorter than Long.MAX_VALUE (in the TimeUnit of epsilon).",
									null)));
				}
			}
			return new TimeSpan((long)(dividend.getTimeInEpsilon() / divisor),
					epsilon);
		}

		/// <summary>
		/// Returns the epsilon value representing the granularity of simulation time for this experiment.
		/// </summary>
		/// <returns>The granularity of simulation time</returns>
		public static TimeUnit getEpsilon()
		{
			return epsilon;
		}

		/// <summary>Returns the smallest distinguishable TimeSpan.</summary>
		/// <returns>The smallest distinguishable TimeSpan, i.e. one interval of the epsilon unit</returns>
		public static TimeSpan getEpsilonSpan()
		{
			return new TimeSpan(1, epsilon);
		}

		/// <summary>
		/// Returns the reference time unit specifying what is meant by the simulation time step of 1 in statements
		/// without an explicit declaration of a time unit like in <c>new TimeSpan(5)</c>.
		/// </summary>
		/// <returns>the reference time unit</returns>
		public static TimeUnit getReferenceUnit()
		{
			return referenceUnit;
		}

		/// <summary>
		/// Sets the epsilon value representing the granularity of simulation time to the given TimeUnit parameter.
		/// This is a package private method for internal framework use only.
		/// </summary>
		/// <param name="epsilonValue">
		/// The granularity of simulation time, i.e. the smallest distinguishable span of simulation time.
		/// </param>
		public static void setEpsilon(TimeUnit epsilonValue)
		{
		  	epsilon = epsilonValue;
		}

		/// <summary>
		/// Sets the reference time unit specifying what is meant by the simulation time step of 1 in statements
		/// without an explicit declaration of a time unit like in <c>new TimeSpan(5)</c>. This is a package
		/// private method for internal framework use only.
		/// </summary>
		/// <param name="referenceUnit">the reference time unit</param>
		public static void setReferenceUnit(TimeUnit referenceUnit)
		{
			TimeOperations.referenceUnit = referenceUnit;
		}

		/// <summary>
		/// Formats the given instant of time according to the timeFormatter. This is a package private method for
		/// internal framework use only.
		/// </summary>
		/// <param name="instant">the instant of time to be formatted</param>

		public static string formatTimeInstant(TimeInstant instant)
		{
			return myTimeFormatter.buildTimeString(instant);
		}

		/// <summary>
		/// Formats the given span of time according to the timeFormatter. This is a package private method for
		/// internal framework use only.
		/// </summary>
		/// <param name="span">the span of time to be formatted</param>
		public	static string formatTimeSpan(TimeSpan span)
		{
			return myTimeFormatter.buildTimeString(span);
		}

		/**
		 * Returns the time Formatter. This is a package private method for internal
		 * framework use only.
		 */
		public static TimeFormatter getTimeFormatter()
		{
			return myTimeFormatter;
		}

		/// <summary>
		/// Sets the time Formatter. This is a package private method for internal framework use only.
		/// </summary>
		/// <param name="myTimeFormatter">the Time Formatter</param>
		/// <param name="userDefined">
		/// indicates that a user-defined <c>TimeFormatter</c> is supplied (<c>true</c>) or a default
		/// <c>TimeFormatter</c> is assigned (<c>false</c>). Default <c>TimeFormatter</c>s (e.g. as set up when an
		/// <c>Experiment</c> object is created) will not overwrite a <c>TimeFormatter</c> potentially previously
		/// set the the user by calling <c>Experiment.setTimeFormatter(...)</c>.
		/// </param>
		public static void setTimeFormatter(TimeFormatter myTimeFormatter, bool userDefined)
		{

			// user defined -> use always, mark as user-defined
			if (userDefined)
			{
				TimeOperations.myTimeFormatter = myTimeFormatter;
				customTimeFormatterUsed = true;
				return;
			}

			// otherwise (not user-defined) -> use only if no TimeFormatter already set by the user
			if (!customTimeFormatterUsed)
			{
				TimeOperations.myTimeFormatter = myTimeFormatter;
				return;
			}
		}

		/// <summary>
		/// Sets the TimeFormatter used by this environment to a <c>SingleUnitTimeFormatter</c>, using the current
		/// reference unit and four floating point digits. This is a package private method for internal framework
		/// use only.
		/// </summary>
		public static TimeFormatter getDefaultTimeFormatter()
		{
			return new SingleUnitTimeFormatter(referenceUnit, epsilon, 4, false);
			
		}

		/// <summary>Returns the TimeInstant when the experiment has started.</summary>
		/// <returns>The point in simulation time, the experiment has started.</returns>
		public static TimeInstant getStartTime()
		{
			return startTime;
		}

		public static void setStartTime(TimeInstant startTime)
		{
			TimeOperations.startTime = startTime;
		}
	}
}