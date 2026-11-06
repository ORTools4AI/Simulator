// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.exception
{


    /**
     * Exception is thrown to indicate that the simulation has finished properly. It
     * is needed to unlock all threads still alive after the simulation has reached
     * its criterium to stop. Users must not make use of it nor is this exception
     * supposed to be caught by users. Its use inside the DESMOJ framework is
     * transparent because it is derived from <code>RuntimeException</code> which
     * isthe root for a tree of exceptions that are automatically rethrown by any
     * Java method thus not needing to be rethrown by the user, too.
     */
    public class SimFinishedException : DESMOJException
    {
        //   private static final long serialVersionUID = 1L;

        //   /**
        // * Constructs a SimFinishedException inserting the given position String and
        // * the given simulation time in the ErrorMessage documenting this exception.
        // * 
        // * @param position
        // *            java.lang.String : contains the position, i.e. Object and
        // *            method that throws this exception
        // * @param time
        // *            TimeInstant : The point in simulation time that this exception is
        // *            thrown
        // */
        public SimFinishedException(Model origin, String position, TimeInstant time):base(new ErrorMessage(origin, "SimFinishedException thrown!",
                    "Position " + position, "Simulation has come to an end.",
                    "No Error.", time))
        {

            //super(new ErrorMessage(origin, "SimFinishedException thrown!",
            //        "Position " + position, "Simulation has come to an end.",
            //        "No Error.", time));

        }
  
    }
}