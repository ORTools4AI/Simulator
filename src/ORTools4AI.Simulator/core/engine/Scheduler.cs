// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler, modified by Ruth Meyer, Justin Neumann
// @author Ruth Meyer
// @author Felix Klueckmann
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;
using System.Text;

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * The scheduler is the main element controlling the run of a simulation. It
     * controls the event-list in which all scheduled events are stored, picks the
     * next event to be processed and advances the simulation clock accordingly.
     */
	public class Scheduler : NamedObject {

		/**
		 * The experiment this scheduler serves.
		 */
		protected Experiment myExperiment;

		/**
		 * The actual simulation clock for this scheduler.
		 */
		protected SimClock clock;

		/**
		 * The event-list used by the scheduler.
		 */
		public EventList evList;

		/**
		 * Flag to indicate whether the current simulation is running or stopped. If
		 * true, the simulation is stopped, if false, the simulation is still
		 * running. This flag is especially polled by processes exiting any
		 * synchronized method used for passivating the process thus giving control
		 * back to the scheduler.
		 */
		protected bool simulationFinished;

		/**
		 * Contains the event-note used by the current Schedulables.
		 */
		private EventNote _currentNote;

		/**
		 * Contains the current active Entity.
		 */
		private Entity _currentEntity1;

		/**
		 * Contains the current active Entity.
		 */
		private Entity _currentEntity2;

		/**
		 * Contains the current active Entity.
		 */
		private Entity _currentEntity3;

		/**
		 * Contains the current active SimProcess.
		 */
		private SimProcess _currentProcess;

		/**
		 * Contains the current active Event.
		 */
		private EventAbstract _currentEvent;

		/**
		 * Contains the Schedulable that has created the current EventNode
		 */
		private Schedulable _currentSource;

		/**
		 * Contains the current active model.
		 */
		private Model _currentModel;

		/**
		 * Contains the current active Schedulable. This is either the entity or
		 * simProcess if available, or the external event, if none of the first two
		 * are available.
		 */
		private Schedulable _currentSchedulable;

		/**
		 * Flag indicating if the event-list will process concurrent Events in
		 * random order or not. Default is not (<code>false</code>). (Modification
		 * by Ruth Meyer, 11/2001)
		 */
		private bool _randomizingConcurrentEvents = false;

		/**
		 * The execution speed rate. Default is zero (as-fast-as-possible).
		 * (Modification by Felix Klueckmann, 05/2009)
		 */
		private double _executionSpeedRate = 0.0;

		/**
		 * The point in physical time (real time) of the last change of the
		 * execution speed rate (in nanoseconds). (Modification by Felix Klueckmann,
		 * 05/2009)
		 */
		private long _realTimeAtResetInNanos;

		// Returns a monotonically increasing nanosecond timestamp.
		// DateTime.Now.Nanosecond is a 0-999 sub-second component, NOT a clock.
		private static long nanoTime() =>
			(long)(System.Diagnostics.Stopwatch.GetTimestamp()
				   * (1_000_000_000.0 / System.Diagnostics.Stopwatch.Frequency));

		/**
		 * The point in simulation time of the last change of the execution speed
		 * rate or the last time the simulation was stopped . (Modification by Felix
		 * Klueckmann, 05/2009)
		 */
		private TimeInstant _simulationTimeAtReset;

		/**
		 * Flag indicating if the the execution speed rate was changed or the
		 * experiment was stopped since the last call of processNextEventNote().
		 * (Modification by Felix Klueckmann, 05/2009)
		 */
		private bool _timeReset;

		/**
		 * Lock for process synchronisation during a realtime execution.
		 * (Modification by Felix Klueckmann, 06/2009)
		 */
		private readonly object _syncLock = new object();

		/**
		 * A threadsafte consumer/producer queue to store
		 * RealTimeEventWrapper-Objects send by external systems
		 */
		private BlockingQueue<RealTimeEventWrapper> _realTimeEventQueue;

		private EventListLogger? _eventListLogger;

		private readonly Stack<EventNote> _notePool = new();

		private EventNote RentNote(Entity w1, Entity w2, Entity w3, EventAbstract what, TimeInstant when, int pri, Schedulable src)
		{
			if (_notePool.TryPop(out var note))
			{
				note.Reset(w1, w2, w3, what, when, pri, src);
				return note;
			}
			return new EventNote(w1, w2, w3, what, when, pri, src);
		}

		private void ReturnNote(EventNote note)
		{
			note.ClearForPool();
			_notePool.Push(note);
		}

		/// <summary>
		/// Constructs a scheduler with given name and the event-list (i.e. inheritor of <c>EventList</c>) to use.
		/// </summary>
		/// <param name="exp">The experiment that uses this Scheduler</param>
		/// <param name="name">The scheduler's name</param>
		/// <param name="eventList">The event-list to store scheduled events</param>

		public Scheduler(Experiment exp, String name, EventList eventList):base(name + "_scheduler") {

			//super(name + "_scheduler"); // create a namedObject with given name
			myExperiment = exp;
			evList = eventList;
			clock = new SimClock(name); // set reference to clock
			simulationFinished = false; // set flag to "not yet finished",
			_realTimeEventQueue = new LinkedBlockingQueue<RealTimeEventWrapper>();
		}

		/// <summary>Returns the actual simulation time as displayed by the simulation clock.</summary>
		/// <returns>The current point in simulation time</returns>
		public TimeInstant presentTime() {

			return clock.getTime();

		}

		public void setEventListLogger(EventListLogger logger) => _eventListLogger = logger;

		/// <summary>
		/// Returns the currently active Entity. Returns <c>null</c> if the current Schedulable happens to be an
		/// external event or a simprocess. Note that in case the current Event refers to more than one entity
		/// (<c>EventOfTwoEntities</c>, <c>EventOfThreeEntitties</c>), only the first entity is returned; to obtain
		/// all such entities, use <c>getAllCurrentEntities()</c> instead.
		/// </summary>
		/// <returns>
		/// The currently active Entity or <c>null</c> in case of an external event or a simprocess being the
		/// currently active Schedulable
		/// </returns>
		public Entity getCurrentEntity() {

			return _currentEntity1;

		}

		/// <summary>
		/// Returns the currently active entities. Returns an empty list if the current Schedulable happens to be
		/// an external event or a SimProcess.
		/// </summary>
		/// <returns>A list containing the currently active entities</returns>
		public List<Entity> getAllCurrentEntities() {

			List<Entity> entities = new List<Entity>();
			if (_currentEntity1 != null) entities.Add(_currentEntity1);
			if (_currentEntity2 != null) entities.Add(_currentEntity2);
			if (_currentEntity3 != null) entities.Add(_currentEntity3);

			return entities;

		}


		/// <summary>
		/// Returns the current active Event or <c>null</c>. Note that this method can also return an external
		/// event that can not be handled like an Event since it does not support scheduling together with an
		/// entity. Returns <c>null</c> if the current Schedulable happens to be a Sim-process that has been
		/// activated, thus no kind of Event is associated with it.
		/// </summary>
		/// <returns>
		/// The currently active Event or external event or <c>null</c> if the current Schedulable happens to be an
		/// activated SimProcess
		/// </returns>
		public EventAbstract getCurrentEvent() {

			return _currentEvent;

		}

		/// <summary>Returns the currently active model.</summary>
		/// <returns>The currently active model or <c>null</c> in case of no model being connected so far.</returns>
		public Model getCurrentModel() {

			return _currentModel;

		}

		/// <summary>
		/// Returns the Schedulable object that as created the current EventNode, thus being responsible for what
		/// is going on at the moment
		/// </summary>
		/// <returns>The source of the currently active object(s).</returns>
		/// <remarks>See also Entity See also SimProcess See also ExternalEvent</remarks>
		protected Schedulable getCurrentSource() {

			return _currentSource;

		}

		/// <summary>
		/// Returns the currently active Schedulable object. This can be any of its subtypes Entity, SimProcess or
		/// external event in that order. For events referring to multiple enities, the first entity is returned.
		/// </summary>
		/// <returns>The currently active Schedulable</returns>
		/// <remarks>See also Entity See also SimProcess See also ExternalEvent</remarks>
		public Schedulable getCurrentSchedulable() {

			return _currentSchedulable;

		}

		/// <summary>
		/// Returns the current SimProcess. Note that this method can only return a Sim-process. If the currently
		/// active Schedulable is not instance of Sim-process or an external event, <c>null</c> is returned.
		/// </summary>
		/// <returns>The currently active SimProcess or <c>null</c></returns>
		public SimProcess getCurrentSimProcess() {

			return _currentProcess;

		}

		/// <summary>Returns the current execution Speed Rate.</summary>
		/// <returns>The current execution speed rate.</returns>
		public double getExecutionSpeedRate() {
			return _executionSpeedRate;
		}

		/// <summary>Returns the actual clock for this model.</summary>
		/// <returns>The actual clock for simulation time</returns>
		public SimClock getSimClock() {

			return clock; // not much else to do

		}

		/// <summary>Returns if the event-list processes concurrent Events in random order or not.</summary>
		/// <returns><c>true</c> if concurrent Events are randomized, <c>false</c> otherwise</returns>
		/// <remarks>author: Ruth Meyer</remarks>
		public bool isRandomizingConcurrentEvents() {
			return this._randomizingConcurrentEvents;
		}

		/// <summary>
		/// Preempts the currently running SimProcess. Method is called whenenver another Entity or SimProcess is
		/// scheduled with <c>TimeSpan.ZERO</c> or <c>presentTime()</c> as parameter for the simulation time. The
		/// current SimProcess is interrupted and will be scheduled to continue its lifecycle
		/// </summary>
		/// <param name="preemptNote">
		/// EventNote - The event-note of the Schedulable preempting the current SimProcess
		/// </param>
		void preemptSimProcess(EventNote preemptNote) {

			if (_currentProcess == null) {
				myExperiment.sendWarning("Can't preempt current SimProcess! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: preemptSimProcess(EventNote preemptNote)",
						"The event-note reference passed as parameter is a null "
								+ "reference.",
						"Always make sure to use valid references only.");
				return; // preemption of currentprocess only
			}

			preemptNote.setTime(presentTime());

			EventNote currentNote = RentNote(_currentProcess, null, null, null, presentTime(), int.MaxValue, _currentSchedulable);

			evList.insertAsFirst(currentNote); // schedule currentProcess..
			evList.insertAsFirst(preemptNote);
            // ..to be next after preempting SimPr.
            //TODO:how to implement the passivate with yield ...
            //((SimProcess) _currentProcess).passivate(); // and set current to sleep until then

        }

        /// <summary>
        /// Processes the next event-note on the event-list. Returns <c>true</c> if that EventNote has been
        /// processed correctly, <c>false</c> if an error occurred or the event-list is empty.
        /// </summary>
        /// <returns>
        /// Is <c>true</c> if the next event-note was processed correctly, <c>false</c> if not
        /// </returns>
        //@SuppressWarnings("unchecked")
        internal bool processNextEventNote() {
			// check if there still are Event notes to be processed
			if (!(_executionSpeedRate > 0)) {
				if (evList.isEmpty()) {
					return false; // no more EventNote available -> exit
				}
			} else {

				while (true) {
					lock (_syncLock) {
						if (evList.isEmpty() && _realTimeEventQueue.isEmpty()) {
							return false;
						}
					}
					if (myExperiment.isStopped()) {
						return true; // there is an event
					}
					if (_timeReset) {
						this._realTimeAtResetInNanos = nanoTime();
						this._simulationTimeAtReset = presentTime();
						this._timeReset = false;
					}

					while (!_realTimeEventQueue.isEmpty()) {
						// an event has been sent by an external system

						RealTimeEventWrapper currentRealTimeWrapper = _realTimeEventQueue
								.poll();// get the next RealTimeEventWrapper and
										// remove it from the queue
						ExternalEvent currentRealTimeEvent = currentRealTimeWrapper
								.getExternalEvent();// get the encapsulated
													// ExternalEvent

						long weightedRealTimeEventNanos = (long)((currentRealTimeWrapper
								.getNanos() - _realTimeAtResetInNanos) * _executionSpeedRate);

						if (weightedRealTimeEventNanos < 0) {
							// the ExternalEvent is in the future
							myExperiment
									.sendWarning(
											"Can not schedule real time external event! "
													+ "The external event is discarded.",
											"Scheduler of experiment "
													+ myExperiment.getName()
													+ " Method processNextEventNote() "
													+ "external event: "
													+ currentRealTimeEvent
															.getName()
													+ " deviation in nanoseconds: "
													+ -weightedRealTimeEventNanos,
											"The given real-time time stamp is in the future.",
											"Real-time events are not supposed to be scheduled in the (real time) future.");
						} else {

							TimeInstant realTimeNanosEquivalent = TimeOperations
									.add(_simulationTimeAtReset, new TimeSpan(
											weightedRealTimeEventNanos,
											TimeUnit.NANOSECONDS));
							// calculate the simulation time equivalent to
							// the real-time time stamp of the external event

							if (TimeInstant.isAfterOrEqual(realTimeNanosEquivalent,
									presentTime())) {
								// the event can be scheduled correctly
								currentRealTimeEvent
										.schedule(realTimeNanosEquivalent);
							} else {
								// the simulation time of the event is in the past
								currentRealTimeEvent.schedule(presentTime());

								myExperiment
										.sendWarning(
												"Can not schedule real time external event at the simulation time equivalent to the given timeStamp! "
														+ "The external event is scheduled at the present simulation time instead.",
												"Scheduler of experiment "
														+ myExperiment.getName()
														+ " Method processNextEventNote() "
														+ "external event: "
														+ currentRealTimeEvent
																.getName()
														+ " deviation in nanoseconds: "
														+ (presentTime()
																.getTimeTruncated(
																		TimeUnit.NANOSECONDS) - weightedRealTimeEventNanos),
												"The simulation time equivalent of the given time stamp is before the current simulation time. Can not perform a rollback.",
												"Check if this deviation constitutes a problem.");
							}

						}

					}

					// System.out.println("--------------------------");
					// if (!evList.isEmpty()) {
					// EventNote note = evList.firstNote();
					// System.out.println(note.toString());
					// while (!(note == evList.lastNote())) {
					// System.out.println(evList.nextNote(note));
					// note = evList.nextNote(note);
					// }
					// System.out.println("--------------------------");
					// }
					// test code by felix

					_currentNote = evList.firstNote(); // get next event-note
					long weightedTimeSinceReset = (long)((nanoTime() - _realTimeAtResetInNanos) * _executionSpeedRate);
					// calculate the real time passed since the last time reset
					// considering the execution speed rate

					long timeToWait = (long)((TimeOperations.diff(
							_currentNote.getTime(), _simulationTimeAtReset)
							.getTimeTruncated(TimeUnit.NANOSECONDS) - weightedTimeSinceReset) / _executionSpeedRate);
					// calculate the time the thread has to wait
					if (timeToWait > 0) {
						// sleep until next event is due, or until woken by an incoming real-time event
						lock (_syncLock) {
							int waitMs = (int)Math.Min(timeToWait / 1_000_000L, int.MaxValue);
							Monitor.Wait(_syncLock, waitMs);
						}
					}
					if (!_timeReset && _realTimeEventQueue.isEmpty()) {
						break;
					}

				}
			}
			// get next event-note and remove from EventList atomically
			_currentNote = evList.removeFirst();
			_eventListLogger?.logNote(_currentNote);
			TimeInstant nextTime = _currentNote.getTime();
			// check if the next event-note is in the future
			if (TimeInstant.isAfter(nextTime, presentTime())) {
				this.advanceTime(presentTime(), nextTime);
			} else {
				// throw an exception if the next event-note is in the past
				if (TimeInstant.isBefore(nextTime, presentTime())) {
					throw (new SimAbortedException(
							new ErrorMessage(
									null,
									"The simulation Time of the next event is in the past! Simulation aborted.",
									"Class : Scheduler  Method : processNextEventNote()",
									"the simulation time of the next event-note ("
											+ _currentNote
											+ ") is before the current simulation time : "
											+ presentTime(),
									"Make sure not to schedule events in the past.",
									null)));
				}
			}
			// update the clock and its observers

			// changes to event-list and 'current' variables can only be made after
			// every
			// Observer registered with the clock has made their updates

			// set 'current' variables
			// determine Entity and SimProcess, if applicable
			_currentEntity1 = _currentNote.getEntity1(); // multiple entities
			_currentEntity2 = _currentNote.getEntity2();
			_currentEntity3 = _currentNote.getEntity3();

			_currentProcess = null;
			_currentSchedulable = null;
			_currentSource = _currentNote.getSource();

			if (_currentEntity1 != null)
			{
				_currentModel = _currentEntity1.getModel();
				_currentSchedulable = _currentEntity1;
				_currentEntity1.removeEventNote(_currentNote);
				if (_currentEntity1 is SimProcess) { // a real SimProcess
					_currentProcess = (SimProcess)_currentEntity1;
				}
			}
			if (_currentEntity2 != null)
			{
				_currentEntity2.removeEventNote(_currentNote);
			}
			if (_currentEntity3 != null)
			{
				_currentEntity3.removeEventNote(_currentNote);
			}

			// determine Event resp. external event
			_currentEvent = _currentNote.getEvent();

			if (_currentEvent != null && _currentProcess == null) {

				_currentEvent.removeEventNote(_currentNote);

				if (_currentSchedulable == null) {

					_currentSchedulable = _currentEvent;
					_currentModel = _currentEvent.getModel();
				}
			}

			// just check if everything is still allright
			// currSched. must have been set to something other than null by now
			// otherwise : throw (new desmoj.exception.DESMOJException());
			if (_currentSchedulable == null)
				return false;

			// clear time value and return EventNote to pool
			ReturnNote(_currentNote);
			_currentNote = null;

			// determine if event-oriented or process-oriented
			if (_currentEvent != null) { // event-oriented

				if (_currentEvent.getNumberOfEntities() == 1) // Event for one entity
				{
					 ((Event)_currentEvent).eventRoutine(_currentEntity1);
				}

				else if (_currentEvent.getNumberOfEntities() == 2)// Event for two entities
				{
					((EventOf2Entities)_currentEvent).eventRoutine(_currentEntity1, _currentEntity2);
				}

				else if (_currentEvent.getNumberOfEntities() == 3)// Event for three entities
				{
					((EventOf3Entities)_currentEvent).eventRoutine(_currentEntity1, _currentEntity2, _currentEntity3);
				}

				else //external event!
				{
					((ExternalEvent)_currentEvent).eventRoutine();
				}

				// this is where it all happens
			} else { // process-oriented

				// The current SimProcess to hand control over to may have terminated
				// already
				// so the scheduler must not start it
				if (!_currentProcess.isTerminated()) {

					// Inform process about source of activation
					_currentProcess.setActivatedBy(_currentSource);

					// The current SimProcess can either be running, thus waiting in a
					// block
					// or still not have been started
					if (_currentProcess.isReady()) {
						// currentProcess is already running

						// main thread passes control over to currentProcess'
						// SimThread
						// executing the SimProcess' lifeCycle method until
						// hold / passivate
						// or end of lifeCycle
						_currentProcess.resume();

					} else { // currentProcess is starting for the first time

						_currentProcess.start(); // start him up

					}

					// when hold / passivate or end of lifeCycle are passed, the
					// control
					// is handed back to the main thread here and there's nothing
					// else to do
					// but return true to state that this event-note has been
					// processed correctly

				} else {
					// this must not happen, but we check on it anyway
					myExperiment
							.sendWarning("Can not activate scheduled SimProcess! "
									+ "Command ignored.",
									"Scheduler of experiment "
											+ myExperiment.getName()
											+ " Method processNextEventNote() "
											+ "SimProcess : "
											+ _currentProcess.getName(),
									"The SimProcess to be activated to take over control has "
											+ "already terminated.",
									"Be sure to check the SimProcess' status before resuming." +
									" Use method isTerminated() to check the current status");
				}

			}
			if (_executionSpeedRate > 0) {
				// calculate deviation
				long simTimeSinceReset = TimeOperations.diff(presentTime(),
						_simulationTimeAtReset).getTimeTruncated(
						TimeUnit.NANOSECONDS);
				long realTimeSinceReset = (nanoTime() - _realTimeAtResetInNanos);
				long deviationInNanoseconds = (long)(realTimeSinceReset - (simTimeSinceReset / _executionSpeedRate));
				// System.out.println("SimTimeSinceReset: " + simTimeSinceReset
				// + ", RealTimeSinceReset: " + realTimeSinceReset
				// + ", Weighted deviation: " + deviationInNanoseconds);

				if (_currentEvent != null) {
					// event-oriented

					long deadLine = _currentEvent.getRealTimeConstraint();
					// the deadLine of this event

					if (deadLine > 0 && deadLine < deviationInNanoseconds) {
						// the real-time deadline was missed
						myExperiment.sendWarning(
								"The real-time deadline of this event was missed!",
								"Scheduler of experiment " + myExperiment.getName()
										+ " Method processNextEventNote() "
										+ "Event : " + _currentEvent.getName(),
								"The real-time deadline of this event was "
										+ deadLine + " nanoseconds. It was missed by "
										+ (deviationInNanoseconds - deadLine)
										+ " nanoseconds.",
								"Check if the deadline can be met.");
					}
				} else {
					// process-oriented

					long deadLine = _currentProcess.getRealTimeConstraint();
					// the deadLine of this SimProcess

					if (deadLine > 0 && deadLine < deviationInNanoseconds) {
						// the real-time deadline was missed
						myExperiment
								.sendWarning(
										"The real-time deadline of this SimProcess was missed!",
										"Scheduler of experiment "
												+ myExperiment.getName()
												+ " Method processNextEventNote() "
												+ "SimProcess : "
												+ _currentProcess.getName(),
										"The real-time deadline of this SimProcess was "
												+ deadLine
												+ " nanoseconds. It was missed by "
												+ (deviationInNanoseconds - deadLine)
												+ " nanoseconds.",
										"Check if the deadline can be met.");
					}
				}

			}

			return true; // everything processed alright

		}

		/// <summary>
		/// This method is called to notify simulator about the time passed between the execution of two events
		/// with a positive (non-zero) offset. The implementation of this method just sets the scheduler's clock to
		/// the new instant; note, though, that extensions to DESMO-J may overwrite this method to conduct more
		/// complex operations, e.g. extrapolate variables in continuous simulation.
		/// </summary>
		/// <param name="now">The current point in simulation time</param>
		/// <param name="next">
		/// The next point in simulation time to advance the clock to (e.g. instant of the next event)
		/// </param>
		protected void advanceTime(TimeInstant now, TimeInstant next) {

			// just update the simulation clock to the instant of
			// the next event
			clock.setTime(next);
		}


		/// <summary>
		/// Schedules the external event to happen at the simulation time equivalent to the current value of
		/// wall-clock time.
		/// </summary>
		/// <param name="what">The external event to be scheduled</param>
		internal void realTimeSchedule(RealTimeEventWrapper what) {
			if (!myExperiment.isRunning()) {
				myExperiment
						.sendWarning(
								"Can not schedule real time external event! Command ignored.",
								"Experiment '"
										+ getName()
										+ "' method void realTimeSchedule(ExternalEvent what).",
								"The experiment is not running.",
								"events can only be scheduled when the exeriment is running.");
				return;
			}
			if (what == null) { // check for null reference
				myExperiment.sendWarning("Can't schedule ExternalEvent! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: realTimeSchedule(ExternalEvent what)",
						"The ExternalEvent reference passed is a "
								+ "null references.",
						"Always make sure to use valid references only.");
				return;
			}

			try {
				// put the given Event wrapper into the thread-safe Event queue
				_realTimeEventQueue.put(what);
			} catch (InterruptException e) {
				myExperiment.sendWarning("Can't schedule external event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: realTimeSchedule(ExternalEvent who)",
						"The Thread waiting to schedule the given external event "
								+ "was interrupted.",
						"A Thread has to wait until space becomes available");

			}
			lock (_syncLock) { Monitor.PulseAll(_syncLock); } // wake sleeping real-time loop
		}

		/// <summary>
		/// Schedules the event to happen after a specified time. Checks that only legal combinations of valid
		/// parameters are scheduled. &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid
		/// scheduling types &lt;/CAPTION&gt; &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling
		/// type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt;
		/// &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external
		/// event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="who">The Entity to be scheduled</param>
		/// <param name="dt">
		/// The point in simulation time for the event to happen as an offset to the current simulation time
		/// </param>
		internal void reSchedule(Schedulable who, TimeSpan dt) {

			if (dt == null) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: reSchedule(Schedulable who, TimeSpan dt)",
								"The simulation time reference passed as parameter is a "
										+ "null reference.",
								"Always make sure to use valid references only.");
				return; // time missing
			}

			if (who == null) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: reSchedule(Schedulable who, TimeSpan dt)",
								"The Schedulable reference passed as parameter is a "
										+ "null reference.",
								"Always make sure to use valid references only.");
				return; // Schedulable missing
			}

			if (!who.isScheduled()) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: reSchedule(Schedulable who, TimeSpan dt)",
								"The Schedulable given is not scheduled, thus can not be "
										+ "rescheduled.",
								"To reschedule a Schedulable, it must already be scheduled. "
										+ "You can check that by calling the Schedulable's "
										+ "isScheduled() method");
				return;
				// I can't be rescheduled, I haven't even been scheduled yet
			}

			// all parameters checked, now remove the Schedulable's EventNote
			// first...
			List<EventNote> notes = who.getEventNotes();
			foreach (EventNote note in notes) {
				evList.remove(note);
				note.setTime(TimeOperations.add(presentTime(), dt));

				if (dt == TimeSpan.ZERO) { // preempting schedule for processes

					if (_currentProcess == null) { // currently Event -> no preemption
												   // inserted as first in the event-list with the current time as
												   // activation
						evList.insertAsFirst(note);

					} else { // currently SimProcess -> preempt!

						preemptSimProcess(note);
					}

				} else {

					// all parameters checked, now reinsert note
					evList.insert(note);

				}
			}
		}

		/// <summary>
		/// Schedules the event to happen at the specified time. Checks that only legal combinations of valid
		/// parameters are scheduled. &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid
		/// scheduling types &lt;/CAPTION&gt; &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling
		/// type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt;
		/// &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external
		/// event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="who">The Entity to be scheduled</param>
		/// <param name="time">The point in simulation time for the event to happen</param>
		internal void reSchedule(Schedulable who, TimeInstant time) {

			if (time == null) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: reSchedule(Schedulable who, TimeInstant time)",
								"The simulation time reference passed as parameter is a "
										+ "null reference.",
								"Always make sure to use valid references only.");
				return; // time missing
			}

			if (who == null) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: reSchedule(Schedulable who, TimeInstant time)",
								"The Schedulable reference passed as parameter is a "
										+ "null reference.",
								"Always make sure to use valid references only.");
				return; // Schedulable missing
			}

			if (!who.isScheduled()) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: reSchedule(Schedulable who, TimeInstant time)",
								"The Schedulable given is not scheduled, thus can not be "
										+ "rescheduled.",
								"To reschedule a Schedulable, it must already be scheduled. "
										+ "You can check that by calling the Schedulable's "
										+ "isScheduled() method");
				return;
				// I can't be rescheduled, I haven't even been scheduled yet
			}

			if (TimeInstant.isBefore(time, this.presentTime())) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: reSchedule(Schedulable who, TimeInstant time)",
								"The instant given is in the past.",
								"To reschedule a Schedulable, use a TimeInstant no earlier than the present time. "
										+ "The present time can be obtained using the "
										+ "presentTime() method");
				return;
				// I can't be rescheduled, TimeInstant has already passed.
			}

			// all parameters checked, now remove the Schedulable's EventNote
			// first...
			EventNote note = who.getEventNotes()[0];
			evList.remove(note);
			note.setTime(time);

			if (this.presentTime() == time) { // preempting schedule for processes

				if (_currentProcess == null) { // currently Event -> no preemption
											   // inserted as first in the event-list with the current time as
											   // activation
					evList.insertAsFirst(note);
					return;

				} else { // currently SimProcess -> preempt!

					preemptSimProcess(note);
					return;

				}

			}

			// all parameters checked, now reinsert note
			evList.insert(note);

		}

		internal void reScheduleNoPreempt(Schedulable who, TimeInstant time) =>
			reSchedule(who, time);

		internal void scheduleWithPreempt(Entity who, EventAbstract what) {
			int priority = what != null ? what.getSchedulingPriority() : 0;
			EventNote note = RentNote(who, null, null, what, presentTime(), priority, _currentSchedulable);
			if (_currentProcess == null)
				evList.insertAsFirst(note);
			else
				preemptSimProcess(note);
		}

		internal void scheduleWithPreempt(Entity who1, Entity who2, EventOf2Entities what) {
			EventNote note = RentNote(who1, who2, null, what, presentTime(), what.getSchedulingPriority(), _currentSchedulable);
			if (_currentProcess == null)
				evList.insertAsFirst(note);
			else
				preemptSimProcess(note);
		}

		internal void scheduleWithPreempt(Entity who1, Entity who2, Entity who3, EventOf3Entities what) {
			EventNote note = RentNote(who1, who2, who3, what, presentTime(), what.getSchedulingPriority(), _currentSchedulable);
			if (_currentProcess == null)
				evList.insertAsFirst(note);
			else
				preemptSimProcess(note);
		}

		internal void reScheduleWithPreempt(Schedulable who) {

			if (who == null) {
				myExperiment.sendWarning(
					"Can't reschedule Schedulable with preemption! Command ignored.",
					"Scheduler : " + getName() + " Method: reScheduleWithPreempt(Schedulable who)",
					"The Schedulable reference passed as parameter is a null reference.",
					"Always make sure to use valid references only.");
				return;
			}

			if (!who.isScheduled()) {
				myExperiment.sendWarning(
					"Can't reschedule Schedulable with preemption! Command ignored.",
					"Scheduler : " + getName() + " Method: reScheduleWithPreempt(Schedulable who)",
					"The Schedulable given is not scheduled, thus can not be rescheduled.",
					"To reschedule a Schedulable, it must already be scheduled.");
				return;
			}

			List<EventNote> notes = new List<EventNote>(who.getEventNotes());
			foreach (EventNote note in notes) {
				evList.remove(note);
				note.setTime(presentTime());
				if (_currentProcess == null) {
					evList.insertAsFirst(note);
				} else {
					preemptSimProcess(note);
				}
			}
		}

		/// <summary>
		/// Schedules the event to happen after a specified time span. Checks that only legal combinations of valid
		/// parameters are scheduled. &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid
		/// scheduling types &lt;/CAPTION&gt; &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling
		/// type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt;
		/// &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external
		/// event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="who">The Entity to be scheduled</param>
		/// <param name="what">The event to manipulate the entity</param>
		/// <param name="dt">
		/// The point in simulation time for the event to happen as an offset to the current simulation time
		/// </param>
		internal void schedule(Entity who, EventAbstract what, TimeSpan dt)
		{

			if (dt == null) {
				myExperiment.sendWarning(
						"Can't schedule Entity and Event at given "
								+ "time! Command ignored.", "Scheduler : "
								+ getName() + " Method: schedule(Entity who, "
								+ "EventAbstract what, TimeSpan dt)",
						"The simulation time reference passed as parameter is a "
								+ "null reference.",
						"Always make sure to use valid references only.");
				return; // time missing
			}

			if ((who == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who, "
						+ "EventAbstract what, TimeSpan dt)",
						"The Event and Entity references passed are both "
								+ "null references.",
						"Either Event or Entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who == null) && !(what is ExternalEvent)) {
				myExperiment.sendWarning("Can't schedule Event! Command ignored.",
						"Scheduler : " + getName()
								+ " Method: schedule(Entity who, "
								+ "EventAbstract what, TimeSpan dt)",
						"The Entity reference passed is a null reference but the "
								+ "Event references is not an external event.",
						"If no valid Entity is given, the event must be of type "
								+ "external event.");
				return; // if no Entity it must be ExternalEvent
			}

			if (!(who is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who, "
						+ "EventAbstract what, TimeSpan dt)",
						"The Entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (what != null)
			{
				if (what.getNumberOfEntities() > 1)
				{
					myExperiment.sendWarning("Can't schedule Entity and Event! "
							+ "Command ignored.", "Scheduler : " + getName()
							+ " Method: schedule(Entity who, "
							+ "EventAbstract what, TimeSpan dt)",
							"The method needs the correct Event to be scheduled with.",
							"You are using an event for multiple entities. You need an event for a single entity.");
					return; // Event with only one entity needed
				}
			}

			TimeInstant time = TimeOperations.add(presentTime(), dt);
			// set time for being scheduled

			// determine priority
			int priority = 0;
			if (what == null && who is SimProcess) {
				priority = ((SimProcess)who).getSchedulingPriority();
			} else if (what != null) {
				priority = what.getSchedulingPriority();
			}

			EventNote note = RentNote(who, null, null, what, time, priority, _currentSchedulable);

			if (dt == TimeSpan.ZERO) { // preempting schedule for processes
				if (_currentProcess == null) { // currently Event -> no preemption
											   // inserted as first in the event-list with the current time as
											   // activation
					evList.insertAsFirst(note);
					return;
				} else { // currently SimProcess -> preempt!
					preemptSimProcess(note);
					return;
				}
			}

			// all parameters checked, now schedule Event
			evList.insert(note);

		}

		/// <summary>
		/// Schedules the event to happen after a specified time span. Checks that only legal combinations of valid
		/// parameters are scheduled. &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid
		/// scheduling types &lt;/CAPTION&gt; &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling
		/// type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt;
		/// &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external
		/// event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="who1">The first entity to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled</param>
		/// <param name="what">The event to manipulate the two entities</param>
		/// <param name="dt">
		/// The point in simulation time for the event to happen as an offset to the current simulation time
		/// </param>
		internal void schedule(Entity who1, Entity who2, EventOf2Entities what, TimeSpan dt)
		{

			if (dt == null) {
				myExperiment.sendWarning(
						"Can't schedule Entity and Event at given "
								+ "time! Command ignored.", "Scheduler : "
								+ getName() + " Method: schedule(Entity who1, Entity who2, "
								+ "EventOf2Entities what, TimeSpan dt)",
						"The simulation time reference passed as parameter is a "
								+ "null reference.",
						"Always make sure to use valid references only.");
				return; // time missing
			}

			if ((who1 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, "
						+ "EventOf2Entities what, TimeSpan dt)",
						"The event and first entity references passed are both "
								+ "null references.",
						"Either Event or first entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who2 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, "
						+ "EventOf2Entities what, TimeSpan dt)",
						"The event and second entity references passed are both "
								+ "null references.",
						"Either Event or second entity references must be valid.");
				return; // no real parameters here anyway
			}

			if (!(who1 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, "
						+ "EventOf2Entities what, TimeSpan dt)",
						"The first entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who2 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, "
						+ "EventOf2Entities what, TimeSpan dt)",
						"The second entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			TimeInstant time = TimeOperations.add(presentTime(), dt);
			// set time for being scheduled

			EventNote note = RentNote(who1, who2, null, what, time, what.getSchedulingPriority(), _currentSchedulable);

			if (dt == TimeSpan.ZERO) { // preempting schedule for processes
				if (_currentProcess == null) { // currently Event -> no preemption
											   // inserted as first in the event-list with the current time as
											   // activation
					evList.insertAsFirst(note);
					return;
				} else { // currently SimProcess -> preempt!
					preemptSimProcess(note);
					return;
				}
			}

			// all parameters checked, now schedule Event
			evList.insert(note);

		}

		/// <summary>
		/// Schedules the event to happen after a specified time span. Checks that only legal combinations of valid
		/// parameters are scheduled. &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid
		/// scheduling types &lt;/CAPTION&gt; &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling
		/// type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt;
		/// &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external
		/// event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="who1">The first entity to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled</param>
		/// <param name="who3">The third entity to be scheduled</param>
		/// <param name="what">The event to manipulate the two entities</param>
		/// <param name="dt">
		/// The point in simulation time for the event to happen as an offset to the current simulation time
		/// </param>
		internal void schedule(Entity who1, Entity who2, Entity who3, 
			EventOf3Entities what, TimeSpan dt)
		{

			if (dt == null) {
				myExperiment.sendWarning(
						"Can't schedule Entity and Event at given "
								+ "time! Command ignored.", "Scheduler : "
								+ getName() + " Method: schedule(Entity who1, Entity who2, Entity who3, "
								+ "EventOf3Entities what, TimeSpan dt)",
						"The simulation time reference passed as parameter is a "
								+ "null reference.",
						"Always make sure to use valid references only.");
				return; // time missing
			}

			if ((who1 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeSpan dt)",
						"The event and first entity references passed are both "
								+ "null references.",
						"Either Event or first entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who2 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeSpan dt)",
						"The event and second entity references passed are both "
								+ "null references.",
						"Either Event or second entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who3 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeSpan dt)",
						"The event and third entity references passed are both "
								+ "null references.",
						"Either Event or third entity references must be valid.");
				return; // no real parameters here anyway
			}

			if (!(who1 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeSpan dt)",
						"The first entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who2 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeSpan dt)",
						"The second entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who3 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeSpan dt)",
						"The third entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			TimeInstant time = TimeOperations.add(presentTime(), dt);
			// set time for being scheduled

			EventNote note = RentNote(who1, who2, who3, what, time, what.getSchedulingPriority(), _currentSchedulable);

			if (dt == TimeSpan.ZERO) { // preempting schedule for processes
				if (_currentProcess == null) { // currently Event -> no preemption
											   // inserted as first in the event-list with the current time as
											   // activation
					evList.insertAsFirst(note);
					return;
				} else { // currently SimProcess -> preempt!
					preemptSimProcess(note);
					return;
				}
			}

			// all parameters checked, now schedule Event
			evList.insert(note);

		}

		/// <summary>
		/// Schedules the event to happen at the specified time. Checks that only legal combinations of valid
		/// parameters are scheduled. &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid
		/// scheduling types &lt;/CAPTION&gt; &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling
		/// type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt;
		/// &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external
		/// event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="who">The Entity to be scheduled</param>
		/// <param name="what">The event to manipulate the entity</param>
		/// <param name="when">The point in simulation time for the event to happen</param>
	internal void schedule(Entity who, EventAbstract what, TimeInstant when) {

			if (when == null) {
				myExperiment.sendWarning(
						"Can't schedule Entity and Event at given "
								+ "time! Command ignored.", "Scheduler : "
								+ getName() + " Method: schedule(Entity who, "
								+ "EventAbstract what, TimeInstant when)",
						"The simulation time reference passed as parameter is a "
								+ "null reference.",
						"Always make sure to use valid references only.");
				return; // time missing
			}

			if ((who == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who, "
						+ "EventAbstract what, TimeInstant when)",
						"The Event and Entity references passed are both "
								+ "null references.",
						"Either Event or Entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who == null) && !(what is ExternalEvent)) {
				myExperiment.sendWarning("Can't schedule Event! Command ignored.",
						"Scheduler : " + getName()
								+ " Method: schedule(Entity who, "
								+ "EventAbstract what, TimeInstant when)",
						"The Entity reference passed is a null reference but the "
								+ "Event references is not an external event.",
						"If no valid Entity is given, the event must be of type "
								+ "external event.");
				return; // if no Entity it must be ExternalEvent
			}

			if (!(who is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who, "
						+ "EventAbstract what, TimeInstant when)",
						"The Entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (TimeInstant.isBefore(when, this.presentTime())) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: schedule(Entity who, "
										+ "EventAbstract what, TimeInstant when)",
								"The instant given is in the past.",
								"To schedule a Schedulable, use a TimeInstant no earlier than the present time. "
										+ "The present time can be obtained using the "
										+ "presentTime() method.");
				return;
				// I can't be scheduled, TimeInstant has already passed.
			}

			if (what != null)
			{
				if (what.getNumberOfEntities() > 1)
				{
					myExperiment.sendWarning("Can't schedule Entity and Event! "
							+ "Command ignored.", "Scheduler : " + getName()
							+ " Method: schedule(Entity who, "
							+ "Event what, TimeSpan dt)",
							"The method needs the correct Event to be scheduled with.",
							"You are using an event for multiple entities. You need an event for a single entity.");
					return; // Event needed with Entity
				}
			}

			// determine priority
			int priority = 0;
			if (what == null && who is SimProcess) {
				priority = ((SimProcess)who).getSchedulingPriority();
			} else if (what != null) {
				priority = what.getSchedulingPriority();
			}

			EventNote note = RentNote(who, null, null, what, when, priority, _currentSchedulable);

			if (when == presentTime()) { // preempting schedule for processes
				if (_currentProcess == null) { // currently Event -> no preemption
											   // inserted as first in the event-list with the current time as
											   // activation
					evList.insertAsFirst(note);
					return;
				} else { // currently SimProcess -> preempt!
					preemptSimProcess(note);
					return;
				}

			}

			// all parameters checked, now schedule Event
			evList.insert(note);

		}

		/// <summary>
		/// Schedules the event to happen at the specified time. Checks that only legal combinations of valid
		/// parameters are scheduled. &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid
		/// scheduling types &lt;/CAPTION&gt; &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling
		/// type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt;
		/// &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external
		/// event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="who1">The first entity to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled</param>
		/// <param name="what">The event to manipulate the entity</param>
		/// <param name="when">The point in simulation time for the event to happen</param>
		internal void schedule(Entity who1, Entity who2, EventOf2Entities what, TimeInstant when)
		
		{

			if (when == null) {
				myExperiment.sendWarning(
						"Can't schedule Entity and Event at given "
								+ "time! Command ignored.", "Scheduler : "
								+ getName() + " Method: schedule(Entity who1, Entity who2, "
								+ "EventOf2Entities what, TimeInstant when)",
						"The simulation time reference passed as parameter is a "
								+ "null reference.",
						"Always make sure to use valid references only.");
				return; // time missing
			}

			if ((who1 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, "
						+ "EventOf2Entities what, TimeInstant when)",
						"The event and first entity references passed are both "
								+ "null references.",
						"Either Event or first entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who2 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, "
						+ "EventOf2Entities what, TimeInstant when)",
						"The event and second entity references passed are both "
								+ "null references.",
						"Either Event or second entity references must be valid.");
				return; // no real parameters here anyway
			}

			if (!(who1 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, "
						+ "EventOf2Entities what, TimeInstant when)",
						"The first entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who2 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, "
						+ "EventOf2Entities what, TimeInstant when)",
						"The second entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (TimeInstant.isBefore(when, this.presentTime())) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: schedule(Entity who1, Entity who2, EventOf2Entities what, TimeInstant when)",
								"The instant given is in the past.",
								"To schedule a Schedulable, use a TimeInstant no earlier than the present time. "
										+ "The present time can be obtained using the "
										+ "presentTime() method.");
				return;
				// I can't be scheduled, TimeInstant has already passed.
			}

			EventNote note = RentNote(who1, who2, null, what, when, what.getSchedulingPriority(), _currentSchedulable);

			if (when == presentTime()) { // preempting schedule for processes
				if (_currentProcess == null) { // currently Event -> no preemption
											   // inserted as first in the event-list with the current time as
											   // activation
					evList.insertAsFirst(note);
					return;
				} else { // currently SimProcess -> preempt!
					preemptSimProcess(note);
					return;
				}

			}

			// all parameters checked, now schedule Event
			evList.insert(note);

		}

		/// <summary>
		/// Schedules the event to happen at the specified time. Checks that only legal combinations of valid
		/// parameters are scheduled. &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid
		/// scheduling types &lt;/CAPTION&gt; &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling
		/// type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt;
		/// &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external
		/// event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="who1">The first entity to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled</param>
		/// <param name="who3">The third entity to be scheduled</param>
		/// <param name="what">The event to manipulate the entity</param>
		/// <param name="when">The point in simulation time for the event to happen</param>
		internal void schedule (Entity who1, Entity who2, Entity who3, 
			EventOf3Entities what, TimeInstant when)
		{

			if (when == null) {
				myExperiment.sendWarning(
						"Can't schedule Entity and Event at given "
								+ "time! Command ignored.", "Scheduler : "
								+ getName() + " Method: schedule(Entity who1, Entity who2, Entity who3, "
								+ "EventOf3Entities what, TimeInstant when)",
						"The simulation time reference passed as parameter is a "
								+ "null reference.",
						"Always make sure to use valid references only.");
				return; // time missing
			}

			if ((who1 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeInstant when)",
						"The first Event and Entity references passed are both "
								+ "null references.",
						"Either Event or Entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who2 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeInstant when)",
						"The EEvent and second entity references passed are both "
								+ "null references.",
						"Either Event or second entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who3 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeInstant when)",
						"The event and third entity references passed are both "
								+ "null references.",
						"Either Event or third entity references must be valid.");
				return; // no real parameters here anyway
			}

			if (!(who1 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeInstant when)",
						"The first entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who2 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeInstant when)",
						"The second entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who3 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: schedule(Entity who1, Entity who2, Entity who3, "
						+ "EventOf3Entities what, TimeInstant when)",
						"The third entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (TimeInstant.isBefore(when, this.presentTime())) {
				myExperiment
						.sendWarning(
								"Can't reschedule Schedulable at given time! "
										+ "Command ignored.",
								"Scheduler : "
										+ getName()
										+ " Method: schedule(Entity who1, Entity who2, Entity who3, EventOf3Entities what, TimeInstant when)",
								"The instant given is in the past.",
								"To schedule a Schedulable, use a TimeInstant no earlier than the present time. "
										+ "The present time can be obtained using the "
										+ "presentTime() method.");
				return;
				// I can't be scheduled, TimeInstant has already passed.
			}

			EventNote note = RentNote(who1, who2, who3, what, when, what.getSchedulingPriority(), _currentSchedulable);

			if (when == presentTime()) { // preempting schedule for processes
				if (_currentProcess == null) { // currently Event -> no preemption
											   // inserted as first in the event-list with the current time as
											   // activation
					evList.insertAsFirst(note);
					return;
				} else { // currently SimProcess -> preempt!
					preemptSimProcess(note);
					return;
				}

			}

			// all parameters checked, now schedule Event
			evList.insert(note);

		}

		/// <summary>
		/// Schedules the given Entity and Event to happen straight after the given Schedulable is set to be
		/// activated. Note that the siulation time for the newly entered EventNote will be set to the
		/// Schedulable's time and the new EventNote will be inserted directly after the Schedulable's EventNote.
		/// &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid scheduling types &lt;/CAPTION&gt;
		/// &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling type&lt;/TH&gt; &lt;TH&gt;Entity
		/// object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;Event
		/// oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt; &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt; &lt;TD&gt;SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external event&lt;/TH&gt;
		/// &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt; &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="after">The Schedulable after which the new event-note is to be scheduled</param>
		/// <param name="who">The Entity to be scheduled</param>
		/// <param name="what">The event to be scheduled</param>
		internal void scheduleAfter(Schedulable after, Entity who, EventAbstract what) {

			if (after == null) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after "
						+ ", Entity who, EventAbstract what)",
						"The reference for the Schedulable to schedule after is a "
								+ "null references.",
						"Always check to use valid references.");
				return; // relative Schedulable missing
			}

			if (!after.isScheduled() && (after != _currentSchedulable)) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after "
						+ ", Entity who, EventAbstract what)",
						"The Schedulable to be scheduled after is not scheduled.",
						"The Schedulable taken as reference must be scheduled.");
				return; // relative Schedulable not scheduled
			}

			if ((who == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after "
						+ ", Entity who, EventAbstract what)",
						"The Event and Entity references passed are both "
								+ "null references.",
						"Either Event or Entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who == null) && !(what is ExternalEvent)) {
				myExperiment.sendWarning("Can't schedule Event! Command ignored.",
						"Scheduler : " + getName()
								+ " Method: scheduleAfter(Schedulabe "
								+ "after, Entity who, EventAbstract what)",
						"The Entity reference passed is a null reference but the "
								+ "Event references is not an external event.",
						"If no valid Entity is given, the event must be of type "
								+ "external event.");
				return; // if no Entity it must be ExternalEvent
			}

			if (!(who is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulabe "
						+ "after, Entity who, EventAbstract what)",
						"The Entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (what != null)
			{
				if (what.getNumberOfEntities() > 1)
				{
					myExperiment.sendWarning("Can't schedule Entity and Event! "
							+ "Command ignored.", "Scheduler : " + getName()
							+ " Method: scheduleAfter(Schedulabe "
							+ "after, Entity who, EventAbstract what)",
							"The method needs the correct Event to be scheduled with.",
							"You are using an event for multiple entities. You need an event for a single entity.");
					return; // Event with only one entity needed
				}
			}

			if (after != _currentSchedulable) {
				// all parameters checked, now go on and schedule
				EventNote afterNote = after.getEventNotes()[after.getEventNotes().Count - 1];
				evList.insertAfter(afterNote, RentNote(who, null, null, what, afterNote
						.getTime(), afterNote.getPriority(), _currentSchedulable));
				// sets the time equivalent to the Schedulable's
			} else {
				evList.insertAsFirst(RentNote(who, null, null, what, presentTime(), int.MaxValue, _currentSchedulable));
			}

		}

		/// <summary>
		/// Schedules the given Entity and Event to happen straight after the given Schedulable is set to be
		/// activated. Note that the siulation time for the newly entered EventNote will be set to the
		/// Schedulable's time and the new EventNote will be inserted directly after the Schedulable's EventNote.
		/// &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid scheduling types &lt;/CAPTION&gt;
		/// &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling type&lt;/TH&gt; &lt;TH&gt;Entity
		/// object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;Event
		/// oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt; &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt; &lt;TD&gt;SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external event&lt;/TH&gt;
		/// &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt; &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="after">The Schedulable after which the new event-note is to be scheduled</param>
		/// <param name="who1">The first entity to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled</param>
		/// <param name="what">The event to be scheduled</param>
		internal void scheduleAfter(Schedulable after, Entity who1, Entity who2, 
			EventOf2Entities what)
		{

			if (after == null) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The reference for the Schedulable to schedule after is a "
								+ "null references.",
						"Always check to use valid references.");
				return; // relative Schedulable missing
			}

			if (!after.isScheduled() && (after != _currentSchedulable)) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The Schedulable to be scheduled after is not scheduled.",
						"The Schedulable taken as reference must be scheduled.");
				return; // relative Schedulable not scheduled
			}

			if ((who1 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The event and Entity 'who1' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who1' references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who2 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The event and Entity 'who2' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who2' references must be valid.");
				return; // no real parameters here anyway
			}

			if (!(who1 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The Entity 'who1' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who2 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The Entity 'who2' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (after != _currentSchedulable) {
				// all parameters checked, now go on and schedule
				EventNote afterNote = after.getEventNotes()[after.getEventNotes().Count - 1];
				evList.insertAfter(afterNote, RentNote(who1, who2, null, what, afterNote
						.getTime(), afterNote.getPriority(), _currentSchedulable));
				// sets the time equivalent to the Schedulable's
			} else {
				evList.insertAsFirst(RentNote(who1, who2, null, what, presentTime(),int.MaxValue, _currentSchedulable));
			}

		}

		/// <summary>
		/// Schedules the given Entity and Event to happen straight after the given Schedulable is set to be
		/// activated. Note that the siulation time for the newly entered EventNote will be set to the
		/// Schedulable's time and the new EventNote will be inserted directly after the Schedulable's EventNote.
		/// &lt;DIV align=center&gt; &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid scheduling types &lt;/CAPTION&gt;
		/// &lt;TR&gt; &lt;TH&gt;&lt;DIV align=center&gt;scheduling type&lt;/TH&gt; &lt;TH&gt;Entity
		/// object&lt;/TH&gt; &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;Event
		/// oriented&lt;/TH&gt; &lt;TD&gt;Event or SimProcess&lt;/TD&gt; &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt;
		/// &lt;TR&gt; &lt;TH&gt;process oriented&lt;/TH&gt; &lt;TD&gt;SimProcess&lt;/TD&gt;
		/// &lt;TD&gt;null&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external event&lt;/TH&gt;
		/// &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external event&lt;/TD&gt; &lt;/TR&gt; &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="after">The Schedulable after which the new event-note is to be scheduled</param>
		/// <param name="who1">The first entity to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled</param>
		/// <param name="who3">The third entity to be scheduled</param>
		/// <param name="what">The event to be scheduled</param>
		public void scheduleAfter
			(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)
			
			{

			if (after == null) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The reference for the Schedulable to schedule after is a "
								+ "null references.",
						"Always check to use valid references.");
				return; // relative Schedulable missing
			}

			if (!after.isScheduled() && (after != _currentSchedulable)) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The Schedulable to be scheduled after is not scheduled.",
						"The Schedulable taken as reference must be scheduled.");
				return; // relative Schedulable not scheduled
			}

			if ((who1 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The event and Entity 'who1' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who1' references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who2 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The event and Entity 'who2' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who2' references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who3 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The event and Entity 'who3' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who3' references must be valid.");
				return; // no real parameters here anyway
			}

			if (!(who1 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who1' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who2 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who2' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who3 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who3' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (after != _currentSchedulable) {
				// all parameters checked, now go on and schedule
				EventNote afterNote = after.getEventNotes()[after.getEventNotes().Count- 1];
				evList.insertAfter(afterNote, RentNote(who1, who2, who3, what, afterNote
						.getTime(), afterNote.getPriority(), _currentSchedulable));
				// sets the time equivalent to the Schedulable's
			} else {
				evList.insertAsFirst(RentNote(who1, who2, who3, what, presentTime(), int.MaxValue, _currentSchedulable));
			}

		}

		/// <summary>
		/// Schedules the given Entity and Event to happen straight before the given Schedulable is scheduled. Note
		/// that the simulation time for the newly entered EventNote will be set to the Schedulable's time and the
		/// new EventNote will be inserted directly before the Schedulable's EventNote. &lt;DIV align=center&gt;
		/// &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid scheduling types &lt;/CAPTION&gt; &lt;TR&gt;
		/// &lt;TH&gt;&lt;DIV align=center&gt;scheduling type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt;
		/// &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt;
		/// &lt;TD&gt;Event or SimProcess&lt;/TD&gt; &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt;
		/// &lt;TH&gt;process oriented&lt;/TH&gt; &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt;
		/// &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external
		/// event&lt;/TD&gt; &lt;/TR&gt; &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="before">The Schedulable before which the new event-note is to be scheduled</param>
		/// <param name="who">The Entity to be scheduled</param>
		/// <param name="what">The event to be scheduled</param>
		internal void scheduleBefore(Schedulable before, Entity who, EventAbstract what) {

			if (before == null) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: Schedulable before, Entity who, EventAbstract what",
						"The reference for the Schedulable to schedule before is a "
								+ "null references.",
						"Always check to use valid references.");
				return; // relative Schedulable missing
			}

			if (!before.isScheduled()) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: Schedulable before, Entity who, EventAbstract what",
						"The Schedulable to schedule after is not scheduled.",
						"The Schedulable taken as reference must be scheduled.");
				return; // relative Schedulable not scheduled
			}

			if ((who == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: Schedulable before, Entity who, EventAbstract what",
						"The Event and Entity references passed are both "
								+ "null references.",
						"Either Event or Entity references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who == null) && !(what is ExternalEvent)) {
				myExperiment.sendWarning("Can't schedule Event! Command ignored.",
						"Scheduler : " + getName()
						+ " Method: Schedulable before, Entity who, EventAbstract what",
						"The Entity reference passed is a null reference but the "
								+ "Event references is not an external event.",
						"If no valid Entity is given, the event must be of type "
								+ "external event.");
				return; // if no Entity it must be ExternalEvent
			}

			if (!(who is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: Schedulable before, Entity who, EventAbstract what",
						"The Entity needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (what != null)
			{
				if (what.getNumberOfEntities() > 1)
				{
					myExperiment.sendWarning("Can't schedule Entity and Event! "
							+ "Command ignored.", "Scheduler : " + getName()
							+ " Method: Schedulable before, Entity who, EventAbstract what",
							"The method needs the correct Event to be scheduled with.",
							"You are using an event for multiple entities. You need an event for a single entity.");
					return; // Event needed with Entity
				}
			}

			// all parameters checked, now go on and schedule
			EventNote beforeNote = before.getEventNotes()[0];
			evList.insertBefore(beforeNote, RentNote(who, null, null, what, beforeNote
					.getTime(), beforeNote.getPriority(), _currentSchedulable));
			// sets the time equivalent to the Schedulable's
		}


		/// <summary>
		/// Schedules the given Entity and Event to happen straight before the given Schedulable is scheduled. Note
		/// that the simulation time for the newly entered EventNote will be set to the Schedulable's time and the
		/// new EventNote will be inserted directly before the Schedulable's EventNote. &lt;DIV align=center&gt;
		/// &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid scheduling types &lt;/CAPTION&gt; &lt;TR&gt;
		/// &lt;TH&gt;&lt;DIV align=center&gt;scheduling type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt;
		/// &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt;
		/// &lt;TD&gt;Event or SimProcess&lt;/TD&gt; &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt;
		/// &lt;TH&gt;process oriented&lt;/TH&gt; &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt;
		/// &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external
		/// event&lt;/TD&gt; &lt;/TR&gt; &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="before">The Schedulable before which the new event-note is to be scheduled</param>
		/// <param name="who1">The first entity to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled</param>
		/// <param name="what">The event to be scheduled</param>
		internal void scheduleBefore(Schedulable before, Entity who1, Entity who2, EventOf2Entities what)
		{

			if (before == null) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleBefore(Schedulable before, Entity who1, Entity who2, EventOf2Entities what)",
						"The reference for the Schedulable to schedule before is a "
								+ "null references.",
						"Always check to use valid references.");
				return; // relative Schedulable missing
			}

			if (!before.isScheduled()) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleBefore(Schedulable before, Entity who1, Entity who2, EventOf2Entities what)",
						"The Schedulable to schedule after is not scheduled.",
						"The Schedulable taken as reference must be scheduled.");
				return; // relative Schedulable not scheduled
			}

			if ((who1 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The event and Entity 'who1' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who1' references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who2 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The event and Entity 'who2' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who2' references must be valid.");
				return;

			}

			if (!(who1 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The Entity 'who1' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who2 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, EventOf2Entities what)",
						"The Entity 'who2' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			// all parameters checked, now go on and schedule
			EventNote beforeNote = before.getEventNotes()[0];
			evList.insertBefore(beforeNote, RentNote(who1, who2, null, what, beforeNote
					.getTime(), beforeNote.getPriority(), _currentSchedulable));
			// sets the time equivalent to the Schedulable's
		}

		/// <summary>
		/// Schedules the given Entity and Event to happen straight before the given Schedulable is scheduled. Note
		/// that the simulation time for the newly entered EventNote will be set to the Schedulable's time and the
		/// new EventNote will be inserted directly before the Schedulable's EventNote. &lt;DIV align=center&gt;
		/// &lt;TABLE BORDER &gt; &lt;CAPTION&gt;Valid scheduling types &lt;/CAPTION&gt; &lt;TR&gt;
		/// &lt;TH&gt;&lt;DIV align=center&gt;scheduling type&lt;/TH&gt; &lt;TH&gt;Entity object&lt;/TH&gt;
		/// &lt;TH&gt;Event object&lt;/TH&gt; &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;Event oriented&lt;/TH&gt;
		/// &lt;TD&gt;Event or SimProcess&lt;/TD&gt; &lt;TD&gt;Event&lt;/TD&gt; &lt;/TR&gt; &lt;TR&gt;
		/// &lt;TH&gt;process oriented&lt;/TH&gt; &lt;TD&gt;SimProcess&lt;/TD&gt; &lt;TD&gt;null&lt;/TD&gt;
		/// &lt;/TR&gt; &lt;TR&gt; &lt;TH&gt;external event&lt;/TH&gt; &lt;TD&gt;null&lt;/TD&gt; &lt;TD&gt;external
		/// event&lt;/TD&gt; &lt;/TR&gt; &lt;/TABLE&gt; &lt;/DIV&gt;
		/// </summary>
		/// <param name="before">The Schedulable before which the new event-note is to be scheduled</param>
		/// <param name="who1">The first entity to be scheduled</param>
		/// <param name="who2">The second entity to be scheduled</param>
		/// <param name="who3">The third entity to be scheduled</param>
		/// <param name="what">The event to be scheduled</param>
		internal void scheduleBefore(Schedulable before, Entity who1, Entity who2, Entity who3, 
			EventOf3Entities what) 
		{

			if (before == null) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleBefore(Schedulable before, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The reference for the Schedulable to schedule before is a "
								+ "null references.",
						"Always check to use valid references.");
				return; // relative Schedulable missing
			}

			if (!before.isScheduled()) {
				myExperiment.sendWarning("Can't schedule after Schedulable! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleBefore(Schedulable before, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The Schedulable to schedule after is not scheduled.",
						"The Schedulable taken as reference must be scheduled.");
				return; // relative Schedulable not scheduled
			}

			if ((who1 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The event and Entity 'who1' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who1' references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who2 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The event and Entity 'who2' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who2' references must be valid.");
				return; // no real parameters here anyway
			}

			if ((who3 == null) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The event and Entity 'who3' references passed are both "
								+ "null references.",
						"Either Event or Entity 'who3' references must be valid.");
				return; // no real parameters here anyway
			}

			if (!(who1 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who1' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who2 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who2' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			if (!(who3 is SimProcess) && (what == null)) {
				myExperiment.sendWarning("Can't schedule Entity and Event! "
						+ "Command ignored.", "Scheduler : " + getName()
						+ " Method: scheduleAfter(Schedulable after, Entity who1, Entity who2, Entity who3, EventOf3Entities what)",
						"The Entity 'who3' needs a valid Event to be scheduled with.",
						"Only SimProcesses may be scheduled without events.");
				return; // Event needed with Entity
			}

			// all parameters checked, now go on and schedule
			EventNote beforeNote = before.getEventNotes()[0];
			evList.insertBefore(beforeNote, RentNote(who1, who2, who3, what, beforeNote
					.getTime(), beforeNote.getPriority(), _currentSchedulable));
			// sets the time equivalent to the Schedulable's
		}

		/// <summary>
		/// Creates and sends a debugnote to the experiment's messagedistributor. These debugnotes express the
		/// internal state of the event-list to visualize the changes of state to help find bugs. Classes
		/// <c>Scheduler</c> and <c>Queue</c> both produce debugnotes if set to do so representing the data stored
		/// inside them. The information about the simulation time is extracted from the experiment and must not be
		/// given as a parameter.
		/// </summary>
		/// <param name="description">
		/// The description of the event-list's internal state to be passed with this debugnote
		/// </param>
		void sendDebugNote(string description) {

			myExperiment.getMessageManager().receive(
					new DebugNote(myExperiment.getModel(), presentTime(),
							getName(), description));

		}

		/// <summary>Sets the execution speed rate.</summary>
		/// <param name="executionSpeedRate">the execution speed rate</param>
		/// <remarks>author: Felix Klueckmann</remarks>
		public void setExecutionSpeedRate(double executionSpeedRate) {
			lock (_syncLock) {
				this._executionSpeedRate = executionSpeedRate;
				this._timeReset = true;
				Monitor.PulseAll(_syncLock);
			}
		}

		/// <summary>
		/// Sets the flag indicating if the event-list processes concurrent Events in random order or not. If a
		/// change occurred the event-list has to be changed accordingly.
		/// </summary>
		/// <param name="randomizing"><c>true</c> forces random order, <c>false</c> forces "linear" order</param>
		/// <remarks>author: Ruth Meyer</remarks>
		public void setRandomizingConcurrentEvents(bool randomizing) {
			// nothing new?
			if (this._randomizingConcurrentEvents == randomizing) {
				return; // just keep everything as is
			}
			// ok, we have to change the event-list (oh bother!)
			this._randomizingConcurrentEvents = randomizing;
			EventList newList = randomizing ? new RandomizingEventTreeList() : new EventHeapList();

			// copy old list into new list
			EventNote note = this.evList.firstNote();
			while (note != null) {
				newList.insert(note);
				note = this.evList.nextNote(note);
			}
			this.evList = newList;
		}

		/// <summary>Returns whether the simulation has finished.</summary>
		bool simFinished() {

			return simulationFinished;

		}

		/// <summary>Signals that the experiment is stopped.</summary>
		/// <remarks>author: Felix Klueckmann</remarks>
		public void signalStop() {
			lock (_syncLock) {
				this._timeReset = true;
				Monitor.PulseAll(_syncLock);
			}
		}

		/// <summary>
		/// Returns a string representation of the current state of the event-list. The string is built by
		/// concatenating all string representations of the contained entities, events and TimeInstant objects
		/// calling their <c>toString()</c> methods.
		/// </summary>
		/// <returns>The string representation of the queuelist</returns>
		public string toString() {

			StringBuilder buffer = new StringBuilder(); // strings
			StringBuilder enBuff = null; // Buffer for Entities
			EventAbstract evBuff; // Buffer for events

			TimeInstant tiBuff; // Buffer for Instants

			buffer.Append("At " + this.presentTime().toString());

			if (this._currentEvent != null) {
				buffer.Append(", current event [");
				if (this._currentEvent.getNumberOfEntities() == 0) {
					buffer.Append("-");
				} else {
					buffer.Append(this._currentEntity1);
					if (this._currentEntity2 != null) buffer.Append("," + this._currentEntity2);
					if (this._currentEntity3 != null) buffer.Append("," + this._currentEntity3);
				}
				buffer.Append("][" + this._currentEvent + "]");
			} else if (this._currentProcess != null) {
				buffer.Append(" current process [" + this._currentProcess + "]");
			}

			buffer.Append(" <br>EvenList: <br> ");

			if (evList.isEmpty()) {
				// if empty, give short note
				buffer.Append("- empty -");
				return buffer.ToString();
			} else {

				// go thru list and present Event notes
				int i = 0; // counter for position

				//for (EventNote iNote = evList.firstNote(); iNote != null; iNote = evList
				//		.nextNote(iNote)) { // loop thru list

				foreach(var iNote in evList.iterators()) { 
					buffer.Append(i + ":");
					i++; // increment counter

					// set Event
					evBuff = iNote.getEvent();

					// set Entities
					Entity e1 = iNote.getEntity1();
					Entity e2 = iNote.getEntity2();
					Entity e3 = iNote.getEntity3();
					if (e1 == null) {
						enBuff = null;
					} else if (e2 == null) {
						enBuff = new StringBuilder(e1.toString());
					} else if (e3 == null) {
						enBuff = new StringBuilder(e1.toString() + "," + e2.toString());
					} else {
						enBuff = new StringBuilder(e1.toString() + "," + e2.toString() + "," + e3.toString());
					}

					// set Time
					tiBuff = iNote.getTime();

					// set Entity
					if (enBuff == null)
						buffer.Append("[-]");
					else
						buffer.Append("[" + enBuff.ToString() + "]");
					// embrace in brackets

					// set Event
					if (evBuff == null)
						buffer.Append("[-]");
					else
						buffer.Append("[" + evBuff.toString() + "]");
					// embrace in brackets

					// set time
					if (tiBuff == null)
						buffer.Append("[-]"); // can never happen...
					else
						buffer.Append("[" + tiBuff.toString() + "]<br>");
					// embrace in brackets
					// and make a linebreak
				}

			}
			// get it to the client who asked for it...
			return buffer.ToString();
		}
	}

    internal class LinkedBlockingQueue<T> : BlockingQueue<RealTimeEventWrapper>
    {
    }

    internal class BlockingQueue<T>
    {
        internal bool isEmpty()
        {
            throw new NotImplementedException();
        }

        internal RealTimeEventWrapper poll()
        {
            throw new NotImplementedException();
        }

        internal void put(RealTimeEventWrapper what)
        {
            throw new NotImplementedException();
        }
    }
}