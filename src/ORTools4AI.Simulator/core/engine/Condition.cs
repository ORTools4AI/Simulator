// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * Derive from this class to create a conditional which an entity (including
     * processes as special entities) may fulfill or not, e.g. for the purpose 
     * of queue filtering. Override method check(E e) such that it returns 
     * <code>true</code> whenever an entity complies to the condition.
     * 
     * For conditions that do not apply to a specific Entity, but to the 
     * model as whole (e.g. when to stop an experiment), use  
     * <code>ModelCondition</code>.
     */
    public abstract class Condition<T> : ModelComponent where T:Entity
    {

        /**
         * Stores the arguments of a Constructor-call to make a recreation
         * of the Condition possible.
         */
        private object[] _arguments = null;

        /**
         * Stores if all arguments of a Constructor-call are of primitive
         * types or Strings.
         */
        private bool _argumentsPrimitive;

        /// <summary>Constructs a condition with the given name and parameters for trace files.</summary>
        /// <param name="owner">The main model this condition is associated to</param>
        /// <param name="name">The name of this condition</param>
        /// <param name="showInTrace">
        /// Flag for showing this condition in trace-files. Set it to <c>true</c> if model should show up in
        /// trace, <c>false</c> if model should not be shown in trace.
        /// </param>
        /// <param name="args">Object... : Arguments to pass to the condition (can be omitted)</param>
        public Condition(Model owner, string name, bool showInTrace, params object[] args) : base(owner, name, showInTrace)
        {

            //super(owner, name, showInTrace); // create a ModelComponent

            var types = new Type[3 + args.Length];
            //types[0] =  Model.class;
            //types[1] = String.class;
            //types[2] = Boolean.TYPE;

            _arguments = new object[3 + args.Length];
            _arguments[0] = owner;
            _arguments[1] = name;
            _arguments[2] = showInTrace;

            _argumentsPrimitive = true;

            for (int i = 0; i < args.Length; i++)
            {

                var type = getType(args[i]);
                _argumentsPrimitive &= (type != null);
                types[i + 3] = type;
                _arguments[i + 3] = args[i];
            }

        }

        //TODO Javadoc
        public bool hasPrimitiveArguments()
        {
            return _argumentsPrimitive;
        }

        //TODO Javadoc
        public object[] getConstructArguments()
        {
            return _arguments;
        }

        /// <summary>
        /// Returns a boolean showing whether the given Entity complies to the condition tested by this
        /// method or not. Inherit from this class and implement this abstract method to return true if the
        /// passed Entity conforms to your special condition.
        /// </summary>
        /// <param name="e">The Entity to test the condition on</param>
        /// <returns>
        /// Is <c>true</c>, if the entity given conforms to the condition, <c>false</c> otherwise.
        /// </returns>
        public abstract bool check(T e); // changed to generic parameter

        /// <returns>Is <c>true</c>, if the model conforms to the condition, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Replaced by ModelCondition.check() Returns a boolean showing whether the model complies to the
        /// condition tested by this method or not. Inherit from this class and override this method to
        /// return true if the model conforms to your special condition; the entity type this condition was
        /// parameterised with is ignored.
        /// </remarks>
        //@Deprecated
        public bool check()
        {
            throw (new DESMOJException(
                    new ErrorMessage(
                            this.getModel(),
                            "Call to deprecated Condition.check() without appropriately overriding this method",
                            this.getName() + ":check()",
                            "Subclass did not provide a routine to check",
                            "Override Condition.check(); preferredly, use ModelCondition for conditions applying to the model as whole",
                            this.presentTime())));
        }

        private Type getType(object obj)
        {
            if (obj == null) return null;
            Type cls = obj.GetType();
            if (cls == typeof(bool))    return typeof(bool);
            if (cls == typeof(int))     return typeof(int);
            if (cls == typeof(char))    return typeof(char);
            if (cls == typeof(byte))    return typeof(byte);
            if (cls == typeof(short))   return typeof(short);
            if (cls == typeof(double))  return typeof(double);
            if (cls == typeof(long))    return typeof(long);
            if (cls == typeof(float))   return typeof(float);
            if (cls == typeof(string))  return typeof(string);
            return null;
        }
    }
}