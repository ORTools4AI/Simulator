// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using System.Xml.Linq;

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * Provides the class for user defined events to change the model's
     * state at a distinct point in simulation time.<p>
     * For events specifically changing the state of up to three entities, refer
     * to <code>Event</code>, <code>EventOf2Entities</code> and <code>EventOf3Entities</code>.
     * Events not associated to a specific entity are called external events as they
     * are are typically used for external influences, e.g. arrivals from outside or incidents.
     * 
     * 
     * Derive from this class to
     * design special external events for a model. To use external events, always
     * create a new object of this class.
     * 
     * @see Event
     * @see EventOf2Entities
     * @see EventOf3Entities
     * @see TimeInstant
     * @see TimeSpan
     */
	public abstract class ExternalEvent : EventAbstract
	{

		/// <summary>
		/// Creates an external event for the given model with the given name and the specified tracemode.
		/// </summary>
		/// <param name="owner">The external event's model</param>
		/// <param name="name">The external event's name</param>
		/// <param name="showInTrace">Flag for showing this external event in tracemessages</param>
		public ExternalEvent(Model owner, String name, bool showInTrace) : base(owner, name, showInTrace) {

			//super(owner, name, showInTrace);
			this.numberOfEntities = 0;

		}

		/// <summary>
		/// Implement this method to express the semantics of this external event. External events are supposed to
		/// act on the model or experiment in general. They are not related to a special Entity (unlike method
		/// <c>eventRoutine(Entity who)</c> of class <c>Event</c>). Override this method to implement this
		/// Externalevent's behaviour.
		/// </summary>
		public abstract void eventRoutine();

		/// <summary>
		/// Schedules this external event to make the desired changes to the experiment or model at the current
		/// point of time plus the given span of time
		/// </summary>
		/// <param name="dt">
		/// The offset to the current simulation time at which this external event is to be scheduled
		/// </param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(TimeSpan dt) {
			if ((dt == null)) {
				sendWarning("Can't schedule external event!", "ExternalEvent : "
						+ getName() + " Method: schedule(Entity who, TimeSpan dt)",
						"The simulation time given as parameter is a null "
								+ "reference.",
						"Be sure to have a valid TimeSpan reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: schedule(Entity wo, TimeSpan dt)",
						"The external event to be scheduled is already scheduled.",
						"Use external events only once, do not reuse them "
								+ "multiple times.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(null, null, null, null, null, TimeOperations.add(presentTime(), dt), null);

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(null, this, dt);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}
		}

		/// <summary>
		/// Schedules this external event to make the desired changes to the experiment or model. The time instant
		/// for which the event is scheduled is determined by a sample from the distribution provided to the
		/// method. The sample is interpreted as offset from the the present time in the reference time unit.
		/// </summary>
		/// <param name="dist">
		/// Numerical distribution to sample the offset to the current simulation time from
		/// </param>
		/// <remarks>See also SimClock</remarks>
		public void schedule<N>(NumericalDist<N> dist) {

			if ((dist == null)) {
				sendWarning("Can't schedule external event!", "ExternalEvent : "
						+ getName() + " Method: schedule(Entity who, NumericalDist<?> dist)",
						"The NumericalDist given as parameter is a null "
								+ "reference.",
						"Be sure to have a valid NumericalDist reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: schedule(Entity wo, NumericalDist<?> dist)",
						"The external event to be scheduled is already scheduled.",
						"Use external events only once, do not reuse them "
								+ "multiple times.");
				return; // was already scheduled
			}

			// determine time span
			TimeSpan dt = dist.sampleTimeSpan();

			// generate trace
			this.generateTraceForScheduling(null, null, null, null, null, TimeOperations.add(presentTime(), dt), " Sampled from " + dist.getQuotedName() + ".");

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(null, this, dt);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}
		}

		/// <summary>
		/// Schedules this external event to make the desired changes to the experiment or model at the specified
		/// point in simulation time.
		/// </summary>
		/// <param name="when">The point in simulation time this external event is scheduled to happen.</param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(TimeInstant when) {
			if ((when == null)) {
				sendWarning("Can't schedule external event!", "ExternalEvent : "
						+ getName()
						+ " Method: schedule(Entity who, TimeInstant when)",
						"The point of simulation time given as parameter is a null "
								+ "reference.",
						"Be sure to have a valid TimeInstant reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: schedule(Entity wo, TimeInstant when)",
						"The external event to be scheduled is already scheduled.",
						"Use external events only once, do not reuse them "
								+ "multiple times.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(null, null, null, null, null, when, null);

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(null, this, when);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}
		}

		/// <param name="dt">The offset to the current simulation time this event is to happen</param>
		/// <remarks>
		/// Replaced by schedule(TimeSpan dt).Schedules this external Event to make the desired changes to the
		/// experiment or model at the specified point in simulation time. The point of time is given as an offset
		/// to the current simulation time as displayed by the simclock. See also SimClock
		/// </remarks>
		//@Deprecated
		public void schedule(SimTime dt) {
			schedule(SimTime.toTimeSpan(dt));
		}

		/// <summary>
		/// Schedules this external event to act on the experiment or model state directly after the given
		/// Schedulable is already set to be activated. Note that this external event's point of simulation time
		/// will be set to be the same as the Schedulable's time. Thus this external event will occur directly
		/// after the given Schedulable but the simulation clock will not change. Make sure that the Schedulable
		/// given as parameter is actually scheduled.
		/// </summary>
		/// <param name="after">The Schedulable this external event should be scheduled after</param>
		public void scheduleAfter(Schedulable after) {

			if ((after == null)) {
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleAfter(Schedulable after, "
								+ "Entity who)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "external event to be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleAfter(Schedulable after)",
						"The external event to be scheduled is already scheduled.",
						"Use method external events only once, do not use them "
								+ "multiple times.");
				return; // was already scheduled
			}

			if (!after.isScheduled()) {
				sendWarning(
						"Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleAfter(Schedulable after)",
						"The Schedulable '"
								+ after.getName()
								+ "' given as a positioning "
								+ "reference has to be already scheduled but is not.",
						"Use method isScheduled() of any Schedulable to find out "
								+ "if it is already scheduled.");
				return; // was not scheduled
			}

			// generate trace
			this.generateTraceForScheduling(null, null, null, after, null, 
				after.getEventNotes()[after.getEventNotes().Count - 1].getTime(), null);

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleAfter(after, null,
					this);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleAfter " + after.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this external event to act on the experiment or model state directly before the given
		/// Schedulable is already set to be activated. Note that this external event's point of simulation time
		/// will be set to be the same as the Schedulable's time. Thus this external event will occur directly
		/// before the given Schedulable but the simulation clock will not change. Make sure that the Schedulable
		/// given as parameter is actually scheduled.
		/// </summary>
		/// <param name="before">The Schedulable this external event should be scheduled before</param>
		public void scheduleBefore(Schedulable before) {

			if ((before == null)) {
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleBefore(Schedulable before, "
								+ "Entity who)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "external event to be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleBefore(Schedulable before)",
						"The external event to be scheduled is already scheduled.",
						"Use method external events only once, do not use them "
								+ "multiple times.");
				return; // was already scheduled
			}

			if (!before.isScheduled()) {
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleBefore(Schedulable before)",
						"The Schedulable '" + before.getName() + "' given as a "
								+ "positioning reference has to be already "
								+ "scheduled but is not.",
						"Use method isScheduled() of any Schedulable to find out "
								+ "if it is already scheduled.");
				return; // was not scheduled
			}

			// generate trace
			this.generateTraceForScheduling(null, null, null, null, before, before.getEventNotes()[0].getTime(), null);

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleBefore(before, null,
					this);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleBefore " + before.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/**
		 * Creates and returns a copy of this event.
		 * Note that subclasses have to implement the interface 
		 * </code>java.lang.Cloneable</code> to actually use this method as 
		 * otherwise, a </code>CloneNotSupportedException</code> will be thrown.
		 * 
		 * @return ExternalEvent : A copy of this event.
		 */
		//protected ExternalEvent clone() throws CloneNotSupportedException {
		//    return (ExternalEvent) super.clone();
		//}   
	}
}