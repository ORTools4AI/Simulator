// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * Keeps track of the names given for Schedulables within an experiment. To help
     * identify individual entities, events and all other types of Schedulables,
     * this class registers all names given to these objects. If an object is
     * created with the same name as some other object before, a number is added to
     * the object's name as a suffix. The number represents the amount of objects
     * already created with that name.
     */
	public class NameCatalog
	{

		/**
		 * Stores the names of all Schedulables within an experiment. The names are
		 * used as the indexing value via the string's method to produce a hashing
		 * key value. The numbers for each individual name are stored as objects of
		 * their wrapper class.
		 */
		private Dictionary<string, int> _catalog;

		/**
		 * Constructs a single namecatalog for registering names of all Schedulables
		 * within an experiment.
		 */
		public NameCatalog()
		{

			//super();
			_catalog = new Dictionary<string, int>();

		}

		/// <summary>
		/// Registers the given name in the namecatalog and returns the correct name with the added number if
		/// necessary. If a <c>null</c> reference is given as parameter, the returned name will be set to "unnamed"
		/// with the number of unnamed objects so far added as a suffix.
		/// </summary>
		/// <param name="name">The name for a new Schedulable</param>
		/// <returns>The registered name including the number if necessary</returns>
		public string registeredName(string name)
		{

			if (name == null)
				name = "unnamed";



			if (_catalog.TryGetValue(name, out int i))
			{

				i++;
				_catalog[name]= i;
				return name + "#" + i;
			}
			else
			{
				_catalog.Add(name, 1);
				return name + "#1";
			}

		}

		/// <summary>Returns the first part of name without added number as suffix.</summary>
		/// <param name="name">The name obtain the first part from.</param>
		/// <returns>The first part of the name without number suffix</returns>
		public string getNameWithoutSuffix(string name)
		{
			if (name == null || name.Equals("unnamed") || name.IndexOf("#") == -1)
				return name; // no suffix to remove
			else
				return name.Substring(0, name.LastIndexOf("#"));
		}
	}
}