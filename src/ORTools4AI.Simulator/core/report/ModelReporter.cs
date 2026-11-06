// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * The Reporter for a model. Carries references to all Reportables and submodels
     * of the Model it reports about. This enables it to produce a vector containing
     * the reportable's reporters ordered by group-ID. It also takes care to
     * retrieve the submodel's reporters recursively.
     */
	public class ModelReporter : Reporter
	{

		/**
		 * The list containing the reportable's reporters ordered by group-ID.
		 */
		private List<Reporter> _reporters;

		/// <summary>
		/// Creates a reporter for the given model. This special reporter retrieves all other reporters associated
		/// to the reportable objects registered at the model. These are collected in a vector, ordered by group-ID
		/// to be sent to the report output when a report is required.
		/// </summary>
		/// <param name="model">The model to report about.</param>
		public ModelReporter(Model model) : base(model)
		{

			//super(model);
			 List<Reportable> reportables = model.getReportables();

			_reporters = new List<Reporter>();
			groupID = 2147483647; // highest groupID possible, so always first in
								  // order
			groupHeading = "Model " + source.getName();
			numColumns = 1;
			columns = new String[numColumns];
			columns[0] = "Description";
			entries = new String[numColumns];

			if (reportables == null)
				return; // nothing to insert
			if (reportables.Count == 0)
				return; // still nothing to insert

			// insert all reportable's reporters in order of group-id
			// buffer frequent used variables for faster access
			Reporter repoBuff; // buffer variable for Reporter

			foreach (Reportable r in reportables)
			{

				if (r != null)
				{

					//repoBuff = r.createReporter();
					repoBuff = r.getReporter();

					if (repoBuff != null)
					{

						// insert according to group-ID
						if (_reporters.Count==0)
						{
							_reporters.Add(repoBuff);
						}
						else
						{ // now sort until pos. found

							for (int k = 0; k < _reporters.Count; k++)
							{

								if (Reporter.isLarger(_reporters[k], repoBuff))
								{
									_reporters.Add(repoBuff);
									break;
								} // endif pos. found

							} // endfor search position

						} // endelse special care for first reporter

					} // endif repoBuff == null

				} // endif ableBuff == null

			} // endfor

		}

		/// <summary>The ModelReporter returns the description of the model.</summary>
		/// <returns>Array containing the data for reporting</returns>
		public override string[] getEntries()
		{

			Model m = (Model)source;
			entries[0] = m.description();

			return entries;

		}

		/// <summary>Returns a list of view of all reportable's reporters ordered by group-ID.</summary>
		/// <returns>The vector containing the reporters associated to the modelreporter</returns>
		public List<Reporter> getReporters()
		{

			return new List<Reporter>(_reporters);

		}
	}
}