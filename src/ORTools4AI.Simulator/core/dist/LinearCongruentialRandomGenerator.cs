// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.dist
{

	/**
     * Linear congruential random generator for uniformly distributed pseudo random
     * numbers configured such that a stream of [0,1] double values is produced.
     * Implements the <code>desmoj.dist.UniformRandomGenerator</code> interface. All
     * <code>Distributions</code> in this package use this random generator by
     * default. The implementation is based on the Java API
     * <code>java.util.Random</code> class' random generator. The Java API Random
     * class uses a 48-bit seed as input to the linear congruential formula. (See
     * Donald Knuth, The Art of Computer Programming, Volume 2, Section 3.2.1.)
     * 
     * @see desmoj.core.dist.UniformRandomGenerator
     * @see desmoj.core.dist.Distribution
     * @see java.util.Random
     */
	public class LinearCongruentialRandomGenerator : UniformRandomGenerator
	{

		/**
		 * The random generator provided by the Java API class
		 * <code>java.util.Random</code>
		 */
		protected Random javaAPIRandomGenerator; // the random generator used

		/**
		 * Creates a DefaultrandomGenerator with seed 42. The value 42 has been
		 * taken by pure coincidence. It is not really related to Douglas Adams'
		 * "Hitchhikers Guide to the Galaxy" trilogy of five books (so far).
		 */
		public LinearCongruentialRandomGenerator()
		{

			javaAPIRandomGenerator = new Random(42);

		}

		/// <summary>Creates a DefaultrandomGenerator with given value as initial seed.</summary>
		/// <param name="seed">The initial seed of the underlying pseudo random generator</param>
		public LinearCongruentialRandomGenerator(long seed)
		{
			javaAPIRandomGenerator = new Random((int)seed);
		}

		/// <summary>
		/// Returns the next pseudo random uniform [0,1] distributed double value from the stream produced by the
		/// underlying pseudo random number generator.
		/// </summary>
		/// <returns>The next pseudo random uniform [0,1] distributed double value</returns>
		public double nextDouble()
		{

			return javaAPIRandomGenerator.NextDouble();

		}

		/// <summary>Sets the seed for the pseudo random number generator.</summary>
		/// <param name="newSeed">The new initial seed value for the pseudo random number generator</param>
		public void setSeed(long newSeed)
		{

			javaAPIRandomGenerator = new Random((int)newSeed);

		}
	}
}