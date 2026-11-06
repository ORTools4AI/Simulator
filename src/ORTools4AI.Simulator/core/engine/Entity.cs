// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler, modified by Justin Neumann
// Modified for ORTools4AI.Simulator.

using System.Text;

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * Represents the superclass for all entities of a model. Entities are supposed
     * to be scheduled together with a compatible event at a certain point of
     * simulation time or relative to another event in present or future simulation
     * time.
     * <p>
     * Entities typically encapsulate all information about a model entity 
     * relevant to the modeller. Events can manipulate these informations when the
     * scheduled point of simulation time is reached and thus change the state of
     * the model. When modelling different types of entities you need to derive
     * different classes from this superclass. Each carrying the specific
     * information to represent its counterpart in the system modelled. Thus a
     * simulation of e.g. a factory would require both machines and material to be
     * subclasses of class <code>Entity</code>. They can act on each other by
     * scheduling themselves or other Entities with the appropriate events. To use
     * more than one entity of one type, create multiple instances of the same 
     * <code>Entity</code> class. 
     * For better identification, all instances created from a subclass of class
     * <code>NamedObject</code> (just as <code>Entity</code> is) get an individual
     * identification number as a suffix to their name so there is no need to name
     * each individual differently yourself.
     * <p> 
     * Entities can carry a queuing priority that can be modified after the entity 
     * has been instantiated, applied for inserting Entities into any kind of Queues:
     * The entity's priority determines it's position inside the queue on entering
     * it. Although within a model all attributes of an entity could be made public
     * it is advisable to support data hiding by providing methods for accessing the
     * internal attributes, as always in oo-design.
     * 
     * @see Event
     * @see SimProcess
     * @see NamedObject
     */
	public abstract class Entity : Schedulable {

		/**
		 * The queuing priority of the entity.
		 */
		private int _myQueuingPriority;

		/**
		 * The <code>Queue</code>s or other <code>QueueBased</code> objects where this Entity is queued.
		 */
		private List<QueueBased> _myQueues;

		/**
		 * The identfication number of this Entity.
		 */
		private long _identNumber;

		public Entity(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace) {

			//super(owner, name, showInTrace); // create Schedulable
			_myQueuingPriority = 0; // default queuing priority
			_myQueues = new List<QueueBased>();

			// creates and registers identifier
			_identNumber = owner.linkWithIdentNumber(this);

		}

		/// <summary>Creates an Entity named after its concrete class, with tracing off.</summary>
		public Entity(Model owner) : this(owner, "_auto_", false)
		{
			rename(GetType().Name);
		}

		/// <returns>
		/// The event-note associated to the entity or <c>null</c> if Entity is not currently scheduled
		/// </returns>
		/// <remarks>
		/// Returns the event-note associated to this Entity object. If the Entity object is not currently
		/// scheduled, <c>null</c> will be returned. Use getEventNotes() for all Event notes scheduled.
		/// </remarks>
		EventNote getEventNote()
		{
			return getEventNotes().Count==0 ? null : getEventNotes()[0];
		}

		/// <summary>
		/// Returns a list of events associated to this Entity object. If the Entity object is not currently
		/// scheduled, an empty list will be returned. Remind that all different Event classes can be included.
		/// </summary>
		/// <returns>The events associated to the entity</returns>
		public List<EventAbstract> getScheduledEvents()
		{
			var list = new List<EventAbstract>();
			foreach (EventNote note in _schedule)
			{
				list.Add(note.getEvent());
			}
			return list;

		}

		/// <summary>Returns the entity's identification number.</summary>
		/// <returns>The entity's identification number</returns>
		public long getIdentNumber() {

			return _identNumber; // returns the identification number

		}

		/// <summary>
		/// Returns the entity's queuing priority. Default priority of an entity is zero. Higher priorities are
		/// positive, lower priorities negative.
		/// </summary>
		/// <returns>The entity's priority</returns>
		public int getQueueingPriority() {

			return _myQueuingPriority;

		}

		/// <summary>
		/// Returns the entity's queuing priority. Default priority of an entity is zero. Higher priorities are
		/// positive, lower priorities negative.
		/// </summary>
		/// <returns>The entity's priority</returns>
		/// <remarks>
		/// Replaced by <c>getQueueingPriority()</c> to avoid confusing with the scheduling priority of an event or
		/// process.
		/// </remarks>
		public int getPriority() {

			return this.getQueueingPriority();

		}

		/// <returns>The <c>QueueList</c> or <c>null</c>.</returns>
		/// <remarks>
		/// Prefer usage of getQueues() as an entity may be queued in multiple queues, call getQueueList() to
		/// obtaint the QueueList&lt;?&gt; objects Returns the <c>QueueList</c> where this <c>Entity</c> is queued
		/// in or <c>null</c>.
		/// </remarks>
		public QueueList<E> getQueue<E>() where E:Entity
		{
			if (_myQueues.Count >0 && _myQueues[0] is ProcessQueue<SimProcess>)
				return (this._myQueues[0] as ProcessQueue<SimProcess>).getQueueList() as QueueList<E>;

			//if (!this._myQueues.isEmpty() && this._myQueues.get(0) instanceof ProcessQueue<?>)

			//return ((ProcessQueue <?>) this._myQueues.get(0)).getQueueList();
			//return null;
			return null;
		}

		/// <summary>
		/// Returns a list of queues and other <c>QueueBased</c> objects where this <c>Entity</c> is queued.
		/// </summary>
		/// <returns>The <c>QueueBased</c>s containing this entity; may be empty if entity is not queued.</returns>
		public List<QueueBased> getQueues()
		{
			return new List<QueueBased>(this._myQueues);
		}

		/// <summary>
		/// Tests if this <c>Entity</c> queued in a at least one queue or other <c>QueueBased</c>.
		/// </summary>
		/// <returns>
		/// Is <c>true</c> if this Entity is queued in at least one queue or other <c>QueueBased</c>, <c>false</c>
		/// otherwise.
		/// </returns>
		public bool isQueued() {
			return this._myQueues.Count > 0;
		}

		/// <summary>
		/// Checks if the two entities have the same priority. Note that this is a static method available through
		/// calling the entity's class i.e. <c>Entity.isEqual(a,b)</c> where <c>a</c> and <c>b</c> are valid Entity
		/// objects.
		/// </summary>
		/// <param name="a">First comparand entity</param>
		/// <param name="b">Second comparand entity</param>
		/// <returns>Is <c>true</c> if <c>a</c> has the same priority as <c>b</c>,<c>/false</c> otherwise</returns>
		public static bool isEqual(Entity a, Entity b) {

			if (a == null || b == null)
			{
				return false;
			}

			return (a.getQueueingPriority() == b.getQueueingPriority());

		}

		/// <summary>
		/// Checks if the first of the two entities has a higher priority than the second. Note that this is a
		/// static method available through calling the entity's class i.e. <c>Entity.isLarger(a,b)</c> where a and
		/// b are valid Entity objects.
		/// </summary>
		/// <param name="a">First comparand entity</param>
		/// <param name="b">Second comparand entity</param>
		/// <returns>
		/// Is <c>true</c> if <c>a</c> has a larger priority than <c>b</c>,<c>/false</c> otherwise
		/// </returns>
		public static bool isLarger(Entity a, Entity b) {

			if (a == null || b == null)
			{
				return false;
			}

			return (a.getQueueingPriority() > b.getQueueingPriority());

		}

		/// <summary>
		/// Checks if the first of the two entities has higher or same priority than the second. Note that this is
		/// a static method available through calling the entity's class i.e. <c>Entity.isLargerOrEqual(a,b)</c>
		/// where a and b are valid Entity objects.
		/// </summary>
		/// <param name="a">First comparand entity</param>
		/// <param name="b">Second comparand entity</param>
		/// <returns>
		/// Is <c>true</c> if <c>a</c> has a larger or equal priority than <c>b</c>,<c>/false</c> otherwise
		/// </returns>
		public static bool isLargerOrEqual(Entity a, Entity b) {

			if (a == null || b == null)
			{
				return false;
			}

			return (a.getQueueingPriority() >= b.getQueueingPriority());

		}

		/// <summary>
		/// Checks if the first of the two entities have different priorities. Note that this is a static method
		/// available through calling the entity's class i.e. <c>Entity.isNotEqual(a,b)</c> where a and b are valid
		/// Entity objects.
		/// </summary>
		/// <param name="a">First comparand entity</param>
		/// <param name="b">Second comparand entity</param>
		/// <returns>
		/// Is <c>true</c> if <c>a</c> has a different priority than <c>b</c>,<c>/false</c> otherwise
		/// </returns>
		public static bool isNotEqual(Entity a, Entity b) {

			if (a == null || b == null)
			{
				return false;
			}

			return (a.getQueueingPriority() != b.getQueueingPriority());

		}


		/// <summary>
		/// Tests if this Entity actually is a SimProcess. Although SimProcess have an individual life-cycle, they
		/// can also be handled like entities and be scheduled to be manipulated by an event.
		/// </summary>
		/// <returns>
		/// Is <c>true</c> if this Entity is an instance of class <c>SimProcess</c>,<c>false</c> otherwise
		/// </returns>
		public bool isSimProcess() {

			return (this is SimProcess);

		}

		/// <summary>
		/// Checks if the first of the two entities has a lower priority than the second. Note that this is a
		/// static method available through calling the entity's class i.e. <c>Entity.isSmaller(a,b)</c> where a
		/// and b are valid Entity objects.
		/// </summary>
		/// <param name="a">First comparand entity</param>
		/// <param name="b">Second comparand entity</param>
		/// <returns>Is <c>true</c> if <c>a</c> has a lower priority than <c>b</c>,<c>/false</c> otherwise</returns>
		public static bool isSmaller(Entity a, Entity b)
		{
			if (a == null || b == null)
			{
				return false;
			}
			return (a.getQueueingPriority() < b.getQueueingPriority());

		}

		/// <summary>
		/// Checks if the first of the two entities has lower or same priority than the second. Note that this is a
		/// static method available through calling the entity's class i.e. <c>Entity.isSmallerOrEqual(a,b)</c>
		/// where a and b are valid Entity objects.
		/// </summary>
		/// <param name="a">First comparand entity</param>
		/// <param name="b">Second comparand entity</param>
		/// <returns>
		/// Is <c>true</c> if <c>a</c> has a smaller or equal priority than <c>b</c>, <c>/false</c> otherwise
		/// </returns>
		public static bool isSmallerOrEqual(Entity a, Entity b) {

			if (a == null || b == null)
			{
				return false;
			}

			return (a.getQueueingPriority() <= b.getQueueingPriority());

		}

		/// <summary>
		/// Schedules this Entity to be manipulated by the given Event at the current time plus the given offset.
		/// Method returns with a warning message if either Entity or Event are already scheduled in the EventList.
		/// </summary>
		/// <param name="what">The Event that manipulates this Entity</param>
		/// <param name="dt">The offset to the current simulation time at which the event is to be scheduled</param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(Event what, TimeSpan dt)  {
			if ((dt == null)) {
				sendWarning(
						"Can't schedule Entity!",
						"Entity : " + getName()
								+ " Method: schedule(Event what, TimeSpan dt)",
						"The simulation time given as parameter is a null reference.",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Event what, TimeSpan dt)",
						"The Event given as parameter is a null reference.",
						"Be sure to have a valid Event reference for this event "
								+ "to be scheduled with.");
				return; // no proper parameter
			}

			if (what.isScheduled()) // Event is already scheduled
			{
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Event what, TimeSpan dt)",
						"The Event '" + what.getName()
								+ "'to be scheduled with this "
								+ "Entity is already scheduled.",
						"Use method reSchedule(TimeSpan dt) to shift the entity "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (!isModelCompatible(what)) // Entity and Event are part of model
			{
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName() + " Method: schedule(Event what, TimeSpan dt)",
						"The Event to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, null, null, null, null, TimeOperations.add(presentTime(), dt));

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(this, what, dt);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity to be manipulated by the given EventOf2Entities at the current time plus the
		/// given offset.
		/// </summary>
		/// <param name="who2">The second entity to be scheduled for the EventOf2Entities.</param>
		/// <param name="what">The event to be scheduled</param>
		/// <param name="dt">The offset to the current simulation time at which the event is to be scheduled</param>
		/// <remarks>See also SimClock</remarks>
		public  void schedule(Entity who2, EventOf2Entities what, TimeSpan dt)  
		{

			if ((who2 == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeSpan dt) {",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event "
								+ "to be scheduled with.");
				return; // no proper parameter
			}

			if ((dt == null)) {
				sendWarning(
						"Can't schedule Entity!",
						"Entity : " + getName()
						+ getName() + " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeSpan dt) {",
						"The simulation time given as parameter is a null reference.",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						   + getName() + " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeSpan dt) {",
						"The EventOf2Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf2Entities reference for this event "
								+ "to be scheduled with.");
				return; // no proper parameter
			}

			if (what.isScheduled()) // Event is already scheduled
			{
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeSpan dt) {",
						"The EventOf2Entities '" + what.getName()
								+ "'to be scheduled with this "
								+ "Entity is already scheduled.",
						"Use method reSchedule(TimeSpan dt) to shift the event "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (!isModelCompatible(what)) // Entity and Event are part of model
			{
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName() + " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeSpan dt) {",
						"The EventOf2Entities to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, who2, null, null, null, TimeOperations.add(presentTime(), dt));

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(this, who2, what, dt);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity to be manipulated by the given EventOf3Entities at the current time plus the
		/// given offset.
		/// </summary>
		/// <param name="who2">The second entity to be scheduled for the EventOf3Entities.</param>
		/// <param name="who3">The third entity to be scheduled for the EventOf3Entities.</param>
		/// <param name="what">The event to be scheduled</param>
		/// <param name="dt">The offset to the current simulation time at which the event is to be scheduled</param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(Entity who2, Entity who3, EventOf3Entities what, TimeSpan dt)
		{

			if ((who2 == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: <E extends Entity, F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeSpan dt)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event "
								+ "to be scheduled with.");
				return; // no proper parameter
			}

			if ((who3 == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: <E extends Entity, F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeSpan dt)",
						"The Entity 'who3' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event "
								+ "to be scheduled with.");
				return; // no proper parameter
			}

			if ((dt == null)) {
				sendWarning(
						"Can't schedule Entity!",
						"Entity : " + getName() + " Method: <E extends Entity, F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeSpan dt)",
						"The simulation time given as parameter is a null reference.",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						   + getName() + " Method: <E extends Entity, F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeSpan dt)",
						"The EventOf3Entities given as parameter is a null reference.",
						"Be sure to have a valid Event reference for this event "
								+ "to be scheduled with.");
				return; // no proper parameter
			}

			if (what.isScheduled()) // Event is already scheduled
			{
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						   + getName() + " Method: <E extends Entity, F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeSpan dt)",
						"The EventOf3Entities '" + what.getName()
								+ "'to be scheduled with this "
								+ "Entity is already scheduled.",
						"Use method reSchedule(TimeSpan dt) to shift the event "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (!isModelCompatible(what)) // Entity and Event are part of model
			{
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						   + getName() + " Method: <E extends Entity, F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeSpan dt)",
						"The EventOf3Entities to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, who2, who3, null, null, TimeOperations.add(presentTime(), dt));

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(this, who2, who3, what, dt);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity to be manipulated by the given Event at the given point of time. Method returns
		/// with a warning message if either Entity or Event are already scheduled in the event-list.
		/// </summary>
		/// <param name="what">The Event that manipulates this Entity</param>
		/// <param name="when">The point in simulation time this event is scheduled to happen.</param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(Event what, TimeInstant when)  {
			if ((when == null)) {
				sendWarning(
						"Can't schedule Entity!",
						"Entity : " + getName()
								+ " Method: schedule(Event what, TimeInstant when)",
						"The simulation time given as parameter is a null reference.",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: schedule(Event what, TimeInstant when)",
						"The Event given as parameter is a null reference.",
						"Be sure to have a valid Event reference for this event "
								+ "to be scheduled with.");
				return; // no proper parameter
			}

			if (what.isScheduled()) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: schedule(Event what, TimeInstant when)",
						"The Event '" + what.getName()
								+ "'to be scheduled with this "
								+ "Entity is already scheduled.",
						"Use method reSchedule(TimeInstant when) to shift the entity "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName()
						+ " Method: schedule(Event what, TimeInstant when)",
						"The Event to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, null, null, null, null, when);

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(this, what, when);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity to be manipulated by the given EventOf2Entities at the given point of time.
		/// Method returns with a warning message if either Entity or Event are already scheduled in the
		/// event-list.
		/// </summary>
		/// <param name="who2">The second entity to be scheduled for the EventOf2Entities.</param>
		/// <param name="what">The event to be scheduled</param>
		/// <param name="when">The point in simulation time this event is scheduled to happen.</param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(Entity who2, EventOf2Entities what, TimeInstant when)				 
		{

			if ((who2 == null)) {
				sendWarning(
						"Can't schedule Entity!",
						"Entity : " + getName()
						+ " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeInstant when)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			if ((when == null)) {
				sendWarning(
						"Can't schedule Entity!",
						"Entity : " + getName()
						+ " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeInstant when)",
						"The simulation time given as parameter is a null reference.",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeInstant when)",
						"The EventOf2Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf2Entities reference for this event "
								+ "to be scheduled with.");
				return; // no proper parameter
			}

			if (what.isScheduled()) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeInstant when)",
						"The EventOf2Entities '" + what.getName()
								+ "'to be scheduled with this "
								+ "Entity is already scheduled.",
						"Use method reSchedule(TimeInstant when) to shift the event "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> schedule(E who2, EventOf2Entities<?, E> what, TimeInstant when)",
						"The EventOf2Entities to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, who2, null, null, null, when);

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(this, who2, what, when);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity to be manipulated by the given EventOf3Entities at the given point of time.
		/// </summary>
		/// <param name="who2">The second entity to be scheduled for the EventOf3Entities.</param>
		/// <param name="who3">The third entity to be scheduled for the EventOf3Entities.</param>
		/// <param name="what">The event to be scheduled</param>
		/// <param name="when">The point in simulation time the event is scheduled to happen.</param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(Entity who2, Entity who3, EventOf3Entities what, TimeInstant when)
		
		{

			if ((who2 == null)) {
				sendWarning(
						"Can't schedule Entity!",
						"Entity : " + getName()
						+ " Method: <E extends Entity,F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeInstant when) {",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			if ((who3 == null)) {
				sendWarning(
						"Can't schedule Entity!",
						"Entity : " + getName()
						+ " Method: <E extends Entity,F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeInstant when) {",
						"The Entity 'who3' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			if ((when == null)) {
				sendWarning(
						"Can't schedule Entity!",
						"Entity : " + getName()
						+ " Method: <E extends Entity,F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeInstant when) {",
						"The simulation time given as parameter is a null reference.",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity,F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeInstant when) {",
						"The EventOf3Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf3Entities reference for this event "
								+ "to be scheduled with.");
				return; // no proper parameter
			}

			if (what.isScheduled()) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity,F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeInstant when) {",
						"The EventOf3Entities '" + what.getName()
								+ "'to be scheduled with this "
								+ "Entity is already scheduled.",
						"Use method reSchedule(TimeInstant when) to shift the event "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName()
						+ " Method: <E extends Entity,F extends Entity> schedule(E who2, F who3, EventOf3Entities<?, E, F> what, TimeInstant when) {",
						"The EventOf3Entities to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, who2, who3, null, null, when);

			// schedule Event
			getModel().getExperiment().getScheduler().schedule(this, who2, who3, what, when);

			if (currentlySendDebugNotes()) {
				sendDebugNote("schedules on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <param name="what">The Event that manipulates this Entity</param>
		/// <param name="dt">The offset to the current time this event is scheduled to happen</param>
		/// <remarks>
		/// Replaced by schedule(Event what,TimeSpan dt). Schedules this Entity to be manipulated by the given
		/// Event at the given offset to the current simulation time. Note that the given point in simulation time
		/// is the positive offset to the current simulation time as displayed by the simulation clock. Method
		/// returns with a warning message if either Entity or Event are already scheduled in the event-list. See
		/// also SimClock
		/// </remarks>
		//@Deprecated
		public void schedule(Event what, SimTime dt)  {
			schedule(what, SimTime.toTimeSpan(dt));
		}

		public void schedule(Event what) {
			if (what == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Event what)",
						"The Event given as parameter is a null reference.",
						"Be sure to have a valid Event reference for this event to be scheduled with.");
				return;
			}
			if (what.isScheduled()) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Event what)",
						"The Event '" + what.getName() + "' to be scheduled with this Entity is already scheduled.",
						"Use method reSchedule(TimeSpan dt) to shift the entity to be scheduled at some other point of time.");
				return;
			}
			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName() + " Method: schedule(Event what)",
						"The Event to be scheduled with this Entity is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(what, null, null, null, null, presentTime());
			getModel().getExperiment().getScheduler().schedule(this, what, TimeSpan.ZERO);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		public void schedule(Entity who2, EventOf2Entities what) {
			if (who2 == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Entity who2, EventOf2Entities what)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (what == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Entity who2, EventOf2Entities what)",
						"The EventOf2Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf2Entities reference for this event to be scheduled with.");
				return;
			}
			if (what.isScheduled()) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Entity who2, EventOf2Entities what)",
						"The EventOf2Entities '" + what.getName() + "' to be scheduled with this Entity is already scheduled.",
						"Use method reSchedule(TimeSpan dt) to shift the event to be scheduled at some other point of time.");
				return;
			}
			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName() + " Method: schedule(Entity who2, EventOf2Entities what)",
						"The EventOf2Entities to be scheduled with this Entity is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(what, who2, null, null, null, presentTime());
			getModel().getExperiment().getScheduler().schedule(this, who2, what, TimeSpan.ZERO);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		public void schedule(Entity who2, Entity who3, EventOf3Entities what) {
			if (who2 == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (who3 == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who3' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (what == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Entity who2, Entity who3, EventOf3Entities what)",
						"The EventOf3Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf3Entities reference for this event to be scheduled with.");
				return;
			}
			if (what.isScheduled()) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedule(Entity who2, Entity who3, EventOf3Entities what)",
						"The EventOf3Entities '" + what.getName() + "' to be scheduled with this Entity is already scheduled.",
						"Use method reSchedule(TimeSpan dt) to shift the event to be scheduled at some other point of time.");
				return;
			}
			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName() + " Method: schedule(Entity who2, Entity who3, EventOf3Entities what)",
						"The EventOf3Entities to be scheduled with this Entity is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(what, who2, who3, null, null, presentTime());
			getModel().getExperiment().getScheduler().schedule(this, who2, who3, what, TimeSpan.ZERO);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		public void schedulePreempt(Event what) {
			if (what == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedulePreempt(Event what)",
						"The Event given as parameter is a null reference.",
						"Be sure to have a valid Event reference for this event to be scheduled with.");
				return;
			}
			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName() + " Method: schedulePreempt(Event what)",
						"The Event to be scheduled with this Entity is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(what, null, null, null, null, presentTime());
			getModel().getExperiment().getScheduler().scheduleWithPreempt(this, what);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		public void schedulePreempt(Entity who2, EventOf2Entities what) {
			if (who2 == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedulePreempt(Entity who2, EventOf2Entities what)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (what == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedulePreempt(Entity who2, EventOf2Entities what)",
						"The EventOf2Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf2Entities reference for this event to be scheduled with.");
				return;
			}
			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName() + " Method: schedulePreempt(Entity who2, EventOf2Entities what)",
						"The EventOf2Entities to be scheduled with this Entity is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(what, who2, null, null, null, presentTime());
			getModel().getExperiment().getScheduler().scheduleWithPreempt(this, who2, what);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		public void schedulePreempt(Entity who2, Entity who3, EventOf3Entities what) {
			if (who2 == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedulePreempt(Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (who3 == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedulePreempt(Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who3' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this event to be scheduled with.");
				return;
			}
			if (what == null) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName() + " Method: schedulePreempt(Entity who2, Entity who3, EventOf3Entities what)",
						"The EventOf3Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf3Entities reference for this event to be scheduled with.");
				return;
			}
			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName() + " Method: schedulePreempt(Entity who2, Entity who3, EventOf3Entities what)",
						"The EventOf3Entities to be scheduled with this Entity is not modelcompatible.",
						"Make sure to use compatible model components only.");
				return;
			}
			this.generateTraceForScheduling(what, who2, who3, null, null, presentTime());
			getModel().getExperiment().getScheduler().scheduleWithPreempt(this, who2, who3, what);
			if (currentlySendDebugNotes())
				sendDebugNote("schedules on EventList<br>" + getModel().getExperiment().getScheduler().toString());
		}

		/// <summary>
		/// Schedules this Entity with the given Event to occur directly after the given Schedulable that is
		/// already scheduled. Note that this event's point of simulation time will be set to be the same as the
		/// Schedulable's time. Thus the event will occur directly after the given Schedulable but the simulation
		/// clock will not change. Will return with a warning message if the Schedulable given as parameter is not
		/// scheduled. If there are multiple schedules for the given Schedulable, the event will be scheduled after
		/// the last occurrence.
		/// </summary>
		/// <param name="after">The Schedulable this Entity should be scheduled after</param>
		/// <param name="what">The Event to manipulate this Entity</param>
		public void scheduleAfter(Schedulable after, Event what)  {

			// check parameters
			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: scheduleAfter(Schedulable after, Event what)",
						"The Event given as parameter is a null reference.",
						"Be sure to have a valid Event reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((after == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: scheduleAfter(Schedulable after, Event what)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Entity to be scheduled with.");
				return; // no proper parameter
			}

			if (!after.isScheduled()) {
				sendWarning(
						"Can't schedule Entity! Command ignored.",
						"Entity : "
								+ getName()
								+ " Method: scheduleAfter(Schedulable after, Event what)",
						"The Schedulable given as parameter is not scheduled, "
								+ "thus no position can be determined for this Entity.",
						"Be sure that the Schedulable given as aprameter is "
								+ "actually scheduled. You can check that by calling its "
								+ "method isScheduled() which returns a boolean telling"
								+ "you whether it is scheduled or not.");
				return; // no proper parameter
			}

			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName()
						+ " Method: scheduleAfter(Schedulable after, Event what)",
						"The Event to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			if (currentlySendTraceNotes()) {
				if (this == currentEntity()) {
					sendTraceNote("schedules '" + what.getName()
							+ "' of itself after '" + after.getName() + "' at "
							+ after.getEventNotes()[after.getEventNotes().Count - 1].getTime().toString());
				} else {
					sendTraceNote("schedules '" + what.getName() + "' of '"
							+ getName() + "' after '" + after.getName() + "' at "
							+ after.getEventNotes()[after.getEventNotes().Count - 1].getTime().toString());
				}
			}

			// generate trace
			this.generateTraceForScheduling(what, null, null, after, null, 
				after.getEventNotes()[after.getEventNotes().Count - 1].getTime());

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleAfter(after, this,
					what);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleAfter " + after.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity with the given EventOf2Entities to occur directly after the given Schedulable
		/// that is already scheduled. Note that the event's point of simulation time will be set to be the same as
		/// the Schedulable's time. Thus the event will occur directly after the given Schedulable but the
		/// simulation clock will not change. Will return with a warning message if the Schedulable given as
		/// parameter is not scheduled. If there are multiple schedules for the given Schedulable, the event will
		/// be scheduled after the last occurrence.
		/// </summary>
		/// <param name="after">The Schedulable the event should be scheduled after</param>
		/// <param name="what">The event to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled for the EventOf2Entities.</param>
		public void scheduleAfter(Schedulable after, EventOf2Entities what, Entity who2)
		{

			// check parameters
			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> scheduleAfter(Schedulable after, EventOf2Entities<?, E> what, E who2)",
						"The EventOf2Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf2Entities reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((after == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> scheduleAfter(Schedulable after, EventOf2Entities<?, E> what, E who2)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Entity to be scheduled with.");
				return; // no proper parameter
			}

			if ((who2 == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> scheduleAfter(Schedulable after, EventOf2Entities<?, E> what, E who2)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this "
								+ "Entity to be scheduled with.");
				return; // no proper parameter
			}

			if (!after.isScheduled()) {
				sendWarning(
						"Can't schedule Entity! Command ignored.",
						"Entity : "
								+ getName()
								+ " Method: <E extends Entity> scheduleAfter(Schedulable after, EventOf2Entities<?, E> what, E who2)",
						"The Schedulable given as parameter is not scheduled, "
								+ "thus no position can be determined for this Entity.",
						"Be sure that the Schedulable given as aprameter is "
								+ "actually scheduled. You can check that by calling its "
								+ "method isScheduled() which returns a boolean telling"
								+ "you whether it is scheduled or not.");
				return; // no proper parameter
			}

			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> scheduleAfter(Schedulable after, EventOf2Entities<?, E> what, E who2)",
						"The Event to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, who2, null, after, null, 
				after.getEventNotes()[after.getEventNotes().Count - 1].getTime());

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleAfter(after, this, who2,
					what);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleAfter " + after.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity with the given EventOf3Entities to occur directly after the given Schedulable
		/// that is already scheduled. Note that the event's point of simulation time will be set to be the same as
		/// the Schedulable's time. Thus the event will occur directly after the given Schedulable but the
		/// simulation clock will not change. Will return with a warning message if the Schedulable given as
		/// parameter is not scheduled. If there are multiple schedules for the given Schedulable, the event will
		/// be scheduled after the last occurrence.
		/// </summary>
		/// <param name="after">The Schedulable this Entity should be scheduled after</param>
		/// <param name="what">The event to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled for the EventOf3Entities.</param>
		/// <param name="who3">The third entity to be scheduled for the EventOf3Entities.</param>
		public  void scheduleAfter(Schedulable after, EventOf3Entities what, Entity who2, Entity who3)
		
		{

			// check parameters
			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity, F extends Entity> scheduleAfter(Schedulable after, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The EventOf3Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf3Entities reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((after == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity, F extends Entity> scheduleAfter(Schedulable after, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Entity to be scheduled with.");
				return; // no proper parameter
			}

			if ((who2 == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity, F extends Entity> scheduleAfter(Schedulable after, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((who3 == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity, F extends Entity> scheduleAfter(Schedulable after, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The Entity 'who3' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (!after.isScheduled()) {
				sendWarning(
						"Can't schedule Entity! Command ignored.",
						"Entity : "
								+ getName()
								+ " Method: <E extends Entity, F extends Entity> scheduleAfter(Schedulable after, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The Schedulable given as parameter is not scheduled, "
								+ "thus no position can be determined for this Entity.",
						"Be sure that the Schedulable given as aprameter is "
								+ "actually scheduled. You can check that by calling its "
								+ "method isScheduled() which returns a boolean telling"
								+ "you whether it is scheduled or not.");
				return; // no proper parameter
			}

			if (!isModelCompatible(what)) {
				sendWarning("Can't schedule Entity! Command ignored", "Entity : "
						+ getName()
						+ " Method: <E extends Entity, F extends Entity> scheduleAfter(Schedulable after, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The EventOf3Entities to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, who2, who3, after, null, 
				after.getEventNotes()[after.getEventNotes().Count - 1].getTime());

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleAfter(after, this, who2, who3,
					what);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleAfter " + after.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity with the given Event to occur directly before the given Schedulable that is
		/// scheduled. Note that this event's point of simulation time will be set to be the same as the
		/// Schedulable's time. Thus the event will occur directly before the given Schedulable but the simulation
		/// clock will not change. Issues a warning message if the Schedulable given is not scheduled. If there are
		/// multiple schedules for the given Schedulable, the event will be scheduled before the first occurrence.
		/// </summary>
		/// <param name="before">The Schedulable this Entity should be scheduled before</param>
		/// <param name="what">The Event to manipulate this Entity</param>
		public void scheduleBefore(Schedulable before, Event what)  {

			// check parameters
			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, Event what)",
						"The Event given as parameter is a null reference.",
						"Be sure to have a valid Event reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((before == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: scheduleBefore(Schedulable before, Event what)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Entity to be scheduled with.");
				return; // no proper parameter
			}

			if (!before.isScheduled()) {
				sendWarning(
						"Can't schedule Entity! Command ignored.",
						"Entity : " + getName()
								+ " Method: scheduleBefore(Schedulable before, Event what)",
						"The Schedulable given as parameter is not scheduled, "
								+ "thus no position can be determined for this Entity.",
						"Be sure that the Schedulable given as aprameter is "
								+ "actually scheduled. You can check that by calling its "
								+ "method isScheduled() which returns a boolean telling"
								+ "you whether it is scheduled or not.");
				return; // no proper parameter
			}

			if (!isModelCompatible(what)) {
				sendWarning(
						"Can't schedule Entity! Command ignored",
						"Entity : "
								+ getName()
								+ " Method: scheduleBefore(Schedulable before, Event what)",
						"The Event to be scheduled with thisEntity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, null, null, null, before, before.getEventNotes()[0].getTime());

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleBefore(before, this,
					what);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleBefore " + before.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity with the given EventOf2Entities to occur directly before the given Schedulable
		/// that is scheduled. Note that the event's point of simulation time will be set to be the same as the
		/// Schedulable's time. Thus the event will occur directly before the given Schedulable but the simulation
		/// clock will not change. Issues a warning message if the Schedulable given is not scheduled. If there are
		/// multiple schedules for the given Schedulable, the event will be scheduled before the first occurrence.
		/// </summary>
		/// <param name="before">The Schedulable this Entity should be scheduled before</param>
		/// <param name="what">The event to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled for the EventOf2Entities.</param>
		public void scheduleBefore(Schedulable before, EventOf2Entities what, Entity who2) 
		{

			// check parameters
			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> void scheduleBefore(Schedulable before, EventOf2Entities<?, E> what, E who2)",
						"The EventOf2Entities given as parameter is a null reference.",
						"Be sure to have a valid EventOf2Entities reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((before == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> void scheduleBefore(Schedulable before, EventOf2Entities<?, E> what, E who2)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Entity to be scheduled with.");
				return; // no proper parameter
			}

			if ((who2 == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity> void scheduleBefore(Schedulable before, EventOf2Entities<?, E> what, E who2)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (!before.isScheduled()) {
				sendWarning(
						"Can't schedule Entity! Command ignored.",
						"Entity : " + getName()
								+ " Method: <E extends Entity> void scheduleBefore(Schedulable before, EventOf2Entities<?, E> what, E who2)",
						"The Schedulable given as parameter is not scheduled, "
								+ "thus no position can be determined for this Entity.",
						"Be sure that the Schedulable given as aprameter is "
								+ "actually scheduled. You can check that by calling its "
								+ "method isScheduled() which returns a boolean telling"
								+ "you whether it is scheduled or not.");
				return; // no proper parameter
			}

			if (!isModelCompatible(what)) {
				sendWarning(
						"Can't schedule Entity! Command ignored",
						"Entity : "
								+ getName()
								+ " Method: <E extends Entity> void scheduleBefore(Schedulable before, EventOf2Entities<?, E> what, E who2)",
						"The EventOf2Entities to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, who2, null, null, before, before.getEventNotes()[0].getTime());

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleBefore(before, this, who2,
					what);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleBefore " + before.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this Entity with the given EventOf3Entities to occur directly before the given Schedulable
		/// that is scheduled. Note that the event's point of simulation time will be set to be the same as the
		/// Schedulable's time. Thus the event will occur directly before the given Schedulable but the simulation
		/// clock will not change. Issues a warning message if the Schedulable given is not scheduled. If there are
		/// multiple schedules for the given Schedulable, the event will be scheduled before the first occurrence.
		/// </summary>
		/// <param name="before">The Schedulable this Entity should be scheduled before</param>
		/// <param name="what">The event to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled for the EventOf3Entities.</param>
		/// <param name="who3">The third entity to be scheduled for the EventOf3Entities.</param>
		public void scheduleBefore(Schedulable before, EventOf3Entities what, Entity who2, Entity who3)
			
		{

			// check parameters
			if ((what == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity, F extends Entity> void scheduleBefore(Schedulable before, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The Event given as parameter is a null reference.",
						"Be sure to have a valid Event reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if ((before == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity, F extends Entity> void scheduleBefore(Schedulable before, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "Entity to be scheduled with.");
				return; // no proper parameter
			}

			if ((who2 == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity, F extends Entity> void scheduleBefore(Schedulable before, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The Entity 'who2' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this "
								+ "Entity to be scheduled with.");
				return; // no proper parameter
			}

			if ((who3 == null)) {
				sendWarning("Can't schedule Entity! Command ignored.", "Entity : "
						+ getName()
						+ " Method: <E extends Entity, F extends Entity> void scheduleBefore(Schedulable before, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The Entity  'who3' given as parameter is a null reference.",
						"Be sure to have a valid Entity reference for this "
								+ "Entity to be scheduled with.");
				return; // no proper parameter
			}

			if (!before.isScheduled()) {
				sendWarning(
						"Can't schedule Entity! Command ignored.",
						"Entity : " + getName()
								+ " Method: <E extends Entity, F extends Entity> void scheduleBefore(Schedulable before, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The Schedulable given as parameter is not scheduled, "
								+ "thus no position can be determined for this Entity.",
						"Be sure that the Schedulable given as aprameter is "
								+ "actually scheduled. You can check that by calling its "
								+ "method isScheduled() which returns a boolean telling"
								+ "you whether it is scheduled or not.");
				return; // no proper parameter
			}

			if (!isModelCompatible(what)) {
				sendWarning(
						"Can't schedule Entity! Command ignored",
						"Entity : "
								+ getName()
								+ " Method: <E extends Entity, F extends Entity> void scheduleBefore(Schedulable before, EventOf3Entities<?, E, F> what, E who2, F who3)",
						"The EventOf3Entities to be scheduled with this Entity is not "
								+ "modelcompatible.",
						"Make sure to use compatible model components only.");
				return; // was already scheduled
			}

			// generate trace
			this.generateTraceForScheduling(what, who2, who3, null, before, before.getEventNotes()[0].getTime());

			// schedule Event
			getModel().getExperiment().getScheduler().scheduleBefore(before, this, who2, who3,
					what);

			if (currentlySendDebugNotes()) {
				sendDebugNote("scheduleBefore " + before.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}


		/// <summary>
		/// Sets the entity's queuing priority to a given integer value. The default priority of each entity
		/// (unless assigned otherwise) is zero. Negative priorities are lower, positive priorities are higher. All
		/// values should be inside the range defined by the C# <c>int</c> type [-2147483648,
		/// +2147483647]. An entity's queuing priority can be used by the modeller to determine how the entity is
		/// treated by queues, though how precisely a queue will use the priority to determine sort order is up to
		/// it's queuing strategy: <c>QueueBased.FIFO</c> sorts entities by their queuing priority, highest
		/// priority first. Entities with the same priority are enqueued based on "first in, first out".
		/// <c>QueueBased.LIFO</c> also sorts entities by their priority, highest priority first. However, entities
		/// with the same priority are enqueued based on "last in, first out". <c>QueueBased.Random</c> assigns a
		/// random position to each entity entering the queue, disregarding priority. Of course, the modeller is
		/// free to use the queuing priority to determine how entities are processed by the components he
		/// implements himself, whether they are queues or not.
		/// </summary>
		/// <param name="newPriority">The new queuing priority value</param>
		public void setQueuingPriority(int newPriority) {

			this._myQueuingPriority = newPriority;

		}

		/// <summary>Sets the entity's queuing priority to a given integer value.</summary>
		/// <param name="newPriority">The new priority value</param>
		/// <remarks>
		/// Replaced by <c>setQueueingPriority(int newPriority)</c> to avoid confusing with the scheduling priority
		/// of an event or process.
		/// </remarks>
		public void setPriority(int newPriority) {
			this.setQueuingPriority(newPriority);
		}



		/// <summary>Informs this <c>Entity</c> to be queued in a given <c>QueueBased</c>.</summary>
		/// <param name="q">The <c>QueueBased</c> where this entity is now queued.</param>
		public void addQueueBased(QueueBased q)
		{
			_myQueues.Add(q);
		}

		/// <summary>Informs this <c>Entity</c> to be no longer queued in a given <c>QueueBased</c>.</summary>
		/// <param name="q">The <c>QueueBased</c> where this entity is no longer queued.</param>
		public void removeQueueBased(QueueBased q)
		{
			_myQueues.Remove(q);
		}

		/// <summary>Removes an event note from this object's schedule.</summary>
		/// <param name="note">The event note to remove.</param>
		internal void removeEventNote(EventNote note)
		{
			_schedule.Remove(note); // only removes Event in local list

		}

		/// <summary>
		/// Utility method to generate trace output for scheduling this event (internal use only).
		/// </summary>
		/// <param name="Event">the event to be scheduled</param>
		/// <param name="who1">the second entity scheduled with the event (or <c>null</c> if not applicable)</param>
		/// <param name="who2">the third entity scheduled with the event (or <c>null</c> if not applicable)</param>
		/// <param name="after">
		/// the Schedulable after which the event is scheduled (or <c>null</c> if not applicable)
		/// </param>
		/// <param name="before">
		/// the Schedulable before which the event is scheduled (or <c>null</c> if not applicable)
		/// </param>
		/// <param name="at">the TimeInstant at which the event is scheduled</param>
		protected void generateTraceForScheduling(EventAbstract Event, Entity who1, Entity who2, 
			Schedulable after, Schedulable before, TimeInstant at) {

			if (currentlySendTraceNotes()) {

				var trace = new StringBuilder("schedules '" + Event.getName() + "'");
				if (who1 != null) {
					String who1alias = (who1 == currentEntity() ? "itself" : "'" + who1.getName() + "'");
					trace.Append(" with " + who1alias);
					if (who2 != null) {
						String who2alias = (who2 == currentEntity() ? "itself" : "'" + who2.getName() + "'");
						trace.Append(" and " + who2alias);
					}
				}

				if (after != null) {
					String afterAlias = (after == currentEntity() ? "itself" : "'" + after.getName() + "'");
					trace.Append(" after " + afterAlias);
				} else if (before != null) {
					String beforeAlias = (before == currentEntity() ? "itself" : "'" + before.getName() + "'");
					trace.Append(" before " + beforeAlias);
				}

				if (at == this.presentTime()) {
					trace.Append(" now.");
				} else {
					trace.Append(" at " + at.toString() + ".");
				}

				this.sendTraceNote(trace.ToString());
			}
		}

		/// <summary>Creates a copy of the entity with a new identifier and no queue memberships.</summary>
		protected Entity clone() {
			Entity c = (Entity)base.clone();
			c._myQueues = new List<QueueBased>();
			c._identNumber = this.getModel().linkWithIdentNumber(c);
			return c;
		}
	}
}