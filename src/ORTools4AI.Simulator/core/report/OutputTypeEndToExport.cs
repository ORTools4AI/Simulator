// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Xiufeng Li
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.report
{
    /**
    * Interface for Outputs that cannot be written line by line, e.g. Excel.
    * 
    * @date 30.03.2011
    */

    public interface OutputTypeEndToExport : OutputType
    {
        /// <summary>Export a new few file for the writting output.</summary>
        /// <param name="pathname">path to write in</param>
        /// <param name="filename">name of the file</param>
        public void export(string pathname, string filename);
    }

}
