// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Gunnar Kiesel
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.report
{
   
    /**
     * Use this class to create HTML formatted Simulation Error Output.
     */

    public class HTMLErrorOutput: ErrorFileOut, OutputType
    {

        /** Create a new HTMLErrorOutput class * */
        public HTMLErrorOutput():base(1, HTML_TABLE_FORMATTER)
        {
        
        }

        /***************************************************************************
	     * Create a new HTMLErrorOutput class
	     * 
	     * @param simTimeFloatingDigits
	     *            int: The number of floating point digits of the simulation
	     *            time values to be displayed
	     **************************************************************************/
        public HTMLErrorOutput(int simTimeFloatingDigits):base(simTimeFloatingDigits, 
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
