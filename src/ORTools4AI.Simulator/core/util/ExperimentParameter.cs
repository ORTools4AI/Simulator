// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Ruth Meyer, Nicolas Knaak
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.util
{
    /**
     * An experiment parameter implemented as a mutable access point. This class is
     * necessary for setting experiment parameters via a graphical user interface.
     */
    public class ExperimentParameter : MutableAccessPoint
    {
        private string _name;
        private object _value;
        private Type _type;

        /** Constructs a new experiment parameter. */
        public ExperimentParameter(string name, object value)
        {
            _name = name;
            _value = value;

            // For enums, store the declaring type so the parameter accepts any
            // value of that enum, not just the specific value passed in.
            if (value is Enum)
                _type = value.GetType();
            else
                _type = value.GetType();
        }

        public string getName() => _name;

        public object getValue() => _value;

        /** Sets the value if it is assignment-compatible with the parameter's type. */
        public void setValue(object val)
        {
            if (val != null && _type.IsAssignableFrom(val.GetType()))
                _value = val;
        }
    }
}
