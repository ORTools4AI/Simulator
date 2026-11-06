// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Gunnar Kiesel
// Modified for ORTools4AI.Simulator.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORTools4AI.Simulator.core.report
{
  
    /**
     * Use this class to create HTML formatted Simulation Debug Output.
     */

    public class HTMLDebugOutput: DebugFileOut,OutputType
    {

    /** Create a new HTMLDebugOutput class * */
    public HTMLDebugOutput():base(1, HTML_TABLE_FORMATTER)
    {
    }

    /***************************************************************************
	 * Create a new HTMLDebugOutput class
	 * 
	 * @param simTimeFloatingDigits
	 *            int: The number of floating point digits of the simulation
	 *            time values to be displayed
	 **************************************************************************/
    public HTMLDebugOutput(int simTimeFloatingDigits):base(simTimeFloatingDigits, 
        HTML_TABLE_FORMATTER)
    {
      
    }

    /***************************************************************************
	 * set the time of the current output
	 * 
	 * @param timeFloats
	 *            int: time of the curent output
	 **************************************************************************/
    public void setTimeFloats(int timeFloats)
    {
        this.formatter.setTimePrecision(timeFloats);
    }

    /** returns the file appendix for this output type * */
    public string getAppendix()
    {
        return ".html";
    }

}
}
