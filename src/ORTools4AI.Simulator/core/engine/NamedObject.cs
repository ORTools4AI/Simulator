// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{


	/**
     * This superclass for every DESMOJ class provides the ability to carry a name
     * for identification. Gives objects the ability to carry a name to be shown in
     * reports. Each class in the desmoj-Framework is supposed to be able to carry a
     * name for identification and offer methods to reset and tell its name. Since
     * all other desmoj classes inherit from this class, it is ensured that any
     * object, even the user's special objects, have a name to be used in a report.
     * Offering no default constructor enforces users to give each new object in
     * their models a name. The class is set abstract to prevent clients from
     * accidentally creating otherwise functionless (but named) objects.
     */
	public abstract class NamedObject
	{

		/*
		 * The name of the namedObject as a String.
		 */
		private string _myName;

		/// <summary>
		/// Constructs a named object with the given initial name. Note, that this allows clients to produce more
		/// than one object with one name, thus being unable to distinguish those objects in reports!
		/// </summary>
		/// <param name="name">The given initial name for the named object</param>
		public NamedObject(string name)
		{

			if (name == null)
				_myName = "unnamed";
			else
				_myName = name;

		}

		/// <summary>
		/// Returns the name of the named object. This is the same name displayed in reports and trace files when
		/// this named object is shown in those reports or trace files.
		/// </summary>
		/// <returns>The name of the named object</returns>
		public string getName()
		{

			return _myName;

		}

		/// <summary>
		/// Returns the quoted name of the named object. This is the name displayed in reports and trace files when
		/// this named object is shown.
		/// </summary>
		/// <returns>The quoted name of the named object</returns>
		public string getQuotedName()
		{

			return "'" + _myName + "'";

		}

		/// <summary>
		/// Changes the name of the named object. This might be necessary for automatically created named object of
		/// a user defined model, but should not be used for elements of the framework or basic objects of the user
		/// model. Changing names of objects while runtime will confuse any trace output and generally make traces
		/// more difficult if not impossible to follow.
		/// </summary>
		/// <param name="newName">The new name for the named object</param>
		protected void rename(string newName)
		{

			_myName = newName;

		}

		/// <summary>
		/// Overrides the object's toString method to return the named object's name when given as parameter to a
		/// method that expects a string to be passed.
		/// </summary>
		/// <returns>The named object's name</returns>
		public string toString()
		{

			return _myName;

		}
	}

}