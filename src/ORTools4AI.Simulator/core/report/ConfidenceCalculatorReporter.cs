// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Captures all relevant information about the ConfidenceCalculator.
     */

	public class ConfidenceCalculatorReporter : TallyReporter {

		// ****** methods ******

		/// <summary>
		/// Constructor for a new ConfidenceCalculatorReporter. Note that although any Reportable is accepted you
		/// should make sure that only subtypes of ConfidenceCalculator are passed to this constructor. Otherwise
		/// the number of column titles and their individual headings will differ from the actual content collected
		/// by this reporter.
		/// </summary>
		/// <param name="informationSource">The ConfidenceCalculator to report about.</param>
		public ConfidenceCalculatorReporter(
				Reportable informationSource):base(informationSource) {
			//super(informationSource); // make a Reporter

			numColumns = 11;
			columns = new string[numColumns];
			columns[0] = "Title";
			columns[1] = "(Re)set";
			columns[2] = "Obs";
			columns[3] = "Mean";
			columns[4] = "Std.Dv";
			columns[5] = "Min";
			columns[6] = "Max";
			columns[7] = "ConfLevel";
			columns[8] = "ConfMeanLower";
			columns[9] = "ConfMeanUpper";
			columns[10] = "Unit";
			groupHeading = "ConfidenceCalculators";
			groupID = 1661; // see Reporter for more information about groupID
			entries = new string[numColumns];
		}

		/// <summary>
		/// Returns an array of Strings each containing the data for the corresponding column in array
		/// <c>columns[]</c>. Implement this method in a way, that an array of the same length as the column titles
		/// is produced containing the data at the point of time this method is called by someone else to produce
		/// up-to-date information.
		/// </summary>
		/// <returns>Array containing the data for reporting</returns>
		public String[] getEntries() {
			if (source is ConfidenceCalculator) {
				// the Tally we report about (source = informationSource)
				var cc = (ConfidenceCalculator)source;

				// Title
				entries[0] = cc.getName();
				// (Re)set
				entries[1] = cc.resetAt().toString();
				// Obs
				entries[2] =cc.getObservations().ToString();
				// Mean/Total
				// no observations made, so Mean can not be calculated
				if (cc.getObservations() == 0) {
					entries[3] = "Insufficient data";
				} else // return mean value
				{
					entries[3] = cc.getMean().ToString();
				}

				// Std.Dev/Total
				// not enough observations are made, so Std.Dev can not be
				// calculated
				if (cc.getObservations() < 2) {
					entries[4] = "Insufficient data";
				} else // return standard deviation
				{
					entries[4] = cc.getStdDev().ToString();
				}

				// Min./Total
				if (cc.getObservations() == 0) {
					entries[5] = "Insufficient data";
				} else {
					entries[5] = cc.getMinimum().ToString();
				}
				// Max./Total
				if (cc.getObservations() == 0) {
					entries[6] = "Insufficient data";
				} else {
					entries[6] = cc.getMaximum().ToString();
				}

				// ConfLevel
				entries[7] = cc.getConfidenceLevel().ToString();

				// LowerConfidenceInterval
				if (cc.getObservations() < 2) {
					entries[8] = "Insufficient data";
				} else {
					entries[8] = cc.getConfidenceIntervalOfMeanLowerBound().ToString();
				}

				// UpperConfidenceInterval
				if (cc.getObservations() < 2) {
					entries[9] = "Insufficient data";
				} else {
					entries[9] = cc.getConfidenceIntervalOfMeanUpperBound().ToString();
				}

				// Unit
				entries[10] = cc.getUnitText();

			} else {
				for (int i = 0; i < numColumns; i++) {
					entries[i] = "Invalid source!";
				} // end for
			} // end else

			return entries;
		}
	} // end class TallyReporter

}