// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * external event switching on the trace output for the experiment. The
     * messagemanager's channel for tracenotes is switched on to forward tracenotes
     * to the configured Output.
     */
    public class ExternalEventTraceOn : ExternalEvent
    {
        /// <summary>Creates the external event to start the trace output for an experiment.</summary>
        /// <param name="owner">The model this external event is associated to</param>
        /// <param name="showInTrace">
        /// The flag indicating if this external event is shown in the trace output
        /// </param>
        public ExternalEventTraceOn(Model owner, bool showInTrace) : base(owner, "TraceOn", showInTrace)
        {

            //super(owner, "TraceOn", showInTrace);

        }

        /**
         * Switches the messagemanager's trace note channel on to forward tracenotes
         * to the configured output.
         */
        public override void eventRoutine()
        {

            Experiment ex = getModel().getExperiment();
            ex.getMessageManager().switchOn(Experiment.tracenote);
            if (currentlySendTraceNotes())
                sendTraceNote("Trace switched on");

        }
    }
}