// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{

	/**
     * Controls all reports given by reportable model components used during an
     * experiment.
     */
	public class ReportManager : NamedObject
	{

		/**
		 * Keeps references to all Reporters of this experiment
		 */
		private List<Reporter> _reporters;

		/// <summary>Creates a new reportmanager with the given name.</summary>
		/// <param name="name">the reportmanager's name</param>
		public ReportManager(string name) : base(name + "_ReportManager")
		{

			//super(name + "_ReportManager"); // create the NamedObject

			_reporters = new List<Reporter>(); // init list for reporters

		}

		/// <summary>
		/// Adds a report to the very end of the vector hanlded by this reportmanager. This is needed to place
		/// submodel reporters behind existing model reporters to prevent multiple models to be mixed.
		/// </summary>
		/// <param name="r">Reporter</param>
		public void addLast(Reporter r)
		{

			if (r == null)
				return;
			else
				_reporters.Add(r);

		}

		/// <summary>
		/// De-registers the given reporter from the experiment's reportmanager. The reporter will be removed from
		/// the list of current available reporters and thus will not produce output whenever a report has to be
		/// produced. When de-registering, the order according to group-ID is preserved. If an invalid parameter is
		/// given (i.e. a <c>null</c> reference) this method simply returns
		/// </summary>
		/// <param name="rep">The reporter to be de-registered</param>
		public void deRegister(Reporter rep)
		{

			// check parameter
			if (rep == null)
				return; // wrong parameters ???
			_reporters.Remove(rep); // no check if contained or not
									// necessary

		}

		/// <summary>Returns a list view of all registered reporters in the appropriate order.</summary>
		/// <returns>The list of reporters registered at the reportmanager</returns>
		public List<Reporter> elements()
		{

			return new List<Reporter>(_reporters);

		}

		/// <summary>
		/// Returns a boolean value indicating whether this reportmanager contains reporters to be sent to the
		/// reportoutput or not.
		/// </summary>
		/// <returns>Is <c>true</c> if the reportmanager conatains reporters, <c>false</c> otherwise</returns>
		public bool isEmpty()
		{

			return _reporters.Count == 0;

		}

		/// <summary>
		/// Registers the given reporter at the experiment's reportmanager. All reporters registered will be sent
		/// to the reportout whenever a report has to be produced. When registering, the order according to
		/// group-ID is preserved.
		/// </summary>
		/// <param name="rep">The reporter to be registered</param>
		public void register(Reporter rep)
		{

			// check parameter
			if (rep == null)
				return; // wrong parameter

			if (_reporters.Contains(rep))
				return; // already listed

			// register rep with special care for fist element
			if (_reporters.Count == 0)
				_reporters.Add(rep);
			else
			{

				for (int i = 0; i < _reporters.Count; i++)
				{

					if (Reporter.isLarger(rep, _reporters[i]))
					{
						_reporters[i] = rep;
						return;
					}

				}

				// if come to here, it must be smaller than all other reporters in
				// vector
				_reporters.Add(rep);

			}

		}
	}
}