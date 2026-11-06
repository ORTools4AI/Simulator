// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * external event switching off the trace output for the experiment. The
     * messagemanager's channel for tracenotes is switched off to stop forwarding
     * tracenotes to the configured output.
     */
    public class ExternalEventTraceOff : ExternalEvent
    {
        /// <summary>Creates the external event to stop the trace output for an experiment.</summary>
        /// <param name="owner">The model this external event is associated to</param>
        /// <param name="showInTrace">
        /// The flag indicating if this external event is shown in the trace output
        /// </param>
        public ExternalEventTraceOff(Model owner, bool showInTrace) : base(owner, "TraceOff", showInTrace)
        {

            //super(owner, "TraceOff", showInTrace);

        }

        /**
         * Switches the messagemanager's trace note channel off to stop forward
         * tracenotes to the configured output.
         */
        public override void eventRoutine()
        {

            Experiment ex = getModel().getExperiment();
            if (currentlySendTraceNotes())
                sendTraceNote("Trace switched off");
            ex.getMessageManager().switchOff(Experiment.tracenote);

        }
    }
}