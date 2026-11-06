// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using System.Xml.Linq;

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * Derive from this class to create a conditional which the model as whole
     * may fulfill or not, e.g. for the purpose of conditionally stopping an 
     * experiment. Override method check(E e) such that it returns 
     * <code>true</code> whenever the model complies to the condition.
     */
    public abstract class ModelCondition : ModelComponent
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

        /// <summary>Constructs a ModelCondition with the given name and parameters for trace files.</summary>
        /// <param name="owner">The main model this condition is associated to</param>
        /// <param name="name">The name of this condition</param>
        /// <param name="showInTrace">
        /// Flag for showing this condition in trace-files. Set it to <c>true</c> if model should show up in
        /// trace, <c>false</c> if model should not be shown in trace.
        /// </param>
        /// <param name="args">Object... : Arguments to pass to the condition (can be omitted)</param>
        public ModelCondition(Model owner, string name, bool showInTrace, object[] args)
            : base(owner, name, showInTrace)
        {

            //super(owner, name, showInTrace); // create a ModelComponent

            var types = new Type[3 + args.Length];
            //types[0] = Model.class;
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
        /// Returns a boolean showing whether the model complies to the condition tested by this method or
        /// not. Inherit from this class and implement this abstract method to return true if the model
        /// conforms to your special condition.
        /// </summary>
        /// <returns>Is <c>true</c>, if the model conforms to the condition, <c>false</c> otherwise.</returns>
        public abstract bool check();

        private Type getType(object obj)
        {
            var cls = obj.GetType();

            //return cls.equals(Boolean.class) ? Boolean.TYPE :
            //    cls.equals(Integer.class) ? Integer.TYPE :
            //    cls.equals(Character.class) ? Character.TYPE :
            //    cls.equals(Byte.class) ? Byte.TYPE :
            //    cls.equals(Short.class) ? Short.TYPE :
            //    cls.equals(Double.class) ? Double.TYPE :
            //    cls.equals(Long.class) ? Long.TYPE :
            //    cls.equals(Float.class) ? Float.TYPE :
            //    cls.equals(String.class) ? String.class :
            return null;
        }

        // @Deprecated
        /**
         * @deprecated Use ModelCondition instead of Condition for conditions
         * not referring to a single entity.
         * 
         * A wrapper to use a Condition's check() as a ModelCondition.
         */
        public class ConditionWrapper<T> : ModelCondition where T : Entity
        {

            /**
             * Stores the condition to evaluate.
             */
            Condition<T> _cond;

            /// <summary>Constructs a ConditionWrapper.</summary>
            /// <param name="owner">The main model this ConditionWrapper is associated to</param>
            /// <param name="cond">The condition to evaluate</param>
            public ConditionWrapper(Model owner, Condition<T> cond) : base(owner, "Wrapper of " + cond, false, null)
            {
                //super(owner, "Wrapper of " + cond, false);
                this._cond = cond;
            }

            /// <summary>
            /// Returns a boolean showing whether the model complies to the internal condition or not.
            /// </summary>
            /// <returns>
            /// Is <c>true</c>, if the model conforms to the condition, <c>false</c> otherwise.
            /// </returns>
            public override bool check()
            {
                return _cond.check();
            }
        }
    }
}
