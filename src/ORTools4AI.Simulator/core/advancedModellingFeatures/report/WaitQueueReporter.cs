// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author edited by Lorna Slawski (added removed processes)
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.advancedModellingFeatures;


namespace ORTools4AI.Simulator.core.advancedModellingFeatures.report
{

    /**
     * Captures all relevant information about the <code>WaitQueue</code>. That
     * means from the master and the slave queue.
     */

    public class WaitQueueReporter : Reporter {

        // ****** methods ******

        /// <summary>
        /// Constructor for a new WaitQueueReporter. Note that although any Reportable is accepted you should
        /// make sure that only subtypes of WaitQueue are passed to this constructor. Otherwise the number of
        /// column titles and their individual headings will differ from the actual content collected by this
        /// reporter.
        /// </summary>
        /// <param name="informationSource">The WaitQueue to report about</param>
        public WaitQueueReporter(Reportable informationSource):base(informationSource) {
            //super(informationSource); // make a Reporter

            groupHeading = "Wait-Queues";
            groupID = 611; // see Reporter for more information about groupID

            numColumns = 13;
            columns = new String[numColumns];
            columns[0] = "Title";
            columns[1] = "Qorder";
            columns[2] = "(Re)set";
            columns[3] = "CoopCompl";
            columns[4] = "QLimit";
            columns[5] = "Qmax";
            columns[6] = "Qnow";
            columns[7] = "Qavg.";
            columns[8] = "Zeros";
            columns[9] = "avg.Wait";
            columns[10] = "avg.Coop";
            columns[11] = "refused";
            columns[12] = "removed";

            entries = new string[numColumns * 2]; // entries of master and slave
                                                  // queue plus a potential description
        }

        /// <summary>
        /// Returns an array of Strings each containing the data for the corresponding column in array
        /// <c>columns[]</c>. This method is fetching the data from the master and the slave WaitQueue.
        /// Implement this method in a way, that an array of the same length as the columntitles is produced
        /// containing the data at the point of time this method is called by someone else to produce
        /// up-to-date information.
        /// </summary>
        /// <returns>Array containing the data for reporting</returns>
        public override string[] getEntries() {

            if (source is WaitQueue<SimProcess,SimProcess>) {
                // the WaitQueue we report about (source = informationsource)
               var wq = (WaitQueue<SimProcess,SimProcess>)source;

                // *** entries of the master queue
                // Title
                entries[0] = wq.getName();
                // mQorder
                entries[1] = wq.getMQueueStrategy();
                // (Re)set
                entries[2] = wq.resetAt().toString();
                // Completed
                entries[3] = wq.getCooperationsCompleted().ToString();
                // Qlimit
                entries[4] = wq.getQueueLimit().ToString();
                if (wq.getQueueLimit() == int.MaxValue) {
                    entries[4] = "unlimit.";
                }
                // Qmax
                entries[5] = wq.mMaxLength().ToString();
                // Qnow
                entries[6] = wq.mLength().ToString();
                // Qavg.
                entries[7] = wq.mAverageLength().ToString();
                // Zeros
                entries[8] = wq.mZeroWaits().ToString();
                // avg.Wait
                entries[9] = wq.mAverageWaitTime().toString();
                // avg.Coop
                entries[10] = wq.mAverageCoopTime().toString();
                // refused
                entries[11] = wq.getMRefused().ToString();
                // removed          
                entries[12] = wq.getMRemoved().ToString();

                // *** entries of the slave queue
                // Title
                entries[13] = wq.getSlaveQueue().getName();
                // sQorder
                entries[14] = wq.getSQueueStrategy();
                // (Re)set
                entries[15] = wq.getSlaveQueue().resetAt().toString();
                // Completed
                entries[16] = wq.getCooperationsCompleted().ToString();
                // Qlimit
                entries[17] = wq.getSlaveQueue().getQueueLimit().ToString();
                if (wq.getSlaveQueue().getQueueLimit() == int.MaxValue) {
                    entries[17] = "unlimit.";
                }
                // Qmax
                entries[18] = wq.sMaxLength().ToString();
                // Qnow
                entries[19] = wq.sLength().ToString();
                // Qavg.
                entries[20] = wq.sAverageLength().ToString();
                // Zeros
                entries[21] = wq.sZeroWaits().ToString();
                // avg.Wait
                entries[22] = wq.sAverageWaitTime().toString();
                // avg.Coop
                entries[23] = wq.mAverageCoopTime().toString();
                // refused
                entries[24] = wq.getSRefused().ToString();
                // removed          
                entries[25] = wq.getSRemoved().ToString();

            } else {
                for (int i = 0; i < numColumns * 2; i++) {
                    entries[i] = "Invalid source!";
                } // end for
            } // end else

            return entries;
        }

        public override bool isTwoRowReporter() {
            return true;
        }
    } // end class WaitQueueReporter
}