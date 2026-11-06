// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// @author edited by Gunnar Kiesel (setting noOfCells at breakpoint)
// @author modified by Chr. M&uuml;ller (TH Wildau) 28.11.2012
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Captures all relevant information about the Histogram.
     * Extended to show unit and description of reported object.
     */

	public class HistogramReporter : Reporter
	{

		// ****** attributes ******

		/**
		 * The column headings of the histogram part of this HistogramReporter.
		 * Entries should contain in the elements in the same order as the
		 * <code>histEntries[]</code>.
		 */
		private String[] _histColumns;

		/**
		 * The data entries of the histogram part of this HistogramReporter. The
		 * first (leftmost) dimension of this array is representing the number of
		 * cells the interval of the histogram is devided into (incl. under- and
		 * overflow). The second dimension of this array is representing each column
		 * entry of the specified cell. So the second dimension entries should
		 * contain the data elements in the same order as defined in the
		 * <code>histColumns[]</code> array.
		 */
		private String[][] _histEntries;

		/**
		 * The number of columns of the histogram part (table) of this
		 * HistogramReporter.
		 */
		private int _histNumColumns;

		/**
		 * The number of cells the interval of the given Histogram is devided into.
		 */
		private int _noOfCells;

		// ****** methods ******

		/// <summary>
		/// Constructor for a new HistogramReporter. Note that although any Reportable is accepted you should make
		/// sure that only subtypes of Histogram are passed to this constructor. Otherwise the number of column
		/// titles and their individual headings will differ from the actual content collected by this reporter.
		/// </summary>
		/// <param name="informationSource">The Histogram to report about.</param>
		public HistogramReporter(Reportable informationSource) : base(informationSource)
		{
			//super(informationSource); // make a Reporter (source =
			// informationSource)

			// the Histogram we report about (source = informationSource)
			var hist = (Histogram)source;

			groupID = 1411; // see Reporter for more information about groupID

			numColumns = 8;
			columns = new String[numColumns];
			columns[0] = "Title";
			columns[1] = "(Re)set";
			columns[2] = "Obs";
			columns[3] = "Mean";
			columns[4] = "Std.Dev";
			columns[5] = "Min";
			columns[6] = "Max";
			columns[7] = "Unit";
			groupHeading = "Histograms";

			entries = new String[numColumns];

			// *** histogram part ***

			_histNumColumns = 7;
			_noOfCells = hist.getCells();

			_histColumns = new String[_histNumColumns];
			_histColumns[0] = "Cell";
			_histColumns[1] = "Cell Range";
			_histColumns[2] = "n";
			_histColumns[3] = "%";
			_histColumns[4] = "Cum. %";
			_histColumns[5] = "|";
			_histColumns[6] = "Graph";

			_histEntries = new string[_noOfCells + 3][];
			for (int i = 0; i < _histEntries.Length; i++)
				_histEntries[i] = new string[_histNumColumns];
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
			if (source is Histogram)
			{
				// the Histogram we report about (source = informationSource)
				var hist = (Histogram)source;
				bool _showTimeSpansInReport = hist.getShowTimeSpansInReport();
				var tl = (Tally)source;
				TallyRunning tlr = null;
				if (tl is TallyRunning)
				{
					tlr = (TallyRunning)tl;
				}

				// Title
				entries[0] = hist.getName();
				// (Re)set
				entries[1] = hist.resetAt().toString();
				// Obs
				entries[2] = hist.getObservations().ToString();

				// Mean
				// no observations made, so Mean can not be calculated
				if (tl.getObservations() == 0)
				{
					entries[3] = "Insufficient data";
				}
				else // return mean value
				{
					entries[3] = this.format(_showTimeSpansInReport, tl.getMean());
					if (tlr != null)
						entries[3] += " (last "
								+ tlr.getSampleSizeN()
								+ " obs: "
								+ this.format(_showTimeSpansInReport, tlr.getMeanLastN())
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
					entries[4] = this.format(_showTimeSpansInReport, tl.getStdDev());
					if (tlr != null)
						entries[4] += " (last "
								+ tlr.getSampleSizeN()
								+ " obs: "
								+ this.format(_showTimeSpansInReport, tlr.getStdDevLastN())
								+ ")";
				}

				// Min
				if (tl.getObservations() == 0)
				{
					entries[5] = "Insufficient data";
				}
				else
				{
					entries[5] = this.format(_showTimeSpansInReport, tl.getMinimum());
					if (tlr != null)
						entries[5] += " (last "
								+ tlr.getSampleSizeN()
								+ " obs: "
								+ this.format(_showTimeSpansInReport, tlr.getMinimumLastN())
								+ ")";
				}

				// Max
				if (tl.getObservations() == 0)
				{
					entries[6] = "Insufficient data";
				}
				else
				{
					entries[6] = this.format(_showTimeSpansInReport, tl.getMaximum());
					if (tlr != null)
						entries[6] += " (last "
								+ tlr.getSampleSizeN()
								+ " obs: "
								+ this.format(_showTimeSpansInReport, tlr.getMaximumLastN())
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

		/// <summary>
		/// Returns an array of Strings each containing the title for the corresponding column of the histogram
		/// part (table).
		/// </summary>
		/// <returns>Array containing column titles of the histogram part (table).</returns>
		public string[] getHistColumnTitles()
		{
			return _histColumns.Clone() as string[];
		}

		/// <summary>
		/// Returns a two-dimensional array of Strings containing the data for the histogram part of this
		/// HistogramReporter. Implement this method in a way, that the data is collected at the point of time this
		/// method is called by someone else to produce up-to-date information.
		/// </summary>
		/// <returns>
		/// string[][] : Array containing the data for reporting about the histogram part of this
		/// HistogramReporter.
		/// </returns>
		public String[][] getHistEntries()
		{
			// the Histogram we report about (source = informationSource)
			var hist = (Histogram)source;
			bool _showTimeSpansInReport = hist.getShowTimeSpansInReport();

			// get hold of the accumulated percentage
			double cumPerc = 0.0;

			// flag if all remaining cells are empty
			bool tailIsEmpty = false;

			for (int j = 0; j < _noOfCells + 2; j++) // loop through all cells
			{
				if (source is Histogram)
				{
					// Cell
					_histEntries[j][0] = j.ToString();

					// Lower Limit
					String a = this.format(_showTimeSpansInReport, hist.getLowerLimit(j));
					String b = this.format(_showTimeSpansInReport, double.MaxValue);
					if (j + 1 < _noOfCells + 2) b = this.format(_showTimeSpansInReport, hist.getLowerLimit(j + 1));
					if (j == 0) _histEntries[j][1] = "(" + a + ", " + b + ")";
					else _histEntries[j][1] = "[" + a + ", " + b + ")";
					//_histEntries[j][1] = Double.toString(hist.getLowerLimit(j));

					// n
					_histEntries[j][2] = hist.getObservationsInCell(j).ToString();

					// % rounded percentage

					// calculate the percentage with 4 digits after the decimal
					// point
					double perc = StatisticObject.round(100.0 * hist
							.getObservationsInCell(j) / hist
							.getObservations());

					cumPerc += perc; // update the accumulated percentage
									 // to display the perc. round it to 2 digits after the decimal
									 // point
					perc = StatisticObject.round(perc);

					_histEntries[j][3] = perc.ToString();

					// Cum. %

					// round the accumulated percentage
					double rdCumPerc = StatisticObject.round(cumPerc);

					_histEntries[j][4] = rdCumPerc.ToString();

					// check if the accumulated percentage has reached 100%
					// AND it is not the last cell
					if (rdCumPerc > 99.98 && j < (_noOfCells + 1))
					{
						// flag if cells so far are empty
						bool yetEmpty = true;
						// check if all the remaining cells are empty
						for (int k = j; k < _noOfCells + 2; k++) // loop thru
																 // remaining cells
						{
							yetEmpty = (yetEmpty && (hist.getObservationsInCell(k) == 0));
						}

						tailIsEmpty = yetEmpty;
					}

					// |
					_histEntries[j][5] = "|";

					// Graph number of asterix's

					// if all remaining cells are empty
					if (tailIsEmpty)
					{
						_histEntries[j][6] = "the remaining cells are all empty";
						_noOfCells = j; // set the no. of cells to the actual value
						break; // the for-loop for all cells
					}

					// calculate the number of asterix's (one asterix per 2 percent)
					int ast = (int)(perc / 2.0);

					String lineOfAsterix = ""; // make an empty String

					// if percentage between zero and 2 percent
					if (hist.getObservationsInCell(j) > 0)
					{
						lineOfAsterix = "*"; // start with one asterix
					}

					for (int k = 0; k < ast; k++) // fill the String with
												  // asterix's
					{
						lineOfAsterix = lineOfAsterix + "*";
					}

					_histEntries[j][6] = lineOfAsterix;
				}
				else
				{
					for (int i = 0; i < _histNumColumns; i++)
					{
						_histEntries[j][i] = "Invalid source!";
					} // end for
				} // end else
			} // end for

			return _histEntries;
		}

		/// <summary>
		/// Returns the number of columns of the histogram part (table) of this HistogramReporter.
		/// </summary>
		/// <returns>The number of columns of the histogram part (table) of this HistogramReporter</returns>
		public int getHistNumColumns()
		{
			return _histNumColumns;
		}

		/// <summary>Returns the number of cells the interval of the given Histogram is devided into.</summary>
		/// <returns>The number of cells the interval of the given Histogram is devided into.</returns>
		public int getNoOfCells()
		{
			return _noOfCells;
		}

		/// <summary>
		/// Returns the number of observations made by the Histogram object. This method call is passed on to the
		/// Histogram object.
		/// </summary>
		/// <returns>The number of observations made by the Histogram object.</returns>
		public long getObservations()
		{

			return source.getObservations(); // that's all
		}

		/*@TODO: Comment */
		public bool isContinuingReporter()
		{
			return true;
		}

		private string format(bool showTimeSpans, double value)
		{
			string output = value.ToString();
			if (showTimeSpans && value < 0.0) output += " (Invalid)";
			else if (showTimeSpans && value >= long.MaxValue) output += " (Invalid)";
			else if (showTimeSpans) output = new TimeSpan(value).toString();
			return output;
		}
	} // end class HistogramReporter
}
