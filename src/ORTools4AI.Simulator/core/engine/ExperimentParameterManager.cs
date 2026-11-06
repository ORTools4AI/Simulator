// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @author Tim Janz
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * Interface to restrict the functionalities of a parameter-manager
     * to manage experiment-parameters. This Interface should be used as
     * return-type of the Experiment's methods which return the parameter-
     * manager. 
     */
    public interface ExperimentParameterManager
    {
        /// <summary>
        /// Method assign a value to an experiment-parameter declared previously by calling the model's
        /// parameter-manager method to declare an experiment-parameter
        /// </summary>
        /// <param name="name">the parameter's name</param>
        /// <param name="value">the parameter's value</param>
        public void assignExperimentParameter(String name, Object value);
    }

}