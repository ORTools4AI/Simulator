// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using System.Text;

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * A <code>ComplexSimProcess</code> is a <code>SimProcess</code> which
     * serves as a container for other <code>SimProcess</code> es. A
     * <code>ComplexSimProcess</code> has its own lifecycle. As long as this
     * <code>ComplexSimProcess</code> is active all its contained Simprocesses are
     * passive. That means they are blocked and can not proceed in their lifeCycles.
     */
	public abstract class ComplexSimProcess : SimProcess
	{

		/**
		 * The <code>Vector</code> holding all the SimProcesses which are
		 * contained in this ComplexSimProcess.
		 */
		private List<SimProcess> _components;

		/// <summary>Constructs a ComplexSimProcess.</summary>
		/// <param name="owner">The model this ComplexSimProcess is associated to.</param>
		/// <param name="name">The name of this ComplexSimProcess.</param>
		/// <param name="showInTrace">
		/// Flag for showing trace messages of this ComplexSimProcess in trace-files. Set it to <c>true</c> if
		/// ComplexSimProcess should show up in trace. Set it to <c>false</c> if ComplexSimProcess should not be
		/// shown in trace.
		/// </param>
		public ComplexSimProcess(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
		{

			//super(owner, name, showInTrace); // make a SimProcess

			// make a new Vector to store all the components in
			_components = new List<SimProcess>();

			// this ComplexSimProcess is not contained in any other
			// ComplexSimProcess yet
			setSupervisor(null);
		}

		/// <summary>
		/// Adds a SimProcess as a component to this ComplexSimProcess. The Sim-process being added to the
		/// ComplexSimProcess will be passivated and blocked. Use method <c>removeComponent()</c> to remove the
		/// Sim-process from the ComplexSimProcess again.
		/// </summary>
		/// <param name="compnt">The SimProcess to be added as a component to this ComplexSimProcess.</param>
		public Next<int> addComponent(SimProcess compnt)
		{
			var next = new Next<int>(-1);

            // check the given SimProcess
            if (compnt == null) // if compnt is a null pointer instead of a process
			{
				sendWarning("Attempt to add a non existing process to a "
						+ "ComplexSimProcess. The attempted action is ignored!",
						"ComplexSimProcess: " + getName()
								+ " Method: synchronized void"
								+ " addComponent(SimProcess compnt)",
						"The given SimProcess is only a null pointer.",
						"Make sure that only real SimProcesses are added to a  "
								+ "ComplexSimProcess.");

				return next; // ignore that rubbish
			}

			if (!isModelCompatible(compnt)) // if compnt is not modelcompatible
			{
				sendWarning(
						"The process trying to be added to a ComplexSimProcess does "
								+ "not belong to this model. The attempted action is ignored!",
						"ComplexSimProcess: " + getName()
								+ " Method: synchronized void"
								+ " addComponent(SimProcess compnt)",
						"The given process belongs to model "
								+ compnt.getModel().getQuotedName()
								+ " and therefore is not modelcompatible.",
						"Make sure that the processes to be added to a ComplexSimProcess belong "
								+ "to the same model or overwrite the method "
								+ "<code>isModelCompatible()</code>");

				return next; // ignore that rubbish
			}

			// the Simprocess component must not be contained twice in the container
			if (_components.Contains(compnt))
			{
				sendWarning(
						"Attempt to add a process to a ComplexSimProcess twice. "
								+ "The attempted action is ignored!",
						"ComplexSimProcess: " + getName()
								+ " Method: synchronized void"
								+ " addComponent(SimProcess compnt)",
						"The given SimProcess is already a component of this "
								+ "ComplexSimProcess.",
						"Make sure the SimProcess is not contained in the "
								+ "ComplexSimProcess already.");
				return next;
			}

			// the SimProcess to be added to a ComplexSimProcess should be active or
			// passivated but not scheduled. Just in case it is scheduled, cancel
			// that
			if (compnt.isScheduled())
			{
				sendWarning(
						"The SimProcess added to a ComplexSimProcess is scheduled! "
								+ "The scheduled activation of the SimProcess will be "
								+ "cancelled.",
						"ComplexSimProcess: " + getName()
								+ " Method: synchronized void"
								+ " addComponent(SimProcess compnt)",
						"A SimProcess added to a ComplexSimProcess is giving up its "
								+ "own lifeCycle and therefore should not be scheduled anymore."
								+ " It will be carried on by the lifeCycle of the "
								+ "ComplexSimProcess.",
						"Make sure that the SimProcess is either adding itself to a "
								+ "ComplexSimProcess or that the SimProcess is passive.");

				compnt.skipTraceNote();
				compnt.cancel();
			}

			// set the supervisor of the SimProcess added
			compnt.setSupervisor(this);

			// the component SimProcess is blocked
			compnt.setBlocked(true);

			// add the new SimProcess component to the components Vector
			_components.Add(compnt);

			// trace output
			if (currentlySendTraceNotes())
			{
				sendTraceNote("adds " + compnt.getQuotedName()
						+ " as a component to " + this.getQuotedName());
			}

			// debug output
			if (currentlySendDebugNotes())
			{
				sendDebugNote("adds " + compnt.getQuotedName()
						+ " to its components." + "it now looks like <br>"
						+ this.toString());
			}

			// either the component SimProcess is adding itself to the
			// ComplexSimProcess
			// or it is passive already
			if (currentSimProcess() == compnt) // adds itself to the complex
			{
				compnt.skipTraceNote(); // don't tell the user, that we ...
				
				next.passivate=true;
				//TODO: how to passivate the process by yield 
				//((SimProcess)compnt).passivate(); // passivate the component process
			}
			
			return next;
		}

		/// <summary>Checks whether this composite process contains the specified process.</summary>
		/// <param name="elem">The process to find.</param>
		public bool contains(SimProcess elem)
		{

			// forward to the internal Vector
			return _components.Contains(elem);
		}

		/// <summary>Returns the component processes in this composite process.</summary>
		public IEnumerable<SimProcess> getComponents()
		{

			return _components;
		}

		/// <summary>Checks if this ComplexSimProcess has components or not.</summary>
		/// <returns>
		/// <c>true</c> if and only if this ComplexSimProcess has components; <c>false</c> otherwise.
		/// </returns>
		public bool hasComponents()
		{

			// forward to the internal Vector
			return (_components.Count > 0);
		}

		/**
		 * Override this method in a subclass of <code>ComplexSimProcess</code> to
		 * implement the specific behaviour of this process. This method starts
		 * after a <code>ComplexSimProcess</code> has been created and activated
		 * by the scheduler. As long as this <code>ComplexSimProcess</code> is
		 * active all its contained Simprocesses are passive. That means they are
		 * blocked and can not proceed in their lifeCycles.
		 */
		//public override abstract void lifeCycle();

		
		/**
		 * Removes all elements (SimProcesses and ComplexSimProcesses) from this
		 * ComplexSimProcess. This will be done for all ComplexSimPorcesses
		 * recursively, until all simple SimProcesses are removed. The SimProcesses
		 * being removed from the ComplexSimProcess will be activated after the
		 * current SimProcess so they can follow their own lifeCycle again. Of
		 * course, this does only make sense for SimProcesses which are not
		 * terminated already.
		 */
		public void removeAllComponents()
		{

			// loop through all elements
			foreach (var elem in getComponents())
			{
				// buffer the current element
				//SimProcess elem = (SimProcess) e.nextElement();

				// reset the supervisor of the SimProcess element removed
				elem.setSupervisor(null);

				// the SimProcess element is not blocked anymore
				elem.setBlocked(false);

				// activate the removed SimProcess (if it is not terminated yet)
				if (!elem.isTerminated())
				{
					elem.skipTraceNote(); // don't tell the user, that we ...
					elem.activateAfter(currentSimProcess()); // activate the
															 // process
															 // elem again
				}

				// if this ComplexSimProcess contains other ComplexSimProcesses
				// remove them also
				if (elem is ComplexSimProcess)
				{
					((ComplexSimProcess)elem).removeAllComponents();
				}
			}

			// remove the all the SimProcess elements from the components Vector
			_components.Clear();

			// trace output
			if (currentlySendTraceNotes())
			{
				sendTraceNote("removes all elements from " + this.getQuotedName());
			}

			// debug output
			if (currentlySendDebugNotes())
			{
				sendDebugNote("removes all its elements.");
			}

		}

		/// <summary>
		/// Removes a SimProcess from the elements of this ComplexSimProcess. The Sim-process being removed from
		/// the ComplexSimProcess will be activated after the current SimProcess so it can follow its own lifeCycle
		/// again.
		/// </summary>
		/// <param name="elem">
		/// The SimProcess to be removed from the elements of this ComplexSimProcess. Be careful, it might also be
		/// a <c>ComplexSimProcess</c>.
		/// </param>
		public void removeComponent(SimProcess elem)
		{

			// check the given SimProcess
			if (elem == null) // if elem is a null pointer instead of a process
			{
				sendWarning("Attempt to remove a non existing process from a "
						+ "ComplexSimProcess. The attempted action is ignored!",
						"ComplexSimProcess: " + getName()
								+ " Method: synchronized void"
								+ "removeComponent(SimProcess elem)",
						"The given SimProcess is only a null pointer.",
						"Make sure to remove only SimProcesses that are contained in this "
								+ "ComplexSimProcess.");

				return; // ignore that rubbish
			}

			// the SimProcess component must be contained in the container
			if (!_components.Contains(elem))
			{
				sendWarning(
						"Attempt to remove a process which is not an element of this "
								+ "ComplexSimProcess. The attempted action is ignored!",
						"ComplexSimProcess: " + getName()
								+ " Method: synchronized void"
								+ "removeComponent(SimProcess elem)",
						"The given SimProcess is not a component of this "
								+ "ComplexSimProcess.",
						"Make sure to remove only SimProcesses that are contained in "
								+ "this ComplexSimProcess.");
				return;
			}

			// reset the supervisor of the SimProcess element removed
			elem.setSupervisor(null);

			// remove the SimProcess element from the components Vector
			_components.Remove(elem);

			// the SimProcess element is not blocked anymore
			elem.setBlocked(false);

			// activate the removed SimProcess (if it is not terminated yet)
			if (!elem.isTerminated())
			{
				elem.skipTraceNote(); // don't tell the user, that we ...
				elem.activateAfter(currentSimProcess()); // activate the process
														 // elem again
			}

			// trace output
			if (currentlySendTraceNotes())
			{
				sendTraceNote("removes " + elem.getQuotedName() + " from "
						+ this.getQuotedName());
			}

			// debug output
			if (currentlySendDebugNotes())
			{
				sendDebugNote("removes " + elem.getQuotedName()
						+ " from its elements." + "it now looks like <br>"
						+ this.toString());
			}

		}

		/// <summary>
		/// Returns a <c>String</c> representation of this ComplexSimProcess, containing the <c>String</c>
		/// representation of each <c>SimProcess</c> component.
		/// </summary>
		/// <returns>A <c>String</c> representation of this ComplexSimProcess.</returns>
		public string toString()
		{

			var stringOfElems = new StringBuilder();

			stringOfElems.Append(this.getQuotedName() + " consists of : ");

			// make the String by collecting the Strings from all elements
			if (!hasComponents())
			{
				stringOfElems.Append("nothing else.");
			}
			else
			{
				// loop through all elements
				foreach (var e in getComponents())
				{
					stringOfElems.Append(e
							.getQuotedName());

					//if (e.hasMoreElements()) {
					//	stringOfElems.Append(", ");
					//} else {
					//	stringOfElems.Append(".");
					//}
				}
			}

			return stringOfElems.ToString();
		}
	}
}
