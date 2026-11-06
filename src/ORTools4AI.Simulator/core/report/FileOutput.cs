// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.report
{

	/**
     * FileOutput is the class all other filebased Output classes used within the
     * DESMO-j framework are derived from. The basic functionality needed for all
     * FileOutput objects are methods to open and close the stream used for output
     * and set the two necessary types of separators for column output. Two methods
     * for writing Strings into the file are supported. The name is needed for
     * opening a file to identify the output channel in terms of window titles or
     * file names, depending on the type of output the specific subclass relies on.
     * Note that each object of this class supports the creation of several files in
     * a row. You can open and close as many files as needed with one object.
     */
	public class FileOutput {

		/**
		 * The character used to indicate the end of a line.
		 */
		private static string eol = Environment.NewLine; //System.getProperty("line.separator"); // standard OS eol

		/**
		 * The character used to separate individual entries within a line.
		 */
		private static string sep = ";"; // standard String to separate entries

		/**
		 * The name of the file produced by this FileOutput.
		 */
		protected string fileName;

		/**
		 * The FileWriter used to open, close and write file and data.
		 */
		protected  TextWriter file;

		/**
		 * The status of the current file.
		 */
		protected bool fileOpen;

		/**
		 * Remembers if any data has been written to it. If not, it might as well be
		 * deleted.
		 */
		protected bool empty;

		/// <summary>
		/// Constructs a FileOutput object. A FileOuput object is capable of opening a file using method
		/// <c>open(String fileName)</c>. Note that if the output stream used cannot be opened, the whole error
		/// handling is printed to the system's standard output stream. The 'endOfLine' String is set to the
		/// underlying system's String used to separate lines as given by the system property
		/// <c>line.separator</c>.
		/// </summary>
		public FileOutput() {

			fileOpen = false; // no file open yet
			empty = true; // and nothing written to it either
			fileName = null; // name can only be given with method open(String)
		}

		/// <summary>
		/// Flushes the buffer and closes the file. note that if another file with the same name is opened and
		/// written to, it will overwrite the previous file on the disc without notice. All users of Output objects
		/// have to call <c>close()</c> to properly shut down the ouput stream after the last call to the printing
		/// methods. Classes overriding this method should put all necessary actions to properly close their file
		/// into the <c>close()</c> method and make a call to <c>super.close()</c> as their last command to finally
		/// close buffers and files.
		/// </summary>
		public void close() {

			if (!fileOpen)
				return; // file not yet openend

			try {
				file.Flush();
				file.Close();
				fileOpen = false;
				empty = true;
			} catch (IOException ioEx) {
                Console.WriteLine("IOException thrown : " + ioEx);
                Console.WriteLine("description: Can't flush and close " + fileName);
                Console.WriteLine("origin     : Experiment auxiliaries");
                Console.WriteLine("location   : FileOutput.close()");
                Console.WriteLine("hint       : Check access to the file and"
						+ " that it is not in use by some other application.");
                Console.WriteLine("The System will not be shut down. But the file "
							+ fileName
							+ " can not be closed and may contain no data!");
				/*
				 * the system will not be shut down, because this may disrupt other
				 * programs like CoSim from Ralf Bachmann (Universtiy of Hamburg,
				 * germany).
				 */
				// System.exit(-1); // radical but no time for fileselectors now
			}

		}

		/// <summary>
		/// Returns the currently valid String for separating lines of output. This is a platform dependent String
		/// and is set to the underlying system's String by default.
		/// </summary>
		/// <returns>The String used to separate lines</returns>
		public static string getEndOfLine() {

			return eol;

		}

		/// <summary>
		/// Returns the current fileName of this FileOutput or <c>null</c> if it has not been opened and named yet.
		/// </summary>
		/// <returns>The name of the file or <c>null</c> if file has not been opened and named yet</returns>
		public string getFileName() {

			return fileName;

		}

		/// <summary>
		/// Returns the currently active String for separating individual entries in a line of output. This comes
		/// in handy, when trying to write files in a certain format i.e. dbf format. Data from files complying to
		/// that format can easily be read and imported to a variety of standard statistics packages for further
		/// manipulation and analysis.
		/// </summary>
		/// <returns>The String used to separate entries within a line</returns>
		public static string getSeparator() {

			return sep;

		}

		/// <summary>
		/// Returns a boolean value showing if relevant data has been written to the file. If only file headings
		/// have been written, this method will return <c>true</c> to indicate that it does not contain information
		/// and thus might just as well be deleted. If data has been written to it, this method will return
		/// <c>false</c> instead.
		/// </summary>
		/// <returns>
		/// Is <c>true</c> if no relevant data but the heading has been written to this file, <c>false</c>
		/// otherwise
		/// </returns>
		public bool isEmpty() {
			return empty;
		}

		/// <summary>
		/// Returns the current state of the FileOutput. <c>True</c> is returned, if the file has been opened
		/// successfully and can be written to. If the file has been closed or has not been opened yet,
		/// <c>false</c> is returned.
		/// </summary>
		/// <returns>Is <c>true</c> if the file is currently open, <c>false</c> if not</returns>
		public bool isOpen() {

			return fileOpen;

		}

		/// <summary>
		/// Opens a new file with the given fileName for writing data to. If no String is given, the default
		/// filename "unnamed_DESMOJ_file" is used. Note that opening a file with a name that already exists in the
		/// user's target directory will overwrite that existing file without notice! So be careful when choosing
		/// file names and make sure to use unique names.
		/// </summary>
		/// <param name="name">The name of the file to be created</param>
		public void open(string name) {

			if (fileOpen)
				return; // file already opened

			if (name != null)
				fileName = name;// create the named object
			else
				fileName = "unnamed_DESMOJ_File";

			// now try to create a new file in the user's standard directory
			try {
				file = File.CreateText(fileName);

				fileOpen = true;
				empty = true;
			} catch (IOException ioEx) {
                Console.WriteLine("IOException thrown : " + ioEx);
                Console.WriteLine("description: Can't create file " + fileName);
                Console.WriteLine("origin     : While creating the Experiment "
						+ "auxiliaries.");
                Console.WriteLine("location   : method open() in class FileOutput.");
                Console.WriteLine("hint       : Check access to the default path and "
							+ "that no file of thesame name exists");
                Console.WriteLine("The System will not be shut down. But the file "
							+ fileName
							+ " can not be opened and may not exist as "
							+ "expected!");
				/*
				 * the system will not be shut down, because this may disrupt other
				 * programs like CoSim from Ralf Bachmann (Universtiy of Hamburg,
				 * germany).
				 */
				// System.exit(-1); // radical but no time for fileselectors now
			}

		}

		/// <summary>
		/// Sets the end-of-line separator String to the given parameter value. The default value is the
		/// end-of-line String of the underlying platform. Use this method to create files of specific formats.
		/// Data from files complying to that format can easily be read and imported to a variety of standard
		/// statistics packages for further manipulation and analysis.
		/// </summary>
		/// <param name="eolString">The String for separating lines</param>
		public static void setEndOfLine(string eolString) {

			eol = eolString;

		}

		/// <summary>
		/// Sets the entry separator String to the given parameter value. The default value is ';'. This is useful,
		/// when trying to write files in a certain format i.e. dbf format. Data from files complying to that
		/// format can easily be read and imported to a variety of standard statistics packages for further
		/// manipulation and analysis.
		/// </summary>
		/// <param name="sepString">The String for separating entries within a line</param>
		public static void setSeparator(string sepString) {

			sep = sepString;

		}

		/// <summary>
		/// Writes the given String to the open file. If the given String is empty, the method will simply return
		/// without action. Override this method if you want your data to be written with specific tags as used in
		/// the <c>HTMLFileOutput</c> class.
		/// </summary>
		/// <param name="s">The String to write to the file</param>
		/// <remarks>See also HTMLFileOutput</remarks>
		public void write(string s) {
			if (s == null)
				return; // again nulls
			if (!fileOpen)
				return; // file not yet openend
			empty = false; // remember that something's written to file

			// try to write and handle Exception if needed
			try {
				file.Write(s);
			} catch (IOException ioEx) {
				Console.WriteLine("IOException thrown : " + ioEx);
                Console.WriteLine("description: Can't write to file " + fileName);
                Console.WriteLine("origin     : Experiment auxiliaries");
                Console.WriteLine("location   : class FileOutput, method write()");
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

		/// <summary>
		/// Writes the given String to the open file, adding a line separator to the end of the String written. If
		/// the string given is null or "" just a new line character will be written to the file. This method
		/// simply adds the end-of-line String to the String given and calls the <c>void write(String s)</c>
		/// method.
		/// </summary>
		/// <param name="s">The String to write to the file</param>
		public void writeln(string s) {

			if ((s == null) || (s.Length == 0)) {
				write(eol);
				return;
			}

			write(s + eol);

		}

		/// <summary>
		/// Writes the given String to the open file, adding a separator to the end of the String written. If the
		/// string given is null or "" just a new line character will be written to the file. This method simply
		/// adds the end-of-line String to the String given and calls the <c>void write(String s)</c> method.
		/// </summary>
		/// <param name="s">The String to write to the file</param>
		public void writeSep(string s) {

			if ((s == null) || (s.Length == 0)) {
				write(sep);
				return;
			}

			write(s + sep);
		}
	}
}