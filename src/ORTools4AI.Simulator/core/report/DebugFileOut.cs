// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler, modified by Nicolas Knaak
// Modified for ORTools4AI.Simulator.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ORTools4AI.Simulator.core.report
{
    /**
     * DebugOut is used to create a file to write the DebugNotes to. It receives a
     * DebugNotes and divides its individual text information up to be displayed in
     * HTML format on disc in the user's local directory. The messages are displayed
     * in a tabular design with columns for:
     * <ul>
     * <li>The point of simulation time the DebugNote was created</li>
     * <li>The ModelComponent's name responsible for sending the DebugNote</li>
     * <li>The textual description of the ModelComponent's current state</li>
     * </ul>
     * Note that if the framework is used as an Applet in webpages there is no
     * access to the disc and an alternative output on screen or into a graphics
     * window must be registered at the Experiment's MessageManager for this type of
     * messages. Errors affecting the java runtime are always displayed on the
     * system's standard output PrintStream.
     */
    public class DebugFileOut: TableOutput , MessageReceiver
    {

    /**
	 * Buffers the last message received for formatting the HTML table to e
	 * printed without repeating redundant information.
	 */
    private DebugNote _lastNote;

    /// <summary>
    /// Creates a DebugOut to print DebugNotes into a HTML page. By opening the file, the necessary HTML tags
    /// to define a webpage are already inserted into the file. The parameter given should reflect the
    /// experiment that produces this file.
    /// </summary>
    /// <param name="simTimeFloatingDigits">
    /// The number of floating point digits of the simulation time values to be displayed
    /// </param>
    public DebugFileOut(int simTimeFloatingDigits, string format):base(simTimeFloatingDigits, format)
    {

        _lastNote = null;

    }

    /**
	 * Closes this DebugOut. Writes the final necessary HTML-tags to close a
	 * table row, the table and finish the HTML-page. Flushes and closes the
	 * FileOutputStream thereafter.
	 */
    public void close()
    {

       base.close();
        _lastNote = null;

    }

    /// <summary>
    /// Opens a new file with the given filename for writing DebugNotes into a HTML table. The output path is
    /// set to the working directory. If no filename is given, the default filename "DESMOJ_debugfile.html"
    /// is used.
    /// </summary>
    /// <param name="name">The name of the file to be created</param>
    public void open(string name)
    {

        this.open(null, name);

    }

    /// <summary>
    /// Opens a new file with the given file- and pathname for writing DebugNotes into a HTML table. If no
    /// filename is given, the default filename "DESMOJ_debugfile.html" is used. If no pathname is given the
    /// current working directory (stored as property "user.dir") is used.
    /// </summary>
    /// <param name="pathname">The file's output path</param>
    /// <param name="name">The name of the file to be created</param>
    public void open(string pathname, string name)
    {

        base.open(createFileName(pathname, name, "debug"));

        // write tags to open the table used to write the TraceNotes to
        // with the proper heading
        formatter.openTable(name + " - debug notes");
        formatter.openRow();
        formatter.writeHeadingCell("model");
        formatter.writeHeadingCell("time");
        formatter.writeHeadingCell("origin");
        formatter.writeHeadingCell("debug information");
        formatter.closeRow();

    }

    /// <summary>
    /// Receives a debugnote and writes its contents formatted to a HTML table into a file in the user's
    /// default directory. Note that although any type of message may be given to this method, only
    /// debugnotes will be processed. If other types of messages are given to this method, it will simply
    /// return doing nothing.
    /// </summary>
    /// <param name="m">The DebugNote to be written to file in HTML-table format</param>
    public void receive(Message m)
    {

        // check parameters
        if (m == null)
            return; // again nulls
        if (!(m is DebugNote))

            return; // got wrong message
        DebugNote tmp = (DebugNote)m; // cast and buffer for easier access

        formatter.openRow();

        if (_lastNote == null)
        {

            formatter.writeCell(tmp.getModelName(), 1);
            formatter.writeCell(formatter.writeTime(tmp.getTime()), 1);
            formatter.writeCell(tmp.getOrigin(), 1);
            formatter.writeCell(tmp.getDescription(), 1);
        }
        else
        {

            // now write the modelname to debug
            if (tmp.getModelName().Equals(_lastNote.getModelName()))
                formatter.writeCell(" ", 1);
            else
                formatter.writeCell(formatter.writeTime(tmp.getModelName()), 1);

            // now write the time to debug
            if (tmp.getTime().Equals(_lastNote.getTime()))
                formatter.writeCell(" ", 1);
            else
                formatter.writeCell(formatter.writeTime(tmp.getTime()), 1);

            // now write the Origin to debug
            if (tmp.getOrigin().Equals(_lastNote.getOrigin()))
                formatter.writeCell(" ", 1);
            else
                formatter.writeCell(tmp.getOrigin(), 1);

            // always write the description to debug
            formatter.writeCell(tmp.getDescription(), 1);
        }

        // remember the last note for future formatting
        _lastNote = tmp; // set new note to be last

        formatter.closeRow();

    }

    /// <summary>Reporters are not handled by this class so this method simply returns.</summary>
    /// <param name="r">The reporter to be processed</param>
    public void receive(Reporter r)
    {

        return; // No reporters are handled here

    }
}
}
