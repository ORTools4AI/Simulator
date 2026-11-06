// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
	/**
     * Reports all information about a ContDistUniform distribution.
     */
	public class ContDistUniformReporter : DistributionReporter
	{
		/// <summary>Creates a new ContDistUniformReporter.</summary>
		/// <param name="informationSource">The ContDistUniform distribution to report about</param>
		public ContDistUniformReporter(
				Reportable informationSource) : base(informationSource)
		{

			//super(informationSource);

			groupID = 155;

		}

		/// <summary>
		/// Returns the array of strings containing all information about the ContDistUniform distribution.
		/// </summary>
		/// <returns>
		/// The array of Strings containing all information about the ContDistUniform distribution
		/// </returns>
		public string[] getEntries()
		{

			if (source is ContDistUniform)
			{

				// use casted ide as a shortcut for source
				var rdn = (ContDistUniform)source;
				// Title
				entries[0] = rdn.getName();
				// (Re)set
				entries[1] = rdn.resetAt().toString();
				// Obs
				entries[2] = rdn.getObservations().ToString();
				// Type
				entries[3] = "Cont Uniform";
				// param1
				entries[4] = rdn.getLower().ToString();
				// param2
				entries[5] = rdn.getUpper().ToString();
				// param3
				entries[6] = " ";
				// seed
				entries[7] = rdn.getInitialSeed().ToString();

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