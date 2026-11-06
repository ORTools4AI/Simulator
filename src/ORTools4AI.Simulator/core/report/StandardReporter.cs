// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{

	/**
	 * The standard reporter for any reportable object. Its report contains the
	 * reportable's name, the simulation time of the last reset and the number of
	 * observations made by the reportable. These represent the basic data each
	 * reportable can supply. To get more specific information, build a custom
	 * reporter for that reportable.
	 */
	public class StandardReporter : Reporter
	{
		/// <summary>
		/// Constructs a standrad reporter to report about the given reportable. Reports produced by this standard
		/// reporter are always listed last on a report output.
		/// </summary>
		/// <param name="informationSource">The reportable to produce a report about</param>
		public StandardReporter(Reportable informationSource) : base(informationSource)
		{

			//super(informationSource);

			groupID = -2147483648; // lowest possible groupID to display last in
								   // report
			groupHeading = "StandardReporter";

			numColumns = 3;

			columns = new string[numColumns];
			entries = new string[numColumns];

			columns[0] = "Title";
			columns[1] = "(Re)set";
			columns[2] = "Obs";

		}

		/// <summary>
		/// Returns the array of strings containing the basic information any reportable can offer.
		/// </summary>
		/// <returns>
		/// The array of Strings containing all information about the reportable information source
		/// </returns>
		public override string[] getEntries()
		{

			// if (source instanceof desmoj.core.simulator.Reportable) {  // Replaced by test for non-null since instanceof always yields true (JG, 11.03.09)
			if (source != null)
			{

				// Title
				entries[0] = source.getName();
				// (Re)set
				entries[1] = source.resetAt().toString();
				// Observations
				entries[2] = source.getObservations().ToString();

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