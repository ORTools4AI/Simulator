// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author modified by Ruth Meyer, Johannes G&ouml;bel
// @author modified by Chr. M&uumlller (TH Wildau) 28.11.2012
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Captures all relevant information about the Aggregate.
     * Extended to show unit and description of reported object.
     */

	public class AggregateReporter : Reporter
	{

		// ****** methods ******

		/// <summary>
		/// Constructor for a new AggregateReporter. Note that although any Reportable is accepted you should make
		/// sure that only subtypes of Aggregate are passed to this constructor. Otherwise the number of column
		/// titles and their individual headings will differ from the actual content collected by this reporter.
		/// </summary>
		/// <param name="informationSource">The Aggregate to report about</param>

		public AggregateReporter(Reportable informationSource) : base(informationSource)
		{
			//super(informationSource); // make a Reporter

			numColumns = 8;

			columns = new String[numColumns];

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

			entries = new String[numColumns];
		}

		/// <summary>
		/// Returns an array of Strings each containing the data for the corresponding column in array
		/// <c>columns[]</c>. Implement this method in a way, that an array of the same length as the columntitles
		/// is produced containing the data at the point of time this method is called by someone else to produce
		/// up-to-date information.
		/// </summary>
		/// <returns>Array containing the data for reporting</returns>
		public override String[] getEntries()
		{
			if (source is Aggregate)
			{
				// the Aggregate we report about
				Aggregate agg = (Aggregate)source;
				var _showTimeSpansInReport = agg.getShowTimeSpansInReport();

				// source = informationSource
				// Title
				entries[0] = agg.getName();
				// Type
				entries[1] = "Aggregate";
				// (Re)set
				entries[2] = agg.resetAt().toString();
				// Observations
				entries[3] = agg.getObservations().ToString();
				// current value
				entries[4] = this.format(_showTimeSpansInReport, agg.getValue());
				// Min
				entries[5] = this.format(_showTimeSpansInReport, agg.getMinimum());
				// Max
				entries[6] = this.format(_showTimeSpansInReport, agg.getMaximum());
				//cm 21.11.12  Extension for viewing unit
				entries[7] = agg.getUnitText();

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
			var output = value.ToString();
			if (showTimeSpans && value < 0.0) output += " (Invalid)";
			else if (showTimeSpans && value >= long.MaxValue) output += " (Invalid)";
			else if (showTimeSpans) output = new TimeSpan(value).toString();
			return output;
		}
	} // end class CountReporter
}