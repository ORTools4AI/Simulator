// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Tim Janz
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{


    /**
     * Class to Represent a Parameter. This parameter can be an experiment-
     * parameter or a model-parameter. Parameters are variables that are
     * accessible from everywhere in the model. Model-parameters are assignable
     * from model, experiment-parameters from experiment.
     * 
     * @see desmoj.core.simulator.ParameterManager
     * @see desmoj.core.simulator.ModelParameterManager
     * @see desmoj.core.simulator.ExperimentParameterManager
     */
    public class Parameter
    {
        /**
         * Enum to decide, if a parameter is a model-parameter or an experiment-
         * parameter
         */
        public enum ParameterType
        {
            EXPERIMENTPARAMETER, MODELPARAMETER
        }

        /**
         * Parameter can be a model-parameter or an experiment-parameter
         */
        private ParameterType _parameterType;

        /**
         * The type of the parameter's value
         */
        private Type _type;

        /**
         * The parameter's name
         */
        private string _name;

        /**
         * The parameter's value
         */
        private object _value;

        /**
         * The parameter's default value (only for experiment-parameters)
         */
        private object _defaultValue;

        /// <summary>
        /// Constructs a new parameter. New Parameters should be created by the factory- methods of this
        /// class.
        /// </summary>
        /// <param name="parameterType">is this parameter a model-parameter or an experiment-parameter</param>
        /// <param name="type">the parameter's type</param>
        /// <param name="name">the parameter's name</param>
        /// <param name="value">the parameter's value</param>
        /// <param name="defaultValue">the parameter's default value</param>
        private Parameter(ParameterType parameterType, Type type, String name, Object value, Object defaultValue)
        {
            this._parameterType = parameterType;
            this._type = type;
            this._name = name;
            this._value = value;
            this._defaultValue = defaultValue;
        }

        /// <summary>Returns the parameter's type.</summary>
        /// <returns>the parameter's type</returns>
        public Type getType()
        {
            return _type;
        }

        /// <summary>Returns the parameter's name</summary>
        /// <returns>the parameter's name</returns>
        public String getName()
        {
            return _name;
        }

        /// <summary>sets the parameter's value.</summary>
        /// <param name="value">the parameter's new value</param>
        public void setValue(object value)
        {
            if (_type.IsAssignableFrom(value.GetType()))
            {
                this._value = value;
            }
            else
            {
                //TODO Exception (wrong type)
            }
        }

        /// <summary>returns the parameter's value</summary>
        /// <returns>the parameter's value</returns>
        public object getValue()
        {
            if (!hasValue() && !hasDefaultValue())
            {
                //TODO Exception
            }

            return hasValue() ? _value : _defaultValue;
        }

        /// <summary>Checks, if this parameter has a value.</summary>
        /// <returns>if parameter has a value</returns>
        public bool hasValue()
        {
            return (_value != null);
        }

        /// <summary>Checks, if this parameter has a default value.</summary>
        /// <returns>if parameter has a default value</returns>
        public bool hasDefaultValue()
        {
            return (_defaultValue != null);
        }

        /// <summary>Is this parameter a model-parameter or an experiment-parameter?</summary>
        /// <returns>
        /// ParameterType.MODELPARAMETER if this parameter is a model-parameter, or
        /// ParameterType.EXPERIMENTPARAMETER if this parameter is an experiment-parameter
        /// </returns>
        public ParameterType getParameterType()
        {
            return _parameterType;
        }

        /// <summary>Returns the parameter as a String</summary>
        /// <returns>the parameter as a String</returns>
        public override string ToString()
        {
            return "(" + _type.ToString() + ") " + _name + ": " + _value?.ToString();
        }

        /// <summary>Factory-method to create an experiment-parameter.</summary>
        /// <param name="type">the parameter's type</param>
        /// <param name="name">the parameter's name</param>
        /// <param name="defaultValue">the parameter's default value</param>
        /// <returns>the created instance of Parameter</returns>
        public static Parameter createExperimentParameter(Type type, string name, object defaultValue)
        {
            return new Parameter(Parameter.ParameterType.EXPERIMENTPARAMETER, type, name, null, defaultValue);
        }

        /// <summary>Factory-method to create an experiment-parameter.</summary>
        /// <param name="type">the parameter's type</param>
        /// <param name="name">the parameter's name</param>
        /// <returns>the created instance of Parameter</returns>
        public static Parameter createExperimentParameter(Type type, string name)
        {
            return createExperimentParameter(type, name, null);
        }

        /// <summary>Factory-method to create a model-parameter.</summary>
        /// <param name="type">the parameter's type</param>
        /// <param name="name">the parameter's name</param>
        /// <param name="value">the parameter's value</param>
        /// <returns>the created instance of Parameter</returns>
        public static Parameter createModelParameter(Type type, string name, object value)
        {
            return new Parameter(Parameter.ParameterType.MODELPARAMETER, type, name, value, null);
        }

        /// <summary>Factory-method to create a model-parameter.</summary>
        /// <param name="type">the parameter's type</param>
        /// <param name="name">the parameter's name</param>
        /// <returns>the created instance of Parameter</returns>
        public static Parameter createModelParameter(Type type, string name)
        {
            return createModelParameter(type, name, null);
        }
    }
}

