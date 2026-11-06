// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Soenke Claassen based on ideas from Tim Lechler
// @author based on DESMO-C from Thomas Schniewind, 1998
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Captures all relevant information about the Regression analysis.
     */

	public class RegressionReporter : Reporter
	{

		// ****** methods ******

		/// <summary>
		/// Constructor for a new RegressionReporter. Note that although any Reportable is accepted you should make
		/// sure that only subtypes of Regression are passed to this constructor. Otherwise the number of column
		/// titles and their individual headings will differ from the actual content collected by this reporter.
		/// </summary>
		/// <param name="informationSource">The Regression to report about.</param>
		public RegressionReporter(Reportable informationSource) : base(informationSource)
		{
			//super(informationSource); // make a Reporter

			numColumns = 11;
			columns = new string[numColumns];
			columns[0] = "Title";
			columns[1] = "(Re)set";
			columns[2] = "Obs";
			columns[3] = "xMean";
			columns[4] = "yMean";
			columns[5] = "Res.Std.Dev";
			columns[6] = "Reg.Coeff";
			columns[7] = "Intercept";
			columns[8] = "St.Dev.Reg.Coeff";
			columns[9] = "Corr.Coeff";
			columns[10] = "Unit(s)";
			groupHeading = "Regression";
			groupID = 1411; // see Reporter for more information about groupID
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
			if (source is Regression)
			{
				// the Regression we report about. (source = informationSource)
				var rgr = (Regression)source;

				// Title
				entries[0] = rgr.getName() + "<br>based on X: " + rgr.getXName()
						+ "<br>and Y: " + rgr.getYName();
				// (Re)set
				entries[1] = rgr.resetAt().toString();

				// Obs
				entries[2] = rgr.getObservations().ToString();

				// xBarMean
				// no observations made, so xBar mean can not be calculated
				if (rgr.getObservations() == 0)
				{
					entries[3] = "Insufficient data";
				}
				else // return mean value
				{
					entries[3] = rgr.getXMean().ToString();

					// are the x values almost constant?
					if (rgr.xIsConstant())
					{
						entries[3] = entries[3] + " is constant";
					}
				}

				// yBarMean
				// no observations made, so yBar mean can not be calculated
				if (rgr.getObservations() == 0)
				{
					entries[4] = "Insufficient data";
				}
				else // return mean value
				{
					entries[4] = rgr.getYMean().ToString();

					// are the y values almost constant?
					if (rgr.yIsConstant())
					{
						entries[4] = entries[4] + " is constant";
					}
				}

				// Res.Std.Dev
				// not enough observations made,
				// so Res.Std.Dev can not be calculated
				if (rgr.getObservations() <= 5)
				{
					entries[5] = "Insufficient data";
				}
				else // return residual std. dev.
				{
					entries[5] = rgr.residualStdDev().ToString();
				}

				// Reg.Coeff
				// not enough observations made,
				// so Reg.Coeff can not be calculated
				if (rgr.getObservations() <= 5)
				{
					entries[6] = "Insufficient data";
				}
				else // return reg. coeff.
				{
					entries[6] = rgr.regCoeff().ToString();
				}

				// Intercept
				// not enough observations made,
				// so Intercept can not be calculated
				if (rgr.getObservations() <= 5)
				{
					entries[7] = "Insufficient data";
				}
				else // return intercept
				{
					entries[7] = rgr.intercept().ToString();
				}

				// St.Dev.Reg.Coeff
				// not enough observations made,
				// so St.Dev.Reg.Coeff can not be calculated
				if (rgr.getObservations() <= 5)
				{
					entries[8] = "Insufficient data";
				}
				else // return St.Dev.Reg.Coeff
				{
					entries[8] = rgr.stdDevRegCoeff().ToString();
				}

				// Corr.Coeff
				// not enough observations made,
				// so Corr.Coeff can not be calculated
				if (rgr.getObservations() <= 5)
				{
					entries[9] = "Insufficient data";
				}
				else // return correlation coeff.
				{
					entries[9] = rgr.correlationCoeff().ToString();
				}

				//cm 21.11.12  Extension for viewing unit
				entries[10] = rgr.getUnitText();

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
	} // end class RegressionReporter
}
