// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using System.Xml.Linq;

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * The external event to stop a running experiment.
     */
	public class ExternalEventStop : ExternalEvent
	{
		/// <summary>Constructs an external event to abort the experiment.</summary>
		/// <param name="owner">The model this external event is associated to</param>
		/// <param name="name">The external event's name</param>
		/// <param name="showInTrace">Flag indicating if the external event is shown in the report</param>
		public ExternalEventStop(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
		{



		}

		/**
		 * The eventRoutine to abort an experiment.
		 */
		public override void eventRoutine()
		{

			getModel().getExperiment().setStatus(Experiment.STOPPED);

		}

		/// <summary>
		/// Do not use this method to implement the external event's eventRoutine! This method can not be hidden
		/// due to the inheritance relationship to the class <c>Event</c>. Since external events are designed to
		/// act on the model or experiment in general and are not associated to an individual Entity, you should
		/// use the parameterless method <c>void eventRoutine()</c> instead. Calling this method will result in a
		/// warning message and the parameterless method will be called. The given Entity will not be changed. Do
		/// not override this method in your special external events!
		/// </summary>
		/// <remarks>See also ExternalEvent.eventRoutine()</remarks>
		public void eventRoutine(Entity who)
		{

			// send warning only if call was intended to be non-ExternalEvent
			// if null was given, it was intended to be an ExternalEvent
			if (who != null)
			{
				sendWarning("Can't accept Entity as parameter", "ExternalEvent : "
						+ getName() + " Method: void eventRoutine(Entity who)",
						"External events do not act on entities.",
						"If you want an event to act on the given Entity use the "
								+ "class Event and override the"
								+ "eventRoutine(Entity who) method in that class.");
			}

			eventRoutine();

		}

		/// <summary>
		/// Schedules this external event to make the desired changes to the experiment or model at the current
		/// point of time plus the given span of time
		/// </summary>
		/// <param name="dt">
		/// The offset to the current simulation time at which this external event is to be scheduled
		/// </param>
		/// <remarks>See also SimClock</remarks>
		public void schedule(TimeSpan dt)
		{
			if ((dt == null))
			{
				sendWarning("Can't schedule external event!", "ExternalEvent : "
						+ getName() + " Method: schedule(Entity who, TimeSpan dt)",
						"The simulation time given as parameter is a null "
								+ "reference.",
						"Be sure to have a valid TimeSpan reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: schedule(Entity wo, TimeSpan dt)",
						"The external event to be scheduled is already scheduled.",
						"Use external events only once, do not reuse them "
								+ "multiple times.");
				return; // was already scheduled
			}

			if (currentlySendTraceNotes())
			{
				sendTraceNote("ExternalEvent '" + getName() + "' scheduled at "
						+ TimeOperations.add(presentTime(), dt).toString());
				// getModel().getExperiment().getTimeFloats()));
			}

			getModel().getExperiment().getScheduler().schedule(null, this, dt);

			if (currentlySendDebugNotes())
			{
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
		public void schedule(TimeInstant when)
		{
			if ((when == null))
			{
				sendWarning("Can't schedule external event!", "ExternalEvent : "
						+ getName()
						+ " Method: schedule(Entity who, TimeInstant when)",
						"The point of simulation time given as parameter is a null "
								+ "reference.",
						"Be sure to have a valid TimeInstant reference before calling "
								+ "this method.");
				return; // no proper parameter
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: schedule(Entity wo, TimeInstant when)",
						"The external event to be scheduled is already scheduled.",
						"Use external events only once, do not reuse them "
								+ "multiple times.");
				return; // was already scheduled
			}

			if (currentlySendTraceNotes())
			{
				sendTraceNote("ExternalEvent '" + getName() + "' scheduled at "
						+ when.toString());
			}

			getModel().getExperiment().getScheduler().schedule(null, this, when);

			if (currentlySendDebugNotes())
			{
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
		public void schedule(SimTime dt)
		{
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
		public void scheduleAfter(Schedulable after)
		{

			if ((after == null))
			{
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleAfter(Schedulable after, "
								+ "Entity who)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "external event to be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleAfter(Schedulable after)",
						"The external event to be scheduled is already scheduled.",
						"Use method external events only once, do not use them "
								+ "multiple times.");
				return; // was already scheduled
			}

			if (!after.isScheduled())
			{
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

			if (currentlySendTraceNotes())
			{
				sendTraceNote("external event '" + getName()
						+ "' scheduled after '" + after.getName() + "' at "
						+ after.getEventNotes()[after.getEventNotes().Count - 1].getTime().toString());
			}

			getModel().getExperiment().getScheduler().scheduleAfter(after, null,
					this);

			if (currentlySendDebugNotes())
			{
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
		public void scheduleBefore(Schedulable before)
		{

			if ((before == null))
			{
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleBefore(Schedulable before, "
								+ "Entity who)",
						"The Schedulable given as parameter is a null reference.",
						"Be sure to have a valid Schedulable reference for this "
								+ "external event to be scheduled with.");
				return; // no proper parameter
			}

			if (isScheduled())
			{
				sendWarning("Can't schedule external event! Command ignored.",
						"ExternalEvent : " + getName()
								+ " Method: scheduleBefore(Schedulable before)",
						"The external event to be scheduled is already scheduled.",
						"Use method external events only once, do not use them "
								+ "multiple times.");
				return; // was already scheduled
			}

			if (!before.isScheduled())
			{
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

			if (currentlySendTraceNotes())
			{
				sendTraceNote("external event '" + getName()
						+ "' scheduled before '" + before.getName() + "' at "
						+ before.getEventNotes()[0].getTime().toString());
			}

			getModel().getExperiment().getScheduler().scheduleBefore(before, null,
					this);

			if (currentlySendDebugNotes())
			{
				sendDebugNote("scheduleBefore " + before.getQuotedName()
						+ " on EventList<br>"
						+ getModel().getExperiment().getScheduler().toString());
			}

		}
	}
}