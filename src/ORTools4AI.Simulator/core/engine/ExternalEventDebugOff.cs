// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * external event switching off the debug output for the experiment. The
     * messagemanager's channel for debugnotes is switched off to stop forwarding
     * debugnotes to the configured Output.
     */
    public class ExternalEventDebugOff : ExternalEvent
    {
        /// <summary>Creates the external event to stop the debug output for an experiment.</summary>
        /// <param name="owner">The model this external event is associated to</param>
        /// <param name="showInTrace">
        /// The flag indicating if this external event is shown in the trace output
        /// </param>
        public ExternalEventDebugOff(Model owner, bool showInTrace) : base(owner, "DebugOff", showInTrace)
        {

            //super(owner, "DebugOff", showInTrace);

        }

        /**
         * Switches the messagemanager's debug note channel off to stop forwarding
         * debugnotes to the configured output.
         */
        public override void eventRoutine()
        {

            Experiment ex = getModel().getExperiment();
            if (currentlySendTraceNotes())
                sendTraceNote("Debug switched off");
            ex.getMessageManager().switchOff(Experiment.debugnote);

        }

    }
}