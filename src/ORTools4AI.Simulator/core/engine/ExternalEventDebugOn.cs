// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * ExternalEvent switching on the debug output for the experiment. The
     * messagemanager's channel for debugnotes is switched on to forward tracenotes
     * to the configured Output.
     */
    public class ExternalEventDebugOn : ExternalEvent
    {
        /// <summary>Creates the external event to start the debug output for an experiment.</summary>
        /// <param name="owner">The model this external event is associated to</param>
        /// <param name="showInTrace">
        /// The flag indicating if this external event is shown in the trace output
        /// </param>
        public ExternalEventDebugOn(Model owner, bool showInTrace) : base(owner, "DebugOn", showInTrace)
        {

            //super(owner, "DebugOn", showInTrace);

        }

        /**
         * Switches the messagemanager's debug note channel on to forward debugnotes
         * to the configured output.
         */
        public override void eventRoutine()
        {

            Experiment ex = getModel().getExperiment();
            ex.getMessageManager().switchOn(Experiment.debugnote);
            if (currentlySendTraceNotes())
                sendTraceNote("Debug switched on");

        }
    }
}