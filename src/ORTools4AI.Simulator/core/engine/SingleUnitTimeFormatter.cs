// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Felix Klueckmann
// Modified for ORTools4AI.Simulator.

using System.Text;

namespace ORTools4AI.Simulator.core.engine
{



	/**
     * This is the default Time Formatter. Use this class to format TimeSpan and
     * TimeInstant objects like in the following examples: 5.1 , 5.1 SECONDS,
     * 12.035, 12.035 MINUTES.
     */
	/**
     * @author goebel
     *
     */
	public class SingleUnitTimeFormatter : TimeFormatter
	{

		private static Dictionary<TimeUnit, Dictionary<TimeUnit, long>> timeConstants
			= new Dictionary<TimeUnit, Dictionary<TimeUnit, long>>()
			{
				{ 
					TimeUnit.DAYS,new Dictionary<TimeUnit, long>()
								{
									{TimeUnit.HOURS, 24 },
									{TimeUnit.MINUTES, 1440 },
									{ TimeUnit.SECONDS, 86400 },
									{ TimeUnit.MILLISECONDS, 86400000 },
									{ TimeUnit.MICROSECONDS, 86400000000 },
									{TimeUnit.NANOSECONDS, 86400000000000 }
                                }
                },
				{
					TimeUnit.HOURS,new Dictionary<TimeUnit, long>()
					{
                        {TimeUnit.MINUTES, 60 },
						{TimeUnit.SECONDS, 3600 },
						{TimeUnit.MILLISECONDS, 3600000},
						{ TimeUnit.MICROSECONDS, 3600000000 },
						{ TimeUnit.NANOSECONDS, 3600000000000 }
                    }
                },
				{
					TimeUnit.MINUTES,new Dictionary<TimeUnit, long>()
					{
						{TimeUnit.SECONDS, 60 },
						{ TimeUnit.MILLISECONDS, 60000 },
						{ TimeUnit.MICROSECONDS, 60000000 },
						{ TimeUnit.NANOSECONDS, 60000000000 }
						}
                },
				{
					TimeUnit.SECONDS,new Dictionary<TimeUnit, long>()
					{
                        {TimeUnit.MILLISECONDS, 1000 },
						{ TimeUnit.MICROSECONDS, 1000000 },
						{ TimeUnit.NANOSECONDS, 1000000000 }
                    }
                },
				{
					TimeUnit.MILLISECONDS,new Dictionary<TimeUnit, long>()
					{
						{TimeUnit.MICROSECONDS, 1000 },
						{ TimeUnit.NANOSECONDS, 1000000 }
					}
                },
				{
					TimeUnit.MICROSECONDS,new Dictionary<TimeUnit, long>()
					{
						{TimeUnit.NANOSECONDS, 1000 }
					}
                }
			};

		 

		/**
		 * The TimeUnit that is used in this TimeFormatter
		 */
		private readonly TimeUnit _myTimeUnit;
		/**
		 * the value of epsilon used in this TimeFormatter
		 */
		private readonly TimeUnit _epsilon;
		/**
		 * A Factor used for unit conversion
		 */
		private readonly long _myFactor;
		/**
		 * The number of digits after the decimal point which will be displayed for
		 * the time values.
		 */
		protected readonly long _floats;

		/**
		 * 
		 */
		private readonly long _precisionFactor;

		/**
		 * Flag that indicates if the time unit will be included in the time String.
		 * 
		 */
		private readonly bool _writeUnit;

