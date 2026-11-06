// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Malte Unkrig
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.exception
{


	/**
     * InterruptExceptions can be caught when processes are interrupted. They
     * contain an {@link InterruptCode} which is used to provide the interrupted
     * processes with information about the reason for their interruption.
     * 
     * @see InterruptCode InterruptCode
     */
	public class InterruptException : SystemException
	{
		private static readonly long serialVersionUID = 2298081652371353905L;

		/**
		 * The {@link InterruptCode} transporting information about the reason for
		 * the interrupt triggered by this InterruptException.
		 */
		private readonly InterruptCode interruptCode;

		/// <summary>Constructs an <c>InterruptException</c> containing the given <c>InterruptCode</c>.</summary>
		/// <param name="interruptCode">
		/// The InterruptCode which is to be transported inside of this InterruptException.
		/// </param>
		public InterruptException(InterruptCode interruptCode)
		{
			this.interruptCode = interruptCode;
		}

		/**
		 * Equals implementations which delegates to
		 * {@link InterruptCode#equals(Object)}
		 */
		//@Override
		//public bool equals(Object obj)
		//{
		//	return interruptCode.equals(obj);
		//}

		/**
		 * Returns the {@link InterruptCode} associated with this
		 * InterruptException. The InterruptCode contains information on the reason
		 * for a process's interruption.
		 * 
		 * @return the InterruptCode
		 */
		public InterruptCode getInterruptCode()
		{
			return interruptCode;
		}

		/**
		 * HashCode implementations which delegates to
		 * {@link InterruptCode#hashCode()}
		 */
		//@Override
		//public int hashCode()
		//{
		//	return interruptCode.hashCode();
		//}

	}
}
