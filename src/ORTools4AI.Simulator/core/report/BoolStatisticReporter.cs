// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author modified by Ruth Meyer, Johannes G&ouml;bel
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Captures all relevant information about the BoolStatistic.
     */
	public class BoolStatisticReporter : Reporter
	{

		// ****** methods ******

		/// <summary>
		/// Constructor for a new BoolStatisticReporter. Note that although any Reportable is accepted you should
		/// make sure that only subtypes of BoolStatistic are passed to this constructor. Otherwise the number of
		/// column titles and their individual headings will differ from the actual content collected by this
		/// reporter.
		/// </summary>
		/// <param name="informationSource">The BoolStatistic to report about.</param>
		public BoolStatisticReporter(Reportable informationSource) : base(informationSource)
		{
			//super(informationSource); // make a Reporter

			numColumns = 5;
			columns = new string[numColumns];
			columns[0] = "Title";
			columns[1] = "(Re)set";
			columns[2] = "Obs";
			columns[3] = "ObsTrue";
			columns[4] = "Ratio";
			groupHeading = "Boolean Statistics";
			groupID = 1761; // see Reporter for more information about groupID
			entries = new String[numColumns];
		}

		/// <summary>
		/// Returns an array of Strings each containing the data for the corresponding column in array
		/// <c>columns[]</c>. Implement this method in a way, that an array of the same length as the column titles
		/// is produced containing the data at the point of time this method is called by someone else to produce
		/// up-to-date information.
		/// </summary>
		/// <returns>Array containing the data for reporting</returns>
		public override string[] getEntries()
		{
			if (source is BoolStatistic)
			{
				// the Tally we report about (source = informationSource)
				var bs = (BoolStatistic)source;

				// Title
				entries[0] = bs.getName();
				// (Re)set
				entries[1] = bs.resetAt().toString();
				// Obs
				entries[2] = bs.getObservations().ToString();
				// Obs true
				entries[3] = bs.getTrueObs().ToString();
				// True ratio
				if (bs.getObservations() == 0)
				{
					entries[4] = " ";
				}
				else
				{
					entries[4] = bs.getTrueRatio().ToString();
				}

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
	} // end class TallyReporter
}
