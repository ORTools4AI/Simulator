// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler, Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using System.Collections;
using System.Numerics;
using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.advancedModellingFeatures;
using System.Diagnostics;

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * SimProcess represents entities with an own active lifecycle. Since
     * SimProcesses are in fact special entities with extended capabilities (esp.
     * the method <code>lifeCycle()</code>), they inherit from Entity and thus can also
     * be used in conjunction with events. So they can be handled in both ways,
     * event- and process-oriented. Clients are supposed to implement the
     * <code>lifeCycle()</code> method to specify the individual behaviour of a special
     * SimProcess subclass. Since implementing activity- and transaction-oriented
     * synchronization mechanisms requires significant changes in this class,
     * methods that have been implemented by Soenke Claassen have been marked.
     */
	public abstract class SimProcess : Entity
	{

		/**
		 * The scheduling priority of the process.
		 */
		private int _mySchedulingPriority;

		/**
		 * Displays the current blocked status of this SimProcess. A SimProcess is
		 * blocked whenever it has to wait inside a queue or synchronization object.
		 */
		protected bool _isBlocked;

		/**
		 * Displays the current status of this SimProcess. Is <code>true</code> if
		 * lifeCycle method has finished, <code>false</code> if it is still running
		 * or has not been started yet
		 */
		protected bool _isTerminated;

		/**
		 * Displays if the thread in of control of this SimProcess is already the
		 * associated simthread. Is <code>true</code> if the simthread is active and
		 * is carrying on its lifeCycle. Is <code>false</code> if it has not started
		 * its lifeCycle yet or is terminated already.
		 */
		protected bool _isRunning;

		/**
		 * Determines whether or not the life cycle of this process will start
		 * again after finishing. Default is <code>false</code>, i.e. the lifecycle
		 * will only by executed once.
		 */
		private bool _isRepeating;

		/**
		 * If this SimProcess is cooperating as a slave with a master process, it
		 * keeps a reference to its master here. Master is set in the
		 * <code>cooperate()</code> -method, when the slave cooperates with his
		 * master and deleted every time the slave process is activated.
		 * 
		 * @author Soenke Claassen
		 */
		private SimProcess _master;

		/**
		 * If this SimProcess is cooperating as a slave it has to wait in this
		 * waitQueue until a master is cooperating with it.
		 * 
		 * @author Soenke Claassen
		 */
		private ProcessQueue<SimProcess> _slaveWaitQueue;

		/**
		 * The <code>InterruptCode</code> with which this SimProcess is interrupted.
		 * 
		 * @author Soenke Claassen
		 */

		private InterruptCode _irqCode;

		/**
		 * The <code>InterruptException</code> with which this SimProcess is
		 * interrupted.
		 * 
		 * @author Soenke Claassen
		 */
		protected InterruptException _irqException;

		/**
		 * The <code>Vector</code> holding all the resources this SimProcess is
		 * using at the moment.
		 * 
		 * @author Soenke Claassen
		 */
		private readonly List<Resource> _usedResources;

		/**
		 * A reference to the container this SimProcess belongs to. Is
		 * <code>null</code> as long as this SimProcess is not contained in any
		 * <code>ComplexSimProcess</code>.
		 * 
		 * @author Soenke Claassen
		 */
		private ComplexSimProcess _supervisor;

		/**
		 * The realTime deadline for this SimProcess in nanoseconds. In case of a
		 * real-time execution (i. e. the execution speed rate is set to a positive
		 * value) the Scheduler will produce a warning message if a deadline is
		 * missed.
		 */
		private long _realTimeConstraint;

		/**
		 * The Event which will interrupt the current SimProcess at a given point in
		 * simulation time if it is not removed from the event list before that time
		 * instant.
		 */
		private ExternalEvent _currentlyScheduledDelayedInterruptEvent;

		/**
		 * The last Schedulable that has activated this process.
		 */
		private Schedulable _activatedBy;

		/// <summary>The most general constructor of a SimProcess.</summary>
		/// <param name="owner">The model this SimProcess is associated to</param>
		/// <param name="name">The name of the SimProcess</param>
		/// <param name="repeating">
		/// Flag to set the SimProcess' repeating behaviour: If set to <c>true</c>, the lifeCycle will be executed
		/// again after completion, while <c>false</c> will create a process whose lifeCycle is executed only once.
		/// </param>
		/// <param name="showInTrace">
		/// Flag for showing SimProcess in trace-files. Set it to <c>true</c> if SimProcess should show up in
		/// trace. Set it to <c>false</c> if SimProcess should not be shown in trace.
		/// </param>
		public SimProcess(Model owner, String name, bool repeating, bool showInTrace) : base(owner, name, showInTrace)
		{



			// init variables
			_mySchedulingPriority = 0;
			_isBlocked = false; // not waiting in queue so far
			_isRunning = false; // not running so far
			_isTerminated = false; // not terminated either
			_master = null; // this SimProcess has no master, so far
			_slaveWaitQueue = null; // this SimProcess is not waiting in any queue
			_irqCode = null; // this is not interrupted
			_irqException = null; // this is not interrupted
			_isRepeating = repeating;

			// set up the Vector holding the used Resources
			_usedResources = new List<Resource>();

			// this SimProcess is not contained in any ComplexSimProcess yet
			_supervisor = null;
		}

		/// <summary>Short-cut constructor of a SimProcess whose <c>lifeCycle()</c> is only executed once.</summary>
		/// <param name="owner">The model this SimProcess is associated to</param>
		/// <param name="name">The name of the SimProcess</param>
		/// <param name="showInTrace">
		/// Flag for showing SimProcess in trace-files. Set it to <c>true</c> if SimProcess should show up in
		/// trace. Set it to <c>false</c> if SimProcess should not be shown in trace.
		/// </param>
		public SimProcess(Model owner, string name, bool showInTrace) : this(owner, name, false, showInTrace)
		{


		}

		/// <summary>Creates a SimProcess named after its concrete class, with tracing off.</summary>
		public SimProcess(Model owner) : this(owner, "_auto_", false)
		{
			rename(GetType().Name);
		}

		/// <summary>
		/// Schedules the SimProcess to be activated at the present point in simulation time, yielding the same
		/// result as calling <c>activate(new TimeSpan(0))</c>. The process will continue executing its
		/// <c>lifeCycle</c> method after the current <c>Schedulable</c> withdraws program control. Note, however,
		/// other <c>Schedulable</c>s may receive program control before the activated progess, compare remarks to
		/// method <c>activate(TimeSpan dt)</c>.
		/// </summary>
		public void activate()
		{
			if (isBlocked())
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : " + getName() + " Method: activate()",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be activated by other SimProcesses or "
								+ "events!");
				return; // is blocked in some synch construction
			}

			// tell in the trace when the SimProcess will be activated
			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					sendTraceNote("activates itself now");
				}
				else
				{ // this is not the currently running SimProcess
					sendTraceNote("activates " + getQuotedName() + " now");
				}
			}

			// schedule this SimProcess
			getModel().getExperiment().getScheduler()
					.schedule(this, null, new TimeSpan(0));

			// debug output
			if (currentlySendDebugNotes())
			{
				sendDebugNote("is activated on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

			resetMaster(); // if activate(TimeSpan dt) is called for this
						   // SimProcess,
						   // there is no Master anymore controlling it.
		}

		/// <param name="dt">
		/// The offset to the current simulation time that this SimProcess is due to be activated
		/// </param>
		/// <remarks>
		/// Replaced by activate(TimeSpan dt). Schedules the SimProcess to be activated at the given time offset to
		/// the current simulation time. This will allow a SimProcess to continue executing its <c>lifeCycle</c>
		/// method.
		/// </remarks>
		//@Deprecated
		public void activate(SimTime dt)
		{
			activate(SimTime.toTimeSpan(dt));

		}

		/// <summary>
		/// Schedules the SimProcess to be activated at the given point in simulation time. This will allow a
		/// SimProcess to continue executing its <c>lifeCycle</c> method. A <c>TimeInstant</c> representing the
		/// current time instant, i.e. an argument equal to <c>presentTime()</c> can be used to immediately
		/// activate the process, implicitly stalling the current process until the activated process withdraws
		/// program control, referred to as process preemption. If preemption of the current process is not
		/// desired, use <c>activateAfter(this)</c> to schedule the activation of process directly after this
		/// current process (or other <c>Schedulable</c>) or <c>activate(new TimeSpan(0))</c> (or its shortcut
		/// <c>activate()</c>) to activate a process directly after all other event list entries schedule to occur
		/// at the current instant in time with respect to entity prioritization.
		/// </summary>
		/// <param name="when">The point in simulation time this process is to be activated.</param>
		public void activate(TimeInstant when)
		{
			if (isBlocked())
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: activate(TimeInstant when)",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be activated by other SimProcesses or "
								+ "events!");
				return; // is blocked in some synch construction
			}

			if (when == null)
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : " + getName() + " Method:  void activate"
								+ "(TimeInstant when)",
						"The simulation time given as parameter is a null reference",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method");
				return; // no proper parameter
			}
			// tell in the trace when the SimProcess will be activated
			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					if (when == presentTime())
					{
						sendTraceNote("activates itself immediately (NOW)");
					}
					else
					{
						if (TimeInstant.isEqual(when, presentTime()))
						{
							sendTraceNote("activates itself now");
						}
						else
						{
							sendTraceNote("activates itself at " + when.toString());

						}
					}
				}
				else
				{ // this is not the currently running SimProcess

					if (when == presentTime())
					{
						sendTraceNote("activates " + getQuotedName()
								+ " immediately (NOW)");
					}
					else
					{
						// stand auch oben: if (dt.getTimeValue() == 0.0) {
						if (TimeInstant.isEqual(when, presentTime()))
						{
							sendTraceNote("activates " + getQuotedName() + " now");
						}
						else
						{
							sendTraceNote("activates " + getQuotedName() + " at "
									+ when.toString());
						}
					}
				}
			}

			// schedule this SimProcess
			getModel().getExperiment().getScheduler().schedule(this, null, when);

			// debug output
			if (currentlySendDebugNotes())
			{
				sendDebugNote("is activated on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

			resetMaster(); // if activate(TimeInstant when) is called for this
						   // SimProcess,
						   // there is no Master anymore controlling it.
		}

		/// <summary>
		/// Schedules the SimProcess to be activated at the specified point in simulation time. The point of time
		/// is given as an offset to the current simulation time. This will allow a SimProcess to continue
		/// executing its <c>lifeCycle</c> method. Thus in contrast to the entity, no Event is needed for
		/// scheduling here. The special argument <c>TimeSpan.ZERO</c> can be used to immediately activate the
		/// process, implicitly stalling the current process (if any) until the activated process withdraws program
		/// control, referred to as process preemption. If preemption of the current process is not desired, use
		/// <c>activateAfter(this)</c> to schedule the activation of process directly after this current process
		/// (or other <c>Schedulable</c>) or <c>activate(new TimeSpan(0))</c> (or its shortcut <c>activate()</c>)
		/// to activate a process directly after all other event list entries schedule to occur at the current
		/// instant in time with respect to entity prioritization.
		/// </summary>
		/// <param name="dt">The offset to the current simulation time this process is to be activated</param>
		public void activate(TimeSpan dt)
		{
			if (isBlocked())
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: activate(TimeSpan dt)",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be activated by other SimProcesses or "
								+ "events!");
				return; // is blocked in some synch construction
			}

			if (dt == null)
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : " + getName() + " Method:  void activate"
								+ "(TimeSpan dt)",
						"The simulation time given as parameter is a null reference",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method");
				return; // no proper parameter
			}
			// tell in the trace when the SimProcess will be activated
			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					if (dt == TimeSpan.ZERO)
					{
						sendTraceNote("activates itself immediately");
					}
					else
					{
						if (TimeSpan.isEqual(dt, TimeSpan.ZERO))
						{
							sendTraceNote("activates itself now");
						}
						else
						{
							sendTraceNote("activates itself at "
									+ TimeOperations.add(presentTime(), dt)
											.toString());
						}
					}
				}
				else
				{ // this is not the currently running SimProcess

					if (dt == TimeSpan.ZERO)
					{
						sendTraceNote("activates " + getQuotedName()
								+ " immediately");
					}
					else
					{
						if (TimeSpan.isEqual(dt, TimeSpan.ZERO))
						{
							sendTraceNote("activates " + getQuotedName() + " now");
						}
						else
						{
							sendTraceNote("activates "
									+ getQuotedName()
									+ " at "
									+ TimeOperations.add(presentTime(), dt)
											.toString());
						}
					}
				}
			}

			// schedule this SimProcess
			getModel().getExperiment().getScheduler().schedule(this, null, dt);

			// debug output
			if (currentlySendDebugNotes())
			{
				sendDebugNote("is activated on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

			resetMaster(); // if activate(TimeSpan dt) is called for this
						   // SimProcess,
						   // there is no Master anymore controlling it.

		}

		/// <summary>Schedules this process to activate after <paramref name="delay"/> time units.</summary>
		public void activate(double delay) => activate(new TimeSpan(delay));

		/// <summary>
		/// Schedules this SimProcess to be activated directly after the given Schedulable, which itself is already
		/// scheduled. Note that this SimProcess' point of simulation time will be set to be the same as the
		/// Schedulable's time. Thus this SimProcess will continue to execute its <c>lifeCycle()</c> method
		/// directly after the given Schedulable but the simulation clock will not change. Please make sure that
		/// the Schedulable given as parameter is actually scheduled.
		/// </summary>
		/// <param name="after">The Schedulable this SimProcess should be scheduled after</param>
		public void activateAfter(Schedulable after)
		{

			if (after == null)
			{
				sendWarning(
						"Can't activate this SimProcess after the given SimProcess "
								+ "parameter! Command ignored.", "SimProcess : "
								+ getName() + " Method: void "
								+ "activateAfter(Schedulable after)",
						"The Schedulable given as parameter is a null reference",
						"Be sure to have a valid Schedulable reference before "
								+ "calling this method");
				return; // no proper parameter
			}

			if (isBlocked())
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: void activateAfter(Schedulable after)",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be activated by other SimProcesses or "
								+ "events!");
				return; // is blocked in some synch construction
			}

			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					sendTraceNote("activates itself after " + getQuotedName());
				}
				else
				{
					sendTraceNote("activates " + getQuotedName() + " after "
							+ after.getQuotedName());
				}
			}

			// schedule this SimProcess
			getModel().getExperiment().getScheduler()
					.scheduleAfter(after, this, null);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("is activated after " + after.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Schedules this SimProcess to be activated directly before the given Schedulable, which itself is
		/// already scheduled. Note that this SimProcess' point of simulation time will be set to be the same as
		/// the Schedulable's time. Thus this SimProcess will continue to execute its <c>lifeCycle()</c> method
		/// directly before the given Schedulable but the simulation clock will not change. Please make sure that
		/// the Schedulable given as parameter is actually scheduled.
		/// </summary>
		/// <param name="before">The Schedulable this SimProcess should be scheduled before</param>
		public void activateBefore(Schedulable before)
		{

			if (before == null)
			{
				sendWarning("Can't activate this SimProcess before the given "
						+ "SimProcess parameter", "SimProcess : " + getName()
						+ " Method: void activateBefore" + "(Schedulable before)",
						"The Schedulable given as parameter is a null reference",
						"Be sure to have a valid Schedulable reference before "
								+ "calling this method");
				return; // no proper parameter
			}

			if (isBlocked())
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : "
								+ getName()
								+ " Method: void activateBefore(Schedulable before)",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be activated by other SimProcesses or "
								+ "events!");
				return; // is blocked in some synch construction
			}

			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					sendTraceNote("activates itself before "
							+ before.getQuotedName());
				}
				else
				{
					sendTraceNote("activates " + getQuotedName() + " before "
							+ before.getQuotedName());
				}
			}

			// schedule this SimProcess
			getModel().getExperiment().getScheduler()
					.scheduleBefore(before, this, null);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("activateBefore " + before.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

			// hand control over to scheduler only if this is
			// a running thread of SimProcess
			// if ( isRunning ) passivate();

			resetMaster(); // if activateBefore() is called for this SimProcess,
						   // there is no Master anymore controlling it.

		}

		/**
		 * 
		 * Clears the currently scheduled delayed interrupt so that it wont be
		 * performed. This Method should be called to cancel a previously scheduled
		 * delayed interrupt. This is typically the case if all steps to be covered
		 * by the delayed interrupt have been performed in time (before the delayed
		 * interrupt could be executed).
		 * 
		 */
		public void cancelInterruptDelayed()
		{

			if (isDelayedInterruptScheduled())
			{
				sendTraceNote("canceling delayed interrupt scheduled at "
						+ _currentlyScheduledDelayedInterruptEvent.scheduledNext());
				_currentlyScheduledDelayedInterruptEvent.cancel();
				_currentlyScheduledDelayedInterruptEvent = null;
			}
			else
			{
				sendWarning(
						"Cannot cancel a delayed interrupt because no delayed interrupt is scheduled. Action ignored.",
						"SimProcess " + getName()
								+ " Method: cancelInterruptDelayed()",
						"No delayed interrupt has been scheduled.",
						"You can use the Method isDelayedInterruptScheduled() on a SimProcess to test whether a delayed interrupt has been scheduled for it.");
			}
		}

		public bool isDelayedInterruptScheduled()
		{
			return _currentlyScheduledDelayedInterruptEvent != null;
		}

		/// <summary>
		/// Returns <c>true</c> if this process can cooperate with another SimProcess. If this process is already
		/// cooperating with a master <c>false</c> is returned.
		/// </summary>
		/// <returns>Is this process ready to cooperate with another SimProcess?</returns>
		/// <remarks>author: Soenke Claassen</remarks>
		public bool canCooperate()
		{
			return _master == null; // if the master is not set yet this SimProcess
									// can cooperate with another SimProcess
		}

		/// <summary>
		/// Resets the interrupt-status of this SimProcess to not interrupted. Should be called every time the
		/// SimProcess has successfully dealt with the interrupt. The internal <c>InterruptCode</c> of this
		/// SimProcess will be reset to <c>null</c>.
		/// </summary>
		/// <remarks>author: Soenke Claassen</remarks>
		public void clearInterruptCode()
		{
			_irqCode = null;
		}

		/// <summary>
		/// As there is no generally applicable means of cloning a SimProcess (which would require cloning the
		/// execution state as well), this method returns a CloneNotSupportedException.
		/// </summary>
		/// <returns>A copy of this process.</returns>
		protected SimProcess clone()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// The current (master) process is calling this method (within <c>WaitQueue.cooperate()</c>) on the slave
		/// process to lead him through the joint cooperation. After the joint cooperation is finished the master
		/// is still active and after him the slave will be activated.
		/// </summary>
		/// <remarks>author: Soenke Claassen</remarks>
		public void cooperate()
		{
			// this is the slave and current the master

			// check if this slave already has a master
			if (_master != null)
			{
				sendWarning(
						"Slaves can not cooperate with more than one master at a "
								+ "time! The attempted cooperation is ignored.",
						"SimProcess : " + getName() + " Method: cooperate () ",
						"This slave process is already cooperating with another "
								+ "master: " + _master.getName(),
						"Be sure to have finished one cooperation before starting "
								+ "the next one.");
				return; // this process has a master already
			}

			// check if this slave is not terminated yet
			if (_isTerminated)
			{
				sendWarning(
						"Attempt to cooperate with a terminated slave process! "
								+ "The attempted cooperation is ignored.",
						"SimProcess : " + getName() + " Method: cooperate () ",
						"This slave process is already terminated.",
						"Make sure not to cooperate with terminated processes.");
				return; // this process is already terminated
			}

			// check the master
			SimProcess currentMaster = currentSimProcess(); // the current master
															// process
			if (currentMaster == null) // if currentMaster is only a null pointer
			{
				sendWarning("A non existing process is trying to cooperate as a "
						+ "master! The attempted cooperation is ignored!",
						"SimProcess : " + getName() + " Method: cooperate ()",
						"The master process is only a null pointer.",
						"Make sure that only real SimProcesses are cooperating with other "
								+ "processes. ");
				return; // the currentMaster process is only a null pointer
			}

			if (!isModelCompatible(currentMaster))
			{
				sendWarning(
						"The given master SimProcess object does not "
								+ "belong to this model. The attempted cooperation is ignored!",
						"SimProcess : " + getName() + " Method: cooperate ()",
						"The master SimProcess is not modelcompatible.",
						"Make sure that the processes all belong to this model.");
				return; // the currentMaster is not modelcompatible
			}

			// the slave must be waiting in a WaitQueue
			if (_slaveWaitQueue == null)
			{
				sendWarning(
						"Attempt to cooperate with a slave process, that is not "
								+ "waiting in a WaitQueue. The attempted cooperation is ignored!",
						"SimProcess : " + getName() + " Method: cooperate ()",
						"Slave processes must wait in a WaitQueue before they can get into "
								+ "a cooperation.",
						"Make sure that the slave processes are waiting in a WaitQueue.");
				return; // the slave is not waiting in a queue
			}

			// now prepare for the real cooperation
			_master = currentMaster; // set the master for this slave process

			// leave a note in the trace
			if (_master.currentlySendTraceNotes())
			{
				// trace note for a cooperation without any special conditions
				sendTraceNote("cooperates " + this.getQuotedName() + " from "
						+ _slaveWaitQueue.getQuotedName());
			}

			// get this slave out of his slaveWaitQueue
			_slaveWaitQueue.remove(this);
			// this slave process is not waiting in any slaveWaitingQueue anymore
			_slaveWaitQueue = null;
			// and therefore this slave process is not blocked anymore
			_isBlocked = false;

		}

		/**
		 * Method to release the waiting scheduler when the SimThread finishes.
		 */
		public void freeThread()
		{

			// no-op: legacy thread-based scheduler release; not needed with coroutine model
		}

		/// <summary>
		/// Returns the InterruptCode from this SimProcess. If this SimProcess is not interrupted, the
		/// InterruptCode is <c>null</c>.
		/// </summary>
		/// <returns>The InterruptCode of this SimProcess.</returns>
		/// <remarks>author: Soenke Claassen</remarks>
		public InterruptCode getInterruptCode()
		{
			return _irqCode;
		}

		public InterruptException getInterruptException()
		{
			return _irqException;
		}

		/// <summary>
		/// Returns the master when two SimProcesses are cooperating. If this method is called on a SimProcess
		/// which is not a slave <c>null</c> is returned.
		/// </summary>
		/// <returns>
		/// The master process during the cooperation or <c>null</c> if this process is not a slave process.
		/// </returns>
		/// <remarks>author: Soenke Claassen</remarks>
		public SimProcess getMaster()
		{
			return _master;
		}

		/// <summary>
		/// Returns the realTime deadline for this SimProcess (in nanoseconds). In case of a real-time execution
		/// (i. e. the execution speed rate is set to a positive value) the Scheduler will produce a warning
		/// message if a deadline is missed.
		/// </summary>
		/// <returns>the realTimeConstraint in nanoseconds</returns>
		public long getRealTimeConstraint()
		{
			return _realTimeConstraint;
		}

		/// <summary>
		/// Returns the waiting-queue in which this SimProcess is waiting as a slave to cooperate with a master. If
		/// this method is called on a SimProcess which is not a slave <c>null</c> is returned.
		/// </summary>
		/// <returns>
		/// The waiting-queue in which this SimProcess is waiting as a slave or <c>null</c> if this SimProcess is
		/// not waiting as a slave for cooperation.
		/// </returns>
		/// <remarks>author: Soenke Claassen</remarks>
		public ProcessQueue<SimProcess> getSlaveWaitQueue()
		{
			return _slaveWaitQueue;
		}

		/// <summary>Returns the supervising <c>ComplexSimProcess</c> this SimProcess is contained in.</summary>
		/// <returns>
		/// The supervising <c>ComplexSimProcess</c> this SimProcess is contained in. Is <c>null</c> if this
		/// SimProcess is not contained in any <c>ComplexSimProcess</c>.
		/// </returns>
		/// <remarks>author: Soenke Claassen</remarks>
		public ComplexSimProcess getSupervisor()
		{

			return _supervisor;
		}

		/// <summary>
		/// Returns the last <c>Schedulable</c> that did cause the last activation (or interruption) of this
		/// SimProcess.
		/// </summary>
		/// <returns>
		/// The Schedulable (e.g. other SimProcess, Event...) that has caused the last activation of this
		/// SimProcess. As processes may activate themselves, e.g. though a <c>hold(TimeSpan t)</c> or an
		/// <c>activate(TimeSpan t)</c>, this method may return a reference to this process. The method returns
		/// <c>null</c> if this process has not yet been activated.
		/// </returns>
		public Schedulable getActivatedBy()
		{

			return _activatedBy;
		}

		/// <summary>
		/// Sets the last <c>Schedulable</c> which has (re)activated or interrupted this SimProcess.
		/// </summary>
		/// <param name="by">
		/// The Schedulable that has (re)activated or interrupted this SimProcess or <c>null</c> to un-set this
		/// information.
		/// </param>
		public void setActivatedBy(Schedulable by)
		{

			_activatedBy = by;
		}

		/// <summary>
		/// Returns a clone of the internal <c>Vector</c> containing all the <c>Resource</c> objects this
		/// SimProcess is using at the moment.
		/// </summary>
		/// <returns>
		/// the internal <c>Vector</c> containing all the <c>Resource</c> objects this SimProcess is using at the
		/// moment.
		/// </returns>
		/// <remarks>author: Soenke Claassen</remarks>
		public List<Resource> getUsedResources()
		{

			// clone the internal Vector
			//@SuppressWarnings("unchecked")
			var usedRes = new List<Resource>(_usedResources);

			// return the cloned Vector
			return usedRes;
		}

		/// <param name="dt">The duration of the SimProcess' passivation</param>
		/// <remarks>
		/// Replaced by hold(TimeSpan dt). Passivates a SimProcess for the time given. The simthread of this
		/// SimProcess is put into a lock and the scheduler, resp. the experiment's main thread is released from
		/// its block and continues with the next EventNote to be processed.
		/// </remarks>
		//@Deprecated
		public void hold(SimTime dt)
		{
			hold(SimTime.toTimeSpan(dt));
		}

		/// <summary>
		/// Passivates a SimProcess until the given point in simulation time. The simthread of this SimProcess is
		/// put into a lock and the scheduler, resp. the experiment's main thread is released from its block and
		/// continues with the next event-note to be processed.
		/// </summary>
		/// <param name="until">The point in simulation time when the SimProcess' passivation ends.</param>
		public virtual void hold(TimeInstant until)
		{
			if ((until == null))
			{
				sendWarning("Can't schedule SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: void hold(TimeInstant until)",
						"The TimeInstant given as parameter is a null reference.",
						"Be sure to have a valid TimeInstant reference before calling this method.");
				return; // no proper parameter
			}

			if (isBlocked())
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: hold(TimeInstant until)",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be set to be activated by other "
								+ "SimProcesses or events!");
				return; // is blocked in some synch construction
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: void hold(TimeInstant until)",
						"The SimProcess to be scheduled is already scheduled.",
						"Use method reActivate(TimeInstant when) to shift the SimProcess "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (TimeInstant.isBefore(until, presentTime()))
			{
				sendWarning("Can't schedule SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: void hold(TimeInstant until)",
						"The instant given is in the past.",
						"To hold a SimProcess, use a TimeInstant no earlier than the present time. "
								+ "The present time can be obtained using the "
								+ "presentTime() method.");
				return; // do not hold
			}

			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					sendTraceNote("holds until " + until.toString());
				}
				else
				{
					sendTraceNote("holds " + getQuotedName() + "until "
							+ until.toString());
				}
				skipTraceNote(); // skip passivate message
			}

			// schedule to be reactivated at the point of simulation time "until"
			getModel().getExperiment().getScheduler().schedule(this, null, until);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("holds on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}

		/// <summary>
		/// Passivates a SimProcess for the given span of time. The simthread of this SimProcess is put into a lock
		/// and the scheduler, resp. the experiment's main thread is released from its block and continues with the
		/// next EventNote to be processed.
		/// </summary>
		/// <param name="dt">The duration of the SimProcess' passivation</param>
		public virtual void hold(TimeSpan dt)
		{
			if ((dt == null))
			{
				sendWarning("Can't schedule SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: void hold(TimeSpan dt)",
						"The TimeSpan given as parameter is a null reference.",
						"Be sure to have a valid TimeSpan reference before calling this method.");
				return; // no proper parameter
			}

			if (isBlocked())
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : " + getName() + " Method: hold(TimeSpan dt)",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be set to be activated by other "
								+ "SimProcesses or events!");
				return; // is blocked in some synch construction
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: void hold(TimeSpan dt)",
						"The SimProcess to be scheduled is already scheduled.",
						"Use method reActivate(TimeSpan dt) to shift the SimProcess "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					sendTraceNote("holds for " + dt.toString() + " until "
							+ TimeOperations.add(presentTime(), dt).toString());
				}
				else
				{
					sendTraceNote("holds " + getQuotedName() + "for "
							+ dt.toString() + " until "
							+ TimeOperations.add(presentTime(), dt).toString());
				}
				skipTraceNote(); // skip passivate message
			}

			// schedule to be reactivated in dt
			getModel().getExperiment().getScheduler().schedule(this, null, dt);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("holds on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}


		}

		/// <summary>
		/// Passivates a SimProcess for span of time sampled from the distribution provided to the method. The
		/// sample is interpreted in the reference time unit. The SimThread of this SimProcess is put into a lock
		/// and the scheduler, resp. the experiment's main thread is released from its block and continues with the
		/// next EventNote to be processed.
		/// </summary>
		/// <param name="dist">
		/// Numerical distribution to sample the duration of the SimProcess' passivation from
		/// </param>
		public virtual void hold(NumericalDist<Distribution> dist)
		{

			if ((dist == null))
			{
				sendWarning("Can't schedule SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: void hold(NumericalDist<?> dist)",
						"The NumericalDist given as parameter is a null reference.",
						"Be sure to have a valid NumericalDist reference before calling this method.");
				return; // no proper parameter
			}

			if (isBlocked())
			{
				sendWarning(
						"Can't activate SimProcess! Command ignored.",
						"SimProcess : " + getName() + " Method: hold(NumericalDist<?> dist)",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be set to be activated by other "
								+ "SimProcesses or events!");
				return; // is blocked in some synch construction
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: void hold(NumericalDist<?> dist)",
						"The SimProcess to be scheduled is already scheduled.",
						"Use method reActivate(TimeSpan dt) to shift the SimProcess "
								+ "to be scheduled at some other point of time.");
				return; // was already scheduled
			}

			// determine time span
			TimeSpan dt = dist.sampleTimeSpan();

			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					sendTraceNote("holds for " + dt.toString() + " until "
							+ TimeOperations.add(presentTime(), dt).toString() + " as sampled from " + dist.getQuotedName());
				}
				else
				{
					sendTraceNote("holds " + getQuotedName() + "for "
							+ dt.toString() + " until "
							+ TimeOperations.add(presentTime(), dt).toString() + " as sampled from " + dist.getQuotedName());
				}
				skipTraceNote(); // skip passivate message
			}

			// schedule to be reactivated in dt
			getModel().getExperiment().getScheduler().schedule(this, null, dt);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("holds on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}


		}

		/// <summary>
		/// Interrupts the SimProcess setting the given InterruptCode as the reason for the interruption. Blocked,
		/// terminated or already interrupted SimProcesses can not be interrupted. In this case a warning message
		/// will be produced and the interrupt will be ignord. If the SimProcess is cooperating as a slave the
		/// interrupt will be passed to the master.
		/// </summary>
		/// <param name="interruptReason">InterruptCode</param>
		public void interrupt(InterruptCode interruptReason)
		{

			if (interruptReason == null)
			{
				sendWarning(
						"Can't interrupt SimProcess! Command ignored",
						"SimProcess : " + getName() + " Method: void "
								+ "interrupt(InterruptCode interruptReason)",
						"The InterruptCode given as parameter is a null reference.",
						"Be sure to have a valid InterruptCode reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			// if the SimProcess is cooperating as a slave
			if (_master != null)
			{
				if (currentlySendTraceNotes())
				{
					sendTraceNote("interrupts '" + this.getName() + "' , who ...");
				}

				// interrupt the master, too. (with the same reason/InterruptCode)
				_master.interrupt(interruptReason);
			}

			if (isBlocked())
			{
				sendWarning("Can't interrupt SimProcess! Command ignored",
						"SimProcess : " + getName() + " Method: void "
								+ "interrupt(InterruptCode interruptReason)",
						"Blocked SimProcesses can not be interrupted.",
						"You can check if a SimProcess is blocked using method "
								+ "isBlocked().");
				return; // is Blocked
			}

			if (isTerminated())
			{
				sendWarning("Can't interrupt SimProcess! Command ignored",
						"SimProcess : " + getName() + " Method: void "
								+ "interrupt(InterruptCode interruptReason)",
						"Terminated SimProcesses can not be interrupted.",
						"You can check if a SimProcess is terminated using method "
								+ "isTerminated().");
				return; // is Terminated
			}

			if (isInterrupted())
			{
				sendAWarningThatTheCurrentSimProcessHasAlreadyBeenInterrupted("SimProcess : "
						+ getName()
						+ " Method: void "
						+ "interrupt(InterruptCode interruptReason)");
				return;
			}

			if (this == currentSimProcess())
			{
				sendWarning("Can't interrupt SimProcess! Command ignored",
						"SimProcess : " + getName() + " Method: void "
								+ "interrupt(InterruptCode interruptReason)",
						"SimProcess is the currently active SimProcess.",
						"Make sure not to interrupt the currently active "
								+ "SimProcess.");
				return; // is currentSimProcess
			}

			if (currentlySendTraceNotes())
			{
				sendTraceNote("interrupts '" + this.getName() + "', with reason "
						+ interruptReason.getName() + " ["
						+ interruptReason.getCodeNumber() + "]");
			}

			_irqCode = interruptReason; // set the InterruptCode

			// if on EventList, remove first ...
			if (isScheduled())
			{
				skipTraceNote(2);
				cancel();
			}
			else
			{
				skipTraceNote();
			}
			// ... then activate after the one interrupting this SimProcess
			activateAfter(current());
		}

		/// <summary>
		/// Interrupts the SimProcess by throwing the given InterruptException in it's lifeCylce() method. The
		/// InterruptException contains an InterruptCode as the reason for the interruption. Blocked, terminated or
		/// already interrupted SimProcesses can not be interrupted. In this case a warning message will be
		/// produced and the interrupt will be ignord. If the SimProcess is cooperating as a slave the interrupt
		/// will be passed to the master.
		/// </summary>
		/// <param name="interruptReason">InterruptException</param>
		public void interrupt(InterruptException interruptReason)
		{
			if (interruptReason == null
					|| interruptReason.getInterruptCode() == null)
			{
				sendWarning(
						"Can't interrupt SimProcess! Command ignored",
						"SimProcess : " + getName() + " Method: void "
								+ "interrupt(InterruptException interruptReason)",
						"Either the InterruptException given as parameter or the InterruptCode contained in that Exception is a null reference.",
						"Be sure to have a valid InterruptCode reference before "
								+ "calling this method.");
				return; // no proper parameter
			}

			// if the SimProcess is cooperating as a slave
			if (_master != null)
			{
				if (currentlySendTraceNotes())
				{
					sendTraceNote("interrupts '" + this.getName() + "' , who ...");
				}

				// interrupt the master, too. (with the same reason/InterruptCode)
				_master.interrupt(interruptReason);
			}

			if (isBlocked())
			{
				sendWarning("Can't interrupt SimProcess! Command ignored",
						"SimProcess : " + getName() + " Method: void "
								+ "interrupt(InterruptCode interruptReason)",
						"Blocked SimProcesses can not be interrupted.",
						"You can check if a SimProcess is blocked using method "
								+ "isBlocked().");
				return; // is Blocked
			}

			if (isTerminated())
			{
				sendWarning("Can't interrupt SimProcess! Command ignored",
						"SimProcess : " + getName() + " Method: void "
								+ "interrupt(InterruptException interruptReason)",
						"Terminated SimProcesses can not be interrupted.",
						"You can check if a SimProcess is terminated using method "
								+ "isTerminated().");
				return; // is Terminated
			}

			if (isInterrupted())
			{
				sendAWarningThatTheCurrentSimProcessHasAlreadyBeenInterrupted("SimProcess : "
						+ getName()
						+ " Method: void "
						+ "interrupt(InterruptException interruptReason)");
				return;
			}

			if (this == currentSimProcess())
			{
				sendWarning("Can't interrupt SimProcess! Command ignored",
						"SimProcess : " + getName() + " Method: void "
								+ "interrupt(InterruptException interruptReason)",
						"SimProcess is the currently active SimProcess.",
						"Make sure not to interrupt the currently active "
								+ "SimProcess.");
				return; // is currentSimProcess
			}

			if (currentlySendTraceNotes())
			{
				sendTraceNote("interrupts '" + this.getName() + "', with reason "
						+ interruptReason.getInterruptCode().getName() + " ["
						+ interruptReason.getInterruptCode().getCodeNumber() + "]");
			}

			_irqException = interruptReason; // set the InterruptException

			// if on EventList, remove first ...
			if (isScheduled())
			{
				skipTraceNote(2);
				cancel();
			}
			else
			{
				skipTraceNote();
			}
			// ... then activate after the one interrupting this SimProcess
			activateAfter(current());
		}

		/// <summary>
		/// Schedules this process to be interrupted at the given point in simulation time. Only one delayed
		/// interrupt can be scheduled at a time. If a delayed interrupt is scheduled after another delayed
		/// interrupt has already been scheduled a warning message will be produced and the new delayed interrupt
		/// will not be scheduled. A delayed Interrupt must be cleared manually by calling clearInterruptDelayed()
		/// on the SimProcess if it wasn't performed.
		/// </summary>
		/// <param name="when">The Point in time when the interrupt is to be performed.</param>
		/// <returns>
		/// The event which will (at the given point in time) triger the interrupt of this process
		/// </returns>
		public ExternalEvent interruptDelayed(TimeInstant when)
		{
			ExternalEvent delayedInterruptEvent = null;

			// when pr�fen

			if (_currentlyScheduledDelayedInterruptEvent != null)
			{
				sendWarning(
						"Can't schedule a delayed interrupt of this SimProcess! CommandIgnored",
						"SimProcess: "
								+ getName()
								+ " method: void interruptDelayed(TimeInstant when)",
						"Another delayed interrupt has already been scheduled.",
						"A delayed interrupt may only be scheduled if no other delayed interrupt has been scheduled on that SimProcess."
								+ " Did you maybe forget to clear the last delayed interrupt. Delyaed interrupts aren't cleared automatically but must be cleared manually by a call to the method "
								+ "SimProcess#clearInterruptDelayed().");
				return null;
			}



			delayedInterruptEvent = new DelayedInterruptExternalEvent(this);
			if (currentlySendTraceNotes())
			{
				sendTraceNote("scheduling a delayed interrupt at " + when);
			}
			skipTraceNote();
			delayedInterruptEvent.schedule(when);

			if (delayedInterruptEvent.isScheduled())
			{
				// If the delayed interrupt event has been scheduled auccessfully
				// save it to an instance variable so it may be unscheduled if
				// necessary.
				_currentlyScheduledDelayedInterruptEvent = delayedInterruptEvent;
			}
			else
			{
				// For some reason the delayed interrupt event wasn't scheduled. A
				// warning should already haven been sent. So do nothing.
			}

			return _currentlyScheduledDelayedInterruptEvent;
		}

		/// <summary>
		/// Schedules this process to be interrupted after the given delay. Only one delayed interrupt can be
		/// scheduled at a time. If a delayed interrupt is scheduled after another delayed interrupt has already
		/// been scheduled a warning message will be produced and the delayed interrupt will not be scheduled. A
		/// delayed Interrupt must be cleared manually by calling clearInterruptDelayed() on the SimProcess if it
		/// wasn't performed.
		/// </summary>
		/// <param name="delay">The delay after which the interrupt is to be performed.</param>
		/// <returns>The event which will (after the given delay) triger the interrupt of this process</returns>
		public ExternalEvent interruptDelayed(TimeSpan delay)
		{
			TimeInstant when;

			when = TimeOperations.add(presentTime(), delay);

			return interruptDelayed(when);
		}

		/// <summary>
		/// Returns the current block-status of the SimProcess. If a SimProcess is blocked, it is waiting inside a
		/// queue or synchronization block for it's release.
		/// </summary>
		/// <returns>Is <c>true</c> if SimProcess is blocked, <c>false</c> otherwise</returns>
		public bool isBlocked()
		{

			return _isBlocked;

		}

		/// <summary>
		/// Returns the current repeating-status of the SimProcess. If a SimProcess is repeating, the lifeCycle
		/// will be executed again after finishing.
		/// </summary>
		/// <returns>
		/// Is <c>true</c> if the lifeCycle of the process will be repeated finishing, while <c>false</c> indicates
		/// the process will terminate after the lifeCycle is completed.
		/// </returns>
		public bool isRepeating()
		{

			return _isRepeating;

		}

		/// <summary>
		/// Returns the current component status of this SimProcess. If a SimProcess is a component of a
		/// <c>ComplexSimProcess</c> it is blocked and passivated. It exists only within the
		/// <c>ComplexSimProcess</c>; it's own lifeCycle is stopped and will only be activated again when it is
		/// removed from the <c>ComplexSimProcess</c>.
		/// </summary>
		/// <returns>
		/// <c>true</c> if and only if this SimProcess is a component (part of) a <c>ComplexSimProcess</c>;
		/// <c>false</c> otherwise.
		/// </returns>
		/// <remarks>author: Soenke Claassen</remarks>
		public bool isComponent()
		{

			return (_supervisor != null);
		}

		/// <summary>
		/// Returns the current interrupt-status of this SimProcess. If a SimProcess is interrupted, it should deal
		/// with the interrupt and then call the <c>clearInterruptCode()</c> -method.
		/// </summary>
		/// <returns>Is <c>true</c> if this SimProcess is interrupted, <c>false</c> otherwise.</returns>
		/// <remarks>author: Soenke Claassen</remarks>
		public bool isInterrupted()
		{
			return (_irqCode != null || _irqException != null);
		}

		/// <summary>
		/// Returns the current running status of the SimProcess. If a SimProcess is not ready, it has already
		/// finished its <c>lifeCycle()</c> method and can not further be used as a SimProcess. A terminated
		/// SimProcess can still be used like any other Entity which it is derived from.
		/// </summary>
		/// <returns>Is <c>true</c> if the SimProcess is terminated, <c>false</c> otherwise</returns>
		/// <remarks>See also Entity</remarks>
		public bool isReady()
		{

			return _isRunning;

		}

		/// <summary>
		/// Returns the current status of the SimProcess. If a SimProcess is terminated, it has already finished
		/// its <c>lifeCycle()</c> method and can not further be used as a SimProcess. A terminated SimProcess can
		/// still be used like any other Entity which it is derived from.
		/// </summary>
		/// <returns>Is <c>true</c> if the SimProcess is terminated, <c>false</c> otherwise</returns>
		/// <remarks>See also Entity</remarks>
		public bool isTerminated()
		{

			return _isTerminated;

		}



		/// <summary>Makes the SimProcess obtain an array of resources and store them for further usage.</summary>
		/// <param name="obtainedResources">The array of resources obtained.</param>
		/// <remarks>author: Soenke Claassen</remarks>
		public void obtainResources(Resource[] obtainedResources)
		{
			if (obtainedResources.Length <= 0)
			{
				sendWarning("Attempt to obtain resources, but got none! Command "
						+ "ignored!", "SimProcess : " + getName()
						+ " Method:  void obtain"
						+ "Resources(Resource[] obtainedResources)",
						"The array of obtained resources is empty.",
						"Make sure to obtain at least one resource. Check if the "
								+ "resource pool can provide any resources.");
				return; // parameter contains nothing
			}

			// put all the obtained resources in the Vector of used resources
			foreach (Resource obtainedResource in obtainedResources)
			{
				_usedResources.Add(obtainedResource);
			}

			// for debugging purposes
			if (currentlySendDebugNotes())
			{
				// make a string of all resources used by this SimProcess
				string t = "uses: ";

				foreach (Resource resource in _usedResources)
				{
					t += "<br>" + (resource).getName();
				}

				sendDebugNote(t);
			}
		}

		//public abstract void passivate();

		/// <summary>
		/// The current (master) process is calling this method (within <c>TransportJunction.cooperate()</c>) on
		/// the slave process to make him prepare for the transportation. After the transport is finished the
		/// master is still active and after him the slave will be activated.
		/// </summary>
		/// <remarks>author: Soenke Claassen</remarks>
		public void prepareTransport()
		{
			// this is the slave and current the master

			// check if this slave already has a master
			if (_master != null)
			{
				sendWarning(
						"Slaves can not be transported from more than one master at "
								+ "a time! The attempted transport is ignored.",
						"SimProcess : " + getName()
								+ " Method: prepareTransport () ",
						"This slave process is already transported by another "
								+ "master: " + _master.getName(),
						"Be sure to have finished one transportation before starting "
								+ "the next one.");
				return; // this process has a master already
			}

			// check if this slave is not terminated yet
			if (_isTerminated)
			{
				sendWarning("Attempt to transport a terminated slave process! "
						+ "The attempted transport is ignored.", "SimProcess : "
						+ getName() + " Method: prepareTransport () ",
						"This slave process is already terminated.",
						"Make sure not to transport terminated processes.");
				return; // this process is already terminated
			}

			// check the master
			SimProcess currentMaster = currentSimProcess(); // the current master
															// process
			if (currentMaster == null) // if currentMaster is only a null pointer
			{
				sendWarning(
						"A non existing process is trying to transport other "
								+ "processes as a master! The attempted transport is ignored!",
						"SimProcess : " + getName()
								+ " Method: prepareTransport ()",
						"The master process is only a null pointer.",
						"Make sure that only real SimProcesses are transporting other "
								+ "processes. ");
				return; // the currentMaster process is only a null pointer
			}

			if (!isModelCompatible(currentMaster))
			{
				sendWarning(
						"The given master SimProcess object does not "
								+ "belong to this model. The attempted transport is ignored!",
						"SimProcess : " + getName()
								+ " Method: prepareTransport ()",
						"The master SimProcess is not modelcompatible.",
						"Make sure that the processes all belong to this model.");
				return; // the currentMaster is not modelcompatible
			}

			// the slave must be waiting in a WaitQueue
			if (_slaveWaitQueue == null)
			{
				sendWarning(
						"Attempt to transport a slave process, that is not "
								+ "waiting in a TransportJunction. The attempted transport is ignored!",
						"SimProcess : " + getName()
								+ " Method: prepareTransport ()",
						"Slave processes must wait in a TransportJunction before they can be "
								+ "transported.",
						"Make sure that the slave processes are waiting in a "
								+ "TransportJunction.");
				return; // the slave is not waiting in a queue
			}

			// now prepare for the real cooperation
			_master = currentMaster; // set the master for this slave process

			// leave a note in the trace
			if (_master.currentlySendTraceNotes())
			{
				// trace note for a transport without any special conditions
				sendTraceNote("transports " + this.getQuotedName() + " from "
						+ _slaveWaitQueue.getQuotedName());
			}

			// get this slave out of his slaveWaitQueue
			_slaveWaitQueue.remove(this);
			// this slave process is not waiting in any slaveWaitingQueue anymore
			_slaveWaitQueue = null;
			// and therefore this slave process is not blocked anymore
			_isBlocked = false;

		}

		/// <summary>
		/// Re-schedules the SimProcess to be activated at the given TimeSpan offset to the current simulation
		/// time. The SimProcess has already been scheduled but is now supposed to be reactivated at some other
		/// point of simulation time. Passing the special argument <c>TimeSpan.ZERO</c> potentially causes process
		/// preemption, i.e. temprorily stalls the current process (if any), compare method <c>activate(TimeSpan
		/// dt)</c>.
		/// </summary>
		/// <param name="dt">
		/// The offset to the current simulation time that this SimProcess is due to be re-activated
		/// </param>
		public void reActivate(TimeSpan dt)
		{
			if (isBlocked())
			{
				sendWarning(
						"Can't reactivate SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: reActivate(TimeSpan dt)",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be activated by other SimProcesses or "
								+ "events!");
				return; // is blocked in some synch construction
			}

			if (!isScheduled())
			{
				sendWarning("Can't reactivate SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: reActivate(TimeSpan dt)",
						"The SimProcess to be reactivated is not scheduled.",
						"Use method activate(TimeSpan dt) to activate a SimProcess"
								+ "that is not scheduled yet.");
				return; // was already scheduled
			}

			if (dt == null)
			{
				sendWarning(
						"Can't reactivate SimProcess! Command ignored.",
						"SimProcess : " + getName() + " Method:  void reActivate"
								+ "(TimeSpan dt)",
						"The simulation time given as parameter is a null reference",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method");
				return; // no proper parameter
			}

			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					if (dt == TimeSpan.ZERO)
					{
						sendTraceNote("reactivates itself now");
					}
					else
					{
						sendTraceNote("reactivates itself at "
								+ TimeOperations.add(presentTime(), dt));
					}
				}
				else
				{
					if (dt == TimeSpan.ZERO)
					{
						sendTraceNote("reactivates " + getQuotedName() + " now");
					}
					else
					{
						sendTraceNote("reactivates " + getQuotedName() + " at "
								+ TimeOperations.add(presentTime(), dt));
					}
				}
			}

			getModel().getExperiment().getScheduler().reSchedule(this, dt);

			resetMaster(); // if reActivate(TimeSpan dt) is called for this
						   // SimProcess,
						   // there is no Master anymore controlling it.
		}

		/// <summary>Gets the InterruptCode from the master and resets the master to <c>null</c>.</summary>
		/// <remarks>author: Soenke Claassen</remarks>
		public void resetMaster()
		{
			if (_master != null)
			{
				_irqCode = _master.getInterruptCode();
				_irqException = _master.getInterruptException();
			}

			_master = null;
		}

		public void resume()
		{
			if (_generator_function != null && _generator_function.MoveNext())
			{
				if (currentlySendTraceNotes())
				{
					if (this == currentSimProcess())
					{
						sendTraceNote("passivates");
					}
					else
					{
						sendTraceNote("passivates " + getQuotedName());
					}
				}
				var val = _generator_function.Current;
			}

		}

		/// <summary>
		/// Makes the SimProcess return all resources it holds at the moment to all the different Res pools it is
		/// holding resources from. This is useful in situations the Simprocess is about to terminate.
		/// </summary>
		/// <remarks>author: Soenke Claassen</remarks>
		public void returnAllResources()
		{
			// check if something can be returned
			if (_usedResources.Count == 0)
			{
				sendWarning(
						"Attempt to return all resources, but the "
								+ "SimProcess does not hold any resources! Command ignored!",
						"SimProcess : " + getName()
								+ " Method: returnAllResources()",
						"If the SimProcess does not hold any resources it is "
								+ "impossible to return any.",
						"Make sure that the SimProcess holds resources that "
								+ "should be returned!");
				return; // return nothing, go to where you came from
			}

			// repeat while vector of usedResources is not empty
			while (_usedResources.Count > 0)
			{
				// get the first resource and check the Res pool it belongs to
				Res crntResPool = _usedResources[0].getResPool();

				// counter how many resources of that res pool are used
				int n = 1;

				// search the whole vector of usedResources for resources of the
				// current
				// Res pool
				for (int i = 1; i < _usedResources.Count; i++)
				{
					// is the resource of the desired Res pool?
					if (_usedResources[i].getResPool() == crntResPool)
					{
						n++; // increase the counter
					}
				} // end for-loop

				// make the array to store the resources which will be returned
				Resource[] returningRes = new Resource[n];

				// counter for the index of the array
				int k = 0;

				// collect all the resources from the Vector of usedResources
				for (int j = 0; j < _usedResources.Count; j++)
				{
					// is the resource of the desired Res pool?
					if ((_usedResources[j]).getResPool() == crntResPool)
					{
						// put res in array
						returningRes[k] = _usedResources[j];
						k++; // increase counter of array
					}
					if (k == n)
					{
						break; // stop the for-loop
					}
				}

				// return the array of resources to the Res pool they belong to
				crntResPool.takeBack(returningRes);

				// remove the returned resources from the vector of usedResources
				for (int m = 0; m < n; m++) // go through the array of
											// returningResources
				{
					// remove each resource that is in the array of
					// returningResources
					_usedResources.Remove(returningRes[m]);
				}

			} // end while

			// for debugging purposes
			if (currentlySendDebugNotes())
			{
				// make a string including all elements of the vector usedResources
				String s = "All resources returned! Contents of vector usedResources: ";

				if (_usedResources.Count == 0) // anything left ?
				{
					s += "<br>none";
				}

				foreach (var e in _usedResources)
				{
					s += e;
				}

				// send a debugNote representing the state of the vector
				// usedResources
				sendDebugNote(s);
			}

		} // end method returnAllResources

		/// <summary>Makes the SimProcess return a certain number of resources of the given resource pool.</summary>
		/// <param name="resPool">The resource pool which resources will be returned.</param>
		/// <param name="n">The number of resources which will be returned.</param>
		/// <returns>the array containing the resources which will be returned.</returns>
		/// <remarks>author: Soenke Claassen</remarks>
		public Resource[] returnResources(Res resPool, int n)
		{
			// check if nothing should be returned
			if (n <= 0)
			{
				sendWarning(
						"Attempt to return no or a negative number of resources! "
								+ " Command ignored!", "SimProcess : " + getName()
								+ " Method:  Resource[] "
								+ "returnResources(Res resPool, int n)",
						"It makes no sense to return nothing or a negative number "
								+ "of resources.",
						"Make sure to return at least one resource. Only resources "
								+ "which have been obtained once can be returned!");
				return null; // return nothing, go to where you came from
			}

			// check if nothing can be returned
			if (_usedResources.Count == 0)
			{
				sendWarning(
						"Attempt to return a number of resources, but the "
								+ "SimProcess does not hold any resources! Command ignored!",
						"SimProcess : " + getName() + " Method:  Resource[] "
								+ "returnResources(Res resPool, int n)",
						"If the SimProcess does not hold any resources it is "
								+ "impossible to return any.",
						"Make sure that the SimProcess holds the resources that "
								+ "should be returned!");
				return null; // return nothing, go to where you came from
			}

			// make the array to store the resources which will be returned
			Resource[] returningRes = new Resource[n];

			// counter for the index of the array
			int j = 0;

			// collect all the resources from the Vector of usedResources
			for (int i = 0; i < _usedResources.Count; i++)
			{
				// is the resource of the desired kind?
				if ((_usedResources[i]).getResPool() == resPool)
				{
					// put res in array
					returningRes[j] = _usedResources[i];
					j++; // increase counter of array
				}
				if (j == n)
				{
					break; // stop the for-loop
				}
			}

			// for debugging: make a string of all returning resources
			String s = "<b>returns</b> to Res '" + resPool.getName() + "' : ";

			// remove the returning resources from the vector of usedResources
			for (int m = 0; m < j; m++) // go through the array of
										// returningResources
			{
				// remove each resource that is in the array of returningResources
				_usedResources.Remove(returningRes[m]);

				// add them to string of returning resources
				s += "<br>" + returningRes[m].getName();
			}

			if (j < n) // array is not full
			{
				sendWarning("Attempt to return " + n
						+ " resources to the Res pool. "
						+ "But the SimProcess holds only" + j
						+ "resources of that " + "kind. The " + j
						+ "resources will be returned.", "SimProcess : "
						+ getName() + " Method:  Resource[] "
						+ "returnResources(Res resPool, int n)",
						"The SimProcess can not return " + n + " resources, "
								+ "because it holds only" + j + "resources.",
						"Make sure that the SimProcess holds at least as many "
								+ "resources as it should return.");
			}

			// for debugging purposes
			if (currentlySendDebugNotes())
			{
				sendDebugNote(s);

				// make a string of all resources still held by this SimProcess
				String t = "still holds: ";

				if (_usedResources.Count == 0) // anything left ?
				{
					t += "<br>none";
				}

				foreach (Resource resource in _usedResources)
				{
					t += "<br>" + (resource).getName();
				}

				sendDebugNote(t);
			}

			return returningRes; // return the array of resources
		}

		private void sendAWarningThatTheCurrentSimProcessHasAlreadyBeenInterrupted(
				String location)
		{
			String reason;

			if (_irqCode != null)
			{
				reason = "The SimProcess already has an InterruptCode set: "
						+ _irqCode.getName();
			}
			else if (_irqException != null)
			{
				reason = "The SimProcess already has an InterruptException set which contains the following InterruptCode: "
						+ _irqException.getInterruptCode().getName();
			}
			else
			{
				throw new Exception(
						"Apparently this method has been called although the current process hasn't been interrupted.");
				// This should never happen.
			}

			// is Interrupted
			sendWarning(
					"Can't interrupt SimProcess! Command ignored",
					location,
					reason,
					"SimProcesses may only be interrupted if no other "
							+ "InterruptCode or InterruptException is set on that SimProcess. You can check "
							+ "on that using the mehtod isInterrupted(), which must return "
							+ "false if no other InterruptCode or InterruptException is set.");
		}

		/// <summary>
		/// Sets the SimProcess' blocked status to the boolean value given. This is necessary for some operations
		/// in conjunction with some synchronization classes.
		/// </summary>
		/// <param name="blockStatus">The new value for the blocked status</param>
		public void setBlocked(bool blockStatus)
		{

			_isBlocked = blockStatus;

		}

		/// <summary>
		/// Sets the SimProcess' repeating status to the boolean value given, permitting the process' lifeCycle to
		/// either start again (<c>true</c>) or terminate (<c>false</c>) after the current cycle is completed. Note
		/// that setting setting this property to <c>true</c> after the the process has terinated has no effect.
		/// </summary>
		/// <param name="repeatingStatus">The new value for the repeating status</param>
		public void setRepeating(bool repeatingStatus)
		{

			_isRepeating = repeatingStatus;

		}

		/**
		 * Returns the current repeating-status of the SimProcess. If a SimProcess is
		 * repeating, the lifeCycle will be executed again after finishing.
		 * 
		 * @return boolean : Is <code>true</code> if the lifeCycle of the process
		 *         will be repeated finishing, while <code>false</code> indicates
		 *         the process will terminate after the lifeCycle is completed. 
		 */

		/**
		 * Sets the realTime deadline for this SimProcess (in nanoseconds). In case
		 * of a real-time execution (i. e. the execution speed rate is set to a
		 * positive value) the Scheduler will produce a warning message if a
		 * deadline is missed.
		 * 
		 * @param realTimeConstraint
		 *            the realTimeConstraint in nanoseconds to set
		 */
		public void setRealTimeConstraint(long realTimeConstraint)
		{
			_realTimeConstraint = realTimeConstraint;
		}

		/// <summary>
		/// Sets the SimProcess' running status to the boolean value given. This is necessary for some operations
		/// in conjunction with synchronization classes.
		/// </summary>
		/// <param name="runStatus">The new value for the running status</param>
		public void setRunning(bool runStatus)
		{

			_isRunning = runStatus;

		}

		/// <summary>
		/// Sets the SimProcess' slaveWaitQueue variable to the ProcessQueue in which this SimProcess is waiting as
		/// a slave to cooperate with a master.
		/// </summary>
		/// <param name="slvWaitQueue">
		/// The waiting-queue in which this SimProcess is waiting as a slave to cooperate with a master.
		/// </param>
		/// <remarks>author: Soenke Claassen</remarks>
		public void setSlaveWaitQueue<S>(
				ProcessQueue<S> slvWaitQueue)
			where S : SimProcess
		{
			_slaveWaitQueue = slvWaitQueue as ProcessQueue<SimProcess>;
		}

		/// <summary>
		/// Sets the supervising <c>ComplexSimProcess</c> this SimProcess is contained in. Setting it to
		/// <c>null</c> indicates that this SimProcess is not contained in any <c>ComplexSimProcess</c> (anymore).
		/// </summary>
		/// <param name="complexProcess">
		/// The <c>ComplexSimProcess</c> which serves as a container for this SimProcess.
		/// </param>
		/// <remarks>author: Soenke Claassen</remarks>
		public void setSupervisor(ComplexSimProcess complexProcess)
		{

			_supervisor = complexProcess;
		}

		/// <summary>
		/// Sets the attribute indicating that this SimProcess' simthread has finished to the given value. This
		/// method is used by class SimThread only.
		/// </summary>
		/// <param name="termValue">The new value for the attribute indicating the SimThread's end</param>
		public void setTerminated(bool termValue)
		{

			_isTerminated = termValue; // Hasta la vista, baby!

		}

		/// <summary>
		/// Returns the process' scheduling priority. The scheduling priority is used to determine which process to
		/// execute first if two or more processes are activated at the same instant. The default priority is zero.
		/// Higher priorities are positive, lower priorities negative.
		/// </summary>
		/// <returns>The process' priority</returns>
		public int getSchedulingPriority()
		{

			return _mySchedulingPriority;

		}

		/// <summary>
		/// Sets the process' scheduling priority to a given integer value. The default priority (unless assigned
		/// otherwise) is zero. Negative priorities are lower, positive priorities are higher. All values should be
		/// inside the range defined by the C# <c>int</c> type [-2147483648, +2147483647]. An
		/// process' scheduling priority it used to determine which process is executed first if activated at the
		/// same time instant. Should the priority be the same, order of event execution depends on the
		/// <c>EventList</c> in use, e.g. activated first is executed first (<c>EventTreeList</c>) or random
		/// (<c>RandomizingEventTreeList</c>).
		/// </summary>
		/// <param name="newPriority">The new scheduling priority value</param>
		public void setSchedulingPriority(int newPriority)
		{

			this._mySchedulingPriority = newPriority;

		}

		/// <summary>
		/// Starts the simthread associated with this SimProcess. This is method must be called the first time a
		/// SimProcess is supposed to start processing its <c>lifeCycle()</c> method.
		/// </summary>
		public void start()
		{
			// setting this flag shows that the simthread is now ready to take over
			// control from the scheduler's thread
			_isRunning = true; long run = 1;
			if (currentlySendTraceNotes())
			{
				if (isRepeating())
					sendTraceNote("starts (run #" + run + ")");
				else
					sendTraceNote("starts");
			}


			_generator_function = lifeCycle().GetEnumerator();
			if (_generator_function.MoveNext())
			{
				if (currentlySendTraceNotes())
				{
					if (this == currentSimProcess())
					{
						sendTraceNote("passivates");
					}
					else
					{
						sendTraceNote("passivates " + getQuotedName());
					}
				}

				var val = _generator_function.Current;
			}

		}


		/// <summary>
		/// Re-schedules the SimProcess to be activated at the given point in simulation time. The SimProcess has
		/// already been scheduled but is now supposed to be reactivated at some other point of simulation time.
		/// </summary>
		/// <param name="when">The point of simulation time this process is to be reactivated.</param>
		public void reActivate(TimeInstant when)
		{
			if (isBlocked())
			{
				sendWarning(
						"Can't reactivate SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: reActivate(TimeInstant when)",
						"The SimProcess to be activated is blocked inside "
								+ "a higher level synchronization object.",
						"Simprocesses waiting inside higher synchronization "
								+ "constructs can not be activated by other SimProcesses or "
								+ "events!");
				return;
			}

			if (!isScheduled())
			{
				sendWarning("Can't reactivate SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: reActivate(TimeInstant when)",
						"The SimProcess to be reactivated is not scheduled.",
						"Use method activate(TimeInstant when) to activate a SimProcess"
								+ "that is not scheduled yet.");
				return;
			}

			if (when == null)
			{
				sendWarning(
						"Can't reactivate SimProcess! Command ignored.",
						"SimProcess : " + getName() + " Method: void reActivate"
								+ "(TimeInstant when)",
						"The simulation time given as parameter is a null reference",
						"Be sure to have a valid simulation time reference before "
								+ "calling this method");
				return;
			}

			if (TimeInstant.isBefore(when, presentTime()))
			{
				sendWarning("Can't reactivate SimProcess! Command ignored.",
						"SimProcess : " + getName()
								+ " Method: void reActivate(TimeInstant when)",
						"The instant given is in the past.",
						"To reactivate a SimProcess, use a TimeInstant no earlier than the present time.");
				return;
			}

			if (currentlySendTraceNotes())
			{
				if (this == currentSimProcess())
				{
					sendTraceNote("reactivates itself at " + when.toString());
				}
				else
				{
					sendTraceNote("reactivates " + getQuotedName() + " at "
							+ when.toString());
				}
			}

			getModel().getExperiment().getScheduler().reSchedule(this, when);

			resetMaster();
		}

		private IEnumerator<RunStatus>? _generator_function = null;

		// ── User-facing API ───────────────────────────────────────────────────────

		/// <summary>
		/// Override this instead of <see cref="lifeCycle"/> to avoid any reference to
		/// <see cref="RunStatus"/>. Yield the return value of <see cref="Delay"/> or
		/// <see cref="Suspend"/> to hand control back to the scheduler.
		/// <code>
		/// protected override IEnumerable Run() {
		///     while (true) {
		///         yield return Delay(3.0);
		///     }
		/// }
		/// </code>
		/// </summary>
		protected virtual IEnumerable Run() { yield break; }

		/// <summary>Schedules a wakeup after <paramref name="duration"/> and suspends.</summary>
		protected object Delay(double duration)
		{
			hold(new TimeSpan(duration));
			return null;
		}

		/// <summary>Suspends without scheduling a wakeup (another process must reactivate).</summary>
		protected object Suspend() => null;

		/// <summary>
		/// Yields until <paramref name="lease"/> units have been granted by its <see cref="Res"/> pool.
		/// <code>
		/// using var lease = myRes.Request(1);
		/// foreach (var _ in Acquire(lease)) yield return _;
		/// </code>
		/// </summary>
		protected IEnumerable Acquire(ResLease lease)
		{
			while (!lease.Res.provide(lease.N))
				yield return null;
			lease.Acquired = true;
		}

		// ── Scheduler-facing API (backward-compatible) ────────────────────────────

		/// <summary>
		/// Override <see cref="Run"/> for new code. Override this only when you need
		/// direct access to <see cref="RunStatus"/> (legacy pattern).
		/// </summary>
		public virtual IEnumerable<RunStatus> lifeCycle()
		{
			foreach (var _ in Run())
				yield return RunStatus.passivate;
		}

		private sealed class DelayedInterruptExternalEvent : ExternalEvent
		{
			private readonly SimProcess _owner;

			public DelayedInterruptExternalEvent(SimProcess owner)
				: base(owner.getModel(), "DelayedInterruptEvent", true)
			{
				_owner = owner;
			}

			public override void eventRoutine()
			{
				_owner.interrupt(new DelayedInterruptException(
					new InterruptCode("InternalDelayedInterrupt")));
				_owner._currentlyScheduledDelayedInterruptEvent = null;
			}
		}

	} // end class SimProcess


	public enum RunStatus { passivate }

	public enum ToDo { passivate,}
	public class Next<T>
	{
		public bool passivate = false;
		public T Result { get; set; }

		public Next(T result)
		{
			Result = result;
		}

	}
}