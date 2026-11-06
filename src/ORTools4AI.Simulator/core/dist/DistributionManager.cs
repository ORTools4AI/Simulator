// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;


namespace ORTools4AI.Simulator.core.dist
{

	/**
     * Controls all distributions used during an experiment. Provides the service of
     * automatic seed generation for all distributions registered at the
     * distributionmanager. Note that all distributions register at instantiation
     * time at the experiment's distributionmanager automatically.
     */
	public class DistributionManager : NamedObject
	{

		/**
		 * The current default random number generator to be used by newly created
		 * distributions.
		 */
		private UniformRandomGenerator _currentDefaultGenerator;

		/**
		 * Value sets antithetic mode for all distributions registering at the
		 * distributionmanager.
		 */
		private bool _antitheticMode;

		/**
		 * Keeps references to all distributions of this experiment
		 */
		private List<Distribution> _distributions;

		/**
		 * Produces all starting seeds for registered distributions.
		 */
		private UniformRandomGenerator _seedGenerator;

		/**
		 * The seed of the internal seed-generator
		 */
		private long _seed;

		/// <summary>
		/// Creates a new distributionManager with the given name and the given initial seed for the
		/// seed-generator.
		/// </summary>
		/// <param name="name">The distributionmanager's name</param>
		/// <param name="seed">The initial seed for the seedgenerator</param>
		public DistributionManager(string name, long seed) : base(name + "_DistributionManager")
		{

			//super(name + "_DistributionManager"); // create the NamedObject

			_antitheticMode = false; // set antithetic mode to false by default
			_seedGenerator = new LinearCongruentialRandomGenerator(seed); // create
																		  // seed
																		  // generator
			_currentDefaultGenerator = new LinearCongruentialRandomGenerator();

			_distributions = new List<Distribution>(); // init List
													   // for dist
			_seed = seed;
		}

		/// <summary>De-registers a distribution from the experiment.</summary>
		/// <param name="dist">The distribution to be deregistered</param>
		public void deRegister(Distribution dist)
		{

			_distributions.Remove(dist); // remove from List

		}

		/**
		 * Provides all registered distributions with new seed values, thus
		 * resetting all distribution statistics at the same time.
		 */
		public void newSeedAll()
		{

			foreach (Distribution d in _distributions)
			{
				d.setSeed(nextSeed());
			}
		}

		/// <summary>Returns a new seed value to be used as an initial seed for registered distributions.</summary>
		/// <returns>A new seed value for a registered distribution</returns>
		public long nextSeed()
		{

			// get a positive seed value
			return (long)(_seedGenerator.nextDouble() * 100000000);

		}

		/// <summary>
		/// Registers a new distribution at the experiment to control antithetic mode and set random seed values.
		/// </summary>
		/// <param name="dist">The distribution to be registered</param>
		public void register(Distribution dist)
		{

			dist.setAntithetic(_antitheticMode); // set antithetic mode to default
			dist.setSeed(nextSeed()); // set new seed
			_distributions.Add(dist); // add to Vector

		}

		/**
		 * Resets all registered distributions. Just calls all distribution's
		 * individual reset method.
		 */
		public void resetAll()
		{

			foreach (Distribution d in _distributions)
			{
				d.reset();
			}
		}

		/// <summary>
		/// Sets antithetic mode to true on all registered distributions regardless of their previous status. No
		/// reset of statistical counters.
		/// </summary>
		/// <param name="antitheticMode">The new status of antithetic mode</param>
		public void setAntitheticAll(bool antitheticMode)
		{

			foreach (Distribution d in _distributions)
			{
				d.setAntithetic(antitheticMode);
			}
		}

		/// <summary>
		/// Sets the seed of the SeedGenerator to the given value. If the seed is not set here, its default is
		/// zero, unless specified in the experimentoptions.
		/// </summary>
		/// <param name="newSeed">The new seed for the seedgenerator</param>
		public void setSeed(long newSeed)
		{

			_seed = newSeed;

			_seedGenerator.setSeed(newSeed); // go ahead and set it!

		}

		/// <summary>Returns the initial seed.</summary>
		/// <returns>the initial seed</returns>
		public long getSeed()
		{
			return _seed;
		}

		/// <summary>
		/// Sets the underlying pseudo random number generator to be used by all distributions created from now on.
		/// The default generator is LinearCongruentialRandomGenerator; any other generator to be used must
		/// implement the interface UniformRandomGenerator.
		/// </summary>
		/// <param name="randomNumberGenerator">The random number generator class to be used</param>
		/// <remarks>See also LinearCongruentialRandomGenerator See also UniformRandomGenerator</remarks>
		public void setRandomNumberGenerator(
				UniformRandomGenerator randomNumberGenerator)
		{

			this._currentDefaultGenerator = randomNumberGenerator;

		}

		/// <summary>
		/// Returns the underlying pseudo random number generator to be used by all distributions. This method is
		/// intended for internal use (i.e. called by Distribution) only.
		/// </summary>
		/// <remarks>
		/// See also LinearCongruentialRandomGenerator See also MersenneTwisterRandomGenerator See also
		/// UniformRandomGenerator
		/// </remarks>
		public UniformRandomGenerator getRandomNumberGenerator()
		{

			return this._currentDefaultGenerator;

		}

		/**
		 * Returns a list containing all distributions.
		 */
		public List<Distribution> getDistributions()
		{
			return new List<Distribution>(this._distributions);
		}
	}
}