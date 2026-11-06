// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.exception
{

	/**
     * General exception for the desmoj simulation framework. All specific
     * exceptions extend this class, thus building a hierarchic tree of possible
     * exceptions being thrown by the framework and keeping all simulation related
     * exceptions close together. Most exceptions are thrown when the continuation
     * of the simulation can not be determined. This is especially the case with
     * <code>null</code> references passed to a method expecting valid parameters.
     * Note that these exceptions must not be caught by the user and that the user
     * does not need to re-throw these exceptions, since they all inherit from class
     * <code>java.exception.RuntimeException</code> which has the special feature
     * to be re-thrown automatically by any method.
     * 
     * @see java.lang.RuntimeException
     */
	public class DESMOJException : SystemException
	{
		private static readonly long serialVersionUID = 1L;

		/**
		 * Contains the error message containing a description of the circumstances
		 * leading to this exception being thrown.
		 */
		private ErrorMessage _errMsg;

		/// <summary>
		/// Constructs a standard DESMO-J exception with an error message needed to properly describe what error
		/// occured, where it occured, what reasons could be responsible for it and what could be done to prevent
		/// this error from occuring.
		/// </summary>
		/// <param name="message">Describes the error leading to this exception</param>
		public DESMOJException(ErrorMessage message)
		{

			// construct a simple RuntimeException with the description and probable
			// error reason as text.
			//super("DESMOJException\n"
			//        + "Description: " + message.getDescription() + "\n" 
			//        + "Location:    " + message.getLocation() + "\n" 
			//        + "Reason:      " + message.getReason() + "\n" 
			//        + "Prevention:  " + message.getPrevention());
			_errMsg = message;

		}

		/// <summary>
		/// Returns the error message describing the circumstances leading to this exception being thrown. Use this
		/// method to extract the message and pass it to the Experiment's MessageDistributor to write it to the
		/// ErrorFile.
		/// </summary>
		/// <returns>The errormessage describing the reasons for this Exception</returns>
		public ErrorMessage getErrorMessage()
		{

			return _errMsg;

		}
	}
}