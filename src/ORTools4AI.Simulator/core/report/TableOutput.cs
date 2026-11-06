// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.report
{
    public class TableOutput:FileOutput
    {
        /** The table formatter assigned to this output */
        protected TableFormatter formatter;

        /** The table formatter used if formatter is invalid */
        public static readonly string DEFAULT_FORMATTER = "ORTools4AI.Simulator.core.report.HTMLTableFormatter";
        public static readonly string HTML_TABLE_FORMATTER = "ORTools4AI.Simulator.core.report.HTMLTableFormatter";

    /// <summary>Creates a new TableOutput with the given time precision and TableFormatter</summary>
    /// <param name="timePrec">precision for writing time values.</param>
    /// <param name="format">fully qualified class name of assigned TableFormatter</param>
    public TableOutput(int timePrec, string format)
        {
            setSeparator("\t");
            formatter = createFormatter(format);
            formatter.setOutput(this);
            formatter.setTimePrecision(timePrec);
        }

        /// <summary>Opens the table output as a file with the given name</summary>
        /// <param name="name">filename</param>
        public void open(string name)
        {
            base.open(name);
            formatter.open(name);
        }

        /** Closes the table output (and the assigned file). */
        public void close()
        {
            formatter.close();
            base.close();
        }

        /// <summary>
        /// Tries creating a TableFormatter from the given class name. If creation fails (e.g. because the
        /// class name does not exist) the assigned experiments default table formatter is created
        /// </summary>
        /// <param name="formatter">fully qualified class name of TableFormatter subclass.</param>
        /// <returns>
        /// created TableFormatter object Since the Experiment class no longer contains a default formater,
        /// the default formatter is now located in this class
        /// </returns>
        protected TableFormatter createFormatter(string formatter)
        {
           
            try
            {

                //Class <?> formatterClass = Class.forName(formatter);
                //t = (TableFormatter)formatterClass.newInstance();
                var type = Type.GetType(formatter);
                var t=Activator.CreateInstance(type) as TableFormatter;
                return t;
            }
            catch (Exception e)
            {
                // e.printStackTrace();
                return createFormatter(DEFAULT_FORMATTER);
            }
        }

        /// <summary>Creates a valid filename from the given information</summary>
        /// <param name="pathname">name of path</param>
        /// <param name="name">name of file</param>
        /// <param name="type">kind of simulation output (e.g. <c>"report"</c>)</param>
        protected string createFileName(string pathname, string name, string type)
        {

            // check for proper path and filename
            if ((pathname == null) || (pathname.Length == 0)|| (pathname.Equals(".")))
                pathname = Directory.GetCurrentDirectory().ToString();
            if ((name == null) || (name.Length == 0))
                name = "DESMOJ";

            // check for proper suffix of the filename
            string appendix = "." + formatter.getFileFormat().ToLower();
            if ((!name.EndsWith(appendix))
                    || (!name.EndsWith(appendix.ToUpper())))
                return Path.Combine( pathname , name + "_" + type + appendix);
            else
                return Path.Combine(pathname , name);
        }

    }
}
