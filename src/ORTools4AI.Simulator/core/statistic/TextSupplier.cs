// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Lorna Slawski based on the class ValueSupplier
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.observer;

namespace ORTools4AI.Simulator.core.statistic
{

    /**
     * Supplies observers with texts of their interest. It resembles the
     * "observable" part of the observer pattern as described in [Gamm95]. Thus it
     * extends the standard "Observable" class provided by the Java.util package.
     * <br
     * There are two possible ways to have other objects
     * register changes of another object's status:
     * <ol
     * <liCall the <code"update(String updateStr)"</codemethod of an observer to
     * pass the String to be counted directly to one other object interested. Using the
     * direct call allows easy passing of a String to a single observer. No instance
     * of this <codeTextSupplier</codeclass is needed using direct passing of
     * Strings.</li
     * <liUsing the <codeTextSupplier</codeto publish the information to
     * interested observers requires you to derive a class and implement the call of
     * the observer's <codeupdate(Observable, Object)</codemethod with the
     * required texts. This way of passing Strings involves more code to be written,
     * but offer the potential to have multiple observers and change the number of
     * observers during runtime. These options are not available when using the
     * simpler version above.</li
     * </ol
     */

    public abstract class TextSupplier : Observable
    {
        /*
         * The name of the TextSupplier as a String.
         */
        private String _name;

        /**
         * Constructs a simple TextSupplier by giving it a name.
         */
        public TextSupplier(string name)
        {
            // call the constructor of java.util.Observable
            // super();

            // get hold of the name of this TextSupplier
            this._name = name;
        }

        /// <summary>
        /// Returns the name of the named object. This is the same name displayed in reports and trace files
        /// when this named object is shown in those reports or trace files.
        /// </summary>
        /// <returns>The name of the named object.</returns>
        public string getName()
        {

            return _name;
        }

        /// <summary>
        /// Returns the quoted name of the named object. This is the name displayed in reports and trace
        /// files when this named object is shown.
        /// </summary>
        /// <returns>The quoted name of the named object</returns>
        public String getQuotedName()
        {

            return "'" + _name + "'";
        }

        /// <summary>
        /// Indicates that this TextSupplier (as an Observable) has changed and notifies all its observers of
        /// this change providing the modified text as parameter. This is the "Push-model", thus pushing the
        /// String (in the parameter &lt;codearg&lt;/code) to the Observer (thus the
        /// &lt;codeStatisticObject&lt;/code).
        /// </summary>
        /// <param name="arg">The Object wrapping the text with which the observers will be updated.</param>
        public void notifyStatistics(Object arg)
        {
            setChanged(); // call the method from the Observable

            notifyObservers(arg); // call the method from the Observable
        }

        /// <summary>
        /// Changes the name of the named object. This might be necessary for an automatically created named
        /// object of a user defined model, but should not be used for elements of the framework or basic
        /// objects of the user model. Changing names of objects while runtime will confuse any trace output
        /// and generally make traces more difficult if not impossible to follow.
        /// </summary>
        /// <param name="newName">The new name for the named object.</param>
        public void rename(String newName)
        {

            _name = newName;
        }

        /// <summary>
        /// When using the Pull-Model or the automatic update function this method must be overridden by the
        /// user in a way that it provides the text (as a &lt;codestring&lt;/code) desired.
        /// </summary>
        /// <returns>The String within this method.</returns>
        public abstract String text();

        /// <summary>
        /// Overrides the object's toString method to return the named object's name when given as parameter
        /// to a method that expects a string to be passed.
        /// </summary>
        /// <returns>The named object's name.</returns>
        public String toString()
        {
            return _name;
        }
    }
}