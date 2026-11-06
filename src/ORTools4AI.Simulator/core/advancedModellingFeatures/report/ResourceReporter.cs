// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;

namespace ORTools4AI.Simulator.core.advancedModellingFeatures.report
{
	/**
     * Captures all relevant information about the Res.
     */

	public class ResourceReporter : Reporter {

		// ****** methods ******

		/// <summary>
		/// Constructor for a new ResourceReporter. Note that although any Reportable is accepted you should make
		/// sure that only subtypes of Res are passed to this constructor. Otherwise the number of column titles
		/// and their individual headings will differ from the actual content collected by this reporter.
		/// </summary>
		/// <param name="informationSource">The Res to report about.</param>
		public ResourceReporter(Reportable informationSource):base(informationSource) {
			//super(informationSource); // make a Reporter

			numColumns = 14;
			columns = new string[numColumns];
			columns[0] = "Title";
			columns[1] = "Order";
			columns[2] = "pass";
			columns[3] = "(Re)set";
			columns[4] = "Users";
			columns[5] = "Limit";
			columns[6] = "Min";
			columns[7] = "Now";
			columns[8] = "Usage[%]";
			columns[9] = "avg.Wait";
			columns[10] = "QLimit";
			columns[11] = "QMaxL";
			columns[12] = "refus.";
			columns[13] = "DL";
			groupHeading = "Resources";
			groupID = 811; // see Reporter for more information about groupID
			entries = new string[numColumns];
		}

		/// <summary>
		/// Returns an array of Strings each containing the data for the corresponding column in array
		/// <c>columns[]</c>. Implement this method in a way, that an array of the same length as the columntitles
		/// is produced containing the data at the point of time this method is called by someone else to produce
		/// up-to-date information.
		/// </summary>
		/// <returns>Array containing the data for reporting</returns>
		public override string[] getEntries() {
			if (source is Res) {
				// the Res we report about
				var rs = (Res)source; // source
																												  // =
																												  // informationSource

				// Title
				entries[0] = rs.getName();
				// order
				entries[1] = rs.getQueueStrategy();
				// pass
				String pass = "no";
				if (rs.getPassBy()) {
					pass = "yes";
				}
				entries[2] = pass;
				// (Re)set
				entries[3] = rs.resetAt().toString();
				// Users
				entries[4] =rs.getUsers().ToString();
				// Limit
				entries[5] =rs.getLimit().ToString();
				// Min
				entries[6] = rs.getMinimum().ToString();
				// Now.
				entries[7] = rs.getAvail().ToString();
				// Usage[%]
				entries[8] = StatisticObject.round(rs.avgUsage() * 100).ToString();
				// avg.Wait
				entries[9] = rs.averageWaitTime().toString();
				// Qlimit
				entries[10] = rs.getQueueLimit().ToString();
				if (rs.getQueueLimit() == int.MaxValue) {
					entries[10] = "unlimit.";
				}
				// QMaxL
				entries[11] = rs.maxLength().ToString();
				// refused
				entries[12] = rs.getRefused().ToString();
				// DL
				String deadLock = "no";
				if (rs.isDeadlockDetected()) {
					deadLock = "yes";
				}
				entries[13] = deadLock;
			} else {
				for (int i = 0; i < numColumns; i++) {
					entries[i] = "Invalid source!";
				} // end for
			} // end else

			return entries;
		}
	} // end class ResourceReporter
}
