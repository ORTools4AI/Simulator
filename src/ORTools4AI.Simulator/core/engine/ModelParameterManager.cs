// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Tim Janz
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * Interface to restrict the functionalities of a parameter-manager
     * to manage model-parameters. This Interface should be used as return-
     * type of the Model's methods which return the parameter-
     * manager. 
     */
    public interface ModelParameterManager
    {
        /// <summary>
        /// Method to declare a experiment-parameter. The experiment-parameter's declarations are part of the
        /// simulation-model. The experiment- parameter's assignments are in contrast part of an experiment.
        /// </summary>
        /// <param name="type">the experiment-parameter's type</param>
        /// <param name="name">the experiment-parameter's name</param>
        public void declareExperimentParameter(Type type, string name);

        /// <summary>
        /// Method to declare a experiment-parameter. The experiment-parameter's declarations are part of the
        /// simulation-model. The experiment- parameter's assignments are in contrast part of an experiment.
        /// The default value is used, if an experiment dosen't assign a value to this parameter.
        /// </summary>
        /// <param name="type">the experiment-parameter's type</param>
        /// <param name="name">the experiment-parameter's name</param>
        /// <param name="defaultValue">the experiment-parameter's default value</param>
        public void declareExperimentParameter(Type type, string name, object defaultValue);

        /// <summary>Method to declare a model-parameter.</summary>
        /// <param name="type">the model-parameter's type</param>
        /// <param name="name">the model-parameter's name</param>
        public void declareModelParameter(Type type, string name);

        /// <summary>Method to initialize (declare and assign) a model-parameter.</summary>
        /// <param name="type">the model-parameter's type</param>
        /// <param name="name">the model-parameter's name</param>
        /// <param name="value">the model-parameter's value</param>
        public void initializeModelParameter(Type type, string name, object value);

        /// <summary>Method to assign a value to a model-parameter declared previously.</summary>
        /// <param name="name">the model-parameter's name</param>
        /// <param name="value">the model-parameter's value</param>
        public void assignModelParameter(string name, object value);

        /// <summary>
        /// Returns the value of an experiment- or model-parameter. Experiment- and model-parameters are
        /// accessible through the model, to be able to use the parameter's values at design-time.
        /// </summary>
        /// <param name="name">the parameter's name</param>
        /// <returns>the parameter's value</returns>
        public object getParameterValue(string name);

        /// <summary>Returns all declared parameters.</summary>
        /// <returns>the parameters</returns>
        public ICollection<Parameter> getParameters();
    }
}