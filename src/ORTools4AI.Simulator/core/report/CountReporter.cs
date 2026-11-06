// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author modified by Ruth Meyer
// @author modified by Chr. M&uuml;ller (TH Wildau) 28.11.2012
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Captures all relevant information about the Count.
     * Extended to show unit and description of reported object.
     */

	public class CountReporter : Reporter
	{

		// ****** methods ******

		/// <summary>
		/// Constructor for a new CountReporter. Note that although any Reportable is accepted you should make sure
		/// that only subtypes of Count are passed to this constructor. Otherwise the number of column titles and
		/// their individual headings will differ from the actual content collected by this reporter.
		/// </summary>
		/// <param name="informationSource">The Count to report about</param>

		public CountReporter(Reportable informationSource) : base(informationSource)
		{
			//super(informationSource); // make a Reporter

			numColumns = 8;

			columns = new string[numColumns];

			columns[0] = "Title";
			columns[1] = "Type";
			columns[2] = "(Re)set";
			columns[3] = "Obs";
			columns[4] = "Current Value";
			columns[5] = "Min";
			columns[6] = "Max";
			columns[7] = "Unit";
			groupHeading = "Counts and Aggregates";

			groupID = 1311; // see Reporter for more information about groupID

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
			if (source is Count)
			{
				// the Count we report about
				Count cnt = (Count)source;
				// source = informationSource
				// Title
				entries[0] = cnt.getName();
				// Type
				entries[1] = "Count";
				// (Re)set
				entries[2] = cnt.resetAt().toString();
				// Observations
				entries[3] = cnt.getObservations().ToString();
				// current value
				entries[4] = cnt.getValue().ToString();
				// Min
				entries[5] = cnt.getMinimum().ToString();
				// Max
				entries[6] = cnt.getMaximum().ToString();
				//cm 21.11.12  Extension for viewing unit
				entries[7] = cnt.getUnitText();

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
	} // end class CountReporter
}