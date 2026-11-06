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
     * Captures all relevant information about the Tally.
     * Extended to show unit and description of reported object.
     */

	public class TallyReporter : Reporter
	{

		// ****** methods ******

		/// <summary>
		/// Constructor for a new TallyReporter. Note that although any Reportable is accepted you should make sure
		/// that only subtypes of Tally are passed to this constructor. Otherwise the number of column titles and
		/// their individual headings will differ from the actual content collected by this reporter.
		/// </summary>
		/// <param name="informationSource">The Tally to report about.</param>
		public TallyReporter(Reportable informationSource) : base(informationSource)
		{
			// make a Reporter

			numColumns = 8;
			columns = new string[numColumns];
			columns[0] = "Title";
			columns[1] = "(Re)set";
			columns[2] = "Obs";
			columns[3] = "Mean";
			columns[4] = "Std.Dv";
			columns[5] = "Min";
			columns[6] = "Max";
			columns[7] = "Unit";
			groupHeading = "Tallies";
			groupID = 1611; // see Reporter for more information about groupID
			entries = new string[numColumns];
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
			if (source is Tally)
			{
				// the Tally we report about (source = informationSource)
				var tl = (Tally)source;
				var _showTimeSpansInReport = tl.getShowTimeSpansInReport();
				TallyRunning tlr = null;
				if (tl is TallyRunning)
				{
					tlr = (TallyRunning)tl;
				}

				// Title
				entries[0] = tl.getName();
				// (Re)set
				entries[1] = tl.resetAt().toString();
				// Obs
				entries[2] = tl.getObservations().ToString();
				// Mean
				// no observations made, so Mean can not be calculated
				if (tl.getObservations() == 0)
				{
					entries[3] = "Insufficient data";
				}
				else // return mean value
				{
					//entries[3] = this.format(_showTimeSpansInReport, tl.getMean());
					if (tlr != null)
						entries[3] += " (last "
								+ tlr.getSampleSizeN()
								+ " obs: "
								//+ this.format(_showTimeSpansInReport, tlr.getMeanLastN())
								+ ")";
				}

				// Std.Dev
				// not enough observations are made, so Std.Dev can not be
				// calculated
				if (tl.getObservations() < 2)
				{
					entries[4] = "Insufficient data";
				}
				else // return standard deviation
				{
					//entries[4] = this.format(_showTimeSpansInReport, tl.getStdDev());
					if (tlr != null)
						entries[4] += " (last "
								+ tlr.getSampleSizeN()
								+ " obs: "
								//+ this.format(_showTimeSpansInReport, tlr.getStdDevLastN())
								+ ")";
				}

				// Min
				if (tl.getObservations() == 0)
				{
					entries[5] = "Insufficient data";
				}
				else
				{
					//entries[5] = this.format(_showTimeSpansInReport, tl.getMinimum());
					if (tlr != null)
						entries[5] += " (last "
								+ tlr.getSampleSizeN()
								+ " obs: "
								//+ this.format(_showTimeSpansInReport, tlr.getMinimumLastN())
								+ ")";
				}

				// Max
				if (tl.getObservations() == 0)
				{
					entries[6] = "Insufficient data";
				}
				else
				{
					//entries[6] = this.format(_showTimeSpansInReport, tl.getMaximum());
					if (tlr != null)
						entries[6] += " (last "
								+ tlr.getSampleSizeN()
								+ " obs: "
								//+ this.format(_showTimeSpansInReport, tlr.getMaximumLastN())
								+ ")";
				}

				//cm 21.11.12  Extension for viewing unit
				entries[7] = tl.getUnitText();

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
			string output = value.ToString();
			if (showTimeSpans && value < 0.0) output += " (Invalid)";
			else if (showTimeSpans && value >= long.MaxValue) output += " (Invalid)";
			else if (showTimeSpans) output = new TimeSpan(value).toString();
			return output;
		}

	} // end class TallyReporter
}
