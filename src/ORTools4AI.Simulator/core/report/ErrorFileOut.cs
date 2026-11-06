// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler, modified by Nicolas Knaak
// Modified for ORTools4AI.Simulator.


namespace ORTools4AI.Simulator.core.report
{
   
    /**
     * ErrorFileOut is used to create a file to write the ErrorMessages to. It
     * receives an ErrorMessage and divides its individual text information up to be
     * displayed in HTML format on disc in the user's local directory. The messages
     * are displayed in a tabular design with columns for:
     * <ul>
     * <li>The point of simulation time the ErrorMessage was created.</li>
     * <li>A textual description of what went wrong.</li>
     * <li>The location that issued the ErrorMessage.</li>
     * <li>The possible reason why that error condition occurred.</li>
     * <li>A hint for the user on how to prevent this error to happen again.</li>
     * </ul>
     * Note that if the framework is used as an Applet in webpages there is no
     * access to the disc and an alternative output on screen or into a graphics
     * window must be registered at the Experiment's MessageManager for this type of
     * messages. Errors affecting the java runtime are always displayed on the
     * system's standard output PrintStream.
     */
    public class ErrorFileOut : TableOutput , MessageReceiver
    {

    /**
	 * Buffers the last message received for formatting the HTML table to e
	 * printed without repeating redundant information.
	 */
        //field was never used
        //private ErrorMessage _lastNote;

        /**
         * Creates an ErrorOut to print ErrorMessages into a HTML page. By opening
         * the file, the necessary HTML tags to define a webpage are already
         * inserted into the file. The name given should reflect the experiment that
         * produces this file.
         * 
         * @param simTimeFloatingDigits
         *            int : The number of floating point digits of the simulation
         *            time values to be displayed
         */
    public ErrorFileOut(int simTimeFloatingDigits, string format):base(simTimeFloatingDigits, format)
    {

       

    }

    /**
	 * Closes this TraceOut. Writes the final necessary HTML-tags to close a
	 * table row, the table and finish the HTML-page. Flushes and closes the
	 * FileOutputStream thereafter.
	 */
    public void close()
    {

        base.close();
        //_lastNote = null;

    }

    /// <summary>
    /// Opens a new file with the given filename for writing ErrorMessages into a HTML table. The output path
    /// is set to the working directory. If no String is given, the default filename "DESMOJ_errorfile.html"
    /// is used.
    /// </summary>
    /// <param name="name">The name of the file to be created</param>
    public void open(string name)
    {

        this.open(null, name);

    }

    /// <summary>
    /// Opens a new file with the given file- and pathname for writing ErrorMessages into a HTML table. If no
    /// String is given, the default filename "DESMOJ_errorfile.html" is used. If no pathname is given the
    /// current working directory (stored as property "user.dir") is used.
    /// </summary>
    /// <param name="pathname">The output path the file is written to</param>
    /// <param name="name">The name of the file to be created</param>
    public void open(string pathname, string name)
    {

        base.open(createFileName(pathname, name, "error"));

        // write tags to open the table used to write the TraceNotes to
        // with the proper heading
        formatter.openTable(name + " - errors & warnings");
        formatter.openRow();
        formatter.writeHeadingCell("model");
        formatter.writeHeadingCell("time");
        formatter.writeHeadingCell("error");
        formatter.writeHeadingCell("content");
        formatter.closeRow();

    }

    /// <summary>
    /// Receives a TraceNote and writes its contents formatted to a HTML table into a file in the user's
    /// default directory. Note that although any type of message may be given to this method, only
    /// TraceNotes will be processed. If other types of messages are given to this method, it will simply
    /// return doing nothing.
    /// </summary>
    /// <param name="m">The TraceNote to be written to file in HTML-table format</param>
    public void receive(Message m)
    {

        // check parameters
        if (m == null)
            return; // again nulls
        if (!(m is ErrorMessage))

            return; // got wrong message
        ErrorMessage tmp = (ErrorMessage)m;
        // cast and buffer for easier access

        formatter.openRow();
        formatter.writeCell(tmp.getModelName(), 1);
        formatter.writeCell(formatter.writeTime(tmp.getTime()), 1);
        formatter.writeCell("description", 1);
        formatter.writeCell(tmp.getDescription(), 1);
        formatter.closeRow();
        formatter.openRow();
        formatter.writeCell(" ", 1);
        formatter.writeCell(" ", 1);
        formatter.writeCell("location", 1);
        formatter.writeCell(tmp.getLocation(), 1);
        formatter.closeRow();
        formatter.openRow();
        formatter.writeCell(" ", 1);
        formatter.writeCell(" ", 1);
        formatter.writeCell("reason", 1);
        formatter.writeCell(tmp.getReason(), 1);
        formatter.closeRow();
        formatter.openRow();
        formatter.writeCell(" ", 1);
        formatter.writeCell(" ", 1);
        formatter.writeCell("prevention", 1);
        formatter.writeCell(tmp.getPrevention(), 1);
        formatter.closeRow();
        try
        {
            file.Flush();
        }
        catch (IOException ioEx)
        {
           Console.WriteLine("IOException thrown : " + ioEx);
                Console.WriteLine("description: Can't flush " + fileName);
                Console.WriteLine("origin     : Experiment auxiliaries");
                Console.WriteLine("location   : ErrorFileOut.receive(Message)");
                Console.WriteLine("hint       : Check access to the file and"
                    + " that it is not in use by some other application.");
                Console.WriteLine("The System will not be shut down. But it can not be "
                            + "written to the file "
                            + fileName
                            + ".  The file may "
                            + "not contain all the important data!");
            /*
			 * the system will not be shut down, because this may disrupt other
			 * programs like CoSim from Ralf Bachmann (Universtiy of Hamburg,
			 * germany).
			 */
            // System.exit(-1); // radical but no time for fileselectors now
        }
    }

    /// <summary>Reporters are not handled by this class so this method simply returns.</summary>
    /// <param name="r">The Reporter to be processed</param>
    public void receive(Reporter r)
    {
        return; // No reporters are handled here

    }
}
}
