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
     * ReportFileOut is used to create a file to let the reporters write their
     * reports to. It receives a reporter and divides its individual text
     * information up to be displayed in HTML format on disc in the user's local
     * directory. The messages are displayed in a tabular design with columns for
     * each item displayed by the reporter. For each Reporter giving a new group-ID,
     * a new table is opened. This leads to a number of distinct tables displayed in
     * one HTML-page with all reporters grouped by their group-id in one table. Note
     * that if the framework is used as an Applet in webpages there is no access to
     * the disc and an alternative output on screen or into a graphics window must
     * be registered at the Experiment's MessageManager for this type of messages.
     * Errors affecting the java runtime are always displayed on the system's
     * standard output PrintStream.
     */
    public class ReportFileOut : TableOutput , MessageReceiver
    {

    /**
	 * Buffers the last message received for formatting the HTML table to be
	 * printed without repeating redundant information.
	 */
    protected Reporter lastReporter;

    /**
	 * Stores the number of reports written to file. The number is incremented
	 * each time this ReportFileOut is opened and closed again, completing to
	 * write a report file. To keep track of the numerous reports that can be
	 * produced by calling the Model's method <code>report()</code> this
	 * number is added to the report filename automatically.
	 */
    private int _reportNumber;

    /// <summary>
    /// Creates a file to print reports into a HTML page. By opening the file, the necessary HTML tags to
    /// define a webpage are already inserted into the file. The parameter given should reflect the
    /// experiment that produces this file.
    /// </summary>
    /// <param name="timeFloats">
    /// The number of floating point digits of the simulation time values to be displayed
    /// </param>
    public ReportFileOut(int timeFloats, string formatter):base(timeFloats, formatter)
    {

       

        lastReporter = null;
        _reportNumber = 0; // none printed so far

    }

    /**
	 * Closes this traceout. Writes the final necessary HTML-tags to close a
	 * table row, the table and finish the HTML-page. Flushes and closes the
	 * FileOutputStream thereafter.
	 */
    public void close()
    {

        formatter.closeTable();
        lastReporter = null;
        base.close();

    }

    /// <summary>
    /// Opens a new file with the given filename for writing reports into a HTML table. The output path is
    /// set to the working directory. If no String is given, the default filename "DESMOJ_reportfile.html" is
    /// used.
    /// </summary>
    /// <param name="name">The name of the file to be created</param>
    public void open(string name)
    {
        this.open(null, name);

    }

    /// <summary>
    /// Opens a new file with the given file- and pathname for writing reports into a HTML table. If no
    /// String is given, the default filename "DESMOJ_reportfile.html" is used. If no pathname is given the
    /// current working directory (stored as property "user.dir") is used.
    /// </summary>
    /// <param name="pathname">The file's output path</param>
    /// <param name="name">The name of the file to be created</param>
    public void open(string pathname, string name)
    {

        _reportNumber++; // increment counter

        base.open(createFileName(pathname, name, "report"));

        // write the heading for this report file
        formatter.writeHeading(2, name + " Report");

    }

    /// <summary>Messages are not handled by this reportout, so this method simply returns.</summary>
    /// <param name="m">The message passed to this reportout</param>
    public void receive(Message m)
    {

        return;

    }

    /// <summary>
    /// Receives a reporter and writes its contents formatted to a HTML table into a file in the user's
    /// default directory. In case of a ModelReporter it writes the model's information into the file. After
    /// writing the model data, all reportables registered with the ModelReporter are asked for a reporter
    /// and those are also written to the report file. After processing the Reportables, the registered
    /// subModels are asked to produce their reporters which in turn are recursively sent to this method
    /// until no other submodels are available.
    /// </summary>
    /// <param name="r">The Reporter to be processed</param>
    public void receive(Reporter r)
    {

        // check parameters
        if (r == null)
            return; // invalid parameter

        // copy values in buffer variables for faster access
        string[] titleBuf = r.getColumnTitles();
        string[] entryBuf = r.getEntries();

        // first Reporter to be printed here always writes heading and starts
        // table
        if (lastReporter == null)
        {

            // open a table with the heading needed for this type of reporter
            formatter.openTable(r.getHeading());
            formatter.openRow();

            for (int i = 0; i < r.numColumns; i++)
            {

                formatter.writeHeadingCell(titleBuf[i]);
            }

            formatter.closeRow();
            lastReporter = r;
            // trick the next if-clause to be avoided first time

        }

        // if new Reporter group type is received, close last table and open new
        // one
        if (Reporter.isOtherGroup(r, lastReporter))
        {

            formatter.closeTable();
            formatter.writeHorizontalRuler();
            formatter.openTable(r.getHeading());
            formatter.openRow();

            for (int i = 0; i < r.numColumns; i++)
            {

                formatter.writeHeadingCell(titleBuf[i]);
            }

            formatter.closeRow();

        }

        // always write the reporter's content into a table row
        formatter.openRow();

        for (int i = 0; i < r.numColumns; i++)
        {

            formatter.writeCell(entryBuf[i], 1);

        }
        // the row's finished now
        formatter.closeRow();

        // remember the last reporter
        lastReporter = r;

    }
}
}
