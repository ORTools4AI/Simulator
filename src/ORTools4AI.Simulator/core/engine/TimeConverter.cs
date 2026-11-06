// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Ruth Meyer
// Modified for ORTools4AI.Simulator.

using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * @deprecated Originally used for time conversion operations. Refer to 
     * TimeOperations.
     *             
     * TimeConverter is a class used by Experiment to convert sim time values to
     * true time values and vice versa. A true time value is a point in "real" time
     * specified as a String consisting of a date and a time, for example "26.3.2001
     * 13:25". In order to map the sim time values to true time values a reference
     * time and a time unit are needed. The class <code>TimeConverter</code>
     * implements the interface {@link desmoj.core.simulator.Units Units}to easily
     * use the time units and conversion factors defined there.
     * <p>
     * Every experiment has its own time converter and provides methods for time
     * conversion. The <code>TimeConverter</code> class is not intended for direct
     * use in model components because handling of (simulation) time should be done
     * in experiments only. Instead, if a model component needs to convert true time
     * values to sim time or vice versa it has to call the appropriate methods of
     * the class <code>Experiment</code>.
     */
	//@Deprecated
	class TimeConverter : Units {

		/**
		 * The default output format pattern for true time values as a constant.
		 * Specified according to the time format syntax used in the
		 * <code>java.text.SimpleDateFormat</code> class.
		 */
		static readonly string DEFAULT_PATTERN = "dd.MM.yyyy HH:mm:ss:SSS";

		/**
		 * The default date used for true time values is <tt>1.1.1970</tt>.
		 */
		static readonly string DEFAULT_DATE = "1.1.1970";

		/**
		 * The default time used for true time values is <tt>0:00:00</tt>.
		 */
		static readonly string DEFAULT_TIME = "00:00:00";

		/**
		 * The default time unit is <code>Units.MIN</tt>.
		 */
		static readonly int DEFAULT_UNIT = Units.MIN;

		/**
		 * The interval between reference time (= start time of simulation) and
		 * default time of class <code>java.util.Date</code>(= 1.1.1970 0:00:00)
		 * in milliseconds.
		 */
		private long _offset;

		/**
		 * The reference time unit as a constant defined in interface
		 * <code>Units</code>.
		 */
		private int _unit;

		/**
		 * The output formatter for true time values. Uses the default pattern if no
		 * other pattern is specified via call of the
		 * {@link #setTimeFormat(String) setTimeFormat}method.
		 */
		//private SimpleDateFormat _outFormat;

		/**
		 * The start time of the simulation as double (sim time). Default is 0.0.
		 */
		private double _startSimTime;

		/// <summary>Initializes a time converter with minutes as the reference unit.</summary>
		public TimeConverter() : this(DEFAULT_DATE + " " + DEFAULT_TIME, DEFAULT_UNIT) {

		}

		/// <summary>Initializes a time converter with the specified reference unit.</summary>
		/// <param name="referenceTime">A legacy reference-time argument. The converter does not parse it.</param>
		/// <param name="referenceUnit">The reference time unit.</param>
		public TimeConverter(string referenceTime, int referenceUnit)
		{
			// initialize instance variables
			this._unit = referenceUnit;
			//this._outFormat = new SimpleDateFormat(DEFAULT_PATTERN, Locale.GERMANY);
			//this._startSimTime = 0.0;
			//// parse parameter referenceTime
			//String completeRefTime = checkRefTime(referenceTime);
			//String pattern = constructPattern(completeRefTime);
			//SimpleDateFormat inFormat = new SimpleDateFormat(pattern,
			//		Locale.GERMANY);
			//Date ref = inFormat.parse(completeRefTime);
			//// calculate offset = millisecs since 1.1.1970 0:00:00
			//this._offset = ref.getTime();
		}

		/**
		 * Checks if the given reference time is complete. A complete true time
		 * value consists of &lt;date&gt;&lt;blank&gt;&lt;time&gt; with &lt;date&gt; =
		 * [d]d.[M]M.[yy]yy and &lt;time&gt; = [H]H[:[m]m[:[s]s[:[S][S]S]]]. If date
		 * or time is missing, the default values are used instead.
		 */
		private string checkRefTime(string refTime) {
			StringBuilder completeTime = new StringBuilder(17);
			if (refTime == null) {
				// neither date nor time --> use default values for both
				completeTime.Append(DEFAULT_DATE);
				completeTime.Append(" ");
				completeTime.Append(DEFAULT_TIME);
			} else {
				int spacePos = refTime.IndexOf(" ");
				if (spacePos < 0) {
					int pointPos = refTime.IndexOf(".");
					if (pointPos < 0) {
						// apparently time only --> use default date
						completeTime.Append(DEFAULT_DATE);
						completeTime.Append(" ");
						completeTime.Append(refTime);
					} else {
						// apparently date only --> use default time
						completeTime.Append(refTime);
						completeTime.Append(" ");
						completeTime.Append(DEFAULT_TIME);
					}
				} else {
					// both date and time exist --> use given refTime
					completeTime.Append(refTime);
				}
			}
			return completeTime.ToString();
		}

		// /////////////////// helper methods /////////////////////////////////////

		/// <summary>
		/// Constructs a pattern to be used for parsing the given timeString. This private helper method is called
		/// in any of the above methods when a true time parameter has to be parsed. The following assumptions are
		/// made: timeString consists of a date and a time separated by a blank ( &lt;date&gt; &lt;time&gt;)
		/// &lt;date&gt; has the format <c>[d]d.[M]M.[yy]yy</c> &lt;time&gt; has the format
		/// <c>[H]H[:[m]m[:[s]s[:[S][S]S]]]</c> Because the <c>SimpleDateFormat</c> class can use "minimal"
		/// patterns (for example: the pattern "d.M.yy" is okay for all dates consisting of 1 or 2 day digits, 1 or
		/// 2 month digits, and 2 or 4 year digits, separated by dots) the date format is no problem. But the time
		/// has to be checked on the number of elements (hours, minutes, seconds, milliseconds) used in order to
		/// construct the appropriate pattern.
		/// </summary>
		private string constructPattern(string timeString) {
			var pattern = new StringBuilder(14);
			pattern.Append("d.M.yy ");
			int spacePos = timeString.IndexOf(" ");
			if (spacePos < 0) {
				// no time specified --> return pattern "as is"
				return pattern.ToString();
			}
			// determine number of colons (:) in order to get number of time
			// elements
			int numberOfDelimiters = 0;
			int pos = timeString.IndexOf(":", spacePos);
			while (pos >= 0) {
				pos = timeString.IndexOf(":", pos + 1);
				numberOfDelimiters++;
			}
			// the time format depends on the number of elements
			// for each element use the minimal form
			switch (numberOfDelimiters) {
				case 3:
					pattern.Insert(7, ":S");
					break;
				case 2:
					pattern.Insert(7, ":s");
					break;
				case 1:
					pattern.Insert(7, ":m");
					break;
				case 0:
					pattern.Insert(7, "H");
					break;
			}
			return pattern.ToString();
		}

		/// <summary>
		/// Returns the reference time as as String formatted as defined in the default output format pattern. A
		/// different format may be specified via call of the <c>setTimeFormat(String)</c> method.
		/// </summary>
		/// <returns>
		/// the reference time which is the true time of the simulation start. It is mapped to simulation time 0.0
		/// if no other simulation start time is specified.
		/// </returns>
		public string getReferenceTime() {
			return (new DateTime(this._offset)).ToString();  // this._outFormat.format(new Date(this._offset));
		}

		/// <summary>Returns the reference time unit as a constant defined in the <c> Units</c> interface.</summary>
		/// <returns>the reference unit specifying what is meant by the simulation time step 1.0</returns>
		public int getReferenceUnit() {
			return this._unit;
		}

		/// <summary>Provides a legacy formatting reset hook. The current formatter has no pattern to reset.</summary>
		public void resetTimeFormat() {
			//this._outFormat.applyPattern(DEFAULT_PATTERN);
		}

		/// <summary>
		/// Sets the start time of the simulation to <c>initTime</c>. This method must be called if a start time
		/// other than 0.0 is used.
		/// </summary>
		/// <param name="initTime">the simulation start time as double (value of <c>SimTime</c>)</param>
		public void setStartSimTime(double initTime) {
			this._startSimTime = initTime;
		}

		/// <summary>Accepts a legacy formatting pattern. The current formatter ignores it.</summary>
		/// <param name="pattern">The requested formatting pattern.</param>
		public void setTimeFormat(string pattern) {
			//this._outFormat.applyPattern(pattern);
		}

		/// <summary>
		/// Converts the given duration with the given time unit into a sim time value. The duration is interpreted
		/// as the interval between sim time 0.0 and the calculated sim time value.
		/// </summary>
		/// <param name="duration">the duration value to be converted</param>
		/// <param name="unit">
		/// the time unit for the duration as a constant defined in the <c>Units</c>interface
		/// </param>
		/// <returns>SimTime the sim time representing the given duration</returns>
		public SimTime toSimTime(double duration, int unit) {
			// transform duration in sim time
			// - because SimTime doesn't distinguish between points in time and
			// durations of time, a duration is interpreted as the interval between
			// sim time 0.0 and the corresponding duration sim time.
			// - the given duration is in the given unit -> just has to be converted
			// to reference unit
			double simDuration = duration * unitFactors[unit]
					/ unitFactors[this._unit];
			return new SimTime(simDuration);
		}

		/// <summary>Computes a simulation time using the default date and the current reference settings.</summary>
		/// <param name="trueTime">A time string used to derive a format pattern. The string is not parsed.</param>
		public SimTime toSimTime(string trueTime) {
			// transform true time to sim time
			String pattern = constructPattern(trueTime);
			//SimpleDateFormat inFormat = new SimpleDateFormat(pattern,
			//		Locale.GERMANY);
			//DateTime trueDate = inFormat.parse(trueTime);
			// for this, subtract offset from trueDate (both are in milliseconds),
			// convert into reference unit and then add startSimTime (is in ref
			// unit)

			var trueDate = new DateTime();
			return new SimTime(((double)(trueDate.Minute - this._offset))
					* unitFactors[MS] / unitFactors[this._unit] + this._startSimTime);
		}

		/// <summary>Converts the simulation time to a date and time string using default .NET formatting.</summary>
		/// <param name="simTime">The simulation time to convert.</param>
		public string toTrueTime(SimTime simTime) {
			// transform sim time to true time
			// for this, subtract startSimTime from simTime (both are in reference
			// unit),
			// convert into milliseconds and then add offset (is in milliseconds)
			DateTime trueTime = new DateTime(
					(long)((simTime.getTimeValue() - _startSimTime)
							* unitFactors[_unit] / unitFactors[MS] + _offset));
			return trueTime.ToString();        //this._outFormat.format(trueTime); 
		}

		/// <summary>Converts the simulation time to a <see cref="DateTime"/>.</summary>
		/// <param name="simTime">The simulation time to convert.</param>
		public DateTime toTrueDate(SimTime simTime) {
			DateTime trueTime = new DateTime(
					(long)((simTime.getTimeValue() - this._startSimTime)
							* unitFactors[this._unit] / unitFactors[MS] + this._offset));
			return trueTime;
		}

		/// <summary>Converts the simulation time to a date and time string using default .NET formatting.</summary>
		/// <param name="simTime">The simulation time to convert.</param>
		/// <param name="pattern">A legacy formatting pattern. The current formatter ignores it.</param>
		public string toTrueTime(SimTime simTime, String pattern) {
			setTimeFormat(pattern);
			string trueTime = toTrueTime(simTime);
			resetTimeFormat();
			return trueTime;
		}
	}
}
