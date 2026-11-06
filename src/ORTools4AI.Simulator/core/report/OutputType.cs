// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Gunnar Kiesel
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.report
{

    /**
     * Interface for all Outputs (Report, Trace, Error, Debug) in format
     * like HTML, ASCII, XML written line by line.
     */

    public interface OutputType : MessageReceiver
    {

        /// <summary>opens a new file for writting the output</summary>
        /// <param name="pathname">path to write in</param>
        /// <param name="name">name of the file</param>
        public void open(string pathname, string name);

        /**
         * Closes the file 
         *
         */
        public void close();

        /***************************************************************************
         * deliever the file appendix used for the specific format (e.g. html, txt)
         **************************************************************************/
        public string getAppendix();

    }
}