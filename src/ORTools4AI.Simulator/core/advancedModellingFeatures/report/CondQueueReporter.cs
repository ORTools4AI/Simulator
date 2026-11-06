// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.advancedModellingFeatures;
using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.advancedModellingFeatures.report
{

	/**
	 * Captures all relevant information about the CondQueue.
	 */

	public class CondQueueReporter : Reporter
	{

		// ****** methods ******

		/// <summary>
		/// Constructor for a new CondQueueReporter. Note that although any Reportable is accepted you should make
		/// sure that only subtypes of CondQueue are passed to this constructor. Otherwise the number of column
		/// titles and their individual headings will differ from the actual content collected by this reporter.
		/// </summary>
		/// <param name="informationSource">The CondQueue to report about</param>
		public CondQueueReporter(Reportable informationSource) : base(informationSource)
		{
			//super(informationSource); // make a Reporter

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
			columns[9] = "avg.Wait";
			columns[10] = "refus.";
			columns[11] = "All";
			groupHeading = "Cond-Queues";
			groupID = 711; // see Reporter for more information about groupID
			entries = new string[numColumns];
		}

		/// <summary>
		/// Returns an array of Strings each containing the data for the corresponding column in array
		/// <c>columns[]</c>. Implement this method in a way, that an array of the same length as the columntitles
		/// is produced containing the data at the point of time this method is called by someone else to produce
		/// up-to-date information.
		/// </summary>
		/// <returns>Array containing the data for reporting</returns>
		public override string[] getEntries()
		{
			if (source is CondQueue<SimProcess>)
			{
				// the CondQueue we report about
				var cq = (CondQueue<SimProcess>)source;
				// source = informationSource
				// Title
				entries[0] = cq.getName();
				// Qorder
				entries[1] = cq.getQueueStrategy();
				// (Re)set
				entries[2] = cq.resetAt().toString();
				// Obs
				entries[3] = cq.getObservations().ToString();
				// Qlimit
				entries[4] = cq.getQueueLimit().ToString();
				if (cq.getQueueLimit() == int.MaxValue)
				{
					entries[4] = "unlimit.";
				}
				// Qmax
				entries[5] = cq.maxLength().ToString();
				// Qnow
				entries[6] = cq.length().ToString();
				// Qavg..
				entries[7] = cq.averageLength().ToString();
				// Zeros
				entries[8] = cq.zeroWaits().ToString();
				// avg.Wait
				entries[9] = cq.averageWaitTime().toString();
				// refused
				entries[10] = cq.getRefused().ToString();
				// All
				entries[11] = cq.getCheckAll().ToString();
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
	} // end class CondQueueReporter
}

