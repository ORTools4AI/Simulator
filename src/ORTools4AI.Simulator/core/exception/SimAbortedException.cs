// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.exception
{


    /**
     * Exception is thrown to indicate that the simulation has been forced to stop
     * becaus of an error that occured. The error forcing a disruption of the
     * simulation is described in the error message attached to this exception.
     * Users must not make use of it nor is this exception supposed to be caught by
     * users. Its use inside the DESMOJ framework is transparent because it is
     * derived from <code>RuntimeException</code> which isthe root for a tree of
     * exceptions that are automatically rethrown by any Java method thus not
     * needing to be rethrown by the user, too.
     */
    public class SimAbortedException : DESMOJException
    {
        //   private static final long serialVersionUID = 1L;

        //   /**
        // * Creates a new SimAbortedException declaring the reason for the aborted
        // * simulation run in the contents of the error message given as a parameter.
        // * 
        // * @param message
        // *            desmoj.report.ErrorMessage : The reason for the aborted
        // *            simulation
        // */
        //public SimAbortedException(desmoj.core.report.ErrorMessage message) {

        //	super(message);

        //}
        public SimAbortedException(ErrorMessage message) : base(message)
        {
        }
    }
}