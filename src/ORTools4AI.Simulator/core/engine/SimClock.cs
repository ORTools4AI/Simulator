// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// @author modified by Soenke Claassen
// @author modified by Felix Klueckmann
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * The simulation clock shows the actual simulation time. The actual simulation
     * time can be polled by any object but can only be set by the scheduler
     * responsible for the actual model. The simulation clock is extending class
     * <code>java.util.Observable</code> thus representing the 'observable' part in
     * a 'observer'-design pattern as described in [Gamm95] page 107. This enables
     * observers to register themselves at the simulation clock to be notified
     * whenever the simulation time changes. This can be easily used to provide
     * fully automatic statistical counters. Each time the simulation time changes,
     * a counter registered at the simulation clock is notofied and can poll the
     * value it is observing (most likely from a special <code>ValueSupplier</code>
     * object). This way, no explicit calls for the counter to update its observed
     * value are needed. Note that on the other hand this might reduce performance
     * in comparison to explicit update call since the value under observation might
     * not change each time the simulation time is changed.
     * 
     * @see java.util.Observable
     */
	public class SimClock : Observable
	{

		/**
		 * The Simclock's Name
		 */
		String name;

		/**
		 * Stores internally the actual simulation time.
		 */
		private TimeInstant _timeNow;

		/// <summary>
		/// Constructs a simulation clock with no parameters given. By default the actual simulation time is set to
		/// zero.
		/// </summary>
		/// <param name="name">The name of the simulation clock</param>
		/// <remarks>author: Tim Lechler author: modified by Felix Klueckmann</remarks>
		public SimClock(string name)
		{

			this.name = name + "_clock";
			// set the simulation clock to 0
			_timeNow = new TimeInstant(0); // the birth of time ;-)

		}
		/// <summary>
		/// Returns the clock's name as string. This method has become necessary since the simulation clock does
		/// not extend class <c>NamedObjectImp</c>.
		/// </summary>
		/// <returns>The clock's name</returns>
		public string getName()
		{

			return name;

		}

		/// <summary>Returns the actual simulation time.</summary>
		/// <returns>The actual simulation time</returns>
		public TimeInstant getTime()
		{
			return _timeNow;
		}

		/// <summary>
		/// Sets the actual simulation time to a new value. This method has to be protected from user access since
		/// it must not be manipulated by anyone but the scheduler.
		/// </summary>
		/// <param name="newTime">The new simulation time</param>
		public void setTime(TimeInstant newTime)
		{
			//check if newTime is in the future
			if (TimeInstant.isBeforeOrEqual(newTime, _timeNow))
			{
				// check for legal parameter (newTime>oldTime)
				if (TimeInstant.isBefore(newTime, _timeNow))
				{
					//TODO Exception (Wrong Time)
				}
				return;
			}


			// note all observers of change before setting the new time!!!!
			setChanged(); // set the status to changed

			// tell every Observer registered the actual TimeInstant which will be
			// changed now
			notifyObservers(_timeNow);

			_timeNow = newTime; // now make the move for the next time change.
		}

		/// <summary>
		/// Sets the initial simulation time, overriding potential previous calls to this method. Allows negative
		/// values. This method has to be protected from user access since it must not be manipulated by anyone but
		/// the scheduler.
		/// </summary>
		/// <param name="initTime">The initial simulation time</param>
		public void setInitTime(TimeInstant initTime)
		{
			_timeNow = initTime;
		}

		/// <summary>
		/// Returns the clock's name as string. This method has become necessary since the simulation clock does
		/// not extend class <c>NamedObjectImp</c>.
		/// </summary>
		/// <returns>The clock's name</returns>
		public string toString()
		{

			return name;

		}
	}
}