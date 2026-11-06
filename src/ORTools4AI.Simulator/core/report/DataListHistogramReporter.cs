// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Janz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Captures all relevant information about the DataListTally's DataList.
     */

	public class DataListHistogramReporter : Reporter
	{
		/**
		 * Defines the maximal amount of classes found by the algorithm to calculate
		 * the histogram's dimensions.
		 */
		private static readonly byte MAX_CLASSES = 15;

		/**
		 * Counter for the instances of this class
		 */
		private static int _entireReporterCount = 0;

		/**
		 * Counter for the instances of this class which are not already destructed.
		 */
		private static int _activeReporterCount = 0;

		/**
		 * The histogram's dimensions, containing the lowest value, the greatest
		 * value, the interval-size and the count of elements in the histogram.
		 */
		private double[] _dimensions;

		/// <summary>
		/// Constructor for a new DataListReporter. Note that although any Reportable is accepted you should make
		/// sure that only subtypes of DataListTally.DataList are passed to this constructor. Otherwise an
		/// incorrect output will be produced.
		/// </summary>
		/// <param name="informationSource">The DataListTally to report about.</param>
		/// <param name="min">The DataList's minimal value.</param>
		/// <param name="max">The DataList's maximal value.</param>
		public DataListHistogramReporter(Reportable informationSource, double min,
				double max) : base(informationSource)
		{
			//super(informationSource);

			// To produce a single table for each DataList, the reporters need to be
			// counted and groupID is calculated using this counters.
			_activeReporterCount++;
			_entireReporterCount++;

			groupID = 2011 + (_entireReporterCount * 100);
			groupHeading = "Histogram for " + informationSource.getName();

			_dimensions = getIntervalDimensions(min, max);

			if (source is DataList)
			{
				groupHeading = "Histogram for "
						+ ((DataList)informationSource)
								.getTallyName();
				numColumns = ((int)_dimensions[3] <= 1) ? 1
						: (int)_dimensions[3] - 1;
				columns = new String[numColumns];
				entries = new String[numColumns];

				for (int i = 0; i < numColumns; i++)
				{
					columns[i] = "[" + (_dimensions[0] + (i * _dimensions[2]))
							+ " - " + (_dimensions[0] + ((i + 1) * _dimensions[2]))
							+ (i == numColumns - 1 ? "]" : ")");
				}
			}

		}

		/**
		 * Destructor for a DataListReporter
		 */
		protected void finalize()
		{
			_activeReporterCount--;

			if (_activeReporterCount == 0)
				_entireReporterCount = 0;
		}

		/// <summary>
		/// Returns an array of Strings each containing the data for the corresponding column in array
		/// <c>columns[]</c>. Implement this method in a way, that an array of the same length as the column titles
		/// is produced containing the data at the point of time this method is called by someone else to produce
		/// up-to-date information.
		/// </summary>
		/// <returns>Array containing the data for reporting</returns>
		//@Override
		public override string[] getEntries()
		{

			if (source is DataList)
			{
				DataList.Element current = ((DataList)source)
						.getFirst();
				int count = 0;
				int index = 0;

				while (current != null)
				{
					double value = current.getValue();

					if (value < _dimensions[0] + ((index + 1) * _dimensions[2])
							|| index == (numColumns - 1))
					{
						count++;
						current = current.getNext();
					}
					else
					{
						entries[index] = count.ToString();
						count = 0;
						index++;
					}
				}

				entries[index] = count.ToString();

			}
			else
			{
				for (int i = 0; i < numColumns; i++)
					entries[i] = "invalid source!";
			}

			return entries;
		}

		/// <summary>
		/// Calculates the histogram's dimensions, containing the lowest value, the greatest value, the
		/// interval-size and the count of elements in the histogram.
		/// </summary>
		/// <param name="min">The list's minimal value.</param>
		/// <param name="max">The list's maximal value.</param>
		/// <returns>the histogram's dimensions.</returns>
		private static double[] getIntervalDimensions(double min, double max)
		{
			double interval = 1;
			double lower, upper;
			double elements;
			double f = 0;

			int factor = 1;

			if (min > 0)
			{
				while (min < 1)
				{
					min *= 10;
					max *= 10;
					factor *= 10;
				}
			}

			do
			{
				if (max % interval == 0)
					upper = max;
				else
					upper = max + (interval - (max % interval));

				if (min % interval == 0)
					lower = min;
				else
					lower = min - (min % interval);

				elements = (int)((upper - lower) / interval) + 1;

				if (elements <= MAX_CLASSES)
					break;

				if (Math.Log10(interval) % 1 == 0)
				{
					f = 1.25;
					interval *= f;
					continue;
				}

				if (Math.Abs(f - 1.6) < 0.0001)
					f = 1.25;
				else
					f = 1.6;

				interval *= f;
			} while (true);

			while (factor != 1)
			{
				lower /= 10;
				upper /= 10;
				interval /= 10;
				factor /= 10;
			}

			return new double[] { lower, upper, interval, elements };
		}
	}
}