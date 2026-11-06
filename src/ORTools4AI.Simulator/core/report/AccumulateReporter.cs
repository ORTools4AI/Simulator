// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author modified by Chr. M&uuml;ller (TH Wildau) 28.11.2012
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;
namespace ORTools4AI.Simulator.core.report
{

    /**
     * Captures all relevant information about the Accumulate.
     */

    public class AccumulateReporter : Reporter
    {

        // ****** methods ******

        /// <summary>
        /// Constructor for a new AccumulateReporter. Note that although any Reportable is accepted you
        /// should make sure that only subtypes of Accumulate are passed to this constructor. Otherwise the
        /// number of column titles and their individual headings will differ from the actual content
        /// collected by this reporter.
        /// </summary>
        /// <param name="informationSource">The Accumulate to report about.</param>
        public AccumulateReporter(Reportable informationSource) : base(informationSource)
        {
            //super(informationSource); // make a Reporter (source =
            // informationSource)

            numColumns = 9;
            columns = new String[numColumns];
            columns[0] = "Title";
            columns[1] = "(Re)set";
            columns[2] = "Obs";
            columns[3] = "Mean";
            columns[4] = "Std.Dv";
            columns[5] = "Min";
            columns[6] = "Max";
            columns[7] = "Period";
            columns[8] = "Unit";
            groupHeading = "Accumulates";
            groupID = 1711; // see Reporter for more information about groupID
            entries = new String[numColumns];
        }

        /// <summary>
        /// Returns an array of Strings each containing the data for the corresponding column in array
        /// <c>columns[]</c>. Implement this method in a way, that an array of the same length as the
        /// columntitles is produced containing the data at the point of time this method is called by
        /// someone else to produce up-to-date information.
        /// </summary>
        /// <returns>Array containing the data for reporting</returns>
        public override string[] getEntries()
        {
            if (source is Accumulate)
            {

                // the Accumulate we report about (source = informationSource)
                var acc = source as Accumulate;
                bool _showTimeSpansInReport = acc.getShowTimeSpansInReport();

                // Title
                entries[0] = acc.getName();
                // (Re)set
                entries[1] = acc.resetAt().toString();
                // Obs
                entries[2] = acc.getObservations().ToString();

                // Mean
                // has no time passed since the last reset?
                if (engine.TimeSpan.isEqual(TimeOperations
                        .diff(acc.presentTime(), acc.resetAt()), TimeSpan.ZERO)
                        || acc.getObservations() == 0) // OR no observations are
                                                       // made
                {
                    entries[3] = "Insufficient data";
                }
                else // return mean value
                {
                    entries[3] = this.format(_showTimeSpansInReport, acc.getMean());

                }

                // Std.Dev

                // has no time passed since the last reset?
                if (TimeSpan.isEqual(TimeOperations
                        .diff(acc.presentTime(), acc.resetAt()), TimeSpan.ZERO)
                        || acc.getObservations() < 2) // OR not enough
                                                      // observations
                                                      // are made
                {
                    entries[4] = "Insufficient data";
                }
                else // return mean value
                {
                    entries[4] = this.format(_showTimeSpansInReport, acc.getStdDev());

                }

                // Min.
                if (acc.getObservations() == 0)
                {
                    entries[5] = "Insufficient data";
                }
                else
                {
                    entries[5] = this.format(_showTimeSpansInReport, acc.getMinimum());

                }
                // Max
                if (acc.getObservations() == 0)
                {
                    entries[6] = "Insufficient data";
                }
                else
                {
                    entries[6] = this.format(_showTimeSpansInReport, acc.getMaximum());

                }
                // Period
                entries[7] = acc.getPeriodMeasured().toString();

                //cm 21.11.12  Extension for viewing unit
                entries[8] = acc.getUnitText();

            }
            else
            {
                for (int i = 0; i < numColumns; i++)
                {
                    entries[i] = "Invalid source!";
                } // end for
            } // end else

            return entries;
        }

        private string format(bool showTimeSpans, double value)
        {
            String output = value.ToString();
            if (showTimeSpans && value < 0.0) output += " (Invalid)";
            else if (showTimeSpans && value >= long.MaxValue) output += " (Invalid)";
            else if (showTimeSpans) output = new TimeSpan(value).toString();
            return output;
        }

    } // end class AccumulateReporter
}