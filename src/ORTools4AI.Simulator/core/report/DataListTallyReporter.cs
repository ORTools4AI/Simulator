// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Janz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using static ORTools4AI.Simulator.core.statistic.DataListTally;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Captures all relevant information about the DataListTally.
     */

	public class DataListTallyReporter : TallyReporter {

		// ****** methods ******

		/// <summary>
		/// Constructor for a new DataListTallyReporter. Note that although any Reportable is accepted you should
		/// make sure that only subtypes of DataListTally are passed to this constructor. Otherwise the number of
		/// column titles and their individual headings will differ from the actual content collected by this
		/// reporter.
		/// </summary>
		/// <param name="informationSource">The DataListTally to report about.</param>
		public DataListTallyReporter(
				Reportable informationSource) : base(informationSource) {
			// make a Reporter

			numColumns = numColumns + 3;
			//columns = Array.Copy(columns, numColumns);
			columns[numColumns - 3] = "Lower Quartile";
			columns[numColumns - 2] = "Median";
			columns[numColumns - 1] = "Upper Quartile";
			groupHeading = "Data-List Tallies";
			groupID = 1911; // see Reporter for more information about groupID
			entries = new String[numColumns];
		}

		/// <summary>Checks if a double represents an interger number</summary>
		/// <param name="n">the double number to check</param>
		/// <returns>true if the given double number is an integer number, false if not.</returns>
		private static bool isIntegerNumber(double n) {
			return (n - (int)n == 0.0);
		}

		/// <summary>
		/// Calculates the quartiles for the given list of data. This list have to be sorted to produce correct
		/// results. This method is called from the getEntries-Method to get all values needed for box-plots.
		/// </summary>
		/// <param name="list">The sorted list to calculate the quartiles for.</param>
		/// <returns>the quartiles from the given list.</returns>
		private static double[] getQuartiles(
				DataList list) {
			int n = list.getLength();

			if (n == 0)
				return null;

			if (n < 3)
				return new double[] { (n == 2) ? (list.getFirst().getValue() + list
					.getLast().getValue()) / 2 : list.getFirst().getValue() };

			bool medianInteger = isIntegerNumber(n * 0.5d);
			int medianIndex = (int)(medianInteger ? n * 0.5d : Math.Ceiling(n * 0.5d)) - 1;
			bool lowerQuartileInteger = isIntegerNumber(n * 0.25d);
			int lowerQuartileIndex = (int)(lowerQuartileInteger ? n * 0.25d : Math
					.Ceiling(list.getLength() * 0.25d)) - 1;
			bool upperQuartileInteger = isIntegerNumber(n * 0.75d);
			int upperQuartileIndex = (int)(upperQuartileInteger ? n * 0.75d : Math
					.Ceiling(list.getLength() * 0.75d)) - 1;

			double median = 0;
			double lowerQuartile = 0;
			double upperQuartile = 0;

			DataList.Element current = list.getFirst();
			int i = 0;

			while (current != null) {
				if (i == medianIndex)
					median = medianInteger ? (current.getValue() + current
							.getNext().getValue()) / 2 : current.getValue();
				if (i == lowerQuartileIndex)
					lowerQuartile = lowerQuartileInteger ? (current.getValue() + current
							.getNext().getValue()) / 2 : current.getValue();
				if (i == upperQuartileIndex)
					upperQuartile = upperQuartileInteger ? (current.getValue() + current
							.getNext().getValue()) / 2 : current.getValue();

				current = current.getNext();
				i++;
			}

			return new double[] { lowerQuartile, median, upperQuartile };
		}

		/// <summary>
		/// Returns an array of Strings each containing the data for the corresponding column in array
		/// <c>columns[]</c>. Implement this method in a way, that an array of the same length as the column titles
		/// is produced containing the data at the point of time this method is called by someone else to produce
		/// up-to-date information.
		/// </summary>
		/// <returns>Array containing the data for reporting</returns>
		public override string[] getEntries() {
			entries = base.getEntries();

			if (source is DataListTally) {
				DataListTally dlt = (DataListTally)source;

				var list = dlt.getDataListSorted();

				double[] quartiles = getQuartiles(list);

				bool showTimeSpansInReport = dlt.getShowTimeSpansInReport();

				if (quartiles == null)
					for (int i = 1; i <= 3; i++)
						entries[numColumns - i] = "Insufficient data";
				else if (quartiles.Length == 1) {
					entries[numColumns - 3] = "Insufficient data";
					entries[numColumns - 2] = showTimeSpansInReport ? new TimeSpan(
							quartiles[0]).toString() :
							quartiles[0].ToString();
					entries[numColumns - 1] = "Insufficient data";
				} else
					for (int i = 0; i < 3; i++)
						entries[numColumns - (3 - i)] = showTimeSpansInReport ? new TimeSpan(
								quartiles[i]).toString() :
								(quartiles[i].ToString());
			}

			return entries;
		}
	}
}
