// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.advancedModellingFeatures
{

	/**
     * ProcessCoop is the object representing the process cooperation between two
     * processes. It is intended that this process cooperation is used with the
     * <code>WaitQueue</code> construct, where one process is the master which
     * really executes the cooperation and the other process is the slave waiting
     * for a master to lead him through the cooperation. That means that during the
     * cooperation the master is active and the slave is passive. The action carried
     * out for the two processes together is described in the virtual method
     * <code>cooperation</code>, which is to be implemented by the user building the
     * model. This class is encapsulating the cooperation of (at least) these two
     * processes. When using the WaitQueue construct the master will be activated
     * after the cooperation is done and the slave will be activated after the
     * master (if it has not been activated during the cooperation already).
     * 
     * @see desmoj.core.advancedModellingFeatures.WaitQueue
     */

	public abstract class ProcessCoop<M, S> : ModelComponent
	where M : SimProcess
		where S : SimProcess
	{

		// ****** methods ******

		/// <summary>Constructor for a ProcessCoop where a master and a slave process are cooperating.</summary>
		/// <param name="owner">The model this ProcessCoop is associated to.</param>
		/// <param name="name">The ProcessCoop's name</param>
		/// <param name="showInTrace">Flag for trace to produce trace messages.</param>
		public ProcessCoop(Model owner, string name,
				bool showInTrace) : base(owner, name, showInTrace)
		{
			//super(owner, name, showInTrace); // construct ModelComponent
		}

		/// <summary>
		/// Activates (schedules) the current SimProcess at the current Simulation time plus the offset <c>dt</c>.
		/// This method is passed through to the currently running master process.
		/// </summary>
		/// <param name="dt">The offset to the time now, when the current process is to be activated.</param>
		protected void activate(TimeSpan dt)
		{
			// get the current process
			SimProcess currentProcess = currentSimProcess();

			if (currentProcess == null)
			{
				sendWarning("The current process of a cooperation is not found. "
						+ "The attempted action is ignored!", "ProcessCoop: "
						+ getName() + " Method: activate()",
						"The current process is only a null pointer.",
						"Make sure that only real SimProcesses are cooperating.");
				return;
			}

			currentProcess.activate(dt);
		}

		/// <summary>
		/// Schedules the current SimProcess to be activated directly after the given Schedulable, which itself is
		/// already scheduled. Please make sure that the Schedulable given as parameter is actually scheduled. This
		/// method is passed through to the currently running master process.
		/// </summary>
		/// <param name="after">The Schedulable the currently running process should be scheduled after.</param>
		protected void activateAfter(Schedulable after)
		{
			// get the current process
			SimProcess currentProcess = currentSimProcess();

			if (currentProcess == null)
			{
				sendWarning("The current process of a cooperation is not found. "
						+ "The attempted action is ignored!", "ProcessCoop: "
						+ getName() + " Method: activateAfter()",
						"The current process is only a null pointer.",
						"Make sure that only real SimProcesses are cooperating.");
				return;
			}

			currentProcess.activateAfter(after);
		}

		/// <summary>
		/// Schedules the current SimProcess to be activated directly before the given Schedulable, which itself is
		/// already scheduled. Please make sure that the Schedulable given as parameter is actually scheduled. This
		/// method is passed through to the currently running master process.
		/// </summary>
		/// <param name="before">The Schedulable the currently running process should be scheduled before.</param>
		protected void activateBefore(Schedulable before)
		{
			// get the current process
			SimProcess currentProcess = currentSimProcess();

			if (currentProcess == null)
			{
				sendWarning("The current process of a cooperation is not found. "
						+ "The attempted action is ignored!", "ProcessCoop: "
						+ getName() + " Method: activateBefore()",
						"The current process is only a null pointer.",
						"Make sure that only real SimProcesses are cooperating.");
				return;
			}

			currentProcess.activateBefore(before);
		}

		/// <summary>
		/// The user building the model has to implement this method in a subclass. Here the action of the
		/// cooperation carried out by the master process will be described. If simulation time is used to perform
		/// some action the user has to model this by calling the method <c>hold(TimeSpan dt)</c>. The user (the
		/// one who is building the model) is responsible to implement this correctly in this <c>cooperation()</c>
		/// method.
		/// </summary>
		/// <param name="master">The master process which really carries out the cooperation.</param>
		/// <param name="slave">The slave process which is lead through the cooperation by the master.</param>
		public abstract void cooperation(M master, S slave);

		// the user has to implement the cooperation action here...

		/// <summary>
		/// Returns the priority of the current SimProcess (usually the master process). Default priority is zero.
		/// Higher priorities are positive, lower priorities negative. The priority determines the position in a
		/// waiting queue. This method is passed through to the currently running master process.
		/// </summary>
		/// <returns>The priority of the SimProcess.</returns>
		protected int getPriority()
		{
			// get the current process
			SimProcess currentProcess = currentSimProcess();

			if (currentProcess == null)
			{
				sendWarning("The current process of a cooperation is not found. "
						+ "Zero is returned as priority!", "ProcessCoop: "
						+ getName() + " Method: getPriority()",
						"The current process is only a null pointer.",
						"Make sure that only real SimProcesses are cooperating.");
				return 0;
			}

			return currentProcess.getQueueingPriority();
		}

		/// <summary>
		/// Holds the current SimProcess for the given time dt. Hold is used to simulate a time period during which
		/// the process is working on something in reality. But in the simulation the process is not active. Only
		/// the result is important for the simulation. (So the simulation time will be set to the new value and
		/// all the attributes which have changed during this time period are changed.) This method is passed
		/// through to the currently running master process.
		/// </summary>
		/// <param name="dt">The current SimProcess will be passivated during this time period.</param>
		protected void hold(TimeSpan dt)
		{
			// get the current process
			SimProcess currentProcess = currentSimProcess();

			if (currentProcess == null)
			{
				sendWarning("The current process of a cooperation is not found. "
						+ "The attempted action is ignored!", "ProcessCoop: "
						+ getName() + " Method: hold()",
						"The current process is only a null pointer.",
						"Make sure that only real SimProcesses are cooperating.");
				return;
			}

			currentProcess.hold(dt);
		}

		/// <param name="dt">The current SimProcess will be passivated during this time period.</param>
		/// <remarks>This methode is replaced by hold(TimeSpan dt).</remarks>
		//@Deprecated
		protected void hold(SimTime dt)
		{
			hold(SimTime.toTimeSpan(dt));
		}

		/**
		 * Passivates the current SimProcess for an unknown time period. It can only
		 * be activated by other objects of the simulation (SimProcesses or
		 * Entities). This method is passed through to the currently running master
		 * process.
		 */
		protected void passivate()
		{
			// get the current process
			SimProcess currentProcess = currentSimProcess();

			if (currentProcess == null)
			{
				sendWarning("The current process of a cooperation is not found. "
						+ "The attempted action is ignored!", "ProcessCoop: "
						+ getName() + " Method: passivate()",
						"The current process is only a null pointer.",
						"Make sure that only real SimProcesses are cooperating.");
				return;
			}
            //TODO:how to implement the passivate with yield ...
            //((SimProcess)currentProcess).passivate();
		}

		/// <summary>
		/// Reactivates (reschedules) the current SimProcess at the current point of simulation time plus the
		/// offset dt. This method is passed through to the currently running master process.
		/// </summary>
		/// <param name="dt">The offset to the time now, when the current process is to be reactivated.</param>
		protected void reActivate(TimeSpan dt)
		{
			// get the current process
			SimProcess currentProcess = currentSimProcess();

			if (currentProcess == null)
			{
				sendWarning("The current process of a cooperation is not found. "
						+ "The attempted action is ignored!", "ProcessCoop: "
						+ getName() + " Method: reActivate()",
						"The current process is only a null pointer.",
						"Make sure that only real SimProcesses are cooperating.");
				return;
			}

			currentProcess.reActivate(dt);
		}

		/// <summary>
		/// Sets the priority of the current SimProcess to a new integer value. Zero is the default priority.
		/// Negative priorities are lower, positive priorities are higher. All values should be inside the range
		/// defined by the C# <c>int</c> type. The priority determines the position in a
		/// waiting-queue. This method is passed through to the currently running master process.
		/// </summary>
		/// <param name="newPriority">The new priority value.</param>
		protected void setPriority(int newPriority)
		{
			// get the current process
			SimProcess currentProcess = currentSimProcess();

			if (currentProcess == null)
			{
				sendWarning("The current process of a cooperation is not found. "
						+ "The attempted action is ignored!", "ProcessCoop: "
						+ getName() + " Method: setPriority()",
						"The current process is only a null pointer.",
						"Make sure that only real SimProcesses are cooperating.");
				return;
			}

			currentProcess.setPriority(newPriority);
		}
	} // end abstract class ProcessCoop
}
