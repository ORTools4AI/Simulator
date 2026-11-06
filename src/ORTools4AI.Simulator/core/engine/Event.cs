// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;


namespace ORTools4AI.Simulator.core.engine
{

	/**
     * Provides the class for user defined events to change <b>a single</b> entity's
     * internal state. The state of a discrete model is changed by events that occur
     * at distinct points of simulation time.<p>
     * For events changing the state of <b>two</b> or <b>three</b> entities,
     * refer to <code>EventOf2Entities</code> and <code>EventOf3Entities</code>.
     * Events not associated to a specific entity are based on <code>ExternalEvent</code>.
     * <p>
     * For type safety it is recommended to generically assign the entity type an
     * Event operates on by using the generic type
     * <code>Event&lt;E&gt;</code> where
     * <code>E</code> is derived from <code>Entity</code>.
     * <p>
     * All event object should be used only once. Implement the changes of state for the 
     * specific entity associated with this event by overriding the abstract method 
     * <code>eventRoutine(E who)</code>.
     * 
     * @see Entity
     * @see ExternalEvent
     * @see EventOf2Entities
     * @see EventOf3Entities
     * @see TimeInstant
     * @see TimeSpan
     */
	public abstract  class Event : EventAbstract 
	{


		/// <summary>Creates a new event of the given model, with the given name and trace option.</summary>
		/// <param name="owner">The model this event is associated to</param>
		/// <param name="name">The name of this event</param>
		/// <param name="showInTrace">
		/// Flag for showing event in trace-files. Set it to <c>true</c> if event should show up in trace. Set it
		/// to <c>false</c> if event should not be shown in trace.
		/// </param>
		public Event(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
		{

			//super(owner, name, showInTrace);
			this.numberOfEntities = 1;

		}

		/// <summary>
		/// Implement this abstract method to express the changes of state this event does to a single entity. This
		/// event is related to the entity it has been scheduled with. That Entity is given note that since class
		/// <c>SimProcess</c> inherits from class <c>Entity</c>, an event can also be given a process to operate
		/// on. In this case, the process is scheduled and manipulated by this event just like an entity. Implement
		/// this abstract method to express the changes of state this event does to an entity (as <c>SimProcess</c>
		/// is a subclass of <c>Entity</c>, a process can be passed as well). For type safety, it is recommended to
		/// derive your events from the generic type Event&lt;EntityOperatingOn&gt; where <c>EntityOperatingOn</c>
		/// (which is derived from class <c>Entity</c>) represents the entity type your event is supposed to
		/// operate on. Should you decide to derive your event from the raw type <c>Event</c> (which is not
		/// recommended), please take extra care in checking the given Entity parameter to your special
		/// eventRoutine since any subtype of Entity will be accepted! If your model uses several different entity
		/// types, chances are that while developing the model, wrong entity types might be passed.
		/// </summary>
		/// <param name="who">The Entity associated to this event.</param>
		public abstract void eventRoutine(Entity who) ;

		/// <summary>
		/// Schedules this event to act on the given entity at a certain point in simulation time.
		/// </summary>
		/// <param name="who">The first entity to be manipulated by this event</param>
		/// <param name="instant">The point in simulation time this event is scheduled to happen.</param>
		public void schedule(Entity who, TimeInstant instant) {

			if ((instant == null)) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(Entity who, TimeInstant instant)",
						"The TimeInstant given as parameter is a null reference.",
						"Be sure to have a valid TimeInstant reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((who == null)) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(Entity who, TimeInstant instant)",
						"The Entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName()
						+ " Method: schedule(Entity wo, TimeInstant instant)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (!isModelCompatible(who)) {
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName()
						+ " Method: schedule(Entity who, TimeInstant instant)",
						"The Entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// generate trace
			this.generateTraceForScheduling(who, null, null, null, null, instant, null);

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(who, this, instant);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this event to act on the given Entity at the specified point in simulation time. The point of
		/// time is given as an offset to the current simulation time as displayed by the simulation clock.
		/// </summary>
		/// <param name="who">The Entity this event happens to</param>
		/// <param name="dt">The offset to the current simulation time this Event is to happen</param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(Entity who, TimeSpan dt) {
			if ((dt == null)) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(Entity who, TimeSpan dt)",
						"The TimeSpan given as parameter is a null reference.",
						"Be sure to have a valid TimeSpan reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((who == null)) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(Entity who, TimeSpan dt)",
						"The Entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: schedule(Entity wo, TimeSpan dt)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (!isModelCompatible(who)) {
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: schedule(Entity who, TimeSpan dt)",
						"The Entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// generate trace
			this.generateTraceForScheduling(who, null, null, null, null, TimeOperations.add(presentTime(), dt), null);

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(who, this, dt);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this event to act on the given Entity at a point in simulation time determined by a sample
		/// from the distribution provided to the method. The sample is interpreted as offset from the the present
		/// time in the reference time unit.
		/// </summary>
		/// <param name="who">The Entity this event happens to</param>
		/// <param name="dist">
		/// Numerical distribution to sample the offset to the current simulation time from
		/// </param>
		/// <remarks>See also SimClock</remarks>
		public void schedule<N>(Entity who, NumericalDist<N> dist) {

			if ((dist == null)) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(Entity who, NumericalDist<?> dist)",
						"The NumericalDist given as parameter is a null reference.",
						"Be sure to have a valid NumericalDist reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((who == null)) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(Entity who, NumericalDist<?> dist)",
						"The Entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: schedule(Entity wo, NumericalDist<?> dist)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (!isModelCompatible(who)) {
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: schedule(Entity who, NumericalDist<?> dist)",
						"The Entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// determine time span
			TimeSpan dt = dist.sampleTimeSpan();

			// generate trace
			this.generateTraceForScheduling(who, null, null, null, null, TimeOperations.add(presentTime(), dt), " Sampled from " + dist.getQuotedName() + ".");

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(who, this, dt);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <param name="who">The Entity this event happens to</param>
		/// <param name="dt">The offset to the current simulation time this event is to happen</param>
		/// <remarks>
		/// Replaced by schedule(E who,TimeSpan dt). Schedules this event to act on the given Entity at the
		/// specified point in simulation time. The point of time is given as an offset to the current simulation
		/// time as displayed by the simulation clock. See also SimClock
		/// </remarks>
		//@Deprecated
		public void schedule(Entity who, SimTime dt) {
			schedule(who, SimTime.toTimeSpan(dt));
		}

		public void schedule(Entity who) {
			if (who == null) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(Entity who)",
						"The Entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: schedule(Entity who)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple times.");
				return;
			}
			if (!isModelCompatible(who)) {
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName() + " Method: schedule(Entity who)",
						"The Entity to be scheduled with this event is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(who, null, null, null, null, presentTime(), null);
			getModel().getExperiment().getScheduler().schedule(who, this, TimeSpan.ZERO);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		public void schedulePreempt(Entity who) {
			if (who == null) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedulePreempt(Entity who)",
						"The Entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (!isModelCompatible(who)) {
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName() + " Method: schedulePreempt(Entity who)",
						"The Entity to be scheduled with this event is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(who, null, null, null, null, presentTime(), null);
			getModel().getExperiment().getScheduler().scheduleWithPreempt(who, this);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		/// <summary>
		/// Schedules this event to act on the given Entity directly after the given Schedulable is already set to
		/// be activated. Note that this event's point of simulation time will be set to be the same as the
		/// Schedulable's time. Thus this event will occur directly after the given Schedulable but the simulation
		/// clock will not change. Make sure that the Schedulable given as parameter is actually scheduled.
		/// </summary>
		/// <param name="after">The Schedulable this entity should be scheduled after</param>
		/// <param name="who">The Entity to be manipulated by this event</param>
		public void scheduleAfter(Schedulable after, Entity who) {

			if (who == null) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, "
						+ "Entity who)",
						"The Entity 'who' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (after == null) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, "
						+ "Entity who)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Event to be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, "
						+ "Entity who)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (who.isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Entity : "
						+ getName() + " Method: scheduleAfter(Schedulable after, "
						+ "Entity who)", "The Entity '" + who.getName()
						+ "'to be scheduled with this "
						+ "Event is already scheduled.",
						"Use method reSchedule(TimeSpan dt) to shift the entity "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (!after.isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, "
						+ "Entity who)", "The Schedulable '" + after.getName()
						+ "' given as a positioning "
						+ "reference has to be already scheduled but is not.",
						"Use method isScheduled() of any Schedulable to find out "
								+ "if it is already scheduled.");
				return; // was not scheduled
			}

			if (!isModelCompatible(who)) {
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: scheduleAfter(Schedulable after, "
						+ "Entity who)",
						"The Entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// generate trace
			this.generateTraceForScheduling(who, null, null, after, null, after.getEventNotes()
				[after.getEventNotes().Count - 1].getTime(), null);

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleAfter(after, who,
					this);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleAfter " + after.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this event to act on the given Entity directly before the given Schedulable is already set to
		/// be activated. Note that this event's point of simulation time will be set to be the same as the
		/// Schedulable's time. Thus this event will occur directly before the given Schedulable but the simulation
		/// clock will not change. Make sure that the Schedulable given as parameter is actually scheduled.
		/// </summary>
		/// <param name="before">The Schedulable this entity should be scheduled before</param>
		/// <param name="who">The Entity to be manipulated by this event</param>
		public void scheduleBefore(Schedulable before, Entity who) {

			if ((who == null)) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, "
						+ "Entity who)",
						"The Entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((before == null)) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, "
						+ "Entity who)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Event to be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, "
						+ "Entity who)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (who.isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, "
						+ "Entity who)", "The Entity '" + who.getName()
						+ "'to be scheduled with this "
						+ "Event is already scheduled.",
						"Use method reSchedule(TimeSpan dt) to shift the entity "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (!before.isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleBefore(Schedulable after, "
						+ "Entity who)", "The Schedulable '" + before.getName()
						+ "' given as a "
						+ "positioning reference has to be already scheduled but "
						+ "is not.",
						"Use method isScheduled() of any Schedulable to find out "
								+ "if it is already scheduled.");
				return; // was not scheduled
			}

			if (!isModelCompatible(who)) {
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName()
						+ " Method: scheduleBeforer(Schedulable before, "
						+ "Entity who)",
						"The Entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// generate trace
			this.generateTraceForScheduling(who, null, null, null, before, before.getEventNotes()[0].getTime(), null);

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleBefore(before, who,
					this);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleBefore " + before.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>Creates a copy of the event with an empty schedule.</summary>
		//@SuppressWarnings("unchecked")
		public Event clone() {
			return (Event)base.clone();
		}
	}
}