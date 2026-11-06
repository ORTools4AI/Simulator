// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author modified by Ruth Meyer
// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.dist;


namespace ORTools4AI.Simulator.core.engine
{

    /**
     * An <code>ArrivalProcess</code> is some kind of source for
     * <code>SimProcess</code> es. It makes them arrive from the 'outside world'
     * in the system of interest. So it can be viewed as the border of the system
     * (model). The user has to specify at which rate (frequency)
     * <code>SimProcess</code> es are arriving in the system. See the Constructor
     * for more details. The user has to derive from this class to make his own
     * ArrivalProcesses for the arriving processes he needs in his simulated system.
     * He has to implement the abstract method <code>createSuccessor()</code>,
     * which should create (instantiate) the newly arriving process. It is
     * recommended to use one ArrivalProcess for each different kind of arriving
     * process (with its specific arrival rate).
     */
    public abstract class ArrivalProcess<S> : SimProcess where S : SimProcess
    {

        /**
	     * The rate (frequency) at which the SimProcesses arrive at the system
	     * (border).
	     */
        private NumericalDist<double> _arrivalRate;

        /// <summary>Creates a process that generates arrivals using the specified rate distribution.</summary>
        /// <param name="owner">The model that owns the arrival process.</param>
        /// <param name="name">The name of the arrival process.</param>
        /// <param name="arrivalRate">The distribution that supplies the arrival rate.</param>
        /// <param name="showInTrace">Whether to include the arrival process in the trace output.</param>
        public ArrivalProcess(Model owner, string name, NumericalDist<double> arrivalRate,
                bool showInTrace) : base(owner, name, true, showInTrace) // make a SimProcess)
        {


            this._arrivalRate = arrivalRate;
        }

        /// <summary>Creates the next process to activate.</summary>
        /// <returns>The arriving process, or <c>null</c> to stop generating arrivals.</returns>
        public abstract S createSuccessor();

        /// <summary>Returns the distribution used to sample the arrival rate.</summary>
        public NumericalDist<double> getArrivalRate()
        {

            return _arrivalRate;
        }

        /// <summary>Creates and activates the next arriving process, then waits for the sampled delay.</summary>
        /// <remarks>If no successor is returned, the arrival process disables repetition and suspends.</remarks>
        //public void lifeCycle()
        protected override IEnumerable Run()
        {

            // make a new SimProcess
            var arrivingProcess = createSuccessor();

            if (arrivingProcess == null)
            {
                setRepeating(false);
                yield return Suspend();
            }

            // debug out
            if (currentlySendDebugNotes())
            {
                sendDebugNote("activates " + arrivingProcess.getQuotedName());
            }

            // make him arrive at the system right now (after this
            // ArrivalProcess)
            arrivingProcess.activate(new TimeSpan(0));

            // wait until next SimProcess is to arrive
            yield return Delay(_arrivalRate.sample());

        }
    }
}
