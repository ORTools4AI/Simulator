// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.advancedModellingFeatures.report
{
	/**
     * Captures all relevant information about the Bin.
     */

	public class BinReporter : Reporter
	{

		// ****** methods ******

		/// <summary>
		/// Constructor for a new BinReporter. Note that although any Reportable is accepted you should make sure
		/// that only subtypes of Bin are passed to this constructor. Otherwise the number of column titles and
		/// their individual headings will differ from the actual content collected by this reporter.
		/// </summary>
		/// <param name="informationSource">The Bin to report about</param>
		public BinReporter(Reportable informationSource) : base(informationSource)
		{
			//super(informationSource); // make a Reporter

			numColumns = 14;
			columns = new String[numColumns];
			columns[0] = "Title";
			columns[1] = "Order";
			columns[2] = "pass";
			columns[3] = "(Re)set";
			columns[4] = "Prod";
			columns[5] = "Cons";
			columns[6] = "Init";
			columns[7] = "Max";
			columns[8] = "Now";
			columns[9] = "Averg.";
			columns[10] = "avg.Wait";
			columns[11] = "QLimit";
			columns[12] = "QMaxL";
			columns[13] = "refus.";
			groupHeading = "Bins";
			groupID = 1111; // see Reporter for more information about groupID
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
			if (source is Bin)
			{
				// the Bin we report about
				var b = (Bin)source; // source
									 // =
									 // informationSource

				// Title
				entries[0] = b.getName();
				// order
				entries[1] = b.getQueueStrategy();
				// pass
				String pass = "no";
				if (b.getPassBy())
				{
					pass = "yes";
				}
				entries[2] = pass;
				// (Re)set
				entries[3] = b.resetAt().toString();
				// Prod
				entries[4] = b.getProducers().ToString();
				// Cons
				entries[5] = b.getConsumers().ToString();
				// Init
				entries[6] = b.getInitial().ToString();
				// Max
				entries[7] = b.getMaximum().ToString();
				// Now.
				entries[8] = b.getAvail().ToString();
				// Average
				entries[9] = b.avgAvail().ToString();
				// avg.Wait
				entries[10] = b.averageWaitTime().toString();
				// Qlimit
				entries[11] = b.getQueueLimit().ToString();
				if (b.getQueueLimit() == int.MaxValue)
				{
					entries[11] = "unlimit.";
				}
				// QMaxL
				entries[12] = b.maxLength().ToString();
				// refused
				entries[13] = b.getRefused().ToString();
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
	} // end class BinReporter
}
