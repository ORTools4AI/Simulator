// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Nicolas Knaak
// Modified for ORTools4AI.Simulator.


namespace ORTools4AI.Simulator.core.report
{
   
    /**
     * A basic implementation of the TableFormatter interface realizing common
     * properties of table formatters.
     */
    public abstract class AbstractTableFormatter : TableFormatter
    {

    /** Flag indicating if table is open */
    protected bool tableOpen = false;

    /** Flag indicating if table row is open */
    protected bool rowOpen = false;

    /** Precision of time values printed to table */
    private int _timeFloats = 0;

    /** The FileOutput this table writer writes to. */
    protected FileOutput output = null;

	/** The Reporter displayed in current row. */
	protected Reporter _currentReporter = null;

    /// <summary>
    /// Sets tableOpen flag to true. Should always be called from subclass implementations of this method.
    /// </summary>
    /// <returns>value of row open flag</returns>
    /// <remarks>See also TableFormatter.rowIsOpen()</remarks>
    public bool rowIsOpen()
    {
        return rowOpen;
    }

    /// <returns>value of table open flag;</returns>
    /// <remarks>See also TableFormatter.tableIsOpen()</remarks>
    public bool tableIsOpen()
    {
        return false;
    }

    /// <summary>Returns a formatted time string</summary>
    /// <param name="t">time string</param>
    /// <returns>formatted version of time string</returns>
    /// <remarks>See also TableFormatter.writeTime(string)</remarks>
    public string writeTime(string t)
    {
        /*
		if (t == null)
			return "None";

		if (t.lastIndexOf(".") == -1)
			return t; // no decimal point -> just print it

		int decPoint = t.lastIndexOf(".");

		if (t.length() - decPoint <= timeFloats + 1)
			return t; // less floats than specified

		if (t.lastIndexOf("E") == -1)
		    return t.substring(0, decPoint + timeFloats + 1);  // no scientific notation
		else
		    return t.substring(0, decPoint + timeFloats + 1) + t.substring(t.lastIndexOf("E")); // scientific notation
		*/
        return t;
    }

    /// <returns>time precision</returns>
    /// <remarks>See also TableFormatter.timePrecision()</remarks>
    public int timePrecision()
    {
        return _timeFloats;
    }

    /// <summary>Sets an output file to write the table to</summary>
    /// <param name="output">The file output used to write the table.</param>
    public void setOutput(FileOutput output)
    {
        this.output = output;
    }

    /// <summary>Sets the time precision</summary>
    /// <param name="tf">time precision</param>
    public void setTimePrecision(int tf)
    {
        this._timeFloats = tf;
    }

    /// <summary>
    /// Opens a new row. Calls openRow() and saves reference on reporter which will we displayed in that row.
    /// Reference on reporter is not necessary for getting content of reporter!!! But maybe its useful for
    /// some kind of extended reports.
    /// </summary>
    /// <param name="rep">Reporter which will be displayed in new row</param>
    public  void openRow(Reporter rep)
    {
        this._currentReporter = rep;
        openRow();
    }

    /// <summary>
    /// Returns the reporter which is displayed in current row. Maybe null, if openRow() insted of
    /// openRow(Reporter) was called.
    /// </summary>
    /// <returns>the reporter which is displayed in current row.</returns>
    protected Reporter getReporterForCurrentRow()
    {
        return _currentReporter;
    }

        public virtual void open(string name)
        {
            throw new NotImplementedException();
        }

        public virtual void close()
        {
            throw new NotImplementedException();
        }

        public virtual void closeRow()
        {
            throw new NotImplementedException();
        }

        public virtual void closeTable()
        {
            throw new NotImplementedException();
        }

        public virtual void closeTableNoTopTag()
        {
            throw new NotImplementedException();
        }

        public virtual void openRow()
        {
            throw new NotImplementedException();
        }

        public virtual void openTable(string heading)
        {
            throw new NotImplementedException();
        }

        public virtual void writeCell(string s, int spanning)
        {
            throw new NotImplementedException();
        }

        public virtual void writeHeading(int i, string s)
        {
            throw new NotImplementedException();
        }

        public virtual void writeHeadingCell(string s)
        {
            throw new NotImplementedException();
        }

        public virtual void writeHorizontalRuler()
        {
            throw new NotImplementedException();
        }

        public virtual string getFileFormat()
        {
            throw new NotImplementedException();
        }
    }
}
