// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler, Johannes G&ouml;bel
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{

    /**
     * A reporter about a simulation run, including information like
     * simulation duration, computation duration and last reset.
     */
    public class SimulationRunReporter : TableReporter
    {

        /**
         * An inner class providing the simulation report.
         */


        /**
         * System time at which the simulation has started
         */
        protected long startedAtSystemMillis;

        /// <summary>Creates a simulation run reporter for the given model.</summary>
        /// <param name="model">The model to report about.</param>
        public SimulationRunReporter(Model model):base(model, "Simulation Run")
        {

            //super(model, "Simulation Run");

            startedAtSystemMillis = System.DateTime.Now.Millisecond;
            groupID = 2000000000;
            numRows = 5;
            numColumns = 2;
            columns = new string[] { "Property", "Content" };
            groupHeading = "";
        }

        /// <summary>Returns the output of the SimulationRunReporter.</summary>
        /// <returns>String[][] : The simulation data to be written into the report.</returns>
        public override string[][] getEntryTable()
        {

            // Determine Computation duration
            long endAtSystemMillis = DateTime.Now.Millisecond;
            int days_tot = 0; int hours_tot = 0; int minutes_tot = 0; int seconds_tot = 0;
            int elapsed_tot = (int)(endAtSystemMillis - startedAtSystemMillis) / 1000;
            seconds_tot = elapsed_tot % 60;
            elapsed_tot /= 60;
            minutes_tot = elapsed_tot % 60;
            elapsed_tot /= 60;
            hours_tot = elapsed_tot % 24;
            elapsed_tot /= 24;
            days_tot = elapsed_tot;
            //DecimalFormat formatter = new DecimalFormat("00");
            //string duration_tot = (days_tot > 0 ? days_tot + " day" + (days_tot > 1 ? "s " : " ") : "") 
            //    + formatter.format(hours_tot) + ":" + formatter.format(minutes_tot) + ":" + formatter.format(seconds_tot);
            string duration_tot = string.Empty;

            entryTable = new string[numRows][];
            for (int i = 0; i < entryTable.Length; i++)
                entryTable[i] = new string[numColumns];

            Model m = (Model)source;

            entryTable[0][0] = "Simulation duration";
            entryTable[0][1] = "Experiment run from " + TimeOperations.getStartTime() + " until " +
                         m.presentTime() + ".";

            entryTable[1][0] = "Computation duration (HH:MM:SS)";
            entryTable[1][1] = duration_tot;

            entryTable[2][0] = "Resets";
            entryTable[2][1] = TimeInstant.isAfter(m.resetAt(), TimeOperations.getStartTime()) ?
                         "Last reset at " + m.resetAt() + "."
                       : "No resets during the experiment run.";

            entryTable[3][0] = "Seed";
            entryTable[3][1] = getModel().getExperiment().getDistributionManager().getSeed().ToString();

            entryTable[4][0] = "Errors";
            entryTable[4][1] = m.getExperiment().hasError() ?
                         "Attention: At least one error or warning has occurred. See error output for details."
                       : "No errors or warnings have occurred.";

            return entryTable;
        }
    }

    public class SimulationRunReporterProvider : Reportable
    {

        SimulationRunReporter r;

        public SimulationRunReporterProvider(Model model) : base(model, "SimulationrunReportProvider", true, false)
        {
            //super(model, "SimulationrunReportProvider", true, false);
            r = new SimulationRunReporter(model);
        }

        public Reporter createReporter()
        {
            return r;
        }
    }
}