		/// <summary>Constructs a DecimalTimeFormatter.</summary>
		/// <param name="unit">The time values that will be used</param>
		/// <param name="floats">The number of floating point digits to print</param>
		/// <param name="writeUnit">Indicates if the time unit will be included in the time String.</param>
		public SingleUnitTimeFormatter(TimeUnit unit, TimeUnit epsilon, int floats, bool writeUnit)
		{
			this._myTimeUnit = unit;
			this._epsilon = epsilon;
			this._floats = floats;
			this._precisionFactor = (long)Math.Pow(10, floats);
			this._writeUnit = writeUnit;
			this._myFactor = epsilon.Convert(1, _myTimeUnit);


			//var dayConstants = new Dictionary<TimeUnit, long>();
			
			//dayConstants.Add(TimeUnit.HOURS, 24);
			//dayConstants.Add(TimeUnit.MINUTES, 1440);
			//dayConstants.Add(TimeUnit.SECONDS, 86400);
			//dayConstants.Add(TimeUnit.MILLISECONDS, 86400000);
			//dayConstants.Add(TimeUnit.MICROSECONDS, 86400000000);
			//dayConstants.Add(TimeUnit.NANOSECONDS, 86400000000000);

			//var hourConstants = new Dictionary<TimeUnit, long>();
			//hourConstants.Add(TimeUnit.MINUTES, 60l);
			//hourConstants.Add(TimeUnit.SECONDS, 3600l);
			//hourConstants.Add(TimeUnit.MILLISECONDS, 3600000l);
			//hourConstants.Add(TimeUnit.MICROSECONDS, 3600000000l);
			//hourConstants.Add(TimeUnit.NANOSECONDS, 3600000000000l);
			//var minuteConstants = new Dictionary<TimeUnit, long>();
			//minuteConstants.Add(TimeUnit.SECONDS, 60l);
			//minuteConstants.Add(TimeUnit.MILLISECONDS, 60000l);
			//minuteConstants.Add(TimeUnit.MICROSECONDS, 60000000l);
			//minuteConstants.Add(TimeUnit.NANOSECONDS, 60000000000l);

			//var secondConstants = new Dictionary<TimeUnit, long>();
			//secondConstants.Add(TimeUnit.MILLISECONDS, 1000l);
			//secondConstants.Add(TimeUnit.MICROSECONDS, 1000000l);
			//secondConstants.Add(TimeUnit.NANOSECONDS, 1000000000l);
			//var milliConstants = new Dictionary<TimeUnit, long>();
			//milliConstants.Add(TimeUnit.MICROSECONDS, 1000l);
			//milliConstants.Add(TimeUnit.NANOSECONDS, 1000000l);

			//var microConstants = new Dictionary<TimeUnit, long>();
			//microConstants.Add(TimeUnit.NANOSECONDS, 1000l);

			//timeConstants.Add(TimeUnit.DAYS, dayConstants);
			//timeConstants.Add(TimeUnit.HOURS, hourConstants);
			//timeConstants.Add(TimeUnit.MINUTES, minuteConstants);
			//timeConstants.Add(TimeUnit.SECONDS, secondConstants);
			//timeConstants.Add(TimeUnit.MILLISECONDS, milliConstants);
			//timeConstants.Add(TimeUnit.MICROSECONDS, microConstants);


		}

		/**Returns the String-Representation of the given TimeInstant
		 *  
		 */
		public String buildTimeString(TimeInstant instant)
		{
			return buildSingleUnitTimeString(instant.getTimeInEpsilon());
		}

		/**Returns the String-Representation of the given TimeSpan
		 *  
		 */
		public String buildTimeString(TimeSpan span)
		{
			return buildSingleUnitTimeString(span.getTimeInEpsilon());
		}

		/**Returns the String-Representation of the given time object.
		 *	 
		 */
		private string buildSingleUnitTimeString(long timeValue)
		{
			var timeStringBuffer = new StringBuilder();

			if (_myTimeUnit.CompareTo(_epsilon) > 0)
			{
				// unit is a coarser granularity than epsilon
				timeStringBuffer.Append('.');
				//append seperator
				long fractionTime = _myTimeUnit.Convert((timeValue
						% _myFactor) * _precisionFactor,
						_epsilon);

				String fractionTimeString = fractionTime.ToString();
				char zero = '0';
				//append as many zeros as needed
				for (int i = fractionTimeString.Length; i < _floats; i++)
				{
					timeStringBuffer.Append(zero);
				}

				timeStringBuffer.Append(fractionTimeString);

			}
			timeStringBuffer.Insert(0, _myTimeUnit.Convert(timeValue, _epsilon));
			if (_writeUnit)
			{
				//append the name of the time unit
				timeStringBuffer.Append(' ');
				timeStringBuffer.Append(_myTimeUnit.ToString());
			}
			return timeStringBuffer.ToString();
		}

		/* (non-Javadoc)
		 * @see desmoj.core.simulator.TimeFormatter#usesOnlySingleUnit()
		 */
		public string getUnit()
		{
			return _myTimeUnit.ToString().ToLower();
		}
	}
}
