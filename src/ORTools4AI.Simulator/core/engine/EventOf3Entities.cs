// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// @author modified by Justin Neumann
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * Provides the class for user defined events to change <b>three</b> entities'
     * internal states. The state of a discrete model is changed by events that occur
     * at distinct points of simulation time.<p>
     * For events changing the state of <b>one</b> or <b>two</b> entities,
     * refer to <code>Event</code> and <code>EventOf2Entities</code>.
     * Events not associated to a specific entity are based on <code>ExternalEvent</code>.
     * <p>
     * For type safety it is recommended to generically assign the entity types an
     * <code>EventOf3Entities</code> operates on by using the generic type
     * <code>EventOf3Entities&lt;E,F,G&gt;</code> where
     * <code>E</code>, <code>F</code> and <code>G</code> are derived from <code>Entity</code>.
     * <p>
     * All event object should be used only once. Implement the changes of state for the 
     * specific entities associated with this event by overriding the abstract method 
     * <code>eventRoutine(E who1, F who2, G who3)</code>.
     * 
     * @see Entity
     * @see ExternalEvent
     * @see Event
     * @see EventOf2Entities
     * @see TimeInstant
     * @see TimeSpan
     */
	public abstract class EventOf3Entities : EventAbstract
	{

		/// <summary>Creates a new Event of the given model, with the given name and trace option.</summary>
		/// <param name="owner">The model this event is associated to</param>
		/// <param name="name">The name of this event</param>
		/// <param name="showInTrace">
		/// Flag for showing Event in trace-files. Set it to <c>true</c> if Event should show up in trace. Set it
		/// to <c>false</c> if Event should not be shown in trace.
		/// </param>
		public EventOf3Entities(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
		{

			//super(owner, name, showInTrace);
			this.numberOfEntities = 3;

		}

		/// <summary>
		/// Implement this abstract method to express the changes of state this event does to three entities (as
		/// <c>SimProcess</c> is a subclass of <c>Entity</c>, processes can be passed as well). For type safety, it
		/// is recommended to derive your events from the generic type
		/// EventOf3Entities&lt;EntityType1,EntityType2,EntityType3&gt; where <c>EntityType1</c>,
		/// <c>EntityType2</c> and <c>EntityType3</c> (which are derived from class <c>Entity</c>) represent the
		/// entity types your event is supposed to operate on. Should you decide to derive your event from the raw
		/// type <c>EventOf3Entities</c> (which is not recommended), please take extra care in checking the given
		/// Entity parameters to your special eventRoutine since any subtype of Entity will be accepted! If your
		/// model uses several different entity types, chances are that while developing the model, wrong entity
		/// types might be passed.
		/// </summary>
		/// <param name="who1">The first entity associated to this event.</param>
		/// <param name="who2">The second entity associated to this event.</param>
		/// <param name="who3">The second entity associated to this event.</param>
		public abstract void eventRoutine(Entity who1, Entity who2, Entity who3);

		/// <summary>
		/// Schedules this event to act on the given entities at a certain point in simulation time.
		/// </summary>
		/// <param name="who1">The first entity to be manipulated by this event</param>
		/// <param name="who2">The second entity to be manipulated by this event</param>
		/// <param name="who3">The third entity to be manipulated by this event</param>
		/// <param name="instant">The point in simulation time this event is scheduled to happen.</param>
		public void schedule(Entity who1, Entity who2, Entity who3, TimeInstant instant)
		{

			if ((instant == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: eventRoutine(E who1, F who2, G who3)",
						"The TimeInstant given as parameter is a null reference.",
						"Be sure to have a valid TimeInstant reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((who1 == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: eventRoutine(E who1, F who2, G who3)",
						"The first entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if ((who2 == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: eventRoutine(E who1, F who2, G who3)",
						"The second entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if ((who3 == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: eventRoutine(E who1, F who2, G who3)",
						"The third entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName()
						+ " Method: eventRoutine(E who1, F who2, G who3)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (!isModelCompatible(who1))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName()
						+ " Method: eventRoutine(E who1, F who2, G who3)",
						"The first entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who2))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName()
						+ " Method: eventRoutine(E who1, F who2, G who3)",
						"The second entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who3))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName()
						+ " Method: eventRoutine(E who1, F who2, G who3)",
						"The third entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// generate trace
			this.generateTraceForScheduling(who1, who2, who3, null, null, instant, null);

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(who1, who2, who3, this, instant);
			if (currentlySendDebugNotes())
			{
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this event to act on the given entities at the specified point in simulation time. The point
		/// of time is given as an offset to the current simulation time as displayed by the simulation clock.
		/// </summary>
		/// <param name="who1">The first entity this event happens to</param>
		/// <param name="who2">The second entity this event happens to</param>
		/// <param name="who3">The third entity this event happens to</param>
		/// <param name="dt">The offset to the current simulation time this Event is to happen</param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(Entity who1, Entity who2, Entity who3, TimeSpan dt)
		{
			if ((dt == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3, TimeSpan dt)",
						"The TimeSpan given as parameter is a null reference.",
						"Be sure to have a valid TimeSpan reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((who1 == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3, TimeSpan dt)",
						"The first entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if ((who2 == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3, TimeSpan dt)",
						"The second entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if ((who3 == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3, TimeSpan dt)",
						"The third entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: schedule(E who1, F who2, G who3, TimeSpan dt)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (!isModelCompatible(who1))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: schedule(E who1, F who2, G who3, TimeSpan dt)",
						"The first entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who2))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: schedule(E who1, F who2, G who3, TimeSpan dt)",
						"The second entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who3))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: schedule(E who1, F who2, G who3, TimeSpan dt)",
						"The third entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// generate trace
			this.generateTraceForScheduling(who1, who2, who3, null, null, TimeOperations.add(presentTime(), dt), null);

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(who1, who2, who3, this, dt);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this event to act on the given entities at the specified point in simulation time determined
		/// by a sample from the distribution provided to the method. The sample is interpreted as offset from the
		/// the present time in the reference time unit.
		/// </summary>
		/// <param name="who1">The first entity this event happens to</param>
		/// <param name="who2">The second entity this event happens to</param>
		/// <param name="who3">The third entity this event happens to</param>
		/// <param name="dist">
		/// Numerical distribution to sample the offset to the current simulation time from
		/// </param>
		/// <remarks>See also SimClock</remarks>
		public void schedule<N>(Entity who1, Entity who2, Entity who3, NumericalDist<N> dist)
		{

			if ((dist == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3, NumericalDist<?> dist)",
						"The NumericalDist given as parameter is a null reference.",
						"Be sure to have a valid NumericalDist reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((who1 == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3, NumericalDist<?> dist)",
						"The first entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if ((who2 == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3, NumericalDist<?> dist)",
						"The second entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if ((who3 == null))
			{
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3, NumericalDist<?> dist)",
						"The third entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to "
								+ "be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: schedule(E who1, F who2, G who3, NumericalDist<?> dist)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (!isModelCompatible(who1))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: schedule(E who1, F who2, G who3, NumericalDist<?> dist)",
						"The first entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who2))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: schedule(E who1, F who2, G who3, NumericalDist<?> dist)",
						"The second entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who3))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: schedule(E who1, F who2, G who3, NumericalDist<?> dist)",
						"The third entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// determine time span
			TimeSpan dt = dist.sampleTimeSpan();

			// generate trace
			this.generateTraceForScheduling(who1, who2, who3, null, null, TimeOperations.add(presentTime(), dt), " Sampled from " + dist.getQuotedName() + ".");

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(who1, who2, who3, this, dt);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		public void schedule(Entity who1, Entity who2, Entity who3) {
			if (who1 == null) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3)",
						"The first entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (who2 == null) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3)",
						"The second entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (who3 == null) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedule(E who1, F who2, G who3)",
						"The third entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (isScheduled()) {
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: schedule(E who1, F who2, G who3)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple times.");
				return;
			}
			if (!isModelCompatible(who1)) {
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName() + " Method: schedule(E who1, F who2, G who3)",
						"The first entity to be scheduled with this event is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			if (!isModelCompatible(who2)) {
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName() + " Method: schedule(E who1, F who2, G who3)",
						"The second entity to be scheduled with this event is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			if (!isModelCompatible(who3)) {
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName() + " Method: schedule(E who1, F who2, G who3)",
						"The third entity to be scheduled with this event is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(who1, who2, who3, null, null, presentTime(), null);
			getModel().getExperiment().getScheduler().schedule(who1, who2, who3, this, TimeSpan.ZERO);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		public void schedulePreempt(Entity who1, Entity who2, Entity who3) {
			if (who1 == null) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedulePreempt(E who1, F who2, G who3)",
						"The first entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (who2 == null) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedulePreempt(E who1, F who2, G who3)",
						"The second entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (who3 == null) {
				sendWarning("Can't schedule Event!", "Event : " + getName()
						+ " Method: schedulePreempt(E who1, F who2, G who3)",
						"The third entity given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (!isModelCompatible(who1)) {
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName() + " Method: schedulePreempt(E who1, F who2, G who3)",
						"The first entity to be scheduled with this event is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			if (!isModelCompatible(who2)) {
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName() + " Method: schedulePreempt(E who1, F who2, G who3)",
						"The second entity to be scheduled with this event is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			if (!isModelCompatible(who3)) {
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName() + " Method: schedulePreempt(E who1, F who2, G who3)",
						"The third entity to be scheduled with this event is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(who1, who2, who3, null, null, presentTime(), null);
			getModel().getExperiment().getScheduler().scheduleWithPreempt(who1, who2, who3, this);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		/// <summary>
		/// Schedules this event to act on the given Entities directly after the given Schedulable is already set
		/// to be activated. Note that this event's point of simulation time will be set to be the same as the
		/// Schedulable's time. Thus this event will occur directly after the given Schedulable but the simulation
		/// clock will not change. Make sure that the Schedulable given as parameter is actually scheduled.
		/// </summary>
		/// <param name="after">The Schedulable this entity should be scheduled after</param>
		/// <param name="who1">The first entity to be manipulated by this event</param>
		/// <param name="who2">The second entity to be manipulated by this event</param>
		/// <param name="who3">The third entity to be manipulated by this event</param>
		public void scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3)
		{

			if (who1 == null)
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, E who1, F who2, G who3)",
						"The Entity 'who1' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (who2 == null)
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, E who1, F who2, G who3)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (who3 == null)
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, E who1, F who2, G who3)",
						"The Entity 'who3' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (after == null)
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, E who1, F who2, G who3)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Event to be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, E who1, F who2, G who3)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (!after.isScheduled())
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleAfter(Schedulable after, E who1, F who2, G who3)",
						"The Schedulable '" + after.getName()
						+ "' given as a positioning "
						+ "reference has to be already scheduled but is not.",
						"Use method isScheduled() of any Schedulable to find out "
								+ "if it is already scheduled.");
				return; // was not scheduled
			}

			if (!isModelCompatible(who1))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: scheduleAfter(Schedulable after, E who1, F who2, G who3)",
						"The first Entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who2))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: scheduleAfter(Schedulable after, E who1, F who2, G who3)",
						"The second entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who3))
			{
				sendWarning("Can't schedule Event! Command ignored", "Entity : "
						+ getName() + " Method: scheduleAfter(Schedulable after, E who1, F who2, G who3)",
						"The third entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// generate trace
			this.generateTraceForScheduling(who1, who2, who3, after, null, 
				after.getEventNotes()[after.getEventNotes().Count - 1].getTime(), null);

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleAfter(after, who1, who2, who3,
					this);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("scheduleAfter " + after.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this event to act on the given Entities directly before the given Schedulable is already set
		/// to be activated. Note that this event's point of simulation time will be set to be the same as the
		/// Schedulable's time. Thus this event will occur directly before the given Schedulable but the simulation
		/// clock will not change. Make sure that the Schedulable given as parameter is actually scheduled.
		/// </summary>
		/// <param name="before">The Schedulable this entity should be scheduled before</param>
		/// <param name="who1">The first entity to be manipulated by this event</param>
		/// <param name="who2">The second entity to be manipulated by this event</param>
		/// <param name="who3">The third entity to be manipulated by this event</param>
		public void scheduleBefore(Schedulable before, Entity who1, Entity who2, Entity who3)
		{

			if (who1 == null)
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleBefore(Schedulable after, E who1, F who2, G who3)",
						"The Entity 'who1' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (who2 == null)
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleBefore(Schedulable after, E who1, F who2, G who3)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (who3 == null)
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleBefore(Schedulable after, E who1, F who2, G who3)",
						"The Entity 'who3' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((before == null))
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, E who1, F who2, G who3)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Event to be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, E who1, F who2, G who3)",
						"The event to be scheduled is already scheduled.",
						"Use method events only once, do not use them multiple "
								+ "times.");
				return; // was already scheduled
			}

			if (!before.isScheduled())
			{
				sendWarning("Can't schedule Event! Command ignored.", "Event : "
						+ getName() + " Method: scheduleBefore(Schedulable after, "
						+ " Method: scheduleBefore(Schedulable before, E who1, F who2, G who3)",
						"The Schedulable '" + before.getName()
						+ "' given as a "
						+ "positioning reference has to be already scheduled but "
						+ "is not.",
						"Use method isScheduled() of any Schedulable to find out "
								+ "if it is already scheduled.");
				return; // was not scheduled
			}

			if (!isModelCompatible(who1))
			{
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, E who1, F who2, G who3)",
						"The first entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who2))
			{
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, E who1, F who2, G who3)",
						"The second entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			if (!isModelCompatible(who3))
			{
				sendWarning("Can't schedule Event! Command ignored", "Event : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, E who1, F who2, G who3)",
						"The third entity to be scheduled with this event is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // is not compatible
			}

			// generate trace
			this.generateTraceForScheduling(who1, who2, who3, null, before, before.getEventNotes()[0].getTime(), null);

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleBefore(before, who1, who2, who3,
					this);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("scheduleBefore " + before.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>Creates a copy of the event with an empty schedule.</summary>
		//@SuppressWarnings("unchecked")
		protected EventOf3Entities clone()
		{
			return (EventOf3Entities)base.clone();
		}
	}
}