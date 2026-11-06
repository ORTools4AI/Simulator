// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.dist
{

    /**
     * Interface for uniform pseudo random number generators to be used by the
     * distribution classes to compute their samples. Random generators of this type
     * must return uniform distributed double values. Note that two constructors
     * have to be implemented. One constructor without parameters to construct a
     * UniformRandomGenerator with seed zero, and another constructor with a
     * parameter of type long to pass the initial seed value to the pseudo random
     * generation algorithm.
     */
    public interface UniformRandomGenerator
    {
        /// <summary>
        /// Returns the next uniform distributed [0,1] double pseudo random number from the random
        /// generator's stream.
        /// </summary>
        /// <returns>The next uniformly distributed [0,1] double value</returns>
        public double nextDouble();

        /// <summary>
        /// Set the seed to be used by the pseudo random number generating algorithm. Setting the seed to a
        /// specific value forces the random generator to produce identical streams of pseudo random numbers.
        /// This is necessary to ensure that experiments in simulation can be made under identical
        /// conditions, thus enabling modelers to compare results of experiments.
        /// </summary>
        /// <param name="seed">
        /// The seed to be used for creating the folowing stream of pseudo random numbers
        /// </param>
        public void setSeed(long seed);
    }
}