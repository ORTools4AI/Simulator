// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// @author modified by Soenke Claassen
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;


namespace ORTools4AI.Simulator.core.report
{
    /**
   * Captures all relevant information about a queue.
   */
    public class QueueReporter : Reporter
    {
        /// <summary>
        /// Creates a QueueReporter. Note that although any Reportable is accepted you should make sure that
        /// only subtypes of Queue are passed to this constructor. Otherwise the number of column titles and
        /// their individual headings will differ from the actual content collected by this reporter.
        /// </summary>
        /// <param name="informationSource">The Queue to report about</param>
        public QueueReporter(Reportable informationSource) : base(informationSource)
        {

            //super(informationSource);

            numColumns = 12;
            columns = new string[numColumns];
            columns[0] = "Title";
            columns[1] = "Qorder";
            columns[2] = "(Re)set";
            columns[3] = "Obs";
            columns[4] = "QLimit";
            columns[5] = "Qmax";
            columns[6] = "Qnow";
            columns[7] = "Qavg.";
            columns[8] = "Zeros";
            columns[9] = "max.Wait";
            columns[10] = "avg.Wait";
            columns[11] = "refus.";
            groupHeading = "Queues";
            groupID = 201;
            entries = new string[numColumns];

        }

        /// <summary>
        /// Returns an array of Strings each containing the data for the corresponding column in array
        /// <c>columns[]</c>. Implement this method tha an array of the same langth as the columntitles is
        /// produced containing the data at the point of time this method is called by someone else to
        /// produce up-to-date information.
        /// </summary>
        /// <returns>Array containing the data for reporting</returns>
        public override string[] getEntries()
        {

            if (source is engine.Queue<Entity>) {

                // variable valid for block only - for faster access
                var q = (engine.Queue <Entity>) source;

                // Title
                entries[0] = source.getName();
                // Qorder
                entries[1] = q.getQueueStrategy();
                // (Re)set
                entries[2] = source.resetAt().toString();
                // Obs
                entries[3] = source.getObservations().ToString();
                // Qlimit
                entries[4] = q.getQueueLimit().ToString();
                if (q.getQueueLimit() == int.MaxValue)
                {
                    entries[4] = "unlimit.";
                }
                // Qmax
                entries[5] = q.maxLength().ToString();
                // Qnow
                entries[6] = q.length().ToString();
                // Qavg.
                entries[7] = q.averageLength().ToString();
                // Zeros
                entries[8] = q.zeroWaits().ToString();
                // max.Wait
                entries[9] = q.maxWaitTime().toString();
                // avg.Wait
                entries[10] = q.averageWaitTime().toString();
                // refused
                entries[11] = q.getRefused().ToString();

            } else
            {

                for (int i = 0; i < numColumns; i++)
                {
                    entries[i] = "Invalid source!";
                }

            }

            return entries;

        }
    }
}
