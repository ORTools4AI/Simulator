// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

	/**
     * Represents a code to be passed to interrupted SimProcesses to give
     * information about the reason for the interruption. Each new interrupt code
     * instantiated will carry an individual internal integer codenumber to help
     * identify different interrupt code objects. These can be checked using the
     * static <code>equals(InterruptCode a, InterruptCode b)</code> method. It
     * might come handy to clone an interrupt code object to make it known at
     * different objects in a model. To produce a clone, create a new interrupt code
     * using the alternative constructor method giving the interrupt code to be
     * cloned as a parameter. That constructor will return a clone of the given
     * interrupt code.
     */
	public class InterruptCode : NamedObject
	{

		/**
		 * Static counter to provide each new interrupt code with a unique internal
		 * serial number.
		 */
		private static int irqCounter = 0;

		/**
		 * The internal unique number to identify different interrupt codes.
		 */
		private int _irqCode;

		/// <summary>
		/// Constructs a new interrupt code to be an identical clone of the given interrupt code object. The
		/// constructor provides the new interrupt code object with same name and internal codenumber as the
		/// interrupt code object given as parameter.
		/// </summary>
		/// <param name="cloneMe">The interrupt code object to produce a clone of</param>
		public InterruptCode(InterruptCode cloneMe):base(cloneMe.getName())
		{

			//super(cloneMe.getName());
			_irqCode = cloneMe.getCodeNumber();

		}

		/// <summary>
		/// Produces a new interrupt code with a unique internal serial number and the given name.
		/// </summary>
		/// <param name="name">The interrupt code's name</param>
		public InterruptCode(string name):base(name)
		{

			//super(name);

			_irqCode = ++irqCounter; // increment counter and set code

		}

		/// <summary>
		/// Returns <c>true</c> if the two given interrupt codes have the same internal code, <c>false</c>
		/// otherwise.
		/// </summary>
		/// <param name="a">First comparand</param>
		/// <param name="b">Second comparand</param>
		/// <returns>
		/// Is <c>true</c> if the two given interrupt codes have the same internal code, <c>false</c> otherwise
		/// </returns>
		public static bool equals(InterruptCode a, InterruptCode b)
		{

			if ((a != null) && (b != null))
				return (a.getCodeNumber() == b.getCodeNumber());
			else
				return false;

		}

		/// <summary>Returns the internal unique number to identify different interrupt codes.</summary>
		/// <returns>The internal unique number of the interrupt code</returns>
		public int getCodeNumber()
		{
			return _irqCode;
		}
	}
}