// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Felix Klueckmann
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * Provides the basic frame for user defined events. Derive from this class to
     * design special real time external events for a model. To use real time
     * external events, always create a new object of this class.
     */

	public class RealTimeEventWrapper
	{

		/**
		 * A nanosecond timestamp that will be associated to the encapsulated ExternalEvent.
		 * 
		 */
		private long _nanos;

		/**
		 * The ExternalEvent encapsulated by this RealTimEventWrapper
		 * 
		 */
		private ExternalEvent _myExternalEvent;

		/**
		 * Creates an real time wrapper for an external event. This constructor will
		 * set the nanosecond timestamp to the current value of System.nanoTime().
		 * 
		 */
		public RealTimeEventWrapper(ExternalEvent externalEvent)
		{
			this._myExternalEvent = externalEvent;
			this.setNanos((long)(System.Diagnostics.Stopwatch.GetTimestamp()
				* (1_000_000_000.0 / System.Diagnostics.Stopwatch.Frequency)));
		}

		/**
		 * Creates an real time wrapper for an external event.
		 * 
		 */
		public RealTimeEventWrapper(ExternalEvent externalEvent, long nanoTimeStamp)
		{
			this._myExternalEvent = externalEvent;
			this._nanos = nanoTimeStamp;
		}

		/**
		 * Returns the encapsulated ExternalEvent
		 * 
		 */
		public ExternalEvent getExternalEvent()
		{
			return _myExternalEvent;
		}

		/**
		 * Schedules the external event to happen at the simulation time equivalent
		 * to the nanosecond timestamp of this RealTimeEventWrapper.
		 * 
		 */
		public void realTimeSchedule()
		{
			_myExternalEvent.getModel().getExperiment().getScheduler()
					.realTimeSchedule(this);

		}

		/**
		 * Sets the nanosecond timestamp of this RealTimeEventWrapper. Use
		 * method System.nanoTime() to get a time stamp.
		 */
		public void setNanos(long nanos)
		{
			this._nanos = nanos;
		}

		/**
		 * Returns the nanosecond timestamp associated with the encapsulated ExternalEvent.	 *
		 */
		public long getNanos()
		{
			return _nanos;
		}

	}
}
