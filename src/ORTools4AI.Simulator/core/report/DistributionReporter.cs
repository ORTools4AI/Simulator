// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * The basic reporter for all distributions in DESMO-J. All reporters producing
     * output about a special distribution have to extend this reporter to add the
     * special values to its report.
     */
	public class DistributionReporter : Reporter
	{
		/// <summary>DistributionReporter - Konstruktorkommentar.</summary>
		/// <param name="informationSource">Reportable</param>
		public DistributionReporter(
				Reportable informationSource) : base(informationSource)
		{

			//super(informationSource);

			numColumns = 8;
			columns = new string[numColumns];
			entries = new string[numColumns];
			groupID = 100; // low groupID, so always last in order

			columns[0] = "Title";
			columns[1] = "(Re)set";
			columns[2] = "Obs";
			columns[3] = "Type";
			columns[4] = "Parameter 1";
			columns[5] = "Parameter 2";
			columns[6] = "Parameter 3";
			columns[7] = "Seed";
			groupHeading = "Distributions";

		}

		/// <summary>Returns the array of strings containing all information about the distribution.</summary>
		/// <returns>The array of Strings containing all information about the distribution</returns>
		public override string[] getEntries()
		{

			if (source.GetType() == typeof(Distribution))
			{
				// Title
				entries[0] = source.getName();
				// (Re)set
				entries[1] = source.resetAt().toString();
				// Obs
				entries[2] = source.getObservations().ToString();
				// Type
				entries[3] = "unnamed distribution";
				// param1
				entries[4] = " ";
				// param2
				entries[5] = " ";
				// param3
				entries[6] = " ";
				// seed
				entries[7] = ((Distribution)source).getInitialSeed().ToString();

			}
			else
			{
				for (int i = 0; i < numColumns; i++)
				{
					entries[i] = "Invalid source!";
				}
			}

			return entries;

		}
	}
}