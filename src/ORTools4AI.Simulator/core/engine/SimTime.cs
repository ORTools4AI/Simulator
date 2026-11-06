// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// @author modified by Felix Klueckmann
// @author Felix Klueckmann (07/09)
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * @deprecated Represents points in simulation time. Is used to indicate points
     *             in simulation time at which the state of the model changes. Each
     *             point in simulation time is represented by an individual object
     *             of this class and offers its own methods for arithmetic
     *             operations. Ensures that only valid points of time are generated.
     */
	// @Deprecated
	public class SimTime
	{

		/**
		 * NOW defines the value for scheduling an object immediately, even
		 * replacing the current SimProcess. Since the value is otherwise arbitrary
		 * as long as it is unique and can not be confused with any other legal or
		 * illegal (negative) value, it is set to be the constant defined in
		 * java.lang.Double and in java.lang.Float representing a non-number value.
		 * 
		 * @see java.lang.Double
		 * @see java.lang.Float
		 */
		public static readonly SimTime NOW = new SimTime(0.0);

		/**
		 * Stores the point of simulation time as double value for each SimTime
		 * object.
		 */
		private readonly double _myTime;

		/// <summary>
		/// Constructs a simtime object with the given initial time value. It represents a point in simulation
		/// time. Note that only points of future simulation time can be accessed and that trying to create a
		/// simtime object with negative initial value will stop the simulation immediately.
		/// </summary>
		/// <param name="time">The time value of this simtime with a simtime.</param>
		public SimTime(double time)
		{

			//super(); // default object constructor

			if (time < 0.0)
			{ // points of time must be postive
				throw (new SimAbortedException(
						new ErrorMessage(null,
								"Can't create SimTime object! Simulation aborted.",
								"Class : SimTime  Constructor : SimTime(double)",
								"the value passed for instantiation is negative : "
										+ time,
								"Negative values for simulation time are illegal.",
								null)));
			}

			_myTime = time;

		}

		/// <summary>
		/// Constructs a simtime object with the given initial time object. It represents a point in simulation
		/// time. Note that only points of future simulation time can be accessed and that trying to create a
		/// SimTime object with negative initial value will stop the simulation immediately.
		/// </summary>
		/// <param name="time">The time of this SimTime</param>
		public SimTime(SimTime time)
		{

			_myTime = time.getTimeValue();

		}

		/// <summary>
		/// Returns a new simtime object with a time value representing the sum of the time values of the given
		/// simtime objects.
		/// </summary>
		/// <param name="a">Simtime parameter a</param>
		/// <param name="b">Simtime parameter b</param>
		/// <returns>A new simtime as the sum of the simtime parameters</returns>
		public static SimTime add(SimTime a, SimTime b)
		{

			return new SimTime(a.getTimeValue() + b.getTimeValue());

		}

		/// <summary>
		/// Returns a new simtime object with a time value representing the absolute difference of the time values
		/// of the given simtime objects. Note that since no negative time values are allowed, <c>null</c> will be
		/// returned if the result of the subtraction would have resulted in a negative value. That is, a's time
		/// value is smaller than b's.
		/// </summary>
		/// <param name="a">Simtime parameter a of which parameter b will be subtracted</param>
		/// <param name="b">Simtime parameter b subtracted from parameter a</param>
		/// <returns>A new SimTime as the absolute difference of the SimTime parameters given</returns>
		public static SimTime diff(SimTime a, SimTime b)
		{

			if (a.getTimeValue() == b.getTimeValue())
				return new SimTime(0.0);

			double delta = a.getTimeValue() - b.getTimeValue();

			if (delta < 0)
				delta = -delta;

			if (delta < 0.000001)
				delta = 0;
			return new SimTime(delta);

		} // It's as simple as that

		/// <summary>Returns the value of the simtime object as type double.</summary>
		/// <returns>The value of the SimTime object as type double</returns>
		public double getTimeValue()
		{

			return _myTime; // not much else to do

		}

		/// <summary>
		/// Checks if the two simtime parameters describe the same point of simulation time. Note that this is a
		/// static method available through calling the class <c>SimTime</c> i.e. <c>SimTime.isEqual(a,b)</c> where
		/// a and b are valid simtime objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if both parameters describe same point of simulation time</returns>
		public static bool isEqual(SimTime a, SimTime b)
		{

			return (a.getTimeValue() == b.getTimeValue());

		}

		/// <summary>
		/// Checks if the first of two points of simulation time is larger than the second. Larger means, that the
		/// time value of simtime a is larger and hence "later" than simtime b. Note that this is a static method
		/// available through calling the class <c>SimTime</c> i.e. <c>SimTime.isLargerThan(a,b)</c> where a and b
		/// are valid simtime objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is larger (later) than b</returns>
		public static bool isLarger(SimTime a, SimTime b)
		{

			return (a.getTimeValue() > b.getTimeValue());

		}

		/// <summary>
		/// Checks if the first of two points of simulation time is larger or equal to the second. Larger means,
		/// that the time value of simtime a is larger and hence "later" than simtime b. Equal means, that they
		/// both describe the same point in simulation time. Note that this is a static method available through
		/// calling the class <c>SimTime</c> i.e. <c>SimTime.isLargerOrEqual(a,b)</c> where a and b are valid
		/// simtime objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is larger (later) or equal to b</returns>
		public static bool isLargerOrEqual(SimTime a, SimTime b)
		{

			return (a.getTimeValue() >= b.getTimeValue());

		}

		/// <summary>
		/// Checks if the two simtime parameters describe the different points of simulation time. Note that this
		/// is a static method available through calling the class <c>SimTime</c> i.e.
		/// <c>SimTime.isNotEqual(a,b)</c> where a and b are valid simtime objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if parameters describe different points of simulation time</returns>
		public static bool isNotEqual(SimTime a, SimTime b)
		{

			return (a.getTimeValue() != b.getTimeValue());

		}

		/// <summary>
		/// Checks if the first of two points of simulation time is smaller than the second. Smaller means, that
		/// the time value of SimTime a is larger and hence "sooner" than simtime b. Note that this is a static
		/// method available through calling the class <c>SimTime</c> i.e. <c>SimTime.isLargerThan(a,b)</c> where a
		/// and b are valid simtime objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is smaller (sooner) than b</returns>
		public static bool isSmaller(SimTime a, SimTime b)
		{

			return (a.getTimeValue() < b.getTimeValue());

		}

		/// <summary>
		/// Checks if the first of two points of simulation time is smaller or equal to the second. Smaller means,
		/// that the time value of simtime a is smaller and hence "sooner" than simtime b. Equal means, that they
		/// both describe the same point in simulation time. Note that this is a static method available through
		/// calling the class <c>SimTime</c> i.e. <c>SimTime.isSmallerOrEqual(a,b)</c> where a and b are valid
		/// simtime objects.
		/// </summary>
		/// <param name="a">first comparand</param>
		/// <param name="b">second comparand</param>
		/// <returns>True if a is smaller (sooner) or equal to b</returns>
		public static bool isSmallerOrEqual(SimTime a, SimTime b)
		{

			return (a.getTimeValue() <= b.getTimeValue());

		}

		/// <summary>
		/// Returns the special point of time to be used for replacing the current running process or schedule an
		/// entity at the first position in the event-list.
		/// </summary>
		/// <returns>
		/// The special simtime object used to indicate immediate scheduling, preempting a current SimProcess
		/// </returns>
		/// <remarks>sharpen: .ignore</remarks>
		public static SimTime now()
		{

			return NOW;

		}

		/// <summary>Prints the time value of this simtime with all floating point digits.</summary>
		/// <returns>The string representation of a simtime</returns>
		public string toString()
		{

			// convert time value to string

			//changed for sharpen:
			//String s = Double.toString(_myTime);
			string s = ((Double)_myTime).ToString();
			return s;

		}

		/// <summary>Prints the time value of this simtime with the given number of floating point digits.</summary>
		/// <param name="floats">The number of floating point digits to print</param>
		/// <returns>The string representation of a simtime</returns>
		public string toString(int floats)
		{

			// convert time value to String
			var s = _myTime.ToString();

			// is the value larger than 10E7, <- that format is used giving the
			// decimal
			// point quite a different meaning
			if (_myTime > 10E7)
				return s; // so return the whole number

			// get position of decimal point if available
			int decimalPoint = s.LastIndexOf(".");

			if (decimalPoint <= 0)
			{// no decimal point contained which can't really
			 // be
				return s; // print it
			}
			else
			{ // there is a decimal point at position decimalPoint
				if (floats == 0)
					return s.Substring(0, decimalPoint); // no decPoint wanted
				if ((floats + 1) >= (s.Length - decimalPoint))
				{ // no more than
				  // the floats
					return s;
				}
				else
					return s.Substring(0, decimalPoint + floats + 1); // truncate
																	  // at
																	  // decimal
			}

		}

		/// <summary>Converts the given TimeInstant object to a SimTime object.</summary>
		/// <param name="instant">The TimeInstant to be converted</param>
		/// <returns>The resulting SimTime from the conversion</returns>
		/// <remarks>author: Felix Klueckmann (07/09)</remarks>
		public static SimTime toSimTime(TimeInstant instant)
		{
			return new SimTime(instant.getTimeAsDouble());
		}

		/// <summary>Converts the given TimeSpan object to a SimTime object.</summary>
		/// <param name="span">The TimeSpan to be converted</param>
		/// <returns>The resulting SimTime from the conversion</returns>
		/// <remarks>author: Felix Klueckmann (07/09)</remarks>
		public static SimTime toSimTime(TimeSpan span)
		{
			return new SimTime(span.getTimeAsDouble());
		}

		/// <summary>
		/// Converts the given SimTime object to a TimeInstant object. Note that only SimTime objects with a time
		/// value smaller or equal than Long.MAX_VALUE can be converted and that trying to convert a simtime object
		/// with a time value that is bigger than Long.MAX_VALUE will stop the simulation immediately.
		/// </summary>
		/// <param name="simTime">The SimTime to be converted</param>
		/// <returns>The resulting TimeInstant from the conversion</returns>
		/// <remarks>author: Felix Klueckmann (07/09)</remarks>
		public static TimeInstant toTimeInstant(SimTime simTime)
		{
			double timeValue = simTime.getTimeValue();
			if (timeValue > long.MaxValue)
			{
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't convert SimTime to TimeInstant object! Simulation aborted.",
								"Class : SimTime  Methode : toTimeInstant(SimTime)",
								"the double value passed as a parameter is bigger than Long.MAX_VALUE : "
										+ simTime.getTimeValue(),
								"Can not create TimeInstant objects with a time Value bigger than Long.MAX_VALUE.",
								null)));
			}
			return new TimeInstant((long)(timeValue
					* TimeOperations.getEpsilon().Convert(1,
							TimeOperations.getReferenceUnit())),
					TimeOperations.getEpsilon());
		}

		/// <summary>
		/// Converts the given SimTime object to a TimeSpan object. Note that only SimTime objects with a time
		/// value smaller or equal than Long.MAX_VALUE can be converted and that trying to convert a simtime object
		/// with a time value that is bigger than Long.MAX_VALUE will stop the simulation immediately.
		/// </summary>
		/// <param name="simTime">The SimTime to be converted</param>
		/// <returns>The resulting TimeSpan from the conversion</returns>
		/// <remarks>author: Felix Klueckmann (07/09)</remarks>
		public static TimeSpan toTimeSpan(SimTime simTime)
		{
			if (simTime == SimTime.NOW)
			{
				return TimeSpan.ZERO;
			}
			double timeValue = simTime.getTimeValue();
			if (timeValue > long.MaxValue)
			{
				throw (new SimAbortedException(
						new ErrorMessage(
								null,
								"Can't convert SimTime to TimeSpan object! Simulation aborted.",
								"Class : SimTime  Methode : toTimeSpan(SimTime)",
								"the double value passed as a parameter is bigger than Long.MAX_VALUE : "
										+ simTime.getTimeValue(),
								"Can not create TimeSpan objects with a time Value bigger than Long.MAX_VALUE.",
								null)));
			}
			return new TimeSpan(timeValue);
		}
	}
}